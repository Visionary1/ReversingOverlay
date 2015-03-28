using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Annie//by xcsoft
    {
        static Menu Menu { get { return initializer.Menu; } }
        static Orbwalking.Orbwalker Orbwalker { get { return initializer.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static bool stunIsReady { get { return Player.HasBuff("pyromania_particle", true); } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 625f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 595f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 500f);
            R = new Spell(SpellSlot.R, 600f, TargetSelector.DamageType.Magical);

            Q.SetTargetted(0.25f, 1400f);
            W.SetSkillshot(0.25f, 60f * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.25f, 250f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscAutoE", "Use Auto-E", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscAutoCharge", "Use Auto-StunCharge", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use Auto-KillSteal", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscAntigap", "Use Anti-Gapcloser", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscinter", "Use Interrupter", true).SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawW", "W Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawE", "E Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Red)));

            #region DamageIndicator
            var drawDamageMenu = new MenuItem("Draw_Damage", "Draw Combo Damage", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "Draw Combo Damage Fill", true).SetValue(new Circle(true, Color.FromArgb(100, 255, 228, 0)));

            Menu.SubMenu("Drawings").AddItem(drawDamageMenu);
            Menu.SubMenu("Drawings").AddItem(drawFill);

            DamageIndicator.DamageToUnit = getComboDamage;
            DamageIndicator.Enabled = drawDamageMenu.GetValue<bool>();
            DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
            DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;

            drawDamageMenu.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            drawFill.ValueChanged +=
            delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };
            #endregion

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
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

            #region AutoStunCharge
            if (Menu.Item("miscAutoCharge", true).GetValue<bool>() && !stunIsReady && !Player.IsRecalling())
            {
                if (Player.InFountain())
                {
                    if (W.IsReady())
                        W.Cast(Game.CursorPos);

                    if (E.IsReady())
                        E.Cast();
                }
                else
                    E.Cast();
            } 
            #endregion

            #region Call Killsteal
            if (!Menu.Item("miscKs", true).GetValue<bool>())
                Killsteal(); 
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = Menu.Item("drawQ", true).GetValue<Circle>();
            var drawW = Menu.Item("drawW", true).GetValue<Circle>();
            var drawE = Menu.Item("drawE", true).GetValue<Circle>();
            var drawR = Menu.Item("drawR", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (W.IsReady() && drawW.Active)
                Render.Circle.DrawCircle(Player.Position, W.Range, drawW.Color);

            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);

            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Menu.Item("miscAutoE", true).GetValue<bool>() && sender.IsEnemy && sender.Type == GameObjectType.obj_AI_Hero && args.Target.IsMe && E.IsReady())
                E.Cast();
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("miscAntigap", true).GetValue<bool>() || Player.IsDead)
                return;

            if (!stunIsReady)
                return;

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);

            if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("miscinter", true).GetValue<bool>() || Player.IsDead)
                return;

            if (!stunIsReady)
                return;

            if (Q.CanCast(sender))
                Q.Cast(sender);

            if (W.CanCast(sender))
                W.Cast(sender, false, true);
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady())
            {
                W.CastOnBestTarget(0f, false, true);
            }

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
            {
                if(Player.CountEnemiesInRange(E.Range) >= 1)
                    E.Cast();
            }

            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
            {
                var rTarget = TargetSelector.GetTarget(R.Range+(R.Width/2), R.DamageType);

                if (R.CanCast(rTarget) && rTarget.Health <= getComboDamage(rTarget) + (Q.GetDamage(rTarget) * 2) && stunIsReady)
                    R.Cast(rTarget, false, true);
            }
        }

        static void Harass()
        {
            if (!(Player.ManaPercentage() > Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (Menu.Item("HrsUseW", true).GetValue<bool>() && W.IsReady())
            {
                W.CastOnBestTarget(0f, false, true);
            }
        }

        static void Laneclear()
        {
            if (!(Player.ManaPercentage() > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(625f, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = Minions.Where(x => Q.CanCast(x) && Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (Q.CanCast(qTarget))
                    Q.Cast(qTarget);
            }

        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercentage() > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = Mobs.Where(x => Q.CanCast(x)).OrderBy(x => x.Health).FirstOrDefault();

                if(Q.CanCast(qTarget))
                    Q.Cast(qTarget);
            }

            if (Menu.Item("JcUseW", true).GetValue<bool>() && Q.IsReady())
            {
                var wTarget = Mobs.Where(x => W.CanCast(x)).OrderBy(x => x.Health).FirstOrDefault();

                if (W.CanCast(wTarget))
                    W.Cast(wTarget, false, true);
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies)
            {
                if (Q.CanCast(target) && Q.IsKillable(target))
                    Q.Cast(target);

                if (W.CanCast(target) && W.IsKillable(target))
                    W.Cast(target, false, true);

                if (R.CanCast(target) && R.IsKillable(target))
                    R.Cast(target, false, true);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if (W.IsReady())
                damage += W.GetDamage(enemy);

            if (R.IsReady())
                damage += R.GetDamage(enemy);

            return damage;
        }
    }
}
