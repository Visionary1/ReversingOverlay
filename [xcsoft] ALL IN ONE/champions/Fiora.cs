using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Fiora
    {
        static Menu Menu { get { return xcsoftMenu.Menu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Items.Item tiamatItem, hydraItem;
		
        static Spell Q, W, E, R;

        static void Wcancel() { Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos); }

        static float getQBuffDuration { get { var buff = xcsoftFunc.getBuffInstance("fioraqcd"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }
        static float getEBuffDuration { get { var buff = xcsoftFunc.getBuffInstance("burstofspeed"); return buff != null ? buff.EndTime - Game.ClockTime : 0; } }

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 600f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 400f, TargetSelector.DamageType.Physical);

            Q.SetTargetted(0.25f, float.MaxValue);
            hydraItem = new Items.Item(3074, 250f);
            tiamatItem = new Items.Item(3077, 250f);

            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseQD", "Distance", true).SetValue(new Slider(150, 50, 600)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseW", "Use W (AABlock)", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseH", "Hidra", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("CbUseR", "Use R", true).SetValue(true));

//            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsUseQ", "Use Q", true).SetValue(true));
//            Menu.SubMenu("Harass").AddItem(new MenuItem("HrsMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcUseH", "Hydra", true).SetValue(true));
            Menu.SubMenu("Laneclear").AddItem(new MenuItem("LcMana", "if Mana % >", true).SetValue(new Slider(60, 0, 100)));

            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseQ", "Use Q", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseW", "Use W", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcUseE", "Use E", true).SetValue(true));
            Menu.SubMenu("Jungleclear").AddItem(new MenuItem("JcMana", "if Mana % >", true).SetValue(new Slider(20, 0, 100)));

            Menu.SubMenu("Misc").AddItem(new MenuItem("miscKs", "Use KillSteal", true).SetValue(true));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQ", "Q Range", true).SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawR", "R Range", true).SetValue(new Circle(true, Color.Blue)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawQTimer", "Q Timer", true).SetValue(new Circle(true, Color.LightGreen)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawETimer", "E Timer", true).SetValue(new Circle(true, Color.LightGreen)));
			xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

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

            var drawQ = Menu.Item("drawQ", true).GetValue<Circle>();
            var drawR = Menu.Item("drawR", true).GetValue<Circle>();
            var drawQTimer = Menu.Item("drawQTimer", true).GetValue<Circle>();
            var drawETimer = Menu.Item("drawETimer", true).GetValue<Circle>();

            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (R.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
            if (drawQTimer.Active && getQBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawQTimer.Color, "Q: " + getQBuffDuration.ToString("0.00"));
            }
            if (drawETimer.Active && getEBuffDuration > 0)
            {
                var pos_temp = Drawing.WorldToScreen(Player.Position);
                Drawing.DrawText(pos_temp[0], pos_temp[1], drawETimer.Color, "E: " + getEBuffDuration.ToString("0.00"));
            }
        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

/*            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && args.Target.Type != GameObjectType.obj_AI_Minion)
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.W).Name
                    && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                {
                    if (Menu.Item("CbUseW", true).GetValue<bool>())
                    {
                        Utility.DelayAction.Add(30, Orbwalking.ResetAutoAttackTimer);
                        Utility.DelayAction.Add(30, Wcancel);
                    }
                }

                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.Q).Name)
                {
                    if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady())
                        E.Cast();
                }
            }
*/
        }

        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
			//var TargetHP = Menu.Item("CbUseWHP").GetValue<Slider>().Value;
            if (!unit.IsMe || Target == null)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
				
                if (Menu.Item("LcUseE", true).GetValue<bool>() && E.IsReady()
					&& !tiamatItem.IsReady() && !hydraItem.IsReady())
                {    
				E.Cast();
				}
				if (Menu.Item("LcUseH", true).GetValue<bool>())
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
			}
				
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (Menu.Item("CbUseE", true).GetValue<bool>() && E.IsReady()
                    && HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x))
					//&& Target.HealthPercentage() <= TargetHP
					&& !tiamatItem.IsReady() && !hydraItem.IsReady())
                    E.Cast();

                /*if (Menu.Item("CbUseR", true).GetValue<bool>() && R.IsReady()
					&& !tiamatItem.IsReady() && !hydraItem.IsReady() && !E.IsReady()
					&& HeroManager.Enemies.Any(x => x.IsValidTarget(R.Range)))
					R.Cast(Target);*/
				
				if (Menu.Item("CbUseH", true).GetValue<bool>()// && !W.IsReady()
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
				{
					if(tiamatItem.IsReady())
						tiamatItem.Cast();
					else if(hydraItem.IsReady())
						hydraItem.Cast();
				}
			}
        }

        static void Combo()
        {
            var qd = Menu.Item("CbUseQD").GetValue<Slider>().Value;
            if (Menu.Item("CbUseQ", true).GetValue<bool>() && Q.IsReady())
                {
				var qTarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
				if(qTarget.Distance(Player.Position) >= qd || getQBuffDuration < 1)
					Q.Cast(qTarget);
				}
				
            if (Menu.Item("CbUseW", true).GetValue<bool>() && W.IsReady() 
                && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                W.Cast();
        }

        static void Harass()
        {
            if (!(Player.ManaPercent > Menu.Item("HrsMana", true).GetValue<Slider>().Value))
                return;
/*
            if (Menu.Item("HrsUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.CastOnBestTarget();
*/
        }

        static void Laneclear()
        {
            if (!(Player.ManaPercent > Menu.Item("LcMana", true).GetValue<Slider>().Value))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);
/*
            if (Minions.Count <= 0)
                return;

            if (Menu.Item("LcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Minions[0]);*/
        }

        static void Jungleclear()
        {
            if (!(Player.ManaPercent > Menu.Item("JcMana", true).GetValue<Slider>().Value))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (Menu.Item("JcUseQ", true).GetValue<bool>() && Q.IsReady())
                Q.Cast(Mobs[0]);
				
            if (Menu.Item("JcUseW", true).GetValue<bool>() && W.IsReady())
                W.Cast();

           /* if (Menu.Item("JcUseE", true).GetValue<bool>() && E.IsReady() 
                && Mobs.Any(x => Orbwalking.InAutoAttackRange(x)))
                E.Cast();*/
        }

        static void Killsteal()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);
                if (R.CanCast(target) && xcsoftFunc.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
                damage += Q.GetDamage(enemy);
				
            if (R.IsReady())
                damage += R.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
				/*
			if(tiamatItem.IsReady())
				damage += tiamatItem.GetDamage(enemy);
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
				
			if(hydraItem.IsReady())
				damage += hydraItem.GetDamage(enemy);
                damage += (float)Player.GetAutoAttackDamage(enemy, true);*/
			if(E.IsReady())
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
				
            return damage;
        }
    }
}
