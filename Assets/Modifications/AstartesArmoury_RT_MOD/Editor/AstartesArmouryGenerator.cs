using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OwlcatModification.Editor;
using OwlcatModification.Editor.Build;
using UnityEditor;
using UnityEngine;

namespace AstartesArmoury.Editor
{
    internal static class AstartesArmouryGenerator
    {
        internal const string Vigil = "63e82f54c6804c8890658161ebbd5ff9";
        internal const string FinalJudgement = "700385a6da364d12880ddffd1d3896b2";
        internal const string Wrath = "0913355c59664cccb802fa68bce336f6";
        internal const string BallisticFeature = "01b265893bbf4625bf49e3b6675f5776";
        internal const string WeaponFeature = "78c747cfb6324e608501262a0ce158f7";
        internal const string VigilEffect = "7761d44a4fef4e12934bd503901ec72b";
        internal const string VigilBuff = "60f2e3083d2d4498bfc755fe1618f78f";
        internal const string WrathEffect = "3cb894eef8684f068f05817788850a92";
        internal const string WrathBuff = "47d1c8acd7a04a31ad06fb939b91e66d";
        internal const string GrantMarker = "192cf51626f64fcb997547df59eabb0f";

        private const string Annihilator = "781b90112a784f03843bb8faa34d1ae7";
        private const string Eviscerator = "af978f6d159b464d9fd6cf71ce056993";
        private const string EvisceratorUnique = "4d87435ddfa042269c1fe35df0430f8b";
        private const string Sarragus = "dec66b3861c64c088c5f38fd49024d44";

        private static string Root => "Assets/Modifications/AstartesArmoury_RT_MOD";
        private static string Blueprints => Path.Combine(Root, "Blueprints");

        [MenuItem("Astartes Armoury/Generate V1 blueprints")]
        public static void Generate()
        {
            Directory.CreateDirectory(Blueprints);
            Directory.CreateDirectory(Path.Combine(Root, "Localization"));

            JObject vigilBuff = Load("a8521f9dd3ea4824af9ed4ffcd47fd73");
            ReplaceIds(vigilBuff, ("a8521f9dd3ea4824af9ed4ffcd47fd73", VigilBuff));
            SetIdentity(vigilBuff, VigilBuff, "Vigil's Oath Critical Momentum");
            Save("VigilsOath_CriticalMomentum_Buff", vigilBuff);

            JObject vigilEffect = Load("0bccec1be5004ec39aadfa0c739d334a");
            ReplaceIds(vigilEffect,
                ("0bccec1be5004ec39aadfa0c739d334a", VigilEffect),
                (Annihilator, Vigil),
                ("a8521f9dd3ea4824af9ed4ffcd47fd73", VigilBuff));
            SetIdentity(vigilEffect, VigilEffect, "Vigil's Oath On-Hit Effect");
            ((JObject)vigilEffect["Data"]["Components"][0])["OnlyRighteousFury"] = false;
            Save("VigilsOath_OnHit_Feature", vigilEffect);

            JObject wrathBuff = Load("cadf89b500184ca681233b25f5769372");
            ReplaceIds(wrathBuff,
                ("cadf89b500184ca681233b25f5769372", WrathBuff),
                (Sarragus, Wrath));
            SetIdentity(wrathBuff, WrathBuff, "God-Emperor's Wrath Rate of Fire");
            wrathBuff["Data"]["Ranks"] = 10;
            Save("GodEmperorsWrath_RateOfFire_Buff", wrathBuff);

            JObject wrathEffect = Load("14c404c9b52f4bb59292143179dd0a2a");
            ReplaceIds(wrathEffect,
                ("14c404c9b52f4bb59292143179dd0a2a", WrathEffect),
                (Sarragus, Wrath),
                ("cadf89b500184ca681233b25f5769372", WrathBuff));
            SetIdentity(wrathEffect, WrathEffect, "God-Emperor's Wrath Kill Effect");
            JObject applyWrath = (JObject)wrathEffect["Data"]["Components"][0]["ActionsOnKill"]["Actions"][0];
            applyWrath["BuffEndCondition"] = "CombatEnd";
            applyWrath["Permanent"] = true;
            Save("GodEmperorsWrath_OnKill_Feature", wrathEffect);

            JObject featureTemplate = Load("53c19a9468d24539863989b3be9ed1f5");
            Save("AstartesArmoury_BallisticSkill10_Feature",
                MakeStatFeature(featureTemplate, BallisticFeature, "WarhammerBallisticSkill"));
            Save("AstartesArmoury_WeaponSkill10_Feature",
                MakeStatFeature(featureTemplate, WeaponFeature, "WarhammerWeaponSkill"));

            JObject vigil = PrepareItem(Load(Annihilator), Vigil, Annihilator,
                "aa-vigil-effect", VigilEffect, "aa-vigil-bs", BallisticFeature);
            SetWeaponModel(vigil, "e59b1e82f8f33d248ab3c25cf8597719", 98770554346048773L);
            SetWeaponText(vigil, "aa-vigil-name", "aa-vigil-desc");
            Override(vigil, "WarhammerDamage", 30);
            Override(vigil, "WarhammerMaxDamage", 45);
            Override(vigil, "WarhammerPenetration", 25);
            Override(vigil, "RateOfFire", 3);
            Override(vigil, "IsTwoHanded", false);
            Save("VigilsOath_Item", vigil);

            JObject final = PrepareItem(Load(Eviscerator), FinalJudgement, Eviscerator,
                "aa-final-ws", WeaponFeature);
            SetWeaponModel(final, "d65a26d466550cd4a9846420f3c3e006", 3206943657695844805L);
            SetWeaponText(final, "aa-final-name", "aa-final-desc");
            Override(final, "WarhammerDamage", 34);
            Override(final, "WarhammerMaxDamage", 50);
            Override(final, "WarhammerPenetration", 30);
            Override(final, "m_DamageStatBonusFactor", "Two");
            JObject unique = Load(EvisceratorUnique);
            final["Data"]["AbilityContainer"]["Ability3"] = unique["Data"]["AbilityContainer"]["Ability3"].DeepClone();
            AddOverride(final, "WeaponAbilities.Ability3.Type");
            AddOverride(final, "WeaponAbilities.Ability3.Mode");
            AddOverride(final, "WeaponAbilities.Ability3.m_Ability");
            AddOverride(final, "WeaponAbilities.Ability3.m_FXSettings");
            AddOverride(final, "WeaponAbilities.Ability3.AP");
            Save("FinalJudgement_Item", final);

            JObject wrath = PrepareItem(Load(Sarragus), Wrath, Sarragus,
                "aa-wrath-effect", WrathEffect, "aa-wrath-bs", BallisticFeature);
            SetWeaponModel(wrath, "4fcbe5a3d7a730c489e9a23b53e1cb20", 5139631943559371841L);
            SetWeaponText(wrath, "aa-wrath-name", "aa-wrath-desc");
            Override(wrath, "WarhammerDamage", 12);
            Override(wrath, "WarhammerMaxDamage", 18);
            Override(wrath, "WarhammerPenetration", 30);
            Override(wrath, "RateOfFire", 6);
            Override(wrath, "WarhammerMaxAmmo", 96);
            Save("GodEmperorsWrath_Item", wrath);

            Save("AstartesArmoury_RuntimeGrantMarker_Feature", MakeGrantMarker(featureTemplate));
            WriteLocalization();
            AssetDatabase.Refresh();
            Debug.Log("[AstartesArmoury] V1 blueprint generation completed.");
        }

