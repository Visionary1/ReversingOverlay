using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.utility
{
	class SetOrb
	{
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion").SubMenu("Orbwalker");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        internal static void Load()
        {
		Menu.AddSubMenu(new Menu("SetCbMove", "SetCbMove"));
		Menu.AddSubMenu(new Menu("SetHrMove", "SetHrMove"));
		Menu.AddSubMenu(new Menu("SetCbAttack", "SetCbAttack"));
		
		Game.OnUpdate += Game_OnUpdate;
		}
		
		internal static void Game_OnUpdate(EventArgs args)
		{
			if (Player.IsDead)
				return;
			Orbwalker.SetMove(!(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && SetCbMove) || !(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && SetHrMove));
			Orbwalker.SetAttack(!(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && SetCbAttack))
		}
	}
}
	
