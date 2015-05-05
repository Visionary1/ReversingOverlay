using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Fiddlesticks// By RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
		
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 575f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 575f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 750f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 800f, TargetSelector.DamageType.Physical);
            Q.SetTargetted(0.25f, float.MaxValue);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, 1200f);
            R.SetSkillshot(1.5f, 600f, float.MaxValue, false, SkillshotType.SkillshotCircle);
			
			
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();
			
            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ(false);
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana(20);

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
            AIO_Menu.Champion.Drawings.addRRange();

		
			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(35))
            {
				AIO_Func.SC(Q);
				AIO_Func.SC(W);
				AIO_Func.SC(E);
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
            var drawR = AIO_Menu.Champion.Drawings.RRange;
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.IsReady() && Player.Distance(gapcloser.Sender.Position) <= Q.Range)
                Q.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.IsReady()	&& Player.Distance(sender.Position) <= E.Range)
                E.Cast(sender);
				
            if (Q.IsReady()	&& Player.Distance(sender.Position) <= Q.Range)
                Q.Cast(sender);
        }
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

			if (W.IsReady())
				damage += W.GetDamage(enemy)*3;
			
            if (E.IsReady())
                damage += E.GetDamage(enemy)*2;
				
            if (R.IsReady())
                damage += R.GetDamage(enemy);
				
            return damage;
        }
    }
}
