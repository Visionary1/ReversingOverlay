using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class _VelKoz
    {
        static Orbwalking.Orbwalker Orbwalker { get { return ALL_IN_ONE_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 1200f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 1200f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 800f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 1550f, TargetSelector.DamageType.Magical);

            Q.SetSkillshot(0.25f, 50f, 1300f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.25f, 50f, 1700f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 80f, 1500f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(0.3f, 1f, float.MaxValue, false, SkillshotType.SkillshotLine);

            ALL_IN_ONE_Menu.Champion.Combo.addUseQ();
            ALL_IN_ONE_Menu.Champion.Combo.addUseW();
            ALL_IN_ONE_Menu.Champion.Combo.addUseE();
            ALL_IN_ONE_Menu.Champion.Combo.addUseR();

            ALL_IN_ONE_Menu.Champion.Harass.addUseQ();
            ALL_IN_ONE_Menu.Champion.Harass.addUseW();
            ALL_IN_ONE_Menu.Champion.Harass.addUseE();
            ALL_IN_ONE_Menu.Champion.Harass.addIfMana();

            ALL_IN_ONE_Menu.Champion.Laneclear.addUseQ(false);
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Laneclear.addUseE(false);
            ALL_IN_ONE_Menu.Champion.Laneclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseQ();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseW();
            ALL_IN_ONE_Menu.Champion.Jungleclear.addUseE(false);
            ALL_IN_ONE_Menu.Champion.Jungleclear.addIfMana();

            ALL_IN_ONE_Menu.Champion.Misc.addHitchanceSelector();
            ALL_IN_ONE_Menu.Champion.Misc.addUseKillsteal();
            ALL_IN_ONE_Menu.Champion.Misc.addUseAntiGapcloser();
            ALL_IN_ONE_Menu.Champion.Misc.addUseInterrupter();

            ALL_IN_ONE_Menu.Champion.Drawings.addQRange();
            ALL_IN_ONE_Menu.Champion.Drawings.addWRange();
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

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
                    Lasthit();

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

            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!ALL_IN_ONE_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.CanCast(sender))
                E.Cast(sender);
        }

        static void Combo()
        {
            if (ALL_IN_ONE_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= ALL_IN_ONE_Menu.Champion.Misc.SelectedHitchance)
                    Q.Cast(qTarget);

            }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseW && W.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseE && E.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Combo.UseR && R.IsReady())
            { }
        }

        static void Harass()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Harass.IfMana))
                return;

            if (ALL_IN_ONE_Menu.Champion.Harass.UseQ && Q.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseW && W.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseE && E.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Harass.UseR && R.IsReady())
            { }
        }

        static void Lasthit()
        {
            if (!(ALL_IN_ONE_Func.getManaPercent(Player) > ALL_IN_ONE_Menu.Champion.Lasthit.IfMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (ALL_IN_ONE_Menu.Champion.Lasthit.UseQ && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range) && ALL_IN_ONE_Func.isKillable(x, Q));

                if (qTarget != null)
                    Q.Cast(qTarget);
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
            { }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseW && W.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseE && E.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Laneclear.UseR && R.IsReady())
            { }
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

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseE && E.IsReady())
            { }

            if (ALL_IN_ONE_Menu.Champion.Jungleclear.UseR && R.IsReady())
            { }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, W))
                    W.Cast(target);

                if (E.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && ALL_IN_ONE_Func.isKillable(target, R))
                    R.Cast(target);
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

            return damage;
        }
    }
}
