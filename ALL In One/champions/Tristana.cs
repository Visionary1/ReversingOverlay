using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Tristana// By RL244 TristanaQ tristanaecharge(target) tristanawslow(target)
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static float getQBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "TristanaQ"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
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
            AIO_Menu.Champion.Combo.addUseR(false);

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana(40);

            AIO_Menu.Champion.Laneclear.addUseQ(false);
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE(false);
            AIO_Menu.Champion.Laneclear.addIfMana(40);
            
            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW(false);
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();

            AIO_Menu.Champion.Misc.addItem("KillstealW", true);
            AIO_Menu.Champion.Misc.addItem("KillstealE", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();

            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addItem("Q Timer", new Circle(true, Color.Red));
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
                AIO_Func.SC(W);
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

            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawQTimer = AIO_Menu.Champion.Drawings.getCircleValue("Q Timer");
            var pos_temp = Drawing.WorldToScreen(Player.Position);
            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);
            if (drawQTimer.Active && getQBuffDuration > 0)
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));

        }

        
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (R.IsReady()
                && Player.Distance(gapcloser.Sender.Position) <= R.Range)
                R.Cast(gapcloser.Sender);
        }

        static void Orbwalking_OnAfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || (Target == null))
                return;

            AIO_Func.AALcJc(Q);
            AIO_Func.AALcJc(E);
            
            if(!utility.Activator.AfterAttack.AIO)
            AA();
        }
        
        static void AA()
        {
            var Target = TargetSelector.GetTarget(Player.AttackRange, E.DamageType);
            AIO_Func.AACb(Q);
            AIO_Func.AACb(E);
            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && AIO_Menu.Champion.Combo.UseR ||
            Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && AIO_Menu.Champion.Harass.UseR && AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana)
            && R.IsReady() && Target.Health + Target.HPRegenRate <= R.GetDamage(Target)+ (float)Player.GetAutoAttackDamage(Target, true))
            { // 평-R-평 => Kill
                R.Cast(Target);
            }
        }
        
        static void KillstealW()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
				if(W.IsReady())
				{
					var Buff = AIO_Func.getBuffInstance(target, "tristanaecharge");
					if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && W.CanCast(target) && R.IsReady() && AIO_Menu.Champion.Misc.getBoolValue("KillstealR") && (float)Buff.Count > 0 && AIO_Func.isKillable(target, R.GetDamage(target) + W.GetDamage(target)*(((float)Buff.Count-1)*0.25f+1f) + (float)Player.GetAutoAttackDamage(target, true)))
					AIO_Func.CCast(W,target);
					if (W.CanCast(target) && (float)Buff.Count > 0 && AIO_Func.isKillable(target, W.GetDamage(target)*(((float)Buff.Count-1)*0.25f+1f) + (float)Player.GetAutoAttackDamage(target, true)))
					AIO_Func.CCast(W,target);
					else if (W.CanCast(target) && AIO_Func.isKillable(target, W.GetDamage(target) + (float)Player.GetAutoAttackDamage(target, true)))
					AIO_Func.CCast(W,target);
				}
            }
        }

        static void KillstealE()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (E.CanCast(target) && AIO_Func.isKillable(target, E) && E.IsReady())
                E.Cast(target);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
				if(R.IsReady())
				{
					var Buff = AIO_Func.getBuffInstance(target, "tristanaecharge");
					if (R.CanCast(target) && (float)Buff.Count > 0 && AIO_Func.isKillable(target, R.GetDamage(target) + E.GetDamage(target)*(((float)Buff.Count-1)*0.25f+1f)))
						R.Cast(target);
					else if (R.CanCast(target) && AIO_Func.isKillable(target, R))
						R.Cast(target);
				}
            }
        }
        
        
        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;
			var Buff = AIO_Func.getBuffInstance(enemy, "tristanaecharge");
            if (Q.IsReady())
                damage += (float)Player.GetAutoAttackDamage(enemy, true);

            if (W.IsReady())
                damage += ((float)Buff.Count > 0 ? W.GetDamage(enemy)*(((float)Buff.Count-1)*0.25f+1f) : W.GetDamage(enemy));
                
            if (E.IsReady())
                damage += ((float)Buff.Count > 0 ? E.GetDamage(enemy)*(((float)Buff.Count-1)*0.25f+1f) : E.GetDamage(enemy));

            if (R.IsReady())
                damage += R.GetDamage(enemy);
                
            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
            return damage;
        }
    }
}
