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
        
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 925f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 650f);
            E = new Spell(SpellSlot.E, 650f, TargetSelector.DamageType.Magical);//lulufaeriburn
            R = new Spell(SpellSlot.R, 900f);

            Q.SetSkillshot(0.25f, 60f, 1450f, false, SkillshotType.SkillshotLine);
            W.SetTargetted(0.25f, float.MaxValue);
            E.SetTargetted(0.25f, float.MaxValue);
            R.SetTargetted(0.25f, float.MaxValue);

            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();

            ALL_IN_ONE_Menu.Champion.Harass.addUseQ();
            ALL_IN_ONE_Menu.Champion.Harass.addUseE();
            ALL_IN_ONE_Menu.Champion.Harass.addIfMana();

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseE();
            ALL_IN_ONE_Menu.Champion.Laneclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseE();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Misc.addHitchanceSelector();
            ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();
            ALL_IN_ONE_Menu.Champion.Misc.addUseAntiGapcloser();
            ALL_IN_ONE_Menu.Champion.Misc.addUseInterrupter();

            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addERange();
            ALL_IN_ONE_Menu.Champion.Drawings.addRRange();

            ALL_IN_ONE_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

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

            if (ALL_IN_ONE_Menu.Champion.Misc.UseKillsteal)
                Killsteal();

            Q.MinHitChance = ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance;
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = ALL_IN_ONE_Menu.Champion.Drawings.QRange;
            var drawW = ALL_IN_ONE_Menu.Champion.Drawings.WRange;
            var drawE = ALL_IN_ONE_Menu.Champion.Drawings.ERange;
            var drawR = ALL_IN_ONE_Menu.Champion.Drawings.RRange;

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
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.CanCast(sender))
                W.Cast(sender);
        }

        static void Combo()
        {
            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var faeritarget = HeroManager.Enemies.FirstOrDefault(x => ALL_IN_ONE_Func.getBuffInstance(Player, "lulufaeriburn", Player) != null);

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

            if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Harass()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Harass.IfMana))
                return;

            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                var faeritarget = HeroManager.Enemies.FirstOrDefault(x => ALL_IN_ONE_Func.getBuffInstance(Player, "lulufaeriburn", Player) != null);

                if (faeritarget != null)
                {
                    Q.UpdateSourcePosition(faeritarget.ServerPosition, faeritarget.ServerPosition);

                    Q.Cast(faeritarget);
                }
                else
                {
                    Q.UpdateSourcePosition(Player.ServerPosition, Player.ServerPosition);

                    var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                    Q.Cast(qTarget);
                }
            }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseE && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Laneclear()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            {
                var qloc = Q.GetLineFarmLocation(Minions);

                if (qloc.MinionsHit >= 3)
                    Q.Cast(qloc.Position);
            }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseE && E.IsReady())
            {
                var eTarget = Minions.Where(x => x.IsValidTarget(E.Range) && ALL_IN_ONE_Func.isKillable(x, E)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (eTarget != null)
                    E.Cast(eTarget);
            }
        }

        static void Jungleclear()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
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

            if (!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true) + (float)Player.GetAutoAttackDamage(enemy, false);

            return damage;
        }
    }
}
