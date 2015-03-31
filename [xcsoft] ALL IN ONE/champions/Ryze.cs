using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Ryze//by xcsoft
    {
        static Menu Menu { get { return initializer.Menu; } }
        static Orbwalking.Orbwalker Orbwalker { get { return initializer.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 625f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 600f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, 2000f);
            W.SetTargetted(0.25f, 2000f);
            E.SetTargetted(0.25f, 2000f);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use E", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscAntigap", "Use Anti-Gapcloser", true).SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("miscAutointer", "Use Interrupter", true).SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawW", "W Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawE", "E Range", true).SetValue(new Circle(true, Color.Red)));

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

            #region Killsteal
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

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("miscAntigap", true).GetValue<bool>() || Player.IsDead)
                return;

            if (W.CanCast(gapcloser.Sender))
                W.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Menu.Item("miscAutointer", true).GetValue<bool>() || Player.IsDead)
                return;

            if (W.CanCast(sender))
                W.Cast(sender);
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
            {
                E.CastOnBestTarget();
            }

            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
            {
                if(!Q.IsReady() && !E.IsReady() && xcsoft_lib.HealthPercentage(Player) < 90)
                    R.Cast();
            }
        }

        static void Harass()
        {
            if (!(xcsoft_lib.ManaPercentage(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                Q.CastOnBestTarget();
            }

            if (Menu.Item("HrsUseW", true).GetValue<bool>() && W.IsReady())
            {
                W.CastOnBestTarget();
            }

            if (Menu.Item("HrsUseE", true).GetValue<bool>() && E.IsReady())
            {
                E.CastOnBestTarget();
            }
        }

        static void Laneclear()
        {
            if (!(xcsoft_lib.ManaPercentage(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = Minions.Where(x => Q.CanCast(x) && Q.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (Q.CanCast(qTarget))
                    Q.Cast(qTarget);
            }

            if (Menu.Item("LcUseW", true).GetValue<bool>() && W.IsReady())
            {
                var wTarget = Minions.Where(x => W.CanCast(x) && W.IsKillable(x)).OrderByDescending(x => x.Health).FirstOrDefault();

                if (W.CanCast(wTarget))
                    W.Cast(wTarget);
            }

            if (Menu.Item("LcUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (E.CanCast(Minions[0]))
                    E.Cast(Minions[0]);
            }
        }

        static void Jungleclear()
        {
            if (!(xcsoft_lib.ManaPercentage(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                if (Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }

            if (Menu.Item("JcUseW", true).GetValue<bool>() && W.IsReady())
            {
                if (Q.CanCast(Mobs[0]))
                    Q.Cast(Mobs[0]);
            }

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (E.CanCast(Mobs[0]))
                    E.Cast(Mobs[0]);
            }
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x=>x.Health))
            {
                if (Q.CanCast(target) && Q.IsKillable(target))
                    Q.Cast(target);

                if (W.CanCast(target) && W.IsKillable(target))
                    W.Cast(target);

                if (E.CanCast(target) && E.IsKillable(target))
                    E.Cast(target);
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

            return damage;
        }
    }
}
