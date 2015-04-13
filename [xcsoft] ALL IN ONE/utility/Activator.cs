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
        static Menu Menu { get { return xcsoftMenu.Menu_Manual.SubMenu("Simple Activator"); } }

        static List<Items.Item> afterAttackItems = new List<Items.Item>();

        internal static bool afterAttack_AllitemsAreCasted { get { return !utility.Activator.afterAttackItems.Any(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>()); } }

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
            additem_AfterAttack("BoTRK", (int)ItemId.Blade_of_the_Ruined_King, 400f);
        }

        static void additem_AfterAttack(string itemName, int itemId, float itemRange)
        {
            afterAttackItems.Add(new Items.Item(itemId,itemRange));

            Menu.SubMenu("AfterAttack").AddItem(new MenuItem("AfterAttack.Use " + itemId.ToString(), "Use " + itemName)).SetValue(true);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            if (!unit.IsMe || target == null || target.IsDead || unit.IsDead)
                return;

            if (target.Type != GameObjectType.obj_AI_Minion && target.Type != GameObjectType.obj_AI_Hero)
                return;

            var itemone = afterAttackItems.FirstOrDefault(x => x.IsReady() && Menu.Item("AfterAttack.Use " + x.Id.ToString()).GetValue<bool>());

            if (itemone != null)
            {
                if (!itemone.Cast())
                    itemone.Cast((Obj_AI_Base)target);
            }
        }
    }
}
