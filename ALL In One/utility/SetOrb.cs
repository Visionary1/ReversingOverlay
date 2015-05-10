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
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        internal static void Load()
        {
		Orbwalker.AddSubMenu(new Menu("SetCbMove", "SetCbMove"));
		Orbwalker.AddSubMenu(new Menu("SetHrMove", "SetHrMove"));
		Orbwalker.AddSubMenu(new Menu("SetCbAttack", "SetCbAttack"));
		
		Game.OnUpdate += Game_OnUpdate;
		}
		
		internal static void Game_OnUpdate(EventArgs args)
		{
			if (Player.IsDead)
				return;
			SetMove(!(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && SetCbMove) || !(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && SetHrMove));
			SetAttack(!(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && SetCbAttack))
		}
	}
}
	
