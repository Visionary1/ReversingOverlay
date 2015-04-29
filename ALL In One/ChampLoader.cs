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
                case "Fiora"://Added by RL244
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
                case "XinZhao"://Added by RL244
                    champions.XinZhao.Load();
                    break;
                case "Katarina"://Added by xcsoft
                    champions.Katarina.Load();
                    break;
                case "Veigar"://Added by RL244
                    champions.Veigar.Load();
                    break;
                case "Talon"://Added by RL244
                    champions.Talon.Load();
                    break;
                //case "Azir"://Added by RL244 WIP
                //    champions.Azir.Load();
                //    break;
                case "Gangplank"://Added by RL244
                    champions.Gangplank.Load();
                    break;
                case "Blitzcrank"://Added by RL244
                    champions.Blitzcrank.Load();
                    break;
                case "Brand"://Added by RL244
                    champions.Brand.Load();
                    break;
                case "Cassiopeia"://Added by RL244
                    champions.Cassiopeia.Load();
                    break;
                case "KogMaw"://Added by RL244
                    champions.KogMaw.Load();
                    break;
                case "Zyra"://Added by RL244
                    champions.Zyra.Load();
                    break;
                case "Caitlyn"://Added by RL244
                    champions.Caitlyn.Load();
                    break;
                case "MissFortune"://Added by RL244
                    champions.MissFortune.Load();
                    break;
                case "Tristana"://Added by RL244
                    champions.Tristana.Load();
                    break;
                case "Karthus"://Added by xcsoft
                    champions.Karthus.Load();
                    break;
                case "Karma"://Added by RL244 WIP
                    champions.Karma.Load();
                    break;
                case "Jax"://Added by RL244
                    champions.Jax.Load();
                    break;
                case "Darius"://Added by RL244
                    champions.Darius.Load();
                    break;
                case "Amumu"://Added by xcsoft
                    champions.Amumu.Load();
                    break;
                case "Yasuo"://Added by RL244 WIP
                    champions.Yasuo.Load();
                    break;
                case "Syndra"://Added by RL244 WIP
                    champions.Syndra.Load();
                    break;
                case "Viktor"://Added by RL244 WIP
                    champions.Viktor.Load();
                    break;
                case "Janna"://Added by xcsoft
                    champions.Janna.Load();
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
               return true;
            }
            catch
            {
                AIO_Func.sendDebugMsg(tag + ObjectManager.Player.ChampionName + " Not supported.", false);
                Game.PrintChat(AIO_Func.colorChat(System.Drawing.Color.SpringGreen, tag) + AIO_Func.colorChat(System.Drawing.Color.DeepPink, ObjectManager.Player.ChampionName) + " Not supported.");

                AIO_Menu.addItem("Sorry, " + ObjectManager.Player.ChampionName + " Not supported", null);
                return false;
            }
        }
    }
}
