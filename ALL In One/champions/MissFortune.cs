using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

/*
MasteryBladeWeaving 무기연성
MasterySpellWeaving 주문연성 (평타시)
ElixirOfWrath 분노의영약
grievouswound 고통스런 상처
missfortunepassivestack 미포 w패시브
*/


namespace ALL_In_One.champions
{
    class MissFortune//RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;
		

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 650f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 800f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1400f, TargetSelector.DamageType.Physical);

			
            Q.SetTargetted(0.25f, 1400f);
            E.SetSkillshot(0.25f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, 2000f, false, SkillshotType.SkillshotCone);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

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
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addERange(false);
            AIO_Menu.Champion.Drawings.addRRange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
			//Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
			//Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
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
				
			if (AIO_Func.AfterAttack() && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
			{
				var Target = TargetSelector.GetTarget(Q.Range, Q.DamageType);
				var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);
				var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

				if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseW ||
				Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseW && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana ||
				Minions.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Laneclear.UseW && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana ||
				Mobs.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Jungleclear.UseW && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana)
				&& W.IsReady())
				{
					W.Cast();
				}
				
				if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseQ ||
				Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana ||
				Minions.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Laneclear.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana ||
				Mobs.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Jungleclear.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana)
				&& Q.IsReady())
				{
					Q.Cast(Target);
				}
			}
		}

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
            var drawR = AIO_Menu.Champion.Drawings.RRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

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
                E.Cast((SharpDX.Vector3)gapcloser.End);
        }
/*
        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseW ||
			Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseW && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana ||
			Minions.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Laneclear.UseW && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana ||
			Mobs.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Jungleclear.UseW && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana)
			&& W.IsReady())
            {
                W.Cast();
            }
			
            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseQ ||
			Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana ||
			Minions.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Laneclear.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana ||
			Mobs.Count >= 1 && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && AIO_Menu.Champion.Jungleclear.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana)
			&& Q.IsReady())
            {
                Q.Cast(Target);
            }
        }
*/
		
        static void Combo()
        {

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
				var Etarget = TargetSelector.GetTarget(E.Range, E.DamageType);
                AIO_Func.CCast(E,Etarget);
            }

        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
				var Etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
                AIO_Func.CCast(E,Etarget);
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
		}

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
		
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
			
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
                Q.Cast(target);
            }
        }
		
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (target.IsValidTarget(R.Range) && AIO_Func.isKillable(target, R))
                    AIO_Func.CCast(R,target);
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
