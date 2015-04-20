using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace ALL_In_One.champions
{
    class Riven
    {
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }

        static Spell Q, W, E, R;

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 260f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, 250f, TargetSelector.DamageType.Physical);
            E = new Spell(SpellSlot.E, 325f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 900f, TargetSelector.DamageType.Physical);

            R.SetSkillshot(0.25f, 100f, 2200f, false, SkillshotType.SkillshotCone);

            AIO_Menu.Champion.Combo.addUseQ();
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();
            AIO_Menu.Champion.Combo.addUseR();

            AIO_Menu.Champion.Harass.addUseQ();//Harass서브메뉴에 Use Q 옵션 추가
            AIO_Menu.Champion.Harass.addUseW();
            AIO_Menu.Champion.Harass.addUseE();

            AIO_Menu.Champion.Laneclear.addUseQ();//..위와 같음
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addUseE();

            AIO_Menu.Champion.Jungleclear.addUseQ();//..위와 같음
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addUseE();

            AIO_Menu.Champion.Misc.addUseKillsteal();
            AIO_Menu.Champion.Misc.addUseAntiGapcloser();
            AIO_Menu.Champion.Misc.addUseInterrupter();

            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addWRange();
            AIO_Menu.Champion.Drawings.addERange();
            AIO_Menu.Champion.Drawings.addRRange();

            AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            //이벤트들 추가.
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(100))
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

            if (AIO_Menu.Champion.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawQ = AIO_Menu.Champion.Drawings.QRange;
            var drawW = AIO_Menu.Champion.Drawings.WRange;
            var drawE = AIO_Menu.Champion.Drawings.ERange;
            var drawR = AIO_Menu.Champion.Drawings.RRange;

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
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            if (W.CanCast(gapcloser.Sender))
                W.Cast();
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (W.CanCast(sender))
                W.Cast();
        }

        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);

                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                    Q.Cast(qTarget);

            }

            if (AIO_Menu.Champion.Combo.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Combo.UseR && R.IsReady())
            { }
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;

            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Harass.UseR && R.IsReady())
            { }
        }

        static void Laneclear()
        {
            //래인클리어모드. 인게임에서 V키를 누르면 아래코드가 실행되는겁니다.

            //마나 체크. 사용하지않으면 지우세요.
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            //1000범위내에 있는 적군 미니언들을 리스트형식으로 구해온다.
            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (AIO_Menu.Champion.Laneclear.UseQ && Q.IsReady())
            { }

            if (AIO_Menu.Champion.Laneclear.UseW && W.IsReady())
            { }

            if (AIO_Menu.Champion.Laneclear.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Laneclear.UseR && R.IsReady())
            { }
        }

        static void Jungleclear()
        {
            //정글클리어모드. 인게임에서 V키를 누르면 아래코드가 실행되는겁니다.

            //마나 체크. 사용하지않으면 지우세요.
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            //1000범위내에 있는 중립 미니언(정글몹)들을 리스트형식으로 구해온다.
            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (AIO_Menu.Champion.Jungleclear.UseQ && Q.IsReady())
            {
                //Mobs.FirstOrDefault() Mobs 리스트의 첫번째 항목을 반환(FirstOrDefault)
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

            if (AIO_Menu.Champion.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

            if (AIO_Menu.Champion.Jungleclear.UseE && E.IsReady())
            { }

            if (AIO_Menu.Champion.Jungleclear.UseR && R.IsReady())
            { }
        }

        static void Killsteal()
        {
            //킬스틸부분 적챔프가 킬각일때 스펠을 시전합니다.
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                //데미지가 있고 적챔프에게 시전할 수 있는 스펠만 남겨두고 지우세요. 인게임에서 오류납니다.

                //Q스펠을 target한테 사용할 수 있고 target이 Q데미지를 입으면 죽는 체력일 경우 Q스펠 target에게 시전. 이하동문
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast(target);

                if (W.CanCast(target) && AIO_Func.isKillable(target, W))
                    W.Cast(target);

                if (E.CanCast(target) && AIO_Func.isKillable(target, E))
                    E.Cast(target);

                if (R.CanCast(target) && AIO_Func.isKillable(target, R))
                    R.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            //콤보데미지 계산부분입니다. 여기에서 계산한 데미지가 데미지표시기에 출력되는겁니다.
            float damage = 0;

            //Q스펠이 준비상태일때 적 챔프에게 Q스펠 시전했을경우 입혀지는 (방어력, 마저, 방관, 마관 모두 계산된)데미지 추가. 이하동문
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
