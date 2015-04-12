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

        static Items.Item Hydra, Tiamat;

        internal static void Load()
        {
            Items_initialize();

            xcsoftMenu.addSubMenu("Activator");

            Menu.AddItem(new MenuItem("Use Tiamat", "Use Tiamat").SetValue(true));
            Menu.AddItem(new MenuItem("Use Hydra", "Use Hydra").SetValue(true));

            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        static void Items_initialize()
        {
            Tiamat = new Items.Item((int)ItemId.Tiamat_Melee_Only, 250f);
            Hydra = new Items.Item((int)ItemId.Ravenous_Hydra_Melee_Only, 250f);
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || Target == null)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                if (Hydra.IsReady())
                    Hydra.Cast();

                if (Tiamat.IsReady())
                    Tiamat.Cast();
            }
        }
    }
}
