using System;
using System.Reflection;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.View.Equipment;

namespace AstartesArmoury
{
    public static class AstartesArmouryModMain
    {
        private const string AstartesRaceGuid = "2302e1d517f847e6aef04c8c4a24d598";
        private const string GrantMarkerGuid = "192cf51626f64fcb997547df59eabb0f";

        private static readonly string[] WeaponGuids =
        {
            "63e82f54c6804c8890658161ebbd5ff9", // Vigil's Oath
            "700385a6da364d12880ddffd1d3896b2", // Final Judgement
            "0913355c59664cccb802fa68bce336f6"  // God-Emperor's Wrath
        };

        private static readonly GrantOnAreaLoad Handler = new GrantOnAreaLoad();
        private static OwlcatModification s_modification;

        [OwlcatModificationEnterPoint]
        public static void Initialize(OwlcatModification modification)
        {
            s_modification = modification;
            EventBus.Subscribe(Handler);
            new Harmony(modification.Manifest.UniqueName).PatchAll(Assembly.GetExecutingAssembly());
            Log("[Grant] Runtime handler registered.");
        }

        internal static void TryGrantWeapons()
        {
            try
            {
                BaseUnitEntity player = Game.Instance?.Player?.MainCharacterEntity;
                if (player?.Progression?.Race == null ||
                    player.Progression.Race.AssetGuid != AstartesRaceGuid)
                    return;

                BlueprintFeature marker = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(GrantMarkerGuid);
                if (marker == null)
                {
                    LogError("[Grant][ERR] Persistent marker blueprint was not found.");
                    return;
                }

                if (player.Progression.Features.GetRank(marker) > 0)
                    return;

                if (player.Inventory == null)
                {
                    LogError("[Grant][ERR] Main-character inventory is not available after area load.");
                    return;
                }

                foreach (string guid in WeaponGuids)
                {
                    BlueprintItem weapon = ResourcesLibrary.TryGetBlueprint<BlueprintItem>(guid);
                    if (weapon == null)
                    {
                        LogError("[Grant][ERR] Weapon blueprint was not found: " + guid);
                        return;
                    }

                    // If a previous attempt stopped part-way through, keep existing items and add only missing ones.
                    if (!player.Inventory.Contains(weapon, 1))
                        player.Inventory.Add(weapon);
                }

                // The marker is written only after all three items are present. It persists in the save, so
                // moving, selling or dropping a weapon later never causes another grant.
                player.Progression.Features.Add(marker, null, null);
                Log("[Grant] Granted the three Astartes Armoury weapons to the Deathwatch player character.");
            }
            catch (Exception exception)
            {
                LogError("[Grant][ERR] Runtime grant failed.", exception);
            }
        }

        private static void Log(string message)
        {
            s_modification?.Logger.Log(message);
        }

        private static void LogError(string message)
        {
            s_modification?.Logger.Error(message);
        }

        private static void LogError(string message, Exception exception)
        {
            s_modification?.Logger.Error(exception, message);
        }
    }

    [HarmonyPatch(typeof(UnitViewHandSlotData), "OwnerWeaponScale", MethodType.Getter)]
    internal static class FinalJudgementWeaponScalePatch
    {
        private const string FinalJudgementGuid = "700385a6da364d12880ddffd1d3896b2";
        private const float ScaleMultiplier = 1.30f;

        [HarmonyPostfix]
        private static void Postfix(UnitViewHandSlotData __instance, ref float __result)
        {
            BaseUnitEntity owner = __instance?.Owner as BaseUnitEntity;
            if (owner?.Progression?.Race?.AssetGuid != "2302e1d517f847e6aef04c8c4a24d598")
                return;
            if (__instance.VisibleItem?.Blueprint?.AssetGuid != FinalJudgementGuid)
                return;
            __result *= ScaleMultiplier;
        }
    }

    internal sealed class GrantOnAreaLoad : IAreaHandler
    {
        public void OnAreaDidLoad()
        {
            AstartesArmouryModMain.TryGrantWeapons();
        }

        public void OnAreaBeginUnloading()
        {
        }
    }
}
