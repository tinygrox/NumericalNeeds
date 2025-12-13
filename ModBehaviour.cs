using System;
using System.Collections.Generic;
using Duckov.Options;
using Duckov.Options.UI;
using Duckov.UI;
using HarmonyLib;
using tinygrox.DuckovMods.NumericalStats.HarmonyPatches;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class ModBehaviour: Duckov.Modding.ModBehaviour
    {
        private static readonly List<Duckov.Modding.ModBehaviour> s_mods = new List<Duckov.Modding.ModBehaviour>();
        private Harmony _harmony;

        private void LoadSettings()
        {
            ModSettings.SetShowNumericalHealth(OptionsManager.Load("ShowNumericalHealth", true));
            ModSettings.SetShowShowEnemyName(OptionsManager.Load("ShowEnemyName", true));
            ModSettings.SetShowNumericalWaterAndEnergy(OptionsManager.Load("ShowNumericalWaterAndEnergy", true));
            ModSettings.SetShowArmourStats(OptionsManager.Load("ShowArmourStats", true));
            ModSettings.SetMustShowHealthBar(OptionsManager.Load("MustShowHealthBar", false));
            ModSettings.SetShowHealthBarForObjects(OptionsManager.Load("ShowHealthBarForObjects", false));
            ModSettings.SetEditUI(OptionsManager.Load("NumericalStats_EditUI", false));

            ModSettings.LoadDisplayPositions();
        }
        private void Awake()
        {
            Debug.Log("[NumericalStats]ModBehaviour Awake");
            _harmony = new Harmony("tinygrox.DuckovMods.NumericalStats");
            LoadSettings();
            gameObject.AddComponent<DragToggleManager>();
            s_mods.Add(gameObject.AddComponent<ShowWaterAndEnergy>());
            s_mods.Add(gameObject.AddComponent<ArmourStatsDisplay>());
        }

        protected override void OnAfterSetup()
        {
            _harmony.Patch(
                AccessTools.Method(typeof(Health), nameof(Health.RequestHealthBar)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.HarmonyPatches), nameof(HarmonyPatches.HarmonyPatches.MustShowHealthBar))
            );
            _harmony.Patch(
                AccessTools.Method(typeof(HealthBarManager), "CreateHealthBarFor", new[] { typeof(Health), typeof(DamageInfo?) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches.HarmonyPatches), nameof(HarmonyPatches.HarmonyPatches.HealthBarAddNumericalHealthDisplay))
            );
            _harmony.Patch(
                AccessTools.Method(typeof(OptionsPanel), "Setup"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches.HarmonyPatches), nameof(HarmonyPatches.HarmonyPatches.OptionsPanelAddMySettingPanel))
            );
            _harmony.Patch(
                AccessTools.Method(typeof(HealthSimpleBase), "Awake"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches.HarmonyPatches), nameof(HarmonyPatches.HarmonyPatches.HealthBarForNonHealthBar))
            );
            _harmony.Patch(
                AccessTools.PropertySetter(typeof(CharacterMainControl), nameof(CharacterMainControl.CurrentWater)),
                postfix: new HarmonyMethod(typeof(CharacterMainControlPatches), nameof(CharacterMainControlPatches.SetCurrentWaterPostfix))
            );
            _harmony.Patch(
                AccessTools.PropertySetter(typeof(CharacterMainControl), nameof(CharacterMainControl.CurrentEnergy)),
                postfix: new HarmonyMethod(typeof(CharacterMainControlPatches), nameof(CharacterMainControlPatches.SetCurrentEnergyPostfix))
            );

            foreach (Duckov.Modding.ModBehaviour mod in s_mods)
            {
                mod.Setup(master, info);
            }
        }

        protected override void OnBeforeDeactivate()
        {
            _harmony.UnpatchAll("tinygrox.DuckovMods.NumericalStats");
            foreach (Duckov.Modding.ModBehaviour mod in s_mods)
            {
                mod.NotifyBeforeDeactivate();
            }
        }

    }
}
