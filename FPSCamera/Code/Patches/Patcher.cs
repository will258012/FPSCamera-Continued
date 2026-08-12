using AlgernonCommons;
using AlgernonCommons.Patching;
using FPSCamera.Settings;
using FPSCamera.Utils;
using HarmonyLib;
using System;

namespace FPSCamera.Patches;

public sealed class Patcher : PatcherBase
{
    public override void PatchAll()
    {
        try
        {
            base.PatchAll();
            ListMethods();
        }
        catch
        {
            passengerExchangePatcher.UnpatchAll();
            lodPatcher.UnpatchAll();
            new Harmony(HarmonyID).UnpatchAll(HarmonyID);
            throw;
        }
    }

    protected override void OnPatchAll(Harmony harmonyInstance)
    {
        lodPatcher.HarmonyID = $"{HarmonyID}.Lod";
        passengerExchangePatcher.HarmonyID = $"{HarmonyID}.PassengerExchange";

        SetPatchState(lodPatcher, ModSettings.LodOpt != 0);
        SetPatchState(passengerExchangePatcher, ModSettings.ShowPassengerExchangeInfo);
    }

    internal void UpdateLodPatches(bool enabled)
    {
        if (Patched)
            SetPatchState(lodPatcher, enabled);
    }

    internal void UpdatePassengerExchangePatches(bool enabled)
    {
        if (Patched)
            SetPatchState(passengerExchangePatcher, enabled);

        if (!enabled)
            TransportUtils.PassengerExchangeTracker.Reset();
    }

    public override void UnpatchAll()
    {
        passengerExchangePatcher.UnpatchAll();
        lodPatcher.UnpatchAll();
        TransportUtils.PassengerExchangeTracker.Reset();
        base.UnpatchAll();
    }

    private void SetPatchState(OptionalPatcherBase patcher, bool enabled)
    {
        Logging.KeyMessage($"Setting patch {patcher.HarmonyID} to {enabled}");
        if (enabled)
            patcher.PatchAll();
        else
            patcher.UnpatchAll();
        ListMethods();
    }
    public new void ListMethods()
    {
        base.ListMethods();
        lodPatcher.ListMethods();
        passengerExchangePatcher.ListMethods();
    }

    private readonly LodPatcher lodPatcher = new();
    private readonly PassengerExchangePatcher passengerExchangePatcher = new();
}

internal abstract class OptionalPatcherBase : PatcherBase
{
    public override void PatchAll()
    {
        if (Patched)
            return;

        var harmony = new Harmony(HarmonyID);
        try
        {
            Apply(harmony);
            Patched = true;
        }
        catch (Exception e)
        {
            harmony.UnpatchAll(HarmonyID);
            Logging.LogException(e, "Failed to patch ", HarmonyID);
        }
    }

    protected abstract void Apply(Harmony harmony);
}

internal sealed class LodPatcher : OptionalPatcherBase
{
    protected override void Apply(Harmony harmony) => LodPatches.Apply(harmony);
}

internal sealed class PassengerExchangePatcher : OptionalPatcherBase
{
    protected override void Apply(Harmony harmony) => PassengerExchangePatches.Apply(harmony);
}
