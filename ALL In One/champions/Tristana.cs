using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Tristana// By RL244
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;
		

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W, 900f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 700f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 700f, TargetSelector.DamageType.Magical);

            W.SetSkillshot(1.0f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetTargetted(0.25f, 1400f);
            R.SetTargetted(0.25f, 1400f);
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW(false);
            AIO_Menu.Champion.Combo.addUseE();
			AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();
			
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addItem("Made By Rl244", true);
            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

			AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
			Orbwalking.AfterAttack += Orbwalking_OnAfterAttack;
			//Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;
				
			E.Range = Player.AttackRange;
			R.Range = Player.AttackRange;

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

            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealW"))
                KillstealW();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealE"))
                KillstealE();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
				
			#region AfterAttack
			AIO_Func.AASkill(E);
			if(AIO_Func.AfterAttack())
			AA();
			#endregion
		}

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawW = AIO_Menu.Champion.Drawings.WRange;

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

        }

		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (R.IsReady()
				&& Player.Distance(gapcloser.Sender.Position) <= R.Range)
                R.Cast((SharpDX.Vector3)gapcloser.End);
        }

        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			{
			AIO_Func.AALcJc(Q);
			AIO_Func.AALcJc(E);
			}
			
			if(!utility.Activator.AfterAttack.AIO)
			AA();
        }
		
		static void AA()
		{
			var Target = TargetSelector.GetTarget(Player.AttackRange, E.DamageType);
			
			if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseE ||
			Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseE && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana)
			&& E.IsReady())
			{
				E.Cast(Target);
			}
			
			if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseR ||
			Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseR && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana)
			&& R.IsReady() && Target.Health + Target.HPRegenRate <= R.GetDamage(Target)+ (float)Player.GetAutoAttackDamage(Target, true))
			{
				R.Cast(Target);
			}
			
			if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseQ ||
			Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseQ && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana)
			&& Q.IsReady())
			{
				Q.Cast();
			}		
		}
	
        static void Combo()
        {

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

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(W.Range)))
                AIO_Func.CCast(W,Minions[0]);
            }
		}

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;
		
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;
			
            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Any(x=>x.IsValidTarget(W.Range)))
                AIO_Func.CCast(W,Mobs[0]);
            }

        }
		
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                AIO_Func.CCast(W,target);
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
                damage += (float)Player.GetAutoAttackDamage(enemy, true);

            if (W.IsReady())
                damage += W.GetDamage(enemy);
				
            if (E.IsReady())
                damage += E.GetDamage(enemy)*1.25f;

            if (R.IsReady())
                damage += R.GetDamage(enemy);
				
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
            return damage;
        }
    }
}
