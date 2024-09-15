using AlgernonCommons;
using System;
namespace FPSCamera.Utils
{
    public class ModSupport
    {
        public static bool FoundToggleIt { get; private set; }
        public static bool FoundUUI { get; private set; }
        public static bool FoundTLM { get; private set; }

        internal static void Initialize()
        {
            try
            {
                if (AssemblyUtils.IsAssemblyPresent("ToggleIt"))
                    FoundToggleIt = true;

                if (AssemblyUtils.IsAssemblyPresent("UnifiedUILib"))
                    FoundUUI = true;

                var assembly = AssemblyUtils.GetEnabledAssembly("TransportLinesManager");

                if (assembly != null)
                {
                    var n = assembly?.GetType("Klyte.TransportLinesManager.TLMController");

                    if (n != null)
                    {
                        Logging.Error("ModSupport: Found an older version of Transport Lines Manager by Klyte45. Please update to the version by t1a2l for full feature support");
                    }
                    else
                    {
                        FoundTLM = true;
                    }
                }
            }

            catch (Exception e)
            {
                Logging.Error($"ModSupport: Falled to finding the mod");
                Logging.LogException(e);
            }
        }
    }
}

