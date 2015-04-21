using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class KogMaw //RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1280f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, (900f+R.Level*300f), TargetSelector.DamageType.Magical);

			
            Q.SetSkillshot(0.25f, 70f, 1650f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(1.2f, 100f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addUseR();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addUseR();
            AIO_Menu.Champion.Laneclear.addIfMana();
			
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
			AIO_Menu.Champion.Jungleclear.addUseE();
			AIO_Menu.Champion.Jungleclear.addUseR();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Made By Rl244", true);
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addItem("E Real Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addERange(false);
            AIO_Menu.Champion.Drawings.addRRange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
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
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
}

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
			var drawEr = AIO_Menu.Champion.Drawings.getCircleValue("E Real Range");
            var drawR = AIO_Menu.Champion.Drawings.RRange;
			var etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawEr.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range - etarget.MoveSpeed*E.Delay, drawEr.Color);
		
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
		
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.IsReady()
				&& Player.Distance(gapcloser.Sender.Position) <= E.Range)
                E.Cast((Vector3)gapcloser.End);
        }

		
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
				var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                AIO_Func.CCast(Q,Qtarget);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
				var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(E,Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
				if (HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
				W.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
				var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(R,Rtarget);
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
		
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
				var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(Q,Qtarget);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
				var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(E,Etarget);
            }
			
            if (AIO_Menu.Champion.Harass.UseR && R.IsReady())
            {
				var Rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(R,Rtarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
				if (HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
				W.Cast();
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;
		
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(E.Range)))
                    AIO_Func.CCast(E,Minions[0]);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => Orbwalking.InAutoAttackRange(x)))
                    W.Cast();
            }
			
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
				var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.E))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);			
                if (_m != null)
                    AIO_Func.CCast(Q,_m);
            }
		}

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
		
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    AIO_Func.CCast(Q,Mobs.FirstOrDefault());
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                    W.Cast();
            }
			
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(E.Range)))
                    AIO_Func.CCast(E,Mobs[0]);
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
                    AIO_Func.CCast(R,target);
            }
        }
		
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    AIO_Func.CCast(E,target);
            }
        }
		
		
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy, true);

            if (W.IsReady())
                damage += W.GetDamage(enemy);
				
            if (E.IsReady())
                damage += E.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);
				
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
            return damage;
        }
    }
}
