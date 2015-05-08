using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Diana// By RL244 dianamoonlight DianaPassiveMarker DianaOrbs dianashield
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 830f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 200f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 350f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 825f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0.25f, 150f, 1600f, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, 2200f);
                 
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseR(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseR();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
		}

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                AIO_Func.SC(Q);
                AIO_Func.SC(W);
                AIO_Func.SC(E);
                //AIO_Func.SC(R);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
				if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear) 
				{
					Laneclear();
					Jungleclear();
				}
            }

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        
        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (sender.Distance(Player.ServerPosition) <= E.Range && E.IsReady())
                E.Cast();
        }
        static void Combo()
        {
			if(AIO_Menu.Champion.Combo.UseR)
			{
				foreach (var Enemy in HeroManager.Enemies.Where(x => x.Distance(Player.ServerPosition) <= R.Range && x.HasBuff("dianamoonlight")).OrderByDescending(x => x.Health))
				{
					if (R.IsReady() && Enemy != null)
						R.Cast(Enemy);
				}
			}

        }
        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseR && R.IsReady())
				{
					foreach (var Enemy in HeroManager.Enemies.Where(x => x.Distance(Player.ServerPosition) <= R.Range && x.HasBuff("dianamoonlight")).OrderByDescending(x => x.Health))
					{
						if (R.IsReady() && Enemy != null)
							R.Cast(Enemy);
					}
				}
        }
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(R.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
			if (AIO_Menu.Champion.Laneclear.UseR)
			{
				foreach (var Enemy in Mobs.Where(x => x.Distance(Player.ServerPosition) <= R.Range && x.HasBuff("dianamoonlight")).OrderByDescending(x => x.MaxHealth))
				{
					if (R.IsReady() && Enemy != null)
						R.Cast(Enemy);
				}
			}
        }
		
        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(R.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

			if (AIO_Menu.Champion.Jungleclear.UseR)
			{
				foreach (var Enemy in Mobs.Where(x => x.Distance(Player.ServerPosition) <= R.Range && x.HasBuff("dianamoonlight")).OrderByDescending(x => x.MaxHealth))
				{
					if (R.IsReady() && Enemy != null)
						R.Cast(Enemy);
				}
			}
        }
		
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                AIO_Func.CCast(Q,target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                R.Cast(target);
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);
            if (W.IsReady())
                damage += W.GetDamage(enemy);
            if (R.IsReady())
                damage += R.GetDamage(enemy);
            return damage;
        }
    }
}
