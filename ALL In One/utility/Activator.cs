using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.utility
{
    class Activator
    {
        //아이템 목록 && 설명
        //http://lol.inven.co.kr/dataninfo/item/list.php
        //http://www.lolking.net/items/
        //https://mirror.enha.kr/wiki/%EB%A6%AC%EA%B7%B8%20%EC%98%A4%EB%B8%8C%20%EB%A0%88%EC%A0%84%EB%93%9C/%EA%B3%B5%EA%B2%A9%20%EC%95%84%EC%9D%B4%ED%85%9C
        //http://leagueoflegends.wikia.com/wiki/Category:Items

        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Activator");}}

        internal static void Load()
        {
            AIO_Menu.addSubMenu("Activator", "AIO: Activator");

            Menu.AddSubMenu(new Menu("Auto-Potion", "AutoPotion"));
            Menu.AddSubMenu(new Menu("Auto-Spell", "AutoSpell"));
            //Menu.AddSubMenu(new Menu("Activator: ComboMode", "ComboMode"));
            Menu.AddSubMenu(new Menu("BeforeAttack", "BeforeAttack"));
            Menu.AddSubMenu(new Menu("AfterAttack", "AfterAttack"));
            Menu.AddSubMenu(new Menu("OnAttack", "OnAttack"));

            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.Use Health Potion", "Use Health Potion")).SetValue(true);
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.ifHealthPercent", "if Health Percent <")).SetValue(new Slider(55, 0, 100));
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.Use Mana Potion", "Use Mana Potion")).SetValue(true);
            Menu.SubMenu("AutoPotion").AddItem(new MenuItem("AutoPotion.ifManaPercent", "if Mana Percent <")).SetValue(new Slider(55,0,100));
            Menu.SubMenu("OnAttack").AddItem(new MenuItem("OnAttack.RS", "Use Red Smite")).SetValue(true);
            Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.SF", "Skill First")).SetValue(false);
            //Menu.SubMenu("AutoSpell").AddItem(new MenuItem("AutoSpell.Use Heal", "Use Heal")).SetValue(true);
            //Menu.SubMenu("AutoSpell").AddItem(new MenuItem("AutoSpell.Use Ignite", "Use Ignite")).SetValue(true);

            additems();
            addPotions();

            Game.OnUpdate += OnUpdate.Game_OnUpdate;
            Orbwalking.BeforeAttack += BeforeAttack.Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack.Orbwalking_AfterAttack;
        }

        internal class item
        {
            internal string Name { get; set; }
            internal int Id { get; set; }
            internal float Range { get; set; }
            internal bool isTargeted { get; set; }
        }

        static void additems()
        {
            BeforeAttack.additem("Youmuu", (int)ItemId.Youmuus_Ghostblade, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));
            AfterAttack.additem("Tiamat", (int)ItemId.Tiamat_Melee_Only, 250f);
            AfterAttack.additem("Hydra", (int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            AfterAttack.additem("Bilgewater", (int)ItemId.Bilgewater_Cutlass, 450f, true);
            AfterAttack.additem("BoTRK", (int)ItemId.Blade_of_the_Ruined_King, 450f, true);
        }

        static void addPotions()
        {
            potions = new List<Potion>
            {
                new Potion
                {
                    Name = "ItemCrystalFlask",
                    MinCharges = 1,
                    ItemId = (ItemId) 2041,
                    Priority = 1,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "RegenerationPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2003,
                    Priority = 2,
                    TypeList = new List<PotionType> {PotionType.Health}
                },
                new Potion
                {
                    Name = "ItemMiniRegenPotion",
                    MinCharges = 0,
                    ItemId = (ItemId) 2010,
                    Priority = 4,
                    TypeList = new List<PotionType> {PotionType.Health, PotionType.Mana}
                },
                new Potion
                {
                    Name = "FlaskOfCrystalWater",
                    MinCharges = 0,
                    ItemId = (ItemId) 2004,
                    Priority = 3,
                    TypeList = new List<PotionType> {PotionType.Mana}
                }
            };
        }

        //PotionManager part of Marksman
        static List<Potion> potions;
        
        enum PotionType
        {
            Health, Mana
        };

        class Potion
        {
            internal string Name { get; set; }
            internal int MinCharges { get; set; }
            internal ItemId ItemId { get; set; }
            internal int Priority { get; set; }
            internal List<PotionType> TypeList { get; set; }
        }

        static InventorySlot GetPotionSlot(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from item in ObjectManager.Player.InventoryItems
                    where item.Id == potion.ItemId && item.Charges >= potion.MinCharges
                    select item).FirstOrDefault();
        }

        static bool IsBuffActive(PotionType type)
        {
            return (from potion in potions
                    where potion.TypeList.Contains(type)
                    from buff in ObjectManager.Player.Buffs
                    where buff.Name == potion.Name && buff.IsActive
                    select potion).Any();
        }

        internal class OnUpdate
        {
            internal static List<item> itemsList = new List<item>();

            internal static void additem(string itemName, int itemid, float itemRange, bool itemisTargeted = false)
            {
                itemsList.Add(new item { Name = itemName, Id = itemid, Range = itemRange, isTargeted = itemisTargeted });

                Menu.SubMenu("OnUpdate").AddItem(new MenuItem("OnUpdate.Use " + itemid.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Game_OnUpdate(EventArgs args)
            {
                if (ObjectManager.Player.IsDead)
                    return;

                if(!ObjectManager.Player.IsRecalling() && !ObjectManager.Player.InFountain())
                {
                    if (Menu.Item("AutoPotion.Use Health Potion").GetValue<bool>())
                    {
                        if (AIO_Func.getHealthPercent(ObjectManager.Player) <= Menu.Item("AutoPotion.ifHealthPercent").GetValue<Slider>().Value)
                        {
                            var healthSlot = GetPotionSlot(PotionType.Health);

                            if (!IsBuffActive(PotionType.Health) && healthSlot != null)
                                ObjectManager.Player.Spellbook.CastSpell(healthSlot.SpellSlot);
                        }
                    }

                    if (Menu.Item("AutoPotion.Use Mana Potion").GetValue<bool>())
                    {
                        if (AIO_Func.getManaPercent(ObjectManager.Player) <= Menu.Item("AutoPotion.ifManaPercent").GetValue<Slider>().Value)
                        {
                            var manaSlot = GetPotionSlot(PotionType.Mana);

                            if (!IsBuffActive(PotionType.Mana) && manaSlot != null)
                                ObjectManager.Player.Spellbook.CastSpell(manaSlot.SpellSlot);
                        }
                    }
                }

				
            }
        }
		

		
		internal class OnAttack
		{
            internal static List<items> smiteList = new List<items>();
			internal static Spell Smite;
			internal static SpellSlot smiteSlot = SpellSlot.Unknown;
			internal static float smrange = 700f;
            internal static void Game_OnUpdate(EventArgs args)
            {
				setSmiteSlot();
			}
			internal static void Load()
			{
			InitializeItems();
			}
			internal class items
			{
				internal ItemId ItemId { get; set; }
			}
			internal static void setSmiteSlot()
			{
				foreach (var spell in ObjectManager.Player.Spellbook.Spells.Where(spell => String.Equals(spell.Name, "s5_summonersmiteduel", StringComparison.CurrentCultureIgnoreCase))) // Red Smite
				{
					smiteSlot = spell.Slot;
					Smite = new Spell(smiteSlot, smrange);
					return;
				}
			}
			internal static bool CheckInv()
			{
				bool b = false;
				foreach(var items in smiteList)
				{
					if(Player.InventoryItems.Any(f => f.Id == (ItemId)item.Id))
					{
						b = true;
					}
				}
				return b;
			}
			internal static void InitializeItems()
			{
				smiteList = new List<items>
				{
					{
					s0 = new ItemId = (ItemId) 3714
					},
					{
					s1 = new ItemId = (ItemId) 3715
					},
					{
					s2 = new ItemId = (ItemId) 3716
					},
					{
					s3 = new ItemId = (ItemId) 3717
					},
					{
					s4 = new ItemId = (ItemId) 3718
					}
				};
			}

			internal static void Orbwalking_OnAttack(AttackableUnit unit, AttackableUnit target)
			{
				var Target = (Obj_AI_Base)target;
					
				if (!unit.IsMe || Target == null)
						return;
						
				if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("OnAttack.RS").GetValue<bool>())
				{
					if (!CheckInv())
					return;
					Smite.Slot = smiteSlot;
					if(smiteSlot.IsReady())
					Player.Spellbook.CastSpell(smiteSlot, Target);
				}
			}
		}

        internal class BeforeAttack
        {
            internal static List<item> itemsList = new List<item>();

            internal static void additem(string itemName, int itemid, float itemRange, bool itemisTargeted= false)
            {
                itemsList.Add(new item { Name = itemName, Id = itemid, Range = itemRange, isTargeted = itemisTargeted });

                Menu.SubMenu("BeforeAttack").AddItem(new MenuItem("BeforeAttack.Use " + itemid.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
            {
                if (!args.Unit.IsMe || args.Target == null || args.Unit.IsDead || args.Target.IsDead || args.Target.Type != GameObjectType.obj_AI_Hero)
                    return;

                foreach (var item in BeforeAttack.itemsList.Where(x => Items.CanUseItem((int)x.Id) && args.Target.IsValidTarget(x.Range) && Menu.Item("BeforeAttack.Use " + x.Id.ToString()).GetValue<bool>()))
                {
                    if (item.isTargeted)
                        Items.UseItem(item.Id, (Obj_AI_Base)args.Target);
                    else
                        Items.UseItem(item.Id);
                }
            }
        }

        internal class AfterAttack
        {
            internal static List<item> itemsList = new List<item>();
            internal static bool ALLCancleItemsAreCasted { get { return !utility.Activator.AfterAttack.itemsList.Any(x => Items.CanUseItem((int)x.Id) && !x.isTargeted && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>()); } }

            internal static void additem(string itemName, int itemid, float itemRange, bool itemisTargeted = false)
            {
                itemsList.Add(new item { Name = itemName, Id = itemid, Range = itemRange, isTargeted = itemisTargeted });

                Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + itemid.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
            {
                if (!unit.IsMe || target == null || target.IsDead || unit.IsDead || (target.Type != GameObjectType.obj_AI_Minion && target.Type != GameObjectType.obj_AI_Hero))
                    return;

                var itemone = AfterAttack.itemsList.FirstOrDefault(x => Items.CanUseItem((int)x.Id) && target.IsValidTarget(x.Range) && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>());

                if (itemone != null)
                {
					if(Menu.Item("AfterAttack.SF").GetValue<bool>())
					{
						//WIP
					}
					else
					{
						if (itemone.isTargeted)
							Items.UseItem(itemone.Id, (Obj_AI_Base)target);
						else
							Items.UseItem(itemone.Id);
					}
				}
            }
        }
    }
}
