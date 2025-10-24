using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Options.UI;
using Duckov.UI;
using HarmonyLib;
using SodaCraft.Localizations;
using tinygrox.DuckovMods.NumericalStats.OptionsProviders;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VLB;

namespace tinygrox.DuckovMods.NumericalStats
{
    public static class HarmonyPatches
    {
        public const string MyModSettingsButtonName = "NumericalStatsSettingsButton";
        // HealthBarManager.CreateHealthBarFor
        public static void HealthBarAddNumericalHealthDisplay(HealthBar __result)
        {
            if (__result.gameObject.TryGetComponent<NumericalHealthDisplay>(out _))
            {
                return;
            }
            __result.gameObject.AddComponent<NumericalHealthDisplay>();
        }
        // 添加我们的设置面板，难点不是添加，而是怎么创建一个新的 OptionsPanel_TabButton
        // 应该要在 OptionsPanel.Setup 进行 Patch
        public static void OptionsPanelAddMySettingPanel(OptionsPanel __instance, ref List<OptionsPanel_TabButton> ___tabButtons)
        {
            if (___tabButtons.Any(b => b != null && b.gameObject.name == MyModSettingsButtonName))
            {
                return;
            }
            Debug.Log("Adding my settings panel");
            ModSettings.CreateMySettingsTabButton(___tabButtons, out OptionsPanel_TabButton myTabButton);
            if (myTabButton is null)
            {
                Debug.Log("myTabButton is null");
                return;
            }
            myTabButton.gameObject.name = MyModSettingsButtonName;

            // 拿 Common
            var buttonContainer = ___tabButtons.FirstOrDefault()?.transform.parent;
            if(buttonContainer is null) return;
            myTabButton.transform.SetParent(buttonContainer.transform,false);
            myTabButton.transform.localScale = Vector3.one;

            // 初始化 onClicked 事件
            myTabButton.onClicked += (button, data) =>
            {
                data.Use();
                __instance.SetSelection(button);
            };
            var title = myTabButton.GetComponentInChildren<TextMeshProUGUI>();
            if (title)
            {
                title.text = LocalizationManager.GetPlainText("NumericalStats_ModOptionTitle");
            }

            var myTabPanel = ModSettings.GetTabButtonTabObjDelegate(myTabButton);
            if (myTabPanel is null)
            {
                Debug.Log("myTabPanel is null");
                return;
            }

            ModSettings.SetTabButtonTabObjDelegate(myTabButton, myTabPanel);
            myTabPanel.transform.SetParent(myTabButton.transform.parent.parent.Find("ScrollView/Viewport/Content"), false);
            CreateSimpleOption<NumericalVitalsOptionProvider>(myTabPanel.transform, "ShowNumericalWaterAndEnergy");
            CreateSimpleOption<HealthBarNumericalDisplayOptionProvider>(myTabPanel.transform, "ShowNumericalHealth");
            CreateSimpleOption<ShowEnemyNameOptionProvider>(myTabPanel.transform, "ShowEnemyName");
            CreateSimpleOption<ArmourStatsOptionProvider>(myTabPanel.transform, "ShowArmourStats");

            ___tabButtons.Add(myTabButton);
        }
        private static void CreateSimpleOption<TProvider>(Transform parent, string labelText) where TProvider: OptionsProviderBase
        {
            GameObject optionRow = ModSettings.GetOptionUIEntryInstance();

            GameObject tmpDropdownObj = optionRow?.transform.Find("Dropdown")?.gameObject;
            if(tmpDropdownObj is null) return;

            var img = tmpDropdownObj.transform.Find("Image");
            if (img)
            {
                img.gameObject.SetActive(false);
            }
            if (tmpDropdownObj.TryGetComponent(out TMP_Dropdown dropdown))
            {
                dropdown.options.Clear();
            }


            GameObject tmpTextMeshProUGUIObj = optionRow?.transform.Find("Label")?.gameObject;
            if(tmpTextMeshProUGUIObj is null) return;

            if (tmpTextMeshProUGUIObj.TryGetComponent(out TextMeshProUGUI tmpTextMeshProUGUI))
            {
                tmpTextMeshProUGUI.text = LocalizationManager.GetPlainText($"Options_{labelText}");
            }

            optionRow.transform.SetParent(parent, false);
            optionRow.transform.localScale = Vector3.one;
            optionRow.name = $"Option_{labelText.Replace(" ", "")}";
            var provider = optionRow.AddComponent<TProvider>();

            if (optionRow.TryGetComponent<OptionsUIEntry_Dropdown>(out var dropdownComp))
            {
                var setDropdown = ReflectionHelper.CreateFieldSetter<OptionsUIEntry_Dropdown, TMP_Dropdown>("dropdown");
                var setLabel = ReflectionHelper.CreateFieldSetter<OptionsUIEntry_Dropdown, TextMeshProUGUI>("label");
                var setProvider = ReflectionHelper.CreateFieldSetter<OptionsUIEntry_Dropdown, OptionsProviderBase>("provider");
                setProvider(dropdownComp, provider);
                setDropdown(dropdownComp, tmpDropdownObj.GetComponent<TMP_Dropdown>());
                setLabel(dropdownComp, tmpTextMeshProUGUI);
                dropdownComp.SendMessage("SetupDropdown");
            }
        }
    }
}
