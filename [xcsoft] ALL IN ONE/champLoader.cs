using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                default:
                    break;
            } 
        }
    }
}
