using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class __BASE
    {
        static Menu Menu { get { return initializer.Menu; } }
        static Orbwalking.Orbwalker Orbwalker { get { return initializer.Orbwalker; } }
        static Obj_AI_Hero Player { get { return initializer.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetSkillshot(0.25f, 50f, 2000f, true, SkillshotType.SkillshotLine);
            Q.SetCharged(" ", " ", 750, 1550, 1.5f);
            Q.SetTargetted(0.25f, 2000f);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));

            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawW", "W Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawE", "E Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Red)));

            #region DamageIndicator
            var drawDamageMenu = new MenuItem("Draw_Damage", "Draw Damage", true).SetValue(true);
            var drawFill = new MenuItem("Draw_Fill", "Draw Damage Fill", true).SetValue(new Circle(true, Color.Red));

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
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

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

        static void Combo()
        {
            if (!Orbwalking.CanMove(10))
                return;

            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
            { }

            if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady())
            { }

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
            { }

            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
            { }
        }

        static void Harass()
        {
            if (!Orbwalking.CanMove(10) || !(Player.ManaPercentage() > Menu.Item("harassMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
            { }

            if (Menu.Item("HrsUseW", true).GetValue<bool>() && W.IsReady())
            { }

            if (Menu.Item("HrsUseE", true).GetValue<bool>() && E.IsReady())
            { }

            if (Menu.Item("HrsUseR", true).GetValue<bool>() && R.IsReady())
            { }
        }

        static void Laneclear()
        {
            if (!Orbwalking.CanMove(10) || !(Player.ManaPercentage() > Menu.Item("laneclearMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
            { }

            if (Menu.Item("LcUseW", true).GetValue<bool>() && W.IsReady())
            { }

            if (Menu.Item("LcUseE", true).GetValue<bool>() && E.IsReady())
            { }

            if (Menu.Item("LcUseR", true).GetValue<bool>() && R.IsReady())
            { }
        }

        static void Jungleclear()
        {
            if (!Orbwalking.CanMove(10) || !(Player.ManaPercentage() > Menu.Item("jungleclearMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(Player) + 100, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
            { }

            if (Menu.Item("JcUseW", true).GetValue<bool>() && W.IsReady())
            { }

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady())
            { }

            if (Menu.Item("JcUseR", true).GetValue<bool>() && R.IsReady())
            { }
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
