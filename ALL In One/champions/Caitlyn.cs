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
    class Caitlyn//RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1250, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 800, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 950, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 2000, TargetSelector.DamageType.Physical);
			
            Q.SetSkillshot(1.0f, 85f, 2000f, false, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.625f, 67.5f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.75f, 80f, 1600f, true, SkillshotType.SkillshotLine);
            R.SetTargetted(1.35f, 3200f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW(false);
            AIO_Menu.Champion.Combo.addUseE(false);

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana();
			
            AIO_Menu.Champion.Jungleclear.addUseQ();
			AIO_Menu.Champion.Jungleclear.addUseE(false);
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional QRange")).SetValue(new Slider(50, 0, 250));
            AIO_Menu.Champion.Misc.addItem("Made By Rl244", true);
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addItem("Q Safe Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
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
				
			R.Range = 1500f + R.Level*500f;
			
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
            var drawR = AIO_Menu.Champion.Drawings.RRange;
			var drawQr = AIO_Menu.Champion.Drawings.getCircleValue("Q Safe Range");
			var qtarget = TargetSelector.GetTarget(Q.Range + Player.MoveSpeed * Q.Delay, TargetSelector.DamageType.Physical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (Q.IsReady() && drawQr.Active && qtarget != null)
                Render.Circle.DrawCircle(Player.Position, Q.Range - qtarget.MoveSpeed*Q.Delay, drawQr.Color);
				
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

            if (E.IsReady()
				&& Player.Distance(gapcloser.Sender.Position) <= E.Range)
                E.Cast((Vector3)gapcloser.End);
        }

		
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
				var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                AIO_Func.LCast(Q,Qtarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
				var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                AIO_Func.LCast(E,Etarget,50,0);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
				var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                AIO_Func.CCast(W,Wtarget);
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
		
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
				var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
				if(AIO_Menu.Champion.Harass.UseE && !E.IsReady() || !AIO_Menu.Champion.Harass.UseE)
                AIO_Func.LCast(Q,Qtarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
            }
			
            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
				var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
                AIO_Func.CCast(W,Wtarget);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady() && Q.IsReady())
            {
				var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                AIO_Func.LCast(E,Etarget,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,0);
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
                AIO_Func.LH(E,0);
            }
			
            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
				var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.E))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);			
                if (_m != null)
                AIO_Func.LH(Q,float.MaxValue);
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
                AIO_Func.LCast(Q,Mobs.FirstOrDefault(),Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);

            }
			
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(E.Range)))
                AIO_Func.LCast(E,Mobs[0],50,0);
            }

        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                AIO_Func.LCast(Q,target,Menu.Item("Misc.Qtg").GetValue<Slider>().Value,float.MaxValue);
            }
        }
				
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                AIO_Func.LCast(E,target,50,0);
            }
        }		
		
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && Player.Distance(target.Position) > Player.AttackRange - Player.MoveSpeed + target.MoveSpeed && AIO_Func.isKillable(target, R))
				R.Cast(target);
            }
        }
		
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);
				
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
