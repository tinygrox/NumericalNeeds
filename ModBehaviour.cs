using System;
using System.Collections.Generic;
using Duckov.Options;
using Duckov.Options.UI;
using Duckov.UI;
using HarmonyLib;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class ModBehaviour: Duckov.Modding.ModBehaviour
    {
        private static readonly List<Duckov.Modding.ModBehaviour> s_mods = new List<Duckov.Modding.ModBehaviour>();
        private Harmony _harmony;

        private void LoadSettings()
        {
            ModSettings.SetShowNumericalHealth(OptionsManager.Load("ShowNumericalHealth", 1) == 1);
            ModSettings.SetShowShowEnemyName(OptionsManager.Load("ShowEnemyName", 1) == 1);
        }
        private void Awake()
        {
            _harmony = new Harmony("tinygrox.DuckovMods.NumericalStats");
            Debug.Log("[NumericalStats]ModBehaviour Awake");
            s_mods.Add(gameObject.AddComponent<ShowWaterAndEnergy>());
            s_mods.Add(gameObject.AddComponent<ArmourStatsDisplay>());
            LoadSettings();
            // ModSettings.ShowNumericalWaterAndEnergy = OptionsManager.Load("ShowNumericalHealth", 1) == 1;
        }

        private void LateUpdate()
        {
            if (gameObject.TryGetComponent(out ShowWaterAndEnergy component))
            {
                component.enabled = ModSettings.ShowNumericalWaterAndEnergy;
            }
        }

        protected override void OnAfterSetup()
        {
            _harmony.Patch(
                AccessTools.Method(typeof(HealthBarManager), "CreateHealthBarFor", new[] { typeof(Health), typeof(DamageInfo?) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.HealthBarAddNumericalHealthDisplay))
            );
            _harmony.Patch(
                AccessTools.Method(typeof(OptionsPanel), "Setup"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(HarmonyPatches.OptionsPanelAddMySettingPanel))
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
