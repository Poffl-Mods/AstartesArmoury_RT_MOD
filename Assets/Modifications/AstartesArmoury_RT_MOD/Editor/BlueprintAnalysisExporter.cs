using System.IO;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using UnityEditor;
using UnityEngine;

namespace AstartesArmoury.Editor
{
    internal static class BlueprintAnalysisExporter
    {
        private static readonly string[] BlueprintIds =
        {
            "781b90112a784f03843bb8faa34d1ae7", // AnnihilatorAstartesBolter_Item
            "af978f6d159b464d9fd6cf71ce056993", // Eviscerator_Item
            "4d87435ddfa042269c1fe35df0430f8b", // EvisceratorCH5Unique_Item
            "dec66b3861c64c088c5f38fd49024d44", // SarragusHeavyBolter_Item
            "0bccec1be5004ec39aadfa0c739d334a", // Annihilator equipment feature
            "14c404c9b52f4bb59292143179dd0a2a", // Sarragus equipment feature
            "903bb235e56d4dbebbfaf9372976b66f", // Eviscerator chain critical feature
            "53c19a9468d24539863989b3be9ed1f5", // Eviscerator parry feature
            "f70e4a5d21ba4bc9a7fce7e3e84bb59f", // CH5 Eviscerator extra feature
            "a6ec10d23b0f49698e17f30e70423615", // CH5 Eviscerator triple attack
            "a8521f9dd3ea4824af9ed4ffcd47fd73", // Annihilator stacking buff
            "cadf89b500184ca681233b25f5769372", // Sarragus stacking buff
        };

        [MenuItem("Astartes Armoury/Analysis/Export source blueprints")]
        public static void ExportSourceBlueprints()
        {
            string outputDirectory = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "BlueprintAnalysis"));
            Directory.CreateDirectory(outputDirectory);

            foreach (string id in BlueprintIds)
            {
                BlueprintJsonWrapper wrapper = BlueprintsDatabase.LoadWrapperById(id);
                if (wrapper == null)
                {
                    throw new InvalidDataException($"Blueprint {id} could not be loaded.");
                }

                string assetPath = BlueprintsDatabase.GetAssetPath(wrapper.Data);
                string fileName = $"{Path.GetFileNameWithoutExtension(assetPath)}_{id}.json";
                using var writer = new StreamWriter(Path.Combine(outputDirectory, fileName));
                Json.Serializer.Serialize(writer, wrapper);
                Debug.Log($"[AstartesArmoury] Exported {id} ({assetPath})");
            }
        }

        [MenuItem("Astartes Armoury/Analysis/Find equipment stat bonuses")]
        public static void FindEquipmentStatBonuses()
        {
            string outputPath = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "BlueprintAnalysis", "equipment-stat-bonuses.txt"));
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            SimpleBlueprint weapon = BlueprintsDatabase.LoadById<SimpleBlueprint>(BlueprintIds[0]);
            using var report = new StreamWriter(outputPath);
            foreach (var entry in BlueprintsDatabase.SearchByType(weapon.GetType()))
            {
                BlueprintJsonWrapper itemWrapper = BlueprintsDatabase.LoadWrapperById(entry.Guid);
                string itemJson = Serialize(itemWrapper);
                foreach (Match match in Regex.Matches(itemJson, "!bp_([0-9a-f]{32})"))
                {
                    BlueprintJsonWrapper factWrapper = BlueprintsDatabase.LoadWrapperById(match.Groups[1].Value);
                    if (factWrapper == null)
                    {
                        continue;
                    }

                    string factJson = Serialize(factWrapper);
                    bool relevantStat = factJson.Contains("\"Stat\": \"WarhammerBallisticSkill\"")
                        || factJson.Contains("\"Stat\": \"WarhammerWeaponSkill\"");
                    if (relevantStat && Regex.IsMatch(factJson, "\\\"Value\\\"\\s*:\\s*10(?:,|\\s)"))
                    {
                        report.WriteLine($"ITEM {entry.Guid} {entry.Path}");
                        report.WriteLine($"FACT {match.Groups[1].Value} {BlueprintsDatabase.GetAssetPath(factWrapper.Data)}");
                    }
                }
            }
        }

        private static string Serialize(BlueprintJsonWrapper wrapper)
        {
            using var writer = new StringWriter();
            Json.Serializer.Serialize(writer, wrapper);
            return writer.ToString();
        }
    }
}
