using System;

using LeagueSharp;

namespace ALL_In_One
{
    class ChampLoader
    {
        internal static void Load(string champName)
        {
            switch (champName)
            {
                case "MasterYi"://Added by xcsoft
                    champions.MasterYi.Load();
                    break;
                case "Annie"://Added by xcsoft
                    champions.Annie.Load();
                    break;
                case "Garen"://Added by xcsoft
                    champions.Garen.Load();
                    break;
                case "Kalista"://Added by xcsoft
                    champions.Kalista.Load();
                    break;
                case "Ryze"://Added by xcsoft
                    champions.Ryze.Load();
                    break;
                case "Vi"://Added by xcsoft
                    champions.Vi.Load();
                    break;
                case "Vladimir"://Added by xcsoft
                    champions.Vladimir.Load();
                    break;
                case "Chogath"://Added by xcsoft
                    champions.Chogath.Load();
                    break;
                case "Urgot"://Added by fakker
                    champions.Urgot.Load();
                    break;
                //case "Jax":
                //    champions.Jax.Load();
                //    break;
                case "Fiora"://Added by RL144
                    champions.Fiora.Load();
                    break;
                case "Lulu"://xcsoft가 추가함. 기록해두면 좋음.
                    champions.Lulu.Load();
                    break;
                case "Nautilus": //Added by fakker
                    champions.Nautilus.Load();
                    break;
                case "Graves"://Added by fakker , 95% from sharpshooter
                    champions.Graves.Load();
                    break;
                case "Sivir"://Added by fakker , 95% from sharpshooter
                    champions.Sivir.Load();
                    break;
                case "XinZhao"://Added by RL144
                    champions.XinZhao.Load();
                    break;
                case "Katarina"://Added by xcsoft
                    champions.Katarina.Load();
                    break;
                case "Veigar"://Added by RL144
                    champions.Veigar.Load();
                    break;
                case "Talon"://Added by RL144
                    champions.Talon.Load();
                    break;
                default:
                    AIO_Func.sendDebugMsg("(champLoader)Champ Not Supported.", true);
                    break;
            } 
        }

        internal static bool champSupportedCheck(string tag, string checkNamespace)
        {
            try
            {
               AIO_Func.sendDebugMsg(tag + Type.GetType(checkNamespace + ObjectManager.Player.ChampionName).Name + " Supported.", false);
            }
            catch
            {
                AIO_Func.sendDebugMsg(tag + ObjectManager.Player.ChampionName + " Not supported.", false);
                Game.PrintChat(AIO_Func.colorChat(System.Drawing.Color.DeepSkyBlue, tag) + AIO_Func.colorChat(System.Drawing.Color.DeepPink, ObjectManager.Player.ChampionName) + " Not supported.");

                AIO_Menu.addItem("Sorry, " + ObjectManager.Player.ChampionName + " Not supported", null);
                return false;
            }

            return true;
        }
    }
}
