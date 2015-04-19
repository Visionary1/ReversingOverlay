using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class MasterYi
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
		
        static Spell Q, W, E, R;

        static void Wcancel() { Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); }

        static float getPBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "doublestrike"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getRBuffDuration { get { var buff = AIO_Func.getBuffInstance(Player, "Highlander"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);

            Q.SetTargetted(0.25f, float.MaxValue);

            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W (Auto-Attack Reset)", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Champion").SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Champion").SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));

            AIO_Menu.Champion.Drawings.addQRange();
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawRTimer", "R Timer", true).SetValue(new Circle(true, Color.LightGreen)));
            Menu.SubMenu("Champion").SubMenu("Drawings").AddItem(new MenuItem("drawPTimer", "P Timer", true).SetValue(new Circle(true, Color.LightGreen)));

			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

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

			Orbwalker.SetAttack(Player.IsTargetable);

            #region Killsteal
            if (Menu.Item("miscKs", true).GetValue<bool>())
                Killsteal();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawRTimer = Menu.Item("drawRTimer", true).GetValue<Circle>();
            var drawPTimer = Menu.Item("drawPTimer", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);

            if (drawRTimer.Active && getRBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawRTimer.Color, "R: " + getRBuffDuration.ToString("0.00"));
            }
            if (drawPTimer.Active && getPBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawPTimer.Color, "P: " + getPBuffDuration.ToString("0.00"));
            }
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && args.Target.Type != GameObjectType.obj_AI_Minion)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.W).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                {
                    if (Menu.Item("CbUseW", true).GetValue<bool>())
                    {
                        Utility.DelayAction.Add(50, Orbwalking.ResetAutoAttackTimer);
                        Utility.DelayAction.Add(50, Wcancel);
                    }
                }

                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name)
                {
                    Orbwalking.ResetAutoAttackTimer();

                    if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
                        E.Cast();
                }
            }

        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;

            if (!unit.IsMe || Target == null)
                return;
                
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady() && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)) && utility.Activator.AfterAttack.ALLCancleItemsAreCasted)
	                W.Cast();
	
	            if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady())
				    R.Cast();
		    }
        }

        static void Combo()
        {
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                Q.CastOnBestTarget();

            if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady() && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;

            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                Q.CastOnBestTarget();
        }

        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                Q.Cast(Minions.FirstOrDefault());
        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady() && !Player.IsWindingUp)
                Q.Cast(Mobs.FirstOrDefault());

            if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady() && Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

		    if (Q.IsReady())
                damage += Q.GetDamage(enemy);
				
		    if (E.IsReady())
                damage += E.GetDamage(enemy);
				
		    if (getPBuffDuration > 0)
                damage += (float)Player.GetAutoAttackDamage(enemy, false) / 2;
				
		    if (W.IsReady())
                damage += (float)Player.GetAutoAttackDamage(enemy, false);
				
		    if (Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
		    {
			    damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
			    damage += (float)Player.GetAutoAttackDamage(enemy, false);
		    }
		
		    if (Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
		    {
			    damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
			    damage += (float)Player.GetAutoAttackDamage(enemy, false);
		    }

		    if(!Player.IsWindingUp)
			    damage += (float)Player.GetAutoAttackDamage(enemy, true);
				
                return damage;
            }
    }
}

