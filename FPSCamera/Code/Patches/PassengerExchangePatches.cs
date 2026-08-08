using FPSCamera.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using static FPSCamera.Utils.TransportUtils;

namespace FPSCamera.Patches;

internal static class PassengerExchangePatches
{
    internal static void Apply(Harmony harmony)
    {

        var loadPrefix = CreatePatch(typeof(LoadPassengersPatch), nameof(LoadPassengersPatch.Prefix));
        var loadPostfix = CreatePatch(typeof(LoadPassengersPatch), nameof(LoadPassengersPatch.Postfix));
        PatchMethods(harmony, "LoadPassengers", loadPrefix, loadPostfix);

        var unloadPrefix = CreatePatch(typeof(UnloadPassengersPatch), nameof(UnloadPassengersPatch.Prefix));
        var unloadPostfix = CreatePatch(typeof(UnloadPassengersPatch), nameof(UnloadPassengersPatch.Postfix));
        PatchMethods(harmony, "UnloadPassengers", unloadPrefix, unloadPostfix);
    }

    internal static class PublicTransportPassengerMethods
    {
        internal static IEnumerable<MethodBase> Find(string methodName)
        {
            var methods = new HashSet<MethodBase>();
            foreach (var aiType in PTAITypes)
            {
                var method = AccessTools.Method(aiType, methodName);
                if (method != null && methods.Add(method))
                    yield return method;
            }
        }

        [AccessUtils.UsedReflection]
        private static readonly Type[] PTAITypes =
        [
            typeof(BusAI),
            typeof(TrolleybusAI),
            typeof(TramAI),
            typeof(PassengerTrainAI),
            typeof(PassengerPlaneAI),
            typeof(PassengerHelicopterAI),
            typeof(PassengerBlimpAI),
            typeof(PassengerFerryAI),
            typeof(PassengerShipAI),
            typeof(CableCarAI),
        ];
    }

    internal static class LoadPassengersPatch
    {
        internal static void Prefix(ushort vehicleID, out int __state)
            => __state = PassengerExchangeTracker.BeginBoarding(vehicleID);

        internal static void Postfix(ushort vehicleID, int __state)
            => PassengerExchangeTracker.EndBoarding(vehicleID, __state);
    }
    internal static class UnloadPassengersPatch
    {
        internal static void Prefix(ushort vehicleID, out UnloadState __state)
            => __state = new(vehicleID, PassengerExchangeTracker.BeginUnload(vehicleID));

        internal static void Postfix(UnloadState __state)
            => PassengerExchangeTracker.EndUnload(__state.VehicleID, __state.PreviousPassengerCount);

        // Multi-car vehicles overwrite the vehicleID argument while walking their trailer chain.
        // Preserve the original lead vehicle ID for the Postfix instead of reading the final argument value.
        internal readonly struct UnloadState
        {
            internal UnloadState(ushort vehicleID, int previousPassengerCount)
            {
                VehicleID = vehicleID;
                PreviousPassengerCount = previousPassengerCount;
            }

            internal ushort VehicleID { get; }
            internal int PreviousPassengerCount { get; }
        }
    }


    private static HarmonyMethod CreatePatch(Type patchType, string patchName)
    {
        var method = AccessTools.Method(patchType, patchName)
            ?? throw new MissingMethodException(patchType.FullName, patchName);
        return new HarmonyMethod(method)
        {
            after = ["com.IPT", "github.com/bloodypenguin/ImprovedPublicTransport", "com.redirectors.TLM"]
        };
    }

    private static void PatchMethods(Harmony harmony, string methodName, HarmonyMethod prefix, HarmonyMethod postfix)
    {
        var patchCount = 0;
        foreach (var method in PublicTransportPassengerMethods.Find(methodName))
        {
            harmony.Patch(method, prefix: prefix, postfix: postfix);
            patchCount++;
        }

        if (patchCount == 0)
            throw new MissingMethodException($"No public transport {methodName} methods were found");
    }
}
