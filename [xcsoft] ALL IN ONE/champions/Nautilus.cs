using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Nautilus
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

        public static void Load()
        {

            Q = new Spell(SpellSlot.Q, 1100f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 400f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 825f, TargetSelector.DamageType.Magical);

          
            Q.SetSkillshot(0.25f, 90f, 2000f, true, SkillshotType.SkillshotLine);
            R.SetTargetted(0.50f, 500f);
            

            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();

            xcsoftMenu.Harass.isEmpty();

            xcsoftMenu.Lasthit.isEmpty();

            xcsoftMenu.Laneclear.addUseW();
            xcsoftMenu.Laneclear.addUseE();
            xcsoftMenu.Laneclear.addifMana();

            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addUseE();
            xcsoftMenu.Jungleclear.addifMana();

            xcsoftMenu.Misc.addHitchanceSelector();

            xcsoftMenu.Misc.addUseKillsteal();
            xcsoftMenu.Misc.addUseAntiGapcloser();
            xcsoftMenu.Misc.addUseInterrupter();

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addRrange();

           
            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);


            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Orbwalking.BeforeAttack += Orbwalking_OnBeforeAttack;
        }

        static void Game_OnUpdate(EventArgs args)
        {
           
            if (Player.IsDead)
                return;

          
            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

         
            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
           
            if (Player.IsDead)
                return;

        
            var drawQ = xcsoftMenu.Drawings.DrawQRange;
            var drawR = xcsoftMenu.Drawings.DrawRRange;

           
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {

            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.CanCast(sender) || R.CanCast(sender))
            {
                Q.Cast(sender);

                if (args.DangerLevel == Interrupter2.DangerLevel.High && !Q.IsReady())
                {
                    R.Cast(sender);
                }
                
            }
            
        }

        static void Orbwalking_OnBeforeAttack(Orbwalking.BeforeAttackEventArgs unit)
        {
            if(Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (xcsoftMenu.Combo.UseW && W.IsReady())
                {
                    W.Cast();
                }
            }
        }
        static void Combo()
        {
        

            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);


                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= xcsoftMenu.Misc.SelectedHitchance && qTarget.IsValidTarget(Q.Range))
                    Q.Cast(qTarget);

            }


            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                
                if (eTarget != null && eTarget.IsValidTarget(E.Range))
                    E.Cast(eTarget, false, true);
            }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (rTarget != null && rTarget.IsValidTarget(R.Range))
                    R.CastOnBestTarget();
            }
        }



        static void Laneclear()
        {

            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Laneclear.ifMana))
                return;

           
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;



            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
            {
                if (W.CanCast(Minions.FirstOrDefault()))
                     W.Cast();
            }

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
            {
                if (E.CanCast(Minions.FirstOrDefault()))
                    E.Cast(Minions.FirstOrDefault());
            }

        }

        static void Jungleclear()
        {
            
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Jungleclear.ifMana))
                return;

         
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;


            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs.FirstOrDefault()))
                    E.Cast(Mobs.FirstOrDefault());
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

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
    }
}
