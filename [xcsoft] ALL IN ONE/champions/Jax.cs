using System;
using System.Collections.Generic;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;

using Color = System.Drawing.Color;

namespace _xcsoft__ALL_IN_ONE.champions
{
    class Jax
    {
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }//메뉴에있는 오브워커(xcsoftMenu.Orbwalker)를 쓰기편하게 오브젝트명 Orbwalker로 단축한것.
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }//Player오브젝트 = 말그대로 플레이어 챔피언입니다. 이 오브젝트로 챔피언을 움직인다던지 스킬을 쓴다던지 다 됩니다.

        //**********************************************************
        //공동개발자용 주석 문제가 있으면 언제든지 Skype: LSxcsoft
        //***********************************************************

        //스펠 변수 선언.
        static Spell Q, W, E, R;

        public static void Load()//챔피언 로드부분. 게임 로딩이 끝나자마자 제일먼저 실행되는 부분입니다.
        {
            //스펠 설정

            //스펠슬롯, 스펠사거리, 데미지타입(마뎀, 물뎀, 고정뎀)
            Q = new Spell(SpellSlot.Q, 700f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W, float.MaxValue, TargetSelector.DamageType.Magical);
            E = new Spell(SpellSlot.E, float.MaxValue, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R);

            //스펠 프리딕션 설정

            //Q가 스킬샷(투사체)인경우 설정하는 예제
            //스킬시전전 딜레이, 스킬샷범위(두께), 투사체속도, 미니언에 막히는가안막히는가(막히면 true,안막히면 false)

            //메뉴에 아이템추가. xcsoftMenu 클래스로 간편하게 만들어놨음 아래처럼 필요한 옵션만 추가하면 되고, 문제있으면 저한테 물어보세요.

            //메인메뉴.서브메뉴.메소드 혹은 함수명();

            xcsoftMenu.Combo.addUseQ();//Combo서브메뉴에 Use Q 옵션 추가
            xcsoftMenu.Combo.addUseW();
            xcsoftMenu.Combo.addUseE();
            xcsoftMenu.Combo.addUseR();

            xcsoftMenu.Harass.addUseQ();//Harass서브메뉴에 Use Q 옵션 추가
            xcsoftMenu.Harass.addUseW();
            xcsoftMenu.Harass.addUseE();

            xcsoftMenu.Laneclear.addUseW();//Laneclear서브메뉴에 Use W 옵션 추가
            xcsoftMenu.Laneclear.addUseE();

            xcsoftMenu.Jungleclear.addUseQ();//Jungleclear서브메뉴에 Use Q 옵션 추가
            xcsoftMenu.Jungleclear.addUseW();
            xcsoftMenu.Jungleclear.addUseE();

            xcsoftMenu.Misc.addUseKillsteal();//Misc서브메뉴에 Use Killsteal 옵션 추가
            xcsoftMenu.Misc.addUseInterrupter();//Misc서브메뉴에 Use Interrupter 옵션 추가.

            xcsoftMenu.Drawings.addQrange();//Drawings서브메뉴에 Q Range 옵션추가.

            // Drawings 서브메뉴에 데미지표시기 추가하는 메소드.
            xcsoftMenu.Drawings.addDamageIndicator(getComboDamage);

            //이벤트들 추가.
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            //0.01초 마다 발동하는 이벤트. 여기에 코드를 쓰면 0.01초마다 실행됩니다

            //플레이어가 죽어있는상태면 리턴 (return코드 아래부분 실행안한다는 뜻.)
            if (Player.IsDead)
                return;

            //이 부분은 건드릴 필요가 없음. 현재 사용자가 누르고있는 오브워커 버튼에따른 함수 호출.
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

            //메인메뉴->Misc서브메뉴에서 Use Killsteal 옵션이 On인경우 킬스틸 함수 호출.
            if (xcsoftMenu.Misc.UseKillsteal)
                Killsteal();
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            //그리기 이벤트입니다. 1초에 프레임수만큼 실행됨

            //플레이어가 죽어있는상태면 리턴 (return코드 아래부분 실행안한다는 뜻.)
            if (Player.IsDead)
                return;

            //Drawings 설정 정보를 변수에 불러오는겁니다.
            //사용하지 않는 옵션은 지우세요 인게임에서 오류납니다.
            var drawQ = xcsoftMenu.Drawings.DrawQRange;

            //Q스펠이 준비상태(쿨타임아닌상태)이고 Q Range옵션이 On 이면 Q사거리를 플레이어 챔피언위치에다가 그리는겁니다. 이하동문
            if (Q.IsReady() && drawQ.Active)
                Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
        }
                static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //안티갭클로저 이벤트. 적챔피언이 달라붙는 스킬을 사용할때마다 발동합니다.

            //misc서브메뉴에 Use Anti-Gapcloser옵션이 On이 아니거나, 플레이어가 죽은상태면 리턴
            if (!xcsoftMenu.Misc.UseAntiGapcloser || Player.IsDead)
                return;

            //Q스펠을 gapcloser.Sender(달라붙는스킬을 시전한 챔피언)에게 사용할 수 있으면 E스펠을 gapcloser.Sender에게 시전.
            if (E.CanCast(gapcloser.Sender))
                E.Cast(gapcloser.Sender);
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            //인터럽터 이벤트. 카타리나R 피들스틱W 이런 채널링스킬들이 발동할때 이부분이 실행됩니다.

            //Misc서브메뉴에 Use Interrupter옵션이 On이 아니거나, 플레이어가 죽은상태이면 리턴
            if (!xcsoftMenu.Misc.UseInterrupter || Player.IsDead)
                return;

            //E스펠을 sender(채널링스킬을 시전한 챔피언)에게 사용할 수 있으면 E스펠을 sender에게 시전.
            if (E.CanCast(sender))
                E.Cast(sender);
        }

        static void Combo()
        {
            //콤보모드. 인게임에서 스페이스바키를 누르면 아래코드가 실행되는겁니다.
            if (xcsoftMenu.Combo.UseQ && Q.IsReady())
            {
                //타겟셀렉터를 이용해서 Q 사거리내에서 최적의 타겟을 구합니다. 
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);


                //qTarget이 null(없음)이 아니고 맞을확률이 높을경우 qTarget에게 Q시전. 스펠이 타겟팅인경우 프리딕션 사용 x 
                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.High)
                    Q.Cast(qTarget);

            }

            if (xcsoftMenu.Combo.UseW && W.IsReady())
            { }

            if (xcsoftMenu.Combo.UseE && E.IsReady())
            { }

            if (xcsoftMenu.Combo.UseR && R.IsReady())
            { }
        }

        static void Harass()
        {
            //하래스모드. 인게임에서 C키를 누르면 아래코드가 실행되는겁니다.
            if (!(Player.ManaPercent > xcsoftMenu.Harass.ifMana))
                return;

            if (xcsoftMenu.Harass.UseQ && Q.IsReady())
            {
                //타겟셀렉터를 이용해서 Q 사거리내에서 최적의 타겟을 구합니다. 
                var qTarget = TargetSelector.GetTarget(Q.Range, Q.DamageType);


                //qTarget이 null(없음)이 아니고 맞을확률이 높을경우 qTarget에게 Q시전. 스펠이 타겟팅인경우 프리딕션 사용 x 
                if (qTarget != null && Q.GetPrediction(qTarget).Hitchance >= HitChance.High)
                    Q.Cast(qTarget);

            }

            if (xcsoftMenu.Harass.UseW && W.IsReady())
            { }

            if (xcsoftMenu.Harass.UseE && E.IsReady())
            { }
        }

        static void Laneclear()
        {
            //래인클리어모드. 인게임에서 V키를 누르면 아래코드가 실행되는겁니다.
            if (!(Player.ManaPercent > xcsoftMenu.Laneclear.ifMana))
                return;

            var Minions = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

            if (xcsoftMenu.Laneclear.UseW && W.IsReady())
            { }

            if (xcsoftMenu.Laneclear.UseE && E.IsReady())
            { }
        }

        static void Jungleclear()
        {
            //정글클리어모드. 인게임에서 V키를 누르면 아래코드가 실행되는겁니다.
            if (!(Player.ManaPercent > xcsoftMenu.Jungleclear.ifMana))
                return;

            var Mobs = MinionManager.GetMinions(1000, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;

            if (xcsoftMenu.Jungleclear.UseQ && Q.IsReady())
            {
                if (Q.CanCast(Mobs.FirstOrDefault()))
                    Q.Cast(Mobs.FirstOrDefault());
            }

            if (xcsoftMenu.Jungleclear.UseW && W.IsReady())
            {
                if (W.CanCast(Mobs.FirstOrDefault()))
                    W.Cast(Mobs.FirstOrDefault());
            }

            if (xcsoftMenu.Jungleclear.UseE && E.IsReady())
            {
                if (E.CanCast(Mobs.FirstOrDefault()))
                    E.Cast(Mobs.FirstOrDefault());
            }
        }

        static void Killsteal()
        {
            //킬스틸부분 적챔프가 킬각일때 스펠을 시전합니다.
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                //데미지가 있고 적챔프에게 시전할 수 있는 스펠만 남겨두고 지우세요. 인게임에서 오류납니다.

                //Q스펠을 target한테 사용할 수 있고 target이 Q데미지를 입으면 죽는 체력일 경우 Q스펠 target에게 시전. 이하동문
                if (Q.CanCast(target) && xcsoftFunc.isKillable(target, Q))
                    Q.Cast(target);
            }
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            //콤보데미지 계산부분입니다. 여기에서 계산한 데미지가 데미지표시기에 출력되는겁니다.
            float damage = 0;

            //Q스펠이 준비상태일때 적 챔프에게 Q스펠 시전했을경우 입혀지는 데미지 추가. 이하동문
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
