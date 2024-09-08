using AlgernonCommons;
using ColossalFramework.Plugins;
namespace FPSCamera.Utils
{
    public class ModSupport
    {
        public static bool FoundToggleIt { get; private set; }
        //public static bool FoundTrainDisplay { get; private set; }
        public static bool FoundUUI { get; private set; }
        public static bool FoundTLM { get; private set; }

        internal static void Initialize()
        {
            try
            {
                var infos = PluginManager.instance.GetPluginsInfo();
                foreach (var info in infos)
                {
                    if ((info.publishedFileID.AsUInt64 == 1764637396 || info.publishedFileID.AsUInt64 == 2573796841) && info.isEnabled)
                    {
                        Logging.Message("ModSupport: \"Toggle It!\" (or its CHS version) was found!");
                        FoundToggleIt = true;
                        continue;
                    }
                    /*
                    if ((info.publishedFileID.AsUInt64 == 3233229958 || info.name.Contains("TrainDisplay")) && info.isEnabled)
                    {
                        Logging.Message("ModSupport: \"Train Display - Update\" was found!");
                        FoundTrainDisplay = true;
                        continue;
                    }
                    */
                    if ((info.publishedFileID.AsUInt64 == 2966990700 || info.publishedFileID.AsUInt64 == 2255219025) && info.isEnabled)
                    {
                        Logging.Message("ModSupport: \"UnifiedUI \" was found!");
                        FoundUUI = true;
                        continue;
                    }
                    if (info.publishedFileID.AsUInt64 == 3007903394 && info.isEnabled)
                    {
                        Logging.Message("ModSupport: \"Transport Lines Manager by t1a2l\" was found!");
                        FoundTLM = true;
                        continue;
                    }
                }
            }

            catch (System.Exception e)
            {
                Logging.Error($"ModSupport: Falled to finding the mod: ");
                Logging.LogException(e);
            }
        }
    }
}
