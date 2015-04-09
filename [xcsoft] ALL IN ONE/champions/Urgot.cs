using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Urgot
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, Q2, W, E, R;

        public static void Load()
        {
           
            Q = new Spell(SpellSlot.Q, 1000f, TargetSelector.DamageType.Physical);
            Q2 = new Spell(SpellSlot.Q, 1200f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 900f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, float.MaxValue);
            

         
            Q.SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            Q2.SetSkillshot(0.25f, 60f, 1600f, true, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 210f, 1500f, false, SkillshotType.SkillshotCircle);



            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();

            xcsoftMenu.Harass.addUseQ();
            xcsoftMenu.Harass.addUseW();
            xcsoftMenu.Harass.addUseE();

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseW();
            xcsoftMenu.Laneclear.addUseE();

            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addUseE();

            xcsoftMenu.Misc.addUseKillsteal();
            xcsoftMenu.Misc.addUseAntiGapcloser();
            xcsoftMenu.Misc.addUseInterrupter();

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addErange();
            xcsoftMenu.Drawings.addRrange();

           
            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            R.Range = 150 * R.Level + 400;

            if (Player.IsDead)
                return;

            //이 부분은 건드릴 필요가 없음. 현재 사용자가 누르고있는 오브워커 버튼에따른 함수 호출.
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

            //메인메뉴->Misc서브메뉴에서 Use Killsteal 옵션이 On인경우 킬스틸 함수 호출.
            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
           
            if (Player.IsDead)
                return;

            var drawQ = xcsoftMenu.Drawings.DrawQRange;
            var drawE = xcsoftMenu.Drawings.DrawERange;
            var drawR = xcsoftMenu.Drawings.DrawRRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);


            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
          
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender) && W.IsReady())
            {
                W.Cast();
                Q.Cast(gapcloser.Sender);
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;


            if (R.CanCast(sender) && sender.IsEnemy && R.IsInRange(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                R.Cast(sender);
        }

        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && xcsoftMenu.Combo.UseW && Q.IsReady())
            {
                var Q2target = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Physical);
                
                if(Q2target != null && Q2target.HasBuff("urgotcorrosivedebuff"))
                {
                   W.Cast();
                   Q2.Cast();
                }
                else
                {
                   var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                   if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.High)
                       Q.Cast(qTarget);
                }
            }

            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);

                if (eTarget != null && Q.GetPrediction(eTarget).Hitchance >= HitChance.VeryHigh)
                    E.Cast(eTarget);
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Physical);

                if (rTarget != null && rTarget.CountEnemiesInRange(700) < Player.CountAlliesInRange(700) && Player.HealthPercent > 80)
                {
                    R.CastOnUnit(rTarget);
                }
            }
        }

        static void Harass()
        {
            if (!(Player.ManaPercent > xcsoftMenu.Harass.ifMana))
                return;

            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.High)
                    Q.Cast(qTarget);
            }
        }

        static void Laneclear()
        {
            if (!(Player.ManaPercent > xcsoftMenu.Laneclear.ifMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (xcsoftMenu.Laneclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Minions.FirstOrDefault()))
                    Q.Cast(Minions.FirstOrDefault(), false);
            }

        }

        static void Jungleclear()
        {
            
            if (!(Player.ManaPercent > xcsoftMenu.Jungleclear.ifMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (xcsoftMenu.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs.FirstOrDefault()))
                    E.Cast(Mobs.FirstOrDefault(), false, true);
            }

        }

        static void Killsteal()
        {
         
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
             
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);

                if (E.CanCast(target) && xcsoftFunc.isKillable(target, E))
                    E.Cast(target);

            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (E.IsReady())
                damage += E.GetDamage(enemy);


            return damage;
        }
    }
}
