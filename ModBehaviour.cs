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
            ModSettings.SetShowNumericalHealth(OptionsManager.Load("ShowNumericalHealth", true));
            ModSettings.SetShowShowEnemyName(OptionsManager.Load("ShowEnemyName", true));
            ModSettings.SetShowNumericalWaterAndEnergy(OptionsManager.Load("ShowNumericalWaterAndEnergy", true));
            ModSettings.SetShowArmourStats(OptionsManager.Load("ShowArmourStats", true));
        }
        private void Awake()
        {
            Debug.Log("[NumericalStats]ModBehaviour Awake");
            _harmony = new Harmony("tinygrox.DuckovMods.NumericalStats");
            LoadSettings();
            s_mods.Add(gameObject.AddComponent<ShowWaterAndEnergy>());
            s_mods.Add(gameObject.AddComponent<ArmourStatsDisplay>());
        }

        // private void LateUpdate()
        // {
        //     if (gameObject.TryGetComponent(out ShowWaterAndEnergy component))
        //     {
        //         component.enabled = ModSettings.ShowNumericalWaterAndEnergy;
        //     }
        // }

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