        [MenuItem("Astartes Armoury/Build V1")]
        public static void Build()
        {
            Generate();
            var mod = AssetDatabase.LoadAssetAtPath<Modification>(Root + "/AstartesArmoury_RT_MOD.asset");
            if (mod == null) throw new InvalidOperationException("Modification asset could not be loaded.");
            var result = Builder.Build(mod);
            if ((int)result != 0) throw new InvalidOperationException("Build failed: " + result);
            Debug.Log("[AstartesArmoury] Build completed: " + mod.GetFinalBuildPath());
        }

        private static JObject PrepareItem(JObject root, string id, string prototype, params string[] componentPairs)
        {
            SetIdentity(root, id, null);
            JObject data = (JObject)root["Data"];
            data["PrototypeLink"] = prototype;
            data["m_Overrides"] = new JArray();
            foreach (JObject component in data["Components"].OfType<JObject>())
            {
                component["PrototypeLink"] = new JObject
                {
                    ["guid"] = prototype,
                    ["name"] = component["name"]?.ToString() ?? ""
                };
                component["m_Overrides"] = new JArray();
            }
            for (int i = 0; i < componentPairs.Length; i += 2)
            {
                string name = "$AddFactToEquipmentWielder$" + componentPairs[i];
                ((JArray)data["Components"]).Add(new JObject
                {
                    ["$type"] = "65221a9a6133bd0408b019b86642d97e, AddFactToEquipmentWielder",
                    ["name"] = name,
                    ["m_Flags"] = 0,
                    ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
                    ["m_Overrides"] = new JArray(),
                    ["m_Fact"] = "!bp_" + componentPairs[i + 1]
                });
                AddOverride(root, name);
            }
            return root;
        }

