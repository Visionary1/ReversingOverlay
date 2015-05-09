using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Orianna
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static SharpDX.Vector3 BallPosition
        {
            get
            {
                var ball = ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(x => x.IsAlly && !x.IsMe && x.HasBuff("OrianaGhost"));

                return ball != null ? ball.ServerPosition : Player.ServerPosition;
            }
        }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 825f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 245f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 1095f);
            R = new Spell(SpellSlot.R, 380f);

            Q.SetSkillshot(0f, 130f, 1400f, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.25f, 80f, 1700f, true, SkillshotType.SkillshotLine);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();
            AIO_Menu.Champion.Combo.addItem("R Min Targets", new Slider(1, 2, 5));

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addUseInterrupter();
            AIO_Menu.Champion.Misc.addItem("Auto-E", true);

            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addWrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
            {
                switch (Orbwalker.ActiveMode)
                {
                    case Orbwalking.OrbwalkingMode.Combo:
                        Combo();
                        break;
                    case Orbwalking.OrbwalkingMode.Mixed:
                        Harass();
                        break;
                    case Orbwalking.OrbwalkingMode.LaneClear:
                        Laneclear();
                        Jungleclear();
                        break;
                    case Orbwalking.OrbwalkingMode.None:
                        break;
                }
            }

            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();

            Q.MinHitChance = AIO_Menu.Champion.Misc.SelectedHitchance;

            Q.UpdateSourcePosition(BallPosition);
            W.UpdateSourcePosition(BallPosition, BallPosition);
            R.UpdateSourcePosition(BallPosition, BallPosition);
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawW = AIO_Menu.Champion.Drawings.Wrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color, 3);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(BallPosition, W.Range, drawW.Color, 3);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color, 3);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(BallPosition, R.Range, drawR.Color, 3);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (Q.IsReady())
                Q.Cast(sender);

            if (R.CanCast(sender) && args.DangerLevel == Interrupter2.DangerLevel.High)
                R.Cast();
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (AIO_Menu.Champion.Misc.getBoolValue("Auto-E") && sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Hero && args.Target.IsMe && E.IsReady())
                E.CastOnUnit(Player);
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget(0f, false, true);
            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(W.Delay, W.Range, BallPosition) >= 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {

            }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(R.Delay, R.Range, BallPosition) >= AIO_Menu.Champion.Combo.getSliderValue("R Min Targets").Value)
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
                Q.CastOnBestTarget(0f, false, true);
            }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            {
                if (AIO_Func.SelfAOE_Prediction.HitCount(W.Delay, W.Range, BallPosition) >= 1)
                    W.Cast();
            }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
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
                var Qloc = Q.GetCircularFarmLocation(Minions.Where(x=>x.IsValidTarget(Q.Range)).ToList());

                if(Qloc.MinionsHit >= 2)
                    Q.Cast(Qloc.Position);
            }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            {
                if (Minions.Count(x => x.IsValidTarget(W.Range, true, BallPosition)) >= 3)
                    W.Cast();
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
                var Qloc = Q.GetCircularFarmLocation(Mobs.Where(x => x.IsValidTarget(Q.Range)).ToList());

                if (Qloc.MinionsHit >= 1)
                    Q.Cast(Qloc.Position);
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (Mobs.Count(x => x.IsValidTarget(W.Range, true, BallPosition)) >= 1)
                    W.Cast();
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy) * 2;

            if (W.IsReady())
                damage += W.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            if(!Player.IsWindingUp)
            damage += (float)Player.GetAutoAttackDamage(Player) * 2;

            return damage;
        }
    }
}
