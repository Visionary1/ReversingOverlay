using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Vladimir
    {
        static Menu Menu { get { return xcsoftMenu.Menu; } }
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Magical);
            W = new Spell(SpellSlot.W, 280f, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, 600f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Magical);

            Q.SetTargetted(0.25f, float.MaxValue);
            R.SetSkillshot(0.389f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            xcsoftMenu.addComboitems(new object[][] { new object[] 
            { "Q", true }, new object[] 
            { "W", false }, new object[] 
            { "E", true }, new object[] 
            { "R", true } });

            xcsoftMenu.addHarassitems(new object[][] { new object[] 
            { "Q", true }, new object[] 
            { "E", true } }, false);

            xcsoftMenu.addLanclearitems(new object[][] { new object[] 
            { "Q", true}, new object[] 
            { "E", true}}, false);

            xcsoftMenu.addJungleclearitems(new object[][] { new object[] 
            { "Q", true }, new object[] 
            { "E", true } }, false);

            xcsoftMenu.addMiscitems(new object[][] { new object[] 
            { "Killsteal" ,true}, new object[] 
            { "Anti-Gapcloser", true }, new object[] 
            { "Interrupter", true} });

            xcsoftMenu.addDrawingsitems(new object[][] { new object[] 
            { "Q Range", new Circle(true, Color.GreenYellow) }, new object[] 
            { "W Range", new Circle(false, Color.GreenYellow) }, new object[] 
            { "E Range", new Circle(true, Color.GreenYellow) }, new object[] 
            { "R Range", new Circle(false, Color.GreenYellow) } });

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
            if (!Menu.Item("MiscKillsteal", true).GetValue<bool>())
                Killsteal();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = Menu.Item("DrawQ Range", true).GetValue<Circle>();
            var drawW = Menu.Item("DrawW Range", true).GetValue<Circle>();
            var drawE = Menu.Item("DrawE Range", true).GetValue<Circle>();
            var drawR = Menu.Item("DrawR Range", true).GetValue<Circle>();

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
            if (!Menu.Item("MiscAnti-Gapcloser", true).GetValue<bool>() || Player.IsDead)
                return;

            if (Q.CanCast(gapcloser.Sender))
                Q.Cast(gapcloser.Sender);
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.CastOnBestTarget();

            if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady())
            {
                if (xcsoftFunc.anyoneValidInRange(W.Range))
                    W.Cast();
            }

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (xcsoftFunc.anyoneValidInRange(E.Range))
                    E.Cast();
            }

            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
            {
                var rTarget = HeroManager.Enemies.FirstOrDefault(x=> R.GetPrediction(x, true).Hitchance >= HitChance.High);

                if(rTarget != null)
                    R.Cast(rTarget, false, true);
            }
        }

        static void Harass()
        {
            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
            { 
                Q.CastOnBestTarget(); 
            }

            if (Menu.Item("HrsUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (xcsoftFunc.anyoneValidInRange(E.Range))
                    E.Cast(); 
            }
        }

        static void Laneclear()
        {
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = Minions.FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (Menu.Item("LcUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (Minions.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
            }
        }

        static void Jungleclear()
        {
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
            {
                var qTarget = Mobs.FirstOrDefault(x => x.IsValidTarget(Q.Range));

                if (qTarget != null)
                    Q.Cast(qTarget);
            }

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady())
            {
                if (Mobs.Any(x => x.IsValidTarget(E.Range)))
                    E.Cast();
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
