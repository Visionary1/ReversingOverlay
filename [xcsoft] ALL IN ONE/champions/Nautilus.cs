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
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
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
            

            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseE();
            ALL_IN_ONE_Menu.Champion.Laneclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseE();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Misc.addHitchanceSelector();

            ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();
            ALL_IN_ONE_Menu.Champion.Misc.addUseAntiGapcloser();
            ALL_IN_ONE_Menu.Champion.Misc.addUseInterrupter();

            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addRRange();

           
            ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);


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

         
            if (ALL_IN_ONE_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
           
            if (Player.IsDead)
                return;

        
            var drawQ = ALL_IN_ONE_Menu.Champion.Drawings.QRange;
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.RRange;

           
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {

            if (!ALL_IN_ONE_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
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
                if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
                {
                    W.Cast();
                }
            }
        }
        static void Combo()
        {
        

            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
               
                var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);


                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance && qTarget.IsValidTarget(Q.Range))
                    Q.Cast(qTarget);

            }


            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            {
                var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);

                
                if (eTarget != null && eTarget.IsValidTarget(E.Range))
                    E.Cast(eTarget, false, true);
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (rTarget != null && rTarget.IsValidTarget(R.Range))
                    R.CastOnBestTarget();
            }
        }



        static void Laneclear()
        {

            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Laneclear.IfMana))
                return;

           
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;



            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (W.CanCast(Minions.FirstOrDefault()))
                     W.Cast();
            }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                if (E.CanCast(Minions.FirstOrDefault()))
                    E.Cast(Minions.FirstOrDefault());
            }

        }

        static void Jungleclear()
        {
            
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Jungleclear.IfMana))
                return;

         
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;


            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs.FirstOrDefault()))
                    E.Cast(Mobs.FirstOrDefault());
            }

        }

        static void Killsteal()
        {
            
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (E.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, E))
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
