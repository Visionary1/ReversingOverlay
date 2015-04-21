using System;
using System.Drawing;
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
    class Brand //RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1050f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 625f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 750f, TargetSelector.DamageType.Magical);

			
            Q.SetSkillshot(0.25f, 70f, 1400f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.625f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, 340f);
            R.SetTargetted(0.25f, 1000f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();
			
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
			AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Made By Rl244", true);
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Range")).SetValue(new Slider(50, 0, 250));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("W on stuned target", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addItem("W Real Range", new Circle(true, Color.Red));
            AIO_Menu.Champion.Drawings.addERange(false);
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
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
			if (AIO_Menu.Champion.Misc.getBoolValue("W on stuned target"))
				stw();
				
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
			var drawWr = AIO_Menu.Champion.Drawings.getCircleValue("W Real Range");
            var drawR = AIO_Menu.Champion.Drawings.RRange;
			var wtarget = TargetSelector.GetTarget(W.Range + Player.MoveSpeed * W.Delay, TargetSelector.DamageType.Magical);

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (W.IsReady() && drawWr.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range - wtarget.MoveSpeed*W.Delay, drawWr.Color);
		
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
		
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.IsReady()
				&& Player.Distance(gapcloser.Sender.Position) <= W.Range)
                W.Cast((Vector3)gapcloser.End);
            if (!W.IsReady() && Q.IsReady()
				&& Player.Distance(gapcloser.Sender.Position) <= Q.Range)
                Q.Cast((Vector3)gapcloser.End);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.IsReady()
			&& Player.Distance(sender.Position) <= W.Range)
                CastW(sender);
            if (!W.IsReady() && Q.IsReady()
			&& Player.Distance(sender.Position) <= Q.Range)
                CastQ(sender);
        }

		
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
		var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                CastQ(Qtarget);
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
		var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                E.Cast(Etarget);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
		var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
		AIO_Func.CCast(W,Wtarget);

            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
		var Rtarget = TargetSelector.GetTarget(R.Range, R.DamageType);
			
			if(Rtarget.Health + Rtarget.HPRegenRate <= (Q.GetDamage(Rtarget)+W.GetDamage(Rtarget)+E.GetDamage(Rtarget)+R.GetDamage(Rtarget))*2/3 || Rtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= R.Delay)
               { 
				if (HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
            	R.Cast(Rtarget);
				}
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
		
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
		var Qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                CastQ(Qtarget);
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
		var Etarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed*E.Delay, TargetSelector.DamageType.Magical);
                E.Cast(Etarget);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
			var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
			AIO_Func.CCast(W,Wtarget);
            }
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;
		
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
		var _m = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.Q))) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / Q.Speed), (int)(Q.Delay * 1000 + Game.Ping / 2)) > 0);			
                if (_m != null)
                    CastQ(_m);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)))
                    CastW(Minions[0]);
            }
			
            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
		var _m = MinionManager.GetMinions(E.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).FirstOrDefault(m => m.Health < ((Player.GetSpellDamage(m, SpellSlot.E))));			
                if (_m != null)
                    E.Cast(_m);
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
                    CastQ(Mobs.FirstOrDefault());
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(W.Range)))
                    CastW(Mobs[0]);
            }
			
            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(E.Range)))
                    E.Cast(Mobs[0]);
            }

        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    CastQ(target);
            }
        }
		
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    CastW(target);
            }
        }
		
        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);
            }
        }
		
		static void stw()
		{
			if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
			var Wtarget = TargetSelector.GetTarget(W.Range, W.DamageType);
			var pred = W.GetPrediction(Wtarget);
			if (pred.Hitchance == HitChance.Immobile || Wtarget.Buffs.Where(b => b.IsActive && Game.Time < b.EndTime && (b.Type == BuffType.Charm || b.Type == BuffType.Knockback || b.Type == BuffType.Stun || b.Type == BuffType.Suppression || b.Type == BuffType.Snare)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.Time >= W.Delay - 0.4f && W.IsReady())
			W.Cast(Wtarget, false, true);
            }
		}
		
	static void CastQ(Obj_AI_Hero target)
	{
            var qpred = Q.GetPrediction(target, true);
            var qcollision = Q.GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { qpred.CastPosition.To2D() });
            var minioncol = qcollision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
			if (target.IsValidTarget(Q.Range - target.MoveSpeed*(Q.Delay +Player.Distance(target.Position)/Q.Speed) + Menu.Item("Misc.Qtg").GetValue<Slider>().Value) && minioncol <= 0 && qpred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
            {
        	 Q.Cast(qpred.CastPosition);
            }
	}

	static void CastQ(Obj_AI_Base target)
	{
            var prediction = Q.GetPrediction(target, true);
            var minions = prediction.CollisionObjects.Count(thing => thing.IsMinion);

            if (minions <= 0 && prediction.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
			{
            	Q.Cast(prediction.CastPosition);
			}
	}
	
	static void CastW(Obj_AI_Base target)
	{
            var wpred = W.GetPrediction(target, true);
			if (target.IsValidTarget(W.Range) && wpred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
            {
        	 W.Cast(wpred.CastPosition);
            }
	}
		
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

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
