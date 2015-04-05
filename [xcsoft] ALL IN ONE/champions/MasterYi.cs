using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class MasterYi//by xcsoft
    {
        static Menu Menu { get { return initializer.Menu; } }
        static Orbwalking.Orbwalker Orbwalker { get { return initializer.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        static void Wcancel() { Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, float.MaxValue);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W (Auto-Attack Reset)", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));

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
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
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

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.W).Name
                    && HeroManager.Enemies.Any(x => x.IsValidTarget(Orbwalking.GetRealAutoAttackRange(Player) + 450)))
                {
                    if (Menu.Item("CbUseW", true).GetValue<bool>())
                    {
                        Utility.DelayAction.Add(50, Orbwalking.ResetAutoAttackTimer);
                        Utility.DelayAction.Add(50, Wcancel);
                    }
                }

                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name)
                {
                    Utility.DelayAction.Add(250, Orbwalking.ResetAutoAttackTimer);

                    if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
                        E.Cast();

                    if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
                        R.Cast();
                }
            }
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || Target == null)
                return;

            if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady()
                && HeroManager.Enemies.Any(x=> Orbwalking.InAutoAttackRange(x)) 
                && Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                W.Cast();
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.CastOnBestTarget();

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady() 
                && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Harass()
        {
            if (!(xcsoft_lib.ManaPercentage(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.CastOnBestTarget();
        }

        static void Laneclear()
        {
            if (!(xcsoft_lib.ManaPercentage(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Minions[0]);
        }

        static void Jungleclear()
        {
            if (!(xcsoft_lib.ManaPercentage(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Mobs[0]);

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady() 
                && Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && Q.IsKillable(target))
                    Q.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);

            return damage;
        }
    }
}
