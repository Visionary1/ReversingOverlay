using System;
using System.Drawing;
using System.Reflection;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    class initializer
    {
        internal static void initialize()
        {
            xcsoftMenu.initialize("[xcsoft] ALL IN ONE");

            if (!champLoader.champSupportedCheck("[xcsoft] ALL IN ONE: ", "_xcsoft__ALL_IN_ONE.champions."))
                return;

            xcsoftMenu.addOrbwalker(ObjectManager.Player.ChampionName);
            xcsoftMenu.addTargetSelector(ObjectManager.Player.ChampionName);
            xcsoftMenu.addSubMenu_ChampTemplate(ObjectManager.Player.ChampionName);

            champLoader.Load(ObjectManager.Player.ChampionName);

            xcsoftMenu.Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("Blank", string.Empty));
            xcsoftMenu.Menu_Manual.SubMenu("Drawings").AddItem(new MenuItem("txt", "--PUBLIC OPTIONS--"));

            xcsoftMenu.Drawings.addItems(new object[][] { new object[] 
            { "Auto-Attack Real Range", new Circle(true, Color.Silver)},        new object[] 
            { "Auto-Attack Target",     new Circle(true, Color.Red)},           new object[] 
            { "Minion Last Hit",        new Circle(true, Color.GreenYellow)},   new object[] 
            { "Minion Near Kill",       new Circle(true, Color.Gray)},          new object[] 
            { "Jungle Position",        true                                    }}, false);

            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat(xcsoftFunc.colorChat(Color.LightSkyBlue, "[xcsoft] ALL IN ONE: ") + xcsoftFunc.colorChat(Color.Red, ObjectManager.Player.ChampionName) + " Loaded");
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
                return;

            var drawMinionLastHit = xcsoftMenu.Drawings.getCircleValue("Minion Last Hit", false);
            var drawMinionNearKill = xcsoftMenu.Drawings.getCircleValue("Minion Near Kill", false);

            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                foreach (var minion in MinionManager.GetMinions(ObjectManager.Player.Position, ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + 300))
                {
                    if (drawMinionLastHit.Active && ObjectManager.Player.GetAutoAttackDamage(minion, true) >= minion.Health)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, drawMinionLastHit.Color, 5);
                    else
                        if (drawMinionNearKill.Active && ObjectManager.Player.GetAutoAttackDamage(minion, true) * 2 >= minion.Health)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, drawMinionNearKill.Color, 5);
                }
            }

            if (Game.MapId == (GameMapId)11 && xcsoftMenu.Drawings.getBoolValue("Jungle Position", false))
            {
                const byte circleRadius = 100;

                Render.Circle.DrawCircle(new SharpDX.Vector3(7461.018f, 3253.575f, 52.57141f), circleRadius, Color.Blue, 5); // blue team: red
                Render.Circle.DrawCircle(new SharpDX.Vector3(3511.601f, 8745.617f, 52.57141f), circleRadius, Color.Blue, 5); // blue team: blue
                Render.Circle.DrawCircle(new SharpDX.Vector3(7462.053f, 2489.813f, 52.57141f), circleRadius, Color.Blue, 5); // blue team: golems
                Render.Circle.DrawCircle(new SharpDX.Vector3(3144.897f, 7106.449f, 51.89026f), circleRadius, Color.Blue, 5); // blue team: wolfs
                Render.Circle.DrawCircle(new SharpDX.Vector3(7770.341f, 5061.238f, 49.26587f), circleRadius, Color.Blue, 5); // blue team: wariaths

                Render.Circle.DrawCircle(new SharpDX.Vector3(10930.93f, 5405.83f, -68.72192f), circleRadius, Color.Yellow, 5); // Dragon

                Render.Circle.DrawCircle(new SharpDX.Vector3(7326.056f, 11643.01f, 50.21985f), circleRadius, Color.Red, 5); // red team: red
                Render.Circle.DrawCircle(new SharpDX.Vector3(11417.6f, 6216.028f, 51.00244f), circleRadius, Color.Red, 5); // red team: blue
                Render.Circle.DrawCircle(new SharpDX.Vector3(7368.408f, 12488.37f, 56.47668f), circleRadius, Color.Red, 5); // red team: golems
                Render.Circle.DrawCircle(new SharpDX.Vector3(10342.77f, 8896.083f, 51.72742f), circleRadius, Color.Red, 5); // red team: wolfs
                Render.Circle.DrawCircle(new SharpDX.Vector3(7001.741f, 9915.717f, 54.02466f), circleRadius, Color.Red, 5); // red team: wariaths                    
            }

            var drawAA = xcsoftMenu.Drawings.getCircleValue("Auto-Attack Real Range", false);
            var drawTarget = xcsoftMenu.Drawings.getCircleValue("Auto-Attack Target", false);

            if (drawAA.Active)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), drawAA.Color);

            if (drawTarget.Active)
            {
                var aaTarget = xcsoftMenu.Orbwalker.GetTarget();

                if (aaTarget != null)
                    Render.Circle.DrawCircle(aaTarget.Position, aaTarget.BoundingRadius + 15, drawTarget.Color, 6);
            }
        }
    }
}
