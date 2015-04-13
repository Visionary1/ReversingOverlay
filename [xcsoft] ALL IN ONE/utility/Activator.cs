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
        //https://mirror.enha.kr/wiki/%EB%A6%AC%EA%B7%B8%20%EC%98%A4%EB%B8%8C%20%EB%A0%88%EC%A0%84%EB%93%9C/%EA%B3%B5%EA%B2%A9%20%EC%95%84%EC%9D%B4%ED%85%9C

        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        internal static Menu Menu;

        internal static void Load()
        {
            Menu = new Menu("[xcsoft] AIO: Activator", "xcsoft_AIOActivator", true);
            Menu.AddToMainMenu();

            Menu.AddSubMenu(new Menu("BeforeAttack", "BeforeAttack"));
            Menu.AddSubMenu(new Menu("AfterAttack", "AfterAttack"));

            additems();
            
            Orbwalking.BeforeAttack += BeforeAttack.Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += AfterAttack.Orbwalking_AfterAttack;
        }

        static void additems()
        {
            BeforeAttack.addItem("Youmuu", (int)ItemId.Youmuus_Ghostblade, float.MaxValue);

            AfterAttack.additem("Hydra", (int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
            AfterAttack.additem("Tiamat", (int)ItemId.Tiamat_Melee_Only, 250f);
            AfterAttack.additem("BoTRK", (int)ItemId.Blade_of_the_Ruined_King, 400f);
        }

        internal class BeforeAttack
        {
            internal static List<Items.Item> itemsList = new List<Items.Item>();

            internal static void addItem(string itemName, int itemId, float itemRange)
            {
                itemsList.Add(new Items.Item(itemId, itemRange));

                Menu.SubMenu("BeforeAttack").AddItem(new MenuItem("BeforeAttack.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
            {
                if (!args.Unit.IsMe || args.Target == null || args.Unit.IsDead || args.Target.IsDead || args.Target.Type != GameObjectType.obj_AI_Hero)
                    return;

                foreach (var item in BeforeAttack.itemsList.Where(x => x.IsReady() && x.IsInRange(args.Target.Position) && Menu.Item("BeforeAttack.Use " + x.Id.ToString()).GetValue<bool>()))
                    if (!item.Cast()) item.Cast((Obj_AI_Base)args.Target);
            }
        }

        internal class AfterAttack
        {
            internal static List<Items.Item> itemsList = new List<Items.Item>();
            internal static bool AllitemsAreCasted { get { return !utility.Activator.AfterAttack.itemsList.Any(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>()); } }

            internal static void additem(string itemName, int itemId, float itemRange)
            {
                itemsList.Add(new Items.Item(itemId, itemRange));

                Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
            }

            internal static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
            {
                if (!unit.IsMe || target == null || target.IsDead || unit.IsDead || (target.Type != GameObjectType.obj_AI_Minion && target.Type != GameObjectType.obj_AI_Hero))
                    return;

                var itemone = AfterAttack.itemsList.FirstOrDefault(x => x.IsReady() && x.IsInRange(target.Position) && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>());

                if (itemone != null)
                    if (!itemone.Cast()) itemone.Cast((Obj_AI_Base)target);
            }
        }
    }
}