        private static JObject MakeStatFeature(JObject template, string id, string stat)
        {
            JObject root = (JObject)template.DeepClone();
            SetIdentity(root, id, "+10 " + stat);
            JObject data = (JObject)root["Data"];
            data["PrototypeLink"] = "";
            data["m_Overrides"] = new JArray();
            data["Components"] = new JArray(new JObject
            {
                ["$type"] = "a2844c135c0324e439072bd3cc2f9260, AddStatBonus",
                ["name"] = "$AddStatBonus$" + id,
                ["m_Flags"] = 0,
                ["PrototypeLink"] = new JObject { ["guid"] = "", ["name"] = "" },
                ["m_Overrides"] = new JArray(),
                ["Descriptor"] = "None",
                ["Stat"] = stat,
                ["Value"] = 10
            });
            data["HideInUI"] = true;
            data["HideInCharacterSheetAndLevelUp"] = true;
            data["Ranks"] = 1;
            return root;
        }

        private static JObject MakeGrantMarker(JObject template)
        {
            JObject root = (JObject)template.DeepClone();
            SetIdentity(root, GrantMarker, "Astartes Armoury runtime grant marker");
            JObject data = (JObject)root["Data"];
            data["PrototypeLink"] = "";
            data["m_Overrides"] = new JArray();
            data["HideInUI"] = true;
            data["HideInCharacterSheetAndLevelUp"] = true;
            data["Ranks"] = 1;
            data["Components"] = new JArray();
            return root;
        }

        private static void WriteLocalization()
        {
            var strings = new JArray(
                Entry("aa-vigil-name", "Vigil's Oath"),
                Entry("aa-vigil-desc", "A bolt rifle kept ready through the longest watches. Each hit grants +10 percentage points of critical damage until combat ends, stacking up to 20 times. While equipped, the wielder gains +10 Ballistic Skill."),
                Entry("aa-final-name", "Final Judgement"),
                Entry("aa-final-desc", "A two-handed chain blade built to conclude any sentence. It adds twice the wielder's Strength bonus to damage, improves chain-weapon critical damage, grants +10 parry, and can deliver three attacks to one target for 2 AP. While equipped, the wielder gains +10 Weapon Skill."),
                Entry("aa-wrath-name", "God-Emperor's Wrath"),
                Entry("aa-wrath-desc", "A heavy bolter whose cadence rises with every execution. Each kill made with this weapon grants +1 rate of fire until combat ends, stacking up to 10 times. While equipped, the wielder gains +10 Ballistic Skill."));
            File.WriteAllText(Path.Combine(Root, "Localization", "enGB.json"),
                new JObject { ["strings"] = strings }.ToString(Formatting.Indented));
        }

        private static JObject Entry(string key, string value) =>
            new JObject { ["Key"] = key, ["Value"] = value };

        private static void SetWeaponText(JObject root, string name, string description)
        {
            root["Data"]["m_DisplayName"] = Localized(name);
            root["Data"]["m_Description"] = Localized(description);
            AddOverride(root, "m_DisplayName");
            AddOverride(root, "m_Description");
        }

        private static void SetWeaponModel(JObject root, string guid, long fileId)
        {
            root["Data"]["m_VisualParameters"]["m_WeaponModel"] = new JObject
            {
                ["guid"] = guid,
                ["fileid"] = fileId
            };
            AddOverride(root, "m_VisualParameters.m_WeaponModel");
        }

        private static JObject Localized(string key) => new JObject
        {
            ["m_Key"] = key, ["m_OwnerString"] = "", ["m_OwnerPropertyPath"] = "",
            ["m_JsonPath"] = "", ["Shared"] = null
        };

        private static void Override(JObject root, string property, JToken value)
        {
            root["Data"][property] = value;
            AddOverride(root, property);
        }

        private static void AddOverride(JObject root, string property)
        {
            JArray overrides = (JArray)root["Data"]["m_Overrides"];
            if (!overrides.Values<string>().Contains(property)) overrides.Add(property);
        }

        private static void SetIdentity(JObject root, string id, string comment)
        {
            root["AssetId"] = id;
            if (comment != null) root["Data"]["Comment"] = comment;
        }

        private static void ReplaceIds(JObject root, params (string oldId, string newId)[] replacements)
        {
            string json = root.ToString(Formatting.None);
            foreach (var replacement in replacements)
                json = json.Replace(replacement.oldId, replacement.newId);
            JObject replaced = JObject.Parse(json);
            root.RemoveAll();
            foreach (JProperty property in replaced.Properties().ToArray())
                root.Add(property.Name, property.Value);
        }

        private static JObject Load(string id)
        {
            BlueprintJsonWrapper wrapper = BlueprintsDatabase.LoadWrapperById(id);
            if (wrapper == null) throw new InvalidDataException("Blueprint not found: " + id);
            using var writer = new StringWriter();
            Json.Serializer.Serialize(writer, wrapper);
            return JObject.Parse(writer.ToString());
        }

        private static void Save(string name, JObject root)
        {
            root["Data"]["Author"] = "Poffl";
            if (root["Data"]["m_Overrides"] is JArray)
                AddOverride(root, "Author");
            File.WriteAllText(Path.Combine(Blueprints, name + ".jbp"), root.ToString(Formatting.Indented));
        }
    }
}
