using System;
using System.Linq;
using System.Collections.Generic;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One
{
    static class AIO_Func
    {
        internal static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}}
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }

        internal static List<Obj_AI_Base> getCollisionMinions(Obj_AI_Hero source, SharpDX.Vector3 targetPos, float predDelay, float predWidth, float predSpeed)
        {
            var input = new PredictionInput
            {
                Unit = source,
                Radius = predWidth,
                Delay = predDelay,
                Speed = predSpeed,
            };

            input.CollisionObjects[0] = CollisionableObjects.Minions;

            return Collision.GetCollision(new List<SharpDX.Vector3> { targetPos }, input).OrderBy(obj => obj.Distance(source, false)).ToList();
        }

        internal static BuffInstance getBuffInstance(Obj_AI_Base target, string buffName)
        {
            return target.Buffs.Find(x => x.Name == buffName && x.IsValidBuff());
        }

        internal static BuffInstance getBuffInstance(Obj_AI_Base target, string buffName, Obj_AI_Base buffCaster)
        {
            return target.Buffs.Find(x => x.Name == buffName && x.Caster.NetworkId == buffCaster.NetworkId && x.IsValidBuff());
        }

        internal static bool isKillable(Obj_AI_Base target, float damage)
        {
            return target.Health + (target.HPRegenRate / 2) <= damage;
        }

        internal static bool isKillable(Obj_AI_Base target, Spell spell, int stage = 0)
        {
            return target.Health + (target.HPRegenRate/2) <= spell.GetDamage2(target, stage);
        }

        internal static void sendDebugMsg(string message, string tag = "[TeamProjects] ALL In One: ")
        {
            Console.WriteLine(tag + message);
            //Game.PrintChat(tag + message); //임시 롤백
        }

        internal static bool anyoneValidInRange(float range)
        {
            return HeroManager.Enemies.Any(x => x.IsValidTarget(range));
        }
        
        internal static float GetDamageCalc(Obj_AI_Base Sender,Obj_AI_Base Target,Damage.DamageType Type, double Equation = 0d) 
        {
            return (float)Damage.CalcDamage(Sender,Target, Type, Equation);
        }
        
        internal static void CCast(Spell spell, Obj_AI_Base target) //for Circular spells
        {
            if(spell.Type == SkillshotType.SkillshotCircle || spell.Type == SkillshotType.SkillshotCone) // Cone 스킬은 임시로
            {
                if(spell != null && target !=null)
                {
                    var pred = Prediction.GetPrediction(target, spell.Delay, spell.Width/2, spell.Speed);
                    SharpDX.Vector2 castVec = (pred.UnitPosition.To2D() + target.ServerPosition.To2D()) / 2 ;
                    SharpDX.Vector2 castVec2 = Player.ServerPosition.To2D() +
                                               SharpDX.Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * (spell.Range);
                    
                    if (target.IsValidTarget(spell.Range))
                    {
                        if(target.MoveSpeed*(Game.Ping/2000 + spell.Delay+Player.ServerPosition.Distance(target.ServerPosition)/spell.Speed) <= spell.Width*1/2)
                            spell.Cast(target.ServerPosition); //Game.Ping/2000  추가함.
                        else if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && pred.UnitPosition.Distance(target.ServerPosition) < Math.Max(spell.Width,300f))
                        {
                            if(target.MoveSpeed*(Game.Ping/2000 + spell.Delay+Player.ServerPosition.Distance(target.ServerPosition)/spell.Speed) <= spell.Width*2/3 && castVec.Distance(pred.UnitPosition) <= spell.Width*1/2 && castVec.Distance(Player.ServerPosition) <= spell.Range)
                            {
                                spell.Cast(castVec);
                            }
                            else if(castVec.Distance(pred.UnitPosition) > spell.Width*1/2 && Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range)
                            {
                                spell.Cast(pred.UnitPosition);
                            }
                            else
                                spell.Cast(pred.CastPosition); // <- 별로 좋은 선택은 아니지만.. 
                        }
                    }
                    else if (target.IsValidTarget(spell.Range + spell.Width/2)) //사거리 밖 대상에 대해서
                    {
                        if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance && Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range+spell.Width*1/2 && pred.UnitPosition.Distance(target.ServerPosition) < Math.Max(spell.Width,300f))
                        {
                            if(Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range)
                            {
                                if(Player.ServerPosition.Distance(pred.CastPosition) <= spell.Range)
                                spell.Cast(pred.CastPosition);
                            }
                            else if(Player.ServerPosition.Distance(pred.UnitPosition) <= spell.Range+spell.Width*1/2 && target.MoveSpeed*(Game.Ping/2000 + spell.Delay+Player.ServerPosition.Distance(target.ServerPosition)/spell.Speed) <= spell.Width/2)
                            {
                                if(Player.Distance(castVec2) <= spell.Range)
                                spell.Cast(castVec2);
                            }
                        }
                    }
                }
            }
        }
        
        internal static void LCast(Spell spell, Obj_AI_Base target, float alpha = 0f, float colmini = float.MaxValue, bool HeroOnly = false) //for Linar spells  사용예시 AIO_Func.LCast(Q,Qtarget,50,0)  
        {                            //        AIO_Func.LCast(E,Etarget,Menu.Item("Misc.Etg").GetValue<Slider>().Value,float.MaxValue); <- 이런식으로 사용.
            if(spell.Type == SkillshotType.SkillshotLine)
            {
                if(spell != null && target !=null)
                {
                    var pred = Prediction.GetPrediction(target, spell.Delay, spell.Width/2, spell.Speed); //spell.Width/2
                    var collision = spell.GetCollision(Player.ServerPosition.To2D(), new List<SharpDX.Vector2> { pred.CastPosition.To2D() });
                    //var minioncol = collision.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
                    var minioncol = collision.Count(x => (HeroOnly == false ? x.IsMinion : (x is Obj_AI_Hero)));

                    if (target.IsValidTarget(spell.Range - target.MoveSpeed * (spell.Delay + Player.Distance(target.ServerPosition) / spell.Speed) + alpha) && minioncol <= colmini && pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                    {
                        spell.Cast(pred.CastPosition);
                    }
                }
            }
        }
        
        internal static void AtoB(Spell spell, Obj_AI_Base T, float Drag = 700f) //Coded By RL244 AtoB Drag 기본값 700f는 빅토르를 위한 것임.
        {
            if(T != null)
            {
                var T2 = HeroManager.Enemies.Where(x => x != T && CanHit(spell,x,Drag)).FirstOrDefault();
                var pred = Prediction.GetPrediction(T, spell.Delay, spell.Width/2, spell.Speed);
                var T2pred = Prediction.GetPrediction(T2, spell.Delay, spell.Width/2, spell.Speed);
                SharpDX.Vector2 castVec = (pred.UnitPosition.To2D() + T.ServerPosition.To2D()) / 2 ;
                SharpDX.Vector2 castVec2 = Player.ServerPosition.To2D() +
                                           SharpDX.Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * (spell.Range);
                SharpDX.Vector2 castVec3 = T.ServerPosition.To2D() -
                                           SharpDX.Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * (40f);
                if(pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                {
                    if(T.Distance(Player.ServerPosition) >= spell.Range)
                    {
                        if(CanHit(spell,T,Drag) && T2 == null && pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        spell.Cast(castVec2,pred.UnitPosition.To2D());
                        else //if(CanHit(spell,T,Drag) && T2 != null && T2pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)//별로 좋은 생각이 더 안나고 피곤해서 걍관둠.
                        {
                        spell.Cast(castVec2,T.ServerPosition.To2D());//별로 좋은 생각이 더 안나고 피곤해서 걍관둠.
                        }
                    }
                    else
                    {
                        if(T2 == null || !CanHit(spell,T2,Drag))
                        spell.Cast(castVec3,T.ServerPosition.To2D());
                        else if(T2 != null && CanHit(spell,T2,Drag) && T2pred.Hitchance >= AIO_Menu.Champion.Misc.SelectedHitchance)
                        {
                            SharpDX.Vector2 castVec4 = T.ServerPosition.To2D() -
                                                       SharpDX.Vector2.Normalize(T2pred.UnitPosition.To2D() - T.ServerPosition.To2D()) * (40f);
                            spell.Cast(castVec4,T2pred.UnitPosition.To2D());
                        }
                    }
                }
            }
        }
        
        internal static bool CanHit(Spell spell, Obj_AI_Base T, float Drag = 0f)
        {
            return T.IsValidTarget(spell.Range + Drag - ((T.Distance(Player.ServerPosition)-spell.Range)/spell.Speed+spell.Delay)*T.MoveSpeed);
        }
        
        internal static void LH(Spell spell, float ALPHA = 0f) // For Last hit with skill for farming 사용법은 매우 간단. AIO_Func.LH(Q,0) or AIO_Func(Q,float.MaxValue) 이런식으로. 럭스나 베이가같이 타겟이 둘 가능할 경우엔 AIO_Func.LH(Q,1) 이런식.
        {
            var M = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.Health).FirstOrDefault(m => isKillable(m,spell,0) && HealthPrediction.GetHealthPrediction(m, (int)(Player.Distance(m, false) / spell.Speed), (int)(spell.Delay * 1000 + Game.Ping / 2)) > 0);
            if(spell.IsReady() && M != null)
            {
                if(spell.IsSkillshot)
                {
                    if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우 위에 MinionOrderTypes.MaxHealth 없애서 기본값으로 바꿨음 막타잘치게 NotAlly
                    LCast(spell,M,50f,ALPHA);
                    else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
                    CCast(spell,M);
                    else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
                    spell.Cast(M);
                }
                else
                spell.Cast(M);
            }
        }
        
        internal static float PredHealth(Obj_AI_Base Target, Spell spell)
        {
            return HealthPrediction.GetHealthPrediction(Target, (int)(Player.Distance(Target, false) / spell.Speed), (int)(spell.Delay * 1000 + Game.Ping / 2));
        }

        internal static void MotionCancel()
        {
            Game.Say("/d");
        }
        
        internal static void AALcJc(Spell spell, float ExtraTargetDistance = 150f,float ALPHA = float.MaxValue, float Cost = 1f) //지금으로선 새 방식으로 메뉴 만든 경우에만 사용가능. AALaneclear AAJungleclear 대체
        {// 아주 편하게 평캔 Lc, Jc를 구현할수 있습니다(그것도 분리해서!!). 그냥 AIO_Func.AALcJc(Q); 이렇게 쓰세요. 선형 스킬일 경우 세부 설정을 원할 경우 AIO_Func.AALcJc(E,ED,0f); 이런식으로 쓰세요.
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {
                var Minions = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player)/2+200f, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
                var Mobs = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player)/2+200f, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                bool HM = true;
                bool LM = true;
                bool LHM = true;
                if (Cost == 1f)
                {
                    HM = Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana;
                    LM = Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana;
                    LHM = Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana;
                }
                else
                {
                    HM = true;
                    LM = true;
                    LHM = true;
                }
                if (Mobs.Count > 0 && Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true).GetValue<bool>()// || Menu.Item("JcUse" + spell.Slot.ToString(), true).GetValue<bool>())
                    && spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted && LHM)
                    {
                        if(spell.IsSkillshot)
                        {
                        if(spell.Type == SkillshotType.SkillshotLine)
                        LCast(spell,Mobs[0],ExtraTargetDistance,ALPHA);
                        else if(spell.Type == SkillshotType.SkillshotCircle)
                        CCast(spell,Mobs[0]);
                        else if(spell.Type == SkillshotType.SkillshotCone)
                        spell.Cast(Mobs[0]);
                        }
                        else if(!spell.IsSkillshot)
                        spell.Cast(Mobs[0]);
                        else
                        {
                            spell.Cast();
                            Orbwalking.ResetAutoAttackTimer();
                        }
                    }
                }
                if (Minions.Count > 0 && Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true).GetValue<bool>() //  || Menu.Item("LcUse" + spell.Slot.ToString(), true).GetValue<bool>())
                    && spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted && LM)
                    {
                        if(spell.IsSkillshot)
                        {
                        if(spell.Type == SkillshotType.SkillshotLine)
                        LCast(spell,Minions[0],ExtraTargetDistance,ALPHA);
                        else if(spell.Type == SkillshotType.SkillshotCircle)
                        CCast(spell,Minions[0]);
                        else if(spell.Type == SkillshotType.SkillshotCone)
                        spell.Cast(Minions[0]);
                        }
                        else if(!spell.IsSkillshot)
                        spell.Cast(Minions[0]);
                        else
                        {
                            spell.Cast();
                            Orbwalking.ResetAutoAttackTimer();
                        }
                    }
                }
            }
        }
        
        internal static void AACb(Spell spell, float ExtraTargetDistance = 150f,float ALPHA = float.MaxValue, float Cost = 1f) //지금으로선 새 방식으로 메뉴 만든 경우에만 사용가능.
        { // 아주 편하게 평캔 Cb, Hrs를 구현할수 있습니다. 그냥 AIO_Func.AACb(Q); 이렇게 쓰세요. Line 스킬일 경우에만 AIO_Func.AACb(E,ED,0f) 이런식으로 쓰시면 됩니다.
            var target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 150,TargetSelector.DamageType.Physical, true); //
            bool HM = true;
            bool LM = true;
            bool LHM = true;
            if (Cost == 1f)
            {
                HM = Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana;
                LM = Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana;
                LHM = Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana;
            }
            else
            {
                HM = true;
                LM = true;
                LHM = true;
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("Combo.Use " + spell.Slot.ToString(), true) != null)
            {
                if(Menu.Item("Combo.Use " + spell.Slot.ToString(), true).GetValue<bool>() // || Menu.Item("CbUse" + spell.Slot.ToString(), true).GetValue<bool>()) 구버전 지원 중단
                && spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted)
                {
                    if(spell.IsSkillshot)
                    {
                        if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우
                        LCast(spell,target,ExtraTargetDistance,ALPHA);
                        else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
                        CCast(spell,target);
                        else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
                        spell.Cast(target);
                    }
                    else if(!spell.IsSkillshot)
                    spell.Cast(target);
                    else
                    {
                        spell.Cast();
                        Orbwalking.ResetAutoAttackTimer();
                    }
                }
            }
            else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("Harass.Use " + spell.Slot.ToString(), true) != null)
            {
                if(Menu.Item("Harass.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                && spell.IsReady() && utility.Activator.AfterAttack.ALLCancelItemsAreCasted && HM)
                {
                    if(spell.IsSkillshot)
                    {
                        if(spell.Type == SkillshotType.SkillshotLine) // 선형 스킬일경우
                        LCast(spell,target,ExtraTargetDistance,ALPHA);
                        else if(spell.Type == SkillshotType.SkillshotCircle) // 원형 스킬일경우
                        CCast(spell,target);
                        else if(spell.Type == SkillshotType.SkillshotCone) //원뿔 스킬
                        spell.Cast(target);
                    }
                    else if(!spell.IsSkillshot)
                    spell.Cast(target);
                    else
                    {
                        spell.Cast();
                        Orbwalking.ResetAutoAttackTimer();
                    }
                }
            }
        }
        
        internal static void SC(Spell spell, float ExtraTargetDistance = 150f,float ALPHA = float.MaxValue, float Cost = 1f)
        {
            var target = TargetSelector.GetTarget(spell.Range, spell.DamageType, true);
            bool HM = true;
            bool LM = true;
            bool LHM = false;
            if (Cost == 1f)
            {
                HM = Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana;
                LM = Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana;
                LHM = Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana;
            }
            else
            {
                HM = true;
                LM = true;
                LHM = true;
            }
            if(target != null)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("Combo.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Combo.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady())
                    {
                        if(spell.IsSkillshot)
                        {
                            if(spell.Type == SkillshotType.SkillshotLine)
                            LCast(spell,target,ExtraTargetDistance,ALPHA);
                            else if(spell.Type == SkillshotType.SkillshotCircle)
                            {
                            var ctarget = TargetSelector.GetTarget(spell.Range + spell.Width/2, spell.DamageType, true);
                            CCast(spell,ctarget);
                            }
                            else if(spell.Type == SkillshotType.SkillshotCone)
                            spell.Cast(target);
                        }
                        else
                        spell.Cast(target);
                    }
                }
                else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("Harass.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Harass.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady() && HM)
                    {
                        if(spell.IsSkillshot)
                        {
                            if(spell.Type == SkillshotType.SkillshotLine)
                            LCast(spell,target,ExtraTargetDistance,ALPHA);
                            else if(spell.Type == SkillshotType.SkillshotCircle)
                            {
                            var ctarget = TargetSelector.GetTarget(spell.Range + spell.Width/2, spell.DamageType, true);
                            CCast(spell,ctarget);
                            }
                            else if(spell.Type == SkillshotType.SkillshotCone)
                            spell.Cast(target);
                        }
                        else
                        spell.Cast(target);
                    }
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {            
                var Minions = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Enemy);
                var Mobs = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                
                if (Mobs.Count > 0 && Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady() && LHM)
                    {
                        if(spell.IsSkillshot)
                        {
                            if(spell.Type == SkillshotType.SkillshotLine)
                            LCast(spell,Mobs[0],ExtraTargetDistance,ALPHA);
                            else if(spell.Type == SkillshotType.SkillshotCircle)
                            CCast(spell,Mobs[0]);
                            else if(spell.Type == SkillshotType.SkillshotCone)
                            spell.Cast(Mobs[0]);
                        }
                        else
                        spell.Cast(Mobs[0]);
                    }
                }
                if (Minions.Count > 0 && Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady() && LM)
                    {
                        if(spell.IsSkillshot)
                        {
                            if(spell.Type == SkillshotType.SkillshotLine)
                            {
                                if(ALPHA > 1f)
                                LCast(spell,Minions[0],ExtraTargetDistance,ALPHA);
                                else
                                LH(spell, ALPHA);
                            }
                            else if(spell.Type == SkillshotType.SkillshotCircle)
                            CCast(spell,Minions[0]);
                            else if(spell.Type == SkillshotType.SkillshotCone)
                            spell.Cast(Minions[0]);
                        }
                        else
                        LH(spell);
                    }
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit && Menu.Item("Lasthit.Use " + spell.Slot.ToString(), true) != null)
            {
                if(Menu.Item("Lasthit.Use " + spell.Slot.ToString(), true).GetValue<bool>() && spell.IsReady() && LHM)
                {
                    var Mini = MinionManager.GetMinions(spell.Range, MinionTypes.All, MinionTeam.NotAlly);
                    if(Mini.Count() > 0)
                    LH(spell,ALPHA);
                }
            }
        }
        
        internal static void MouseSC(Spell spell, float Cost = 1f) // 베인 니달리 리븐 등등.,.,
        {
            Obj_AI_Hero target = null;
            float TRange = 500f; // spell.Range
            if(Orbwalking.GetRealAutoAttackRange(Player) > 200)
            target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 300f, TargetSelector.DamageType.True, true);
            else
            target = TargetSelector.GetTarget(Orbwalking.GetRealAutoAttackRange(Player) + 500f, TargetSelector.DamageType.True, true);
            bool HM = true;
            bool LM = true;
            bool LHM = true;
            if (Cost == 1f)
            {
                HM = Player.ManaPercent > AIO_Menu.Champion.Harass.IfMana;
                LM = Player.ManaPercent > AIO_Menu.Champion.Laneclear.IfMana;
                LHM = Player.ManaPercent > AIO_Menu.Champion.Jungleclear.IfMana;
            }
            else
            {
                HM = true;
                LM = true;
                LHM = true;
            }
            if(target != null)
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo && Menu.Item("Combo.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Combo.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady())
                    spell.Cast(Game.CursorPos);
                }
                else if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Menu.Item("Harass.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Harass.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady() && HM)
                    spell.Cast(Game.CursorPos);
                }
            }
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
            {            
                var Minions = MinionManager.GetMinions(TRange, MinionTypes.All, MinionTeam.Enemy);
                var Mobs = MinionManager.GetMinions(TRange, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                
                if (Mobs.Count > 0 && Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Jungleclear.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady() && LHM)
                    spell.Cast(Game.CursorPos);
                }
                if (Minions.Count > 0 && Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true) != null)
                {
                    if(Menu.Item("Laneclear.Use " + spell.Slot.ToString(), true).GetValue<bool>()
                    && spell.IsReady() && LM)
                    spell.Cast(Game.CursorPos);
                }
            }
        }
        
        internal static void Heal(Spell spell, float Mana = 40, float Max = 60, float Cost = 1f)
        {
            bool M = true;
            if (Cost == 1f)
            M = Player.ManaPercent > Mana;
            else
            M = true;
            foreach (var Ally in HeroManager.Allies.Where(x => x.Distance(Player.ServerPosition) <= spell.Range && x.HealthPercent < Max && (Player.ChampionName == "Soraka" ? x != Player : x != null))) //소라카는 자신을 힐 못하니까!
            {
                if (spell.IsReady() && M && Ally != null)
                    spell.Cast(Ally);
            }
        }
        
        internal static List<Obj_AI_Hero> GetEnemyList()// 어짜피 원 기능은 중복되니 추가적으로 옵션을 줌.
        {
            return ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsEnemy && x.IsValid && !x.IsDead && !x.IsInvulnerable).ToList();
        }
        
        internal static int EnemyCount(float range, float min = 0, float max = 100)// 어짜피 원 기능은 중복되니 추가적으로 옵션을 줌. 특정 체력% 초과 특정 체력% 이하의 적챔프 카운트
        {
            return GetEnemyList().Where(x => x.Distance(Player.ServerPosition) <= range && x.HealthPercent > min && x.HealthPercent <= max).Count();
        }
        
        internal static int ECTarget(Obj_AI_Hero target, float range, float min = 0, float max = 100)// 어짜피 원 기능은 중복되니 추가적으로 옵션을 줌. 특정 체력% 초과 특정 체력% 이하의 적챔프 카운트
        {
            return GetEnemyList().Where(x => x.Distance(target.ServerPosition) <= range && x.HealthPercent > min && x.HealthPercent <= max).Count();
        }

        internal static double UnitIsImmobileUntil(Obj_AI_Base unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }

        internal static bool CollisionCheck(Obj_AI_Hero source, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source,
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;
            input.CollisionObjects[1] = CollisionableObjects.YasuoWall;

            return Collision.GetCollision(new List<SharpDX.Vector3> { target.ServerPosition }, input).Where(x => x.NetworkId != source.NetworkId && x.NetworkId != target.NetworkId).Any(); // && x.NetworkId != target.NetworkId가 없을 경우 절대로 스킬을 쓰지 않기 때문에 추가.
        }

        internal static bool CollisionCheck(SharpDX.Vector3 from, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                From = from
            };

            input.CollisionObjects[0] = CollisionableObjects.Heroes;

            return Collision.GetCollision(new List<SharpDX.Vector3> { target.ServerPosition }, input).Where(x => x.NetworkId != Player.NetworkId  && x.NetworkId != target.NetworkId).Any(); // && x.NetworkId != target.NetworkId가 없을 경우 절대로 스킬을 쓰지 않기 때문에 추가.
        }

        internal static bool YasuoWallCheck(Obj_AI_Hero source, Obj_AI_Hero target, float width)
        {
            var input = new PredictionInput
            {
                Radius = width,
                Unit = source
            };

            input.CollisionObjects[0] = CollisionableObjects.YasuoWall;

            return Collision.GetCollision(new List<SharpDX.Vector3> { target.ServerPosition }, input).Any();
        }

        internal static int CountEnemyMinionsInRange(this SharpDX.Vector3 point, float range)
        {
            return ObjectManager.Get<Obj_AI_Minion>().Count(h => h.IsValidTarget(range, true, point));
        }

        internal class SelfAOE_Prediction
        {
            internal static int HitCount(float delay, float range)
            {
                byte hitcount = 0;

                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(range, false)))
                {
                    var pred = Prediction.GetPrediction(enemy, delay);

                    if (Player.ServerPosition.Distance(pred.UnitPosition) <= range)
                        hitcount++;
                }

                return hitcount;
            }

            internal static int HitCount(float delay, float range, SharpDX.Vector3 sourcePosition)
            {
                byte hitcount = 0;

                foreach (var enemy in HeroManager.Enemies.Where(x => x.IsValidTarget(range, false, sourcePosition)))
                {
                    var pred = Prediction.GetPrediction(enemy, delay);

                    if (sourcePosition.Distance(pred.UnitPosition) <= range)
                        hitcount++;
                }

                return hitcount;
            }
        }
    }
}

