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
                case "MasterYi":
					MadeBy("xcsoft");
                    champions.MasterYi.Load();
                    break;
                case "Annie":
					MadeBy("xcsoft");
                    champions.Annie.Load();
                    break;
                case "Garen":
					MadeBy("xcsoft");
                    champions.Garen.Load();
                    break;
                case "Kalista":
					MadeBy("xcsoft");
                    champions.Kalista.Load();
                    break;
                case "Ryze":
					MadeBy("xcsoft");
                    champions.Ryze.Load();
                    break;
                case "Vi":
					MadeBy("xcsoft");
                    champions.Vi.Load();
                    break;
                case "Vladimir":
					MadeBy("xcsoft");
                    champions.Vladimir.Load();
                    break;
                case "Chogath":
					MadeBy("xcsoft");
                    champions.Chogath.Load();
                    break;
                case "Urgot":
					MadeBy("fakker");
                    champions.Urgot.Load();
                    break;
                case "Fiora":
					MadeBy("RL244");
                    champions.Fiora.Load();
                    break;
                case "Lulu":
					MadeBy("xcsoft");
                    champions.Lulu.Load();
                    break;
                case "Nautilus": 
					MadeBy("fakker");
                    champions.Nautilus.Load();
                    break;
                case "Graves":
					MadeBy("fakker");
                    champions.Graves.Load();
                    break;
                case "Sivir":
					MadeBy("fakker");
                    champions.Sivir.Load();
                    break;
                case "XinZhao":
					MadeBy("RL244");
                    champions.XinZhao.Load();
                    break;
                case "Katarina":
					MadeBy("xcsoft");
                    champions.Katarina.Load();
                    break;
                case "Veigar":
					MadeBy("RL244");
                    champions.Veigar.Load();
                    break;
                case "Talon":
					MadeBy("RL244");
                    champions.Talon.Load();
                    break;
                case "Azir": 
					MadeBy("RL244"); // Work In Progress
                    champions.Azir.Load();
                    break;
                case "Gangplank":
					MadeBy("RL244");
                    champions.Gangplank.Load();
                    break;
                case "Blitzcrank":
					MadeBy("RL244");
                    champions.Blitzcrank.Load();
                    break;
                case "Brand":
					MadeBy("RL244");
                    champions.Brand.Load();
                    break;
                case "Cassiopeia":
					MadeBy("RL244");
                    champions.Cassiopeia.Load();
                    break;
                case "KogMaw":
					MadeBy("RL244");
                    champions.KogMaw.Load();
                    break;
                case "Zyra":
					MadeBy("RL244");
                    champions.Zyra.Load();
                    break;
                case "Caitlyn":
					MadeBy("RL244");
                    champions.Caitlyn.Load();
                    break;
                case "MissFortune":
					MadeBy("RL244");
                    champions.MissFortune.Load();
                    break;
                case "Tristana":
					MadeBy("RL244");
                    champions.Tristana.Load();
                    break;
                case "Karthus":
					MadeBy("xcsoft");
                    champions.Karthus.Load();
                    break;
                case "Karma":
					MadeBy("RL244");// Work In Progress
                    champions.Karma.Load();
                    break;
                case "Jax":
					MadeBy("RL244");
                    champions.Jax.Load();
                    break;
                case "Darius":
					MadeBy("RL244");
                    champions.Darius.Load();
                    break;
                case "Amumu":
					MadeBy("xcsoft");
                    champions.Amumu.Load();
                    break;
                case "Yasuo":
					MadeBy("RL244");// Work In Progress
                    champions.Yasuo.Load();
                    break;
                case "Syndra":
					MadeBy("RL244");// Work In Progress
                    champions.Syndra.Load();
                    break;
                case "Viktor":
					MadeBy("RL244");// Work In Progress
                    champions.Viktor.Load();
                    break;
                case "Janna":
					MadeBy("xcsoft");
                    champions.Janna.Load();
                    break;
                case "Nami":
					MadeBy("xcsoft");
                    champions.Nami.Load();
                    break;
                case "Sion":
					MadeBy("xcsoft");
                    champions.Sion.Load();
                    break;
                case "MonkeyKing":
					MadeBy("xcsoft");
                    champions.MonkeyKing.Load();
                    break;
                case "Evelynn":
					MadeBy("RL244");
                    champions.Evelynn.Load();
                    break;
                case "Kassadin":
					MadeBy("RL244");
                    champions.Kassadin.Load();
                    break;
                case "Mordekaiser":
					MadeBy("RL244");
                    champions.Mordekaiser.Load();
                    break;
                case "Trundle":
					MadeBy("RL244");
                    champions.Trundle.Load();
                    break;
                case "Nasus":
					MadeBy("RL244");
                    champions.Nasus.Load();
                    break;
                case "Zed":
					MadeBy("xcsoft"); //(Incomplete)
                    champions.Zed.Load();
                    break;
                case "KhaZix":
					MadeBy("RL244");
                    champions.KhaZix.Load();
                    break;
                case "Shyvana":
					MadeBy("RL244");
                    champions.Shyvana.Load();
                    break;
                case "Jinx":
					MadeBy("xcsoft");
                    champions.Jinx.Load();
                    break;
                case "Yorick":
					MadeBy("RL244");
                    champions.Yorick.Load();
                    break;
                case "Renekton":
					MadeBy("RL244");
                    champions.Renekton.Load();
                    break;
                case "Rengar":
					MadeBy("RL244");
                    champions.Rengar.Load();
                    break;
                case "Warwick":
					MadeBy("RL244");
                    champions.Warwick.Load();
                    break;
                case "Ezreal":
					MadeBy("RL244");
                    champions.Ezreal.Load();
                    break;
                case "JarvanIV": //WIP
					MadeBy("RL244");
                    champions.JarvanIV.Load();
                    break;
                default:
                    AIO_Func.sendDebugMsg("(champLoader)Champ Not Supported.");
                    break;
            } 
        }
		
		internal static void MadeBy(string Creator)
		{
			AIO_Func.sendDebugMsg(ObjectManager.Player.ChampionName + " Made By '" + Creator+"'.","[TeamProjects] AIO : ");
		}

        internal static bool champSupportedCheck(string checkNamespace)
        {
            try
            {
               AIO_Func.sendDebugMsg(Type.GetType(checkNamespace + ObjectManager.Player.ChampionName).Name + " Supported.");
               return true;
            }
            catch
            {
                AIO_Func.sendDebugMsg(ObjectManager.Player.ChampionName + " Not supported.","[TeamProjects] AIO : "); //sendDebugMsg와 중복으로 제거함.

                AIO_Menu.addItem("Sorry, " + ObjectManager.Player.ChampionName + " Not supported", null);
                return false;
            }
        }
    }
}
