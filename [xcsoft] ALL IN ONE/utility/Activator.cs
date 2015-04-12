using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE.utility
{
    class Activator
    {
        //아이템 목록 && 설명
        //http://lol.inven.co.kr/dataninfo/item/list.php
        //http://www.lolking.net/items/

        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Menu Menu { get { return xcsoftMenu.Menu_Manual.SubMenu("Activator"); } }

        static List<Items.Item> afterAttackItems = new List<Items.Item>();

        internal static bool afterAttack_AllitemsAreCasted { get { return !utility.Activator.afterAttackItems.Any(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.ToString()).GetValue<bool>()); } }

        internal static void Load()
        {
            xcsoftMenu.addSubMenu("Simple Activator");

            Menu.AddSubMenu(new Menu("AfterAttack", "AfterAttack"));

            items_initialize();

            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void items_initialize()
        {
            additem_AfterAttack("Hydra", (int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            additem_AfterAttack("Tiamat", (int)ItemId.Tiamat_Melee_Only, 250f);
        }

        static void additem_AfterAttack(string itemName, int itemId, float itemRange)
        {
            afterAttackItems.Add(new Items.Item(itemId,itemRange));

            Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || target == null)
                return;

            if (target.Type != GameObjectType.obj_AI_Minion && target.Type != GameObjectType.obj_AI_Hero)
                return;

            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                foreach (var item in afterAttackItems.Where(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>() && HeroManager.Enemies.Any(z => z.IsValidTarget(x.Range))))
                {
                    if (!item.Cast())
                        item.Cast((Obj_AI_Base)target);

                    break;
                }
            }
            
        }
    }
}
