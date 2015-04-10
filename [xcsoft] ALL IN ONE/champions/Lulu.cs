using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Lulu
    {
        
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 925f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 650f);//lulufaeriburn
            E = new Spell(SpellSlot.E, 650f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 900f);

            Q.SetSkillshot(0.25f, 60f, 1450f, false, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetTargetted(0.25f, float.MaxValue);

            xcsoftMenu.Combo.addUseQ();
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();

            xcsoftMenu.Harass.addUseQ();
            xcsoftMenu.Harass.addUseE();
            xcsoftMenu.Harass.addifMana();

            xcsoftMenu.Laneclear.addUseQ();
            xcsoftMenu.Laneclear.addUseE();
            xcsoftMenu.Laneclear.addifMana();

            xcsoftMenu.Jungleclear.addUseQ();
            xcsoftMenu.Jungleclear.addUseE();
            xcsoftMenu.Jungleclear.addifMana();

            xcsoftMenu.Misc.addHitchanceSelector();
            xcsoftMenu.Misc.addUseKillsteal();
            xcsoftMenu.Misc.addUseAntiGapcloser();
            xcsoftMenu.Misc.addUseInterrupter();

            xcsoftMenu.Drawings.addQrange();
            xcsoftMenu.Drawings.addWrange();
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

            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();

            Q.MinHitChance = xcsoftMenu.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = xcsoftMenu.Drawings.DrawQRange;
            var drawW = xcsoftMenu.Drawings.DrawWRange;
            var drawE = xcsoftMenu.Drawings.DrawERange;
            var drawR = xcsoftMenu.Drawings.DrawRRange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.CanCast(sender))
                W.Cast(sender);
        }

        static void Combo()
        {
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
                var faeritarget = HeroManager.Enemies.FirstOrDefault(x => xcsoftFunc.getBuffInstance(Player, "lulufaeriburn", Player) != null);

                if (faeritarget != null)
                {
                    Q.UpdateSourcePosition(faeritarget.ServerPosition, faeritarget.ServerPosition);

                    Q.Cast(faeritarget);
                }
                else
                {
                    Q.UpdateSourcePosition();

                    var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                    Q.Cast(qTarget);
                }
            }

            if (xcsoftMenu.Combo.UseW && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (xcsoftMenu.Combo.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Harass()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Harass.ifMana))
                return;

            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                var faeritarget = HeroManager.Enemies.FirstOrDefault(x => xcsoftFunc.getBuffInstance(Player, "lulufaeriburn", Player) != null);

                if (faeritarget != null)
                {
                    Q.UpdateSourcePosition(faeritarget.ServerPosition, faeritarget.ServerPosition);

                    Q.Cast(faeritarget);
                }
                else
                {
                    Q.UpdateSourcePosition();

                    var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                    Q.Cast(qTarget);
                }
            }

            if (xcsoftMenu.Harass.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Laneclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Laneclear.ifMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (xcsoftMenu.Laneclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetLineFarmLocation(Minions);

                if (qloc.MinionsHit >= 3)
                    Q.Cast(qloc.Position);
            }

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
            {
                var eTarget = Minions.Where(x => x.IsValidTarget(E.Range) && xcsoftFunc.isKillable(x, E)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (eTarget != null)
                    E.Cast(eTarget);
            }
        }

        static void Jungleclear()
        {
            if (!(xcsoftFunc.getManaPercent(Player) > xcsoftMenu.Jungleclear.ifMana))
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

            if (!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true) + (float)Player.GetAutoAttackDamage(enemy, false);

            return damage;
        }
    }
}
