using System;

using LeagueSharp;

namespace ALL_In_One
{
    class ChampLoader
    {
        enum Developer
        {
            xcsoft,
            RL244,
            Fakker
        }

        internal static void Load(string champName)
        {
            switch (champName)
            {
                case "MasterYi":
                    MadeBy(Developer.xcsoft);
                    champions.MasterYi.Load();
                    break;
                case "Annie":
                    MadeBy(Developer.xcsoft);
                    champions.Annie.Load();
                    break;
                case "Garen":
                    MadeBy(Developer.xcsoft);
                    champions.Garen.Load();
                    break;
                case "Kalista":
                    MadeBy(Developer.xcsoft);
                    champions.Kalista.Load();
                    break;
                case "Ryze":
                    MadeBy(Developer.xcsoft);
                    champions.Ryze.Load();
                    break;
                case "Vi":
                    MadeBy(Developer.xcsoft);
                    champions.Vi.Load();
                    break;
                case "Vladimir":
                    MadeBy(Developer.xcsoft);
                    champions.Vladimir.Load();
                    break;
                case "Chogath":
                    MadeBy(Developer.xcsoft);
                    champions.Chogath.Load();
                    break;
                case "Urgot":
                    MadeBy(Developer.Fakker);
                    champions.Urgot.Load();
                    break;
                case "Fiora":
                    MadeBy(Developer.RL244);
                    champions.Fiora.Load();
                    break;
                case "Lulu":
                    MadeBy(Developer.xcsoft);
                    champions.Lulu.Load();
                    break;
                case "Nautilus":
                    MadeBy(Developer.xcsoft);
                    champions.Nautilus.Load();
                    break;
                case "Graves":
                    MadeBy(Developer.xcsoft);
                    champions.Graves.Load();
                    break;
                case "Sivir":
                    MadeBy(Developer.xcsoft);
                    champions.Sivir.Load();
                    break;
                case "XinZhao":
                    MadeBy(Developer.RL244);
                    champions.XinZhao.Load();
                    break;
                case "Katarina":
                    MadeBy(Developer.xcsoft);
                    champions.Katarina.Load();
                    break;
                case "Veigar":
                    MadeBy(Developer.RL244);
                    champions.Veigar.Load();
                    break;
                case "Talon":
                    MadeBy(Developer.RL244);
                    champions.Talon.Load();
                    break;
                case "Azir": 
                    MadeBy(Developer.RL244); // Work In Progress
                    champions.Azir.Load();
                    break;
                case "Gangplank":
                    MadeBy(Developer.RL244);
                    champions.Gangplank.Load();
                    break;
                case "Blitzcrank":
                    MadeBy(Developer.RL244);
                    champions.Blitzcrank.Load();
                    break;
                case "Brand":
                    MadeBy(Developer.RL244);
                    champions.Brand.Load();
                    break;
                case "Cassiopeia":
                    MadeBy(Developer.RL244);
                    champions.Cassiopeia.Load();
                    break;
                case "KogMaw":
                    MadeBy(Developer.RL244);
                    champions.KogMaw.Load();
                    break;
                case "Zyra":
                    MadeBy(Developer.RL244);
                    champions.Zyra.Load();
                    break;
                case "Caitlyn":
                    MadeBy(Developer.RL244); 
                    champions.Caitlyn.Load();
                    break;
                case "MissFortune":
                    MadeBy(Developer.RL244);
                    champions.MissFortune.Load();
                    break;
                case "Tristana":
                    MadeBy(Developer.RL244);
                    champions.Tristana.Load();
                    break;
                case "Karthus":
                    MadeBy(Developer.xcsoft);
                    champions.Karthus.Load();
                    break;
                case "Karma":
                    MadeBy(Developer.RL244);// Work In Progress
                    champions.Karma.Load();
                    break;
                case "Jax":
                    MadeBy(Developer.RL244);
                    champions.Jax.Load();
                    break;
                case "Darius":
                    MadeBy(Developer.RL244);
                    champions.Darius.Load();
                    break;
                case "Amumu":
                    MadeBy(Developer.xcsoft);
                    champions.Amumu.Load();
                    break;
                case "Yasuo":
                    MadeBy(Developer.RL244);
                    champions.Yasuo.Load();
                    break;
                case "Syndra":
                    MadeBy(Developer.RL244);// Work In Progress
                    champions.Syndra.Load();
                    break;
                case "Viktor":
                    MadeBy(Developer.RL244);// Work In Progress
                    champions.Viktor.Load();
                    break;
                case "Janna":
                    MadeBy(Developer.xcsoft);
                    champions.Janna.Load();
                    break;
                case "Nami":
                    MadeBy(Developer.xcsoft);
                    champions.Nami.Load();
                    break;
                case "Sion":
                    MadeBy(Developer.xcsoft);
                    champions.Sion.Load();
                    break;
                case "MonkeyKing":
                    MadeBy(Developer.xcsoft);
                    champions.MonkeyKing.Load();
                    break;
                case "Evelynn":
                    MadeBy(Developer.RL244);
                    champions.Evelynn.Load();
                    break;
                case "Kassadin":
                    MadeBy(Developer.RL244);
                    champions.Kassadin.Load();
                    break;
                case "Mordekaiser":
                    MadeBy(Developer.RL244);
                    champions.Mordekaiser.Load();
                    break;
                case "Trundle":
                    MadeBy(Developer.RL244);
                    champions.Trundle.Load();
                    break;
                case "Nasus":
                    MadeBy(Developer.RL244);
                    champions.Nasus.Load();
                    break;
                case "Zed":
                    MadeBy(Developer.xcsoft); //(Incomplete)
                    champions.Zed.Load();
                    break;
                case "KhaZix":
                    MadeBy(Developer.RL244);
                    champions.KhaZix.Load();
                    break;
                case "Shyvana":
                    MadeBy(Developer.RL244);
                    champions.Shyvana.Load();
                    break;
                case "Jinx":
                    MadeBy(Developer.xcsoft);
                    champions.Jinx.Load();
                    break;
                case "Yorick":
                    MadeBy(Developer.RL244);
                    champions.Yorick.Load();
                    break;
                case "Renekton": // Work In Progress
                    MadeBy(Developer.RL244);
                    champions.Renekton.Load();
                    break;
                case "Rengar":
                    MadeBy(Developer.RL244);
                    champions.Rengar.Load();
                    break;
                case "Warwick":
                    MadeBy(Developer.RL244);
                    champions.Warwick.Load();
                    break;
                case "Ezreal":
                    MadeBy(Developer.RL244);
                    champions.Ezreal.Load();
                    break;
                case "JarvanIV": 
                    MadeBy(Developer.RL244);
                    champions.JarvanIV.Load();
                    break;
                case "Teemo":
                    MadeBy(Developer.RL244);
                    champions.Teemo.Load();
                    break;
                case "Aatrox":
                    MadeBy(Developer.RL244);
                    champions.Aatrox.Load();
                    break;
                case "Nunu":
                    MadeBy(Developer.RL244);
                    champions.Nunu.Load();
                    break;
                case "DrMundo":
                    MadeBy(Developer.RL244);
                    champions.DrMundo.Load();
                    break;
                case "Volibear": 
                    MadeBy(Developer.RL244);
                    champions.Volibear.Load();
                    break;
                case "Riven": 
                    MadeBy(Developer.RL244);
                    champions.Riven.Load();
                    break;
                case "Ahri": 
                    MadeBy(Developer.RL244);
                    champions.Ahri.Load();
                    break;
                case "Corki": 
                    MadeBy(Developer.RL244);
                    champions.Corki.Load();
                    break;
                case "Vayne": 
                    MadeBy(Developer.RL244);
                    champions.Vayne.Load();
                    break;
                default:
                    AIO_Func.sendDebugMsg("(ChampLoader)Champ Not Supported.");
                    break;
            } 
        }
        
        static void MadeBy(Developer Developer)
        {
            AIO_Func.sendDebugMsg(ObjectManager.Player.ChampionName + " Made By '" + Developer.ToString() + "'.");
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
