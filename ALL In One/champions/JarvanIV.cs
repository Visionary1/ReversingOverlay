using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class JarvanIV// By RL244
    {
        static Menu Menu { get { return AIO_Menu.MainMenu_Manual; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;
        static float QD {get{return Menu.Item("Misc.Qtg").GetValue<Slider>().Value; }}
        
        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 770f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 330f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 830f, TargetSelector.DamageType.Magical);
            R = new Spell(SpellSlot.R, 650f, TargetSelector.DamageType.Physical);
            Q.SetSkillshot(0.25f, 80f, float.MaxValue, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.5f, 75f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R.SetTargetted(0.25f, float.MaxValue);
            
            
            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();
            AIO_Menu.Champion.Harass.addUseW(false);
            AIO_Menu.Champion.Harass.addUseE(false);
            AIO_Menu.Champion.Harass.addIfMana();
            
            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW(false);
            AIO_Menu.Champion.Laneclear.addUseE();
            AIO_Menu.Champion.Laneclear.addIfMana();

            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addHitchanceSelector();
            Menu.SubMenu("Misc").AddItem(new MenuItem("Misc.Qtg", "Additional Qrange")).SetValue(new Slider(25, 0, 150));
            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Drawings.addQrange();
            AIO_Menu.Champion.Drawings.addErange();
            AIO_Menu.Champion.Drawings.addRrange();

        
            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if(Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                    AIO_Func.SC(Q,QD);
                AIO_Func.SC(W);
                AIO_Func.SC(E);
                AIO_Func.SC(R);
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            #endregion
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.Qrange;
            var drawE = AIO_Menu.Champion.Drawings.Erange;
            var drawR = AIO_Menu.Champion.Drawings.Rrange;
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
            if (E.IsReady() && drawE.Active)
                Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
            if (R.IsReady() && drawR.Active)
                Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
        }
        
        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;

            if ((Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
            {
                if (args.SData.Name == Player.Spellbook.GetSpell(SpellSlot.E).Name && HeroManager.Enemies.Any(x => x.IsValidTarget(Q.Range)))
                {
                    if (AIO_Menu.Champion.Combo.UseQ || AIO_Menu.Champion.Harass.UseQ)
                    {
                        var Qtarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);
                        AIO_Func.LCast(Q,Qtarget,QD,float.MaxValue);
                    }
                }
            }

        }
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            {
                var Rtarget = TargetSelector.GetTarget(R.Range, R.DamageType);
                if(AIO_Func.isKillable(Rtarget, Q.GetDamage(Rtarget) + R.GetDamage(Rtarget)))
                R.Cast(Rtarget);
            }
        }
        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    AIO_Func.LCast(Q,target,QD);
            }
        }
        
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
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
