using System;

using LeagueSharp;

namespace _xcsoft__ALL_IN_ONE
{
    class champLoader
    {
        //For SandBox
        internal static void Load(string champName)
        {
            switch (champName)
            {
                case "MasterYi":
                    champions.MasterYi.Load();
                    break;
                case "Annie":
                    champions.Annie.Load();
                    break;
                case "Garen":
                    champions.Garen.Load();
                    break;
                case "Kalista":
                    champions.Kalista.Load();
                    break;
                case "Ryze":
                    champions.Ryze.Load();
                    break;
                case "Vi":
                    champions.Vi.Load();
                    break;
                case "Vladimir":
                    champions.Vladimir.Load();
                    break;
                case "Chogath":
                    champions.Chogath.Load();
                    break;
                case "Urgot":
                    champions.Urgot.Load();
                    break;
                default:
                    xcsoftFunc.sendDebugMsg("챔프 지원안함.");
                    break;
            } 
        }

        internal static bool champSupportedCheck(string tag, string checkNamespace)
        {
            try
            {
                xcsoftFunc.sendDebugMsg(tag + Type.GetType(checkNamespace + ObjectManager.Player.ChampionName).Name + " Supported.", false);
                return true;
            }
            catch
            {
                xcsoftFunc.sendDebugMsg(tag + ObjectManager.Player.ChampionName + " Not supported.", false);
                Game.PrintChat(xcsoftFunc.colorChat(System.Drawing.Color.MediumBlue, tag) + xcsoftFunc.colorChat(System.Drawing.Color.DarkGray, ObjectManager.Player.ChampionName) + " Not supported.");

                xcsoftMenu.addItem("Sorry, " + ObjectManager.Player.ChampionName + " Not supported");
                return false;
            }
        }
    }
}
