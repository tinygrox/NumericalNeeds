using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Options;
using Duckov.Options.UI;
using Duckov.Utilities;
using HarmonyLib;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace tinygrox.DuckovMods.NumericalStats
{
    public static class ModSettings
    {
        public static bool ShowNumericalHealth;

        public static bool ShowNumericalWaterAndEnergy;

        public static bool ShowArmourStats;

        public static bool ShowEnemyName;

        public static event Action<bool> OnShowNumericalHealthChanged;
        public static event Action<bool> OnShowEnemyName;

        public static void SetShowNumericalHealth(bool value)
        {
            if (ShowNumericalHealth == value) return;

            ShowNumericalHealth = value;
            OnShowNumericalHealthChanged?.Invoke(value);
        }

        public static void SetShowShowEnemyName(bool value)
        {
            if (ShowEnemyName == value) return;

            ShowEnemyName = value;
            OnShowEnemyName?.Invoke(value);
        }

        // 这个缓存后就可以一直 Instantiate
        private static GameObject s_tabButtonPrefab;

        private static GameObject s_optionUIEntryObject;

        private static GameObject s_optionUIDropDown;

        public static Func<OptionsPanel_TabButton, GameObject> GetTabButtonTabObjDelegate =
            ReflectionHelper.CreateFieldGetter<OptionsPanel_TabButton, GameObject>("tab");
        public static Action<OptionsPanel_TabButton, GameObject> SetTabButtonTabObjDelegate =
            ReflectionHelper.CreateFieldSetter<OptionsPanel_TabButton, GameObject>("tab");

        public static void CreateMySettingsTabButton(List<OptionsPanel_TabButton> originalTabButtons, out OptionsPanel_TabButton resultButton)
        {
            resultButton = null;
            try
            {
                if (s_tabButtonPrefab is null)
                {
                    InitializePrefabs(originalTabButtons);
                    if (s_tabButtonPrefab is null)
                    {
                        Debug.LogError("[NumericalStats] TabButton prefab is null. Cannot create settings tab.");
                        return;
                    }
                }


                var newButtonObject = Object.Instantiate(s_tabButtonPrefab);
                newButtonObject.name = "MyModSettings_TabButton";
                newButtonObject.SetActive(true);
                resultButton = newButtonObject.GetComponent<OptionsPanel_TabButton>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NumericalStats] An exception occurred while creating the settings tab button: {ex}");
            }
        }
        // s_tabButtonPrefab = "常规设置" | s_optionUIEntryObject = "UI_Language" | s_optionUIDropDown = "Dropdown"
        private static void InitializePrefabs(List<OptionsPanel_TabButton> originalOptionsPanelTabButtons)
        {
            GetTabButtonTabObjDelegate ??= ReflectionHelper.CreateFieldGetter<OptionsPanel_TabButton, GameObject>("tab");

            Debug.Log("[NumericalStats] Initializing UI prefabs for settings tab...");

            if (originalOptionsPanelTabButtons == null || originalOptionsPanelTabButtons.Count == 0)
            {
                Debug.LogError("[NumericalStats] Original tab buttons list is empty.");
                return;
            }
            // “常规”！
            OptionsPanel_TabButton originalTabButtonComp = originalOptionsPanelTabButtons.FirstOrDefault();
            if (originalTabButtonComp is null)
            {
                Debug.LogError("[NumericalStats] Failed to get a template tab button.");
                return;
            }

            GameObject originalTabButtonObject = originalTabButtonComp.gameObject;
            // originalTabButtonObject 就是游戏的“常规设置”按钮

            // 1. 直接 Instantiate 拿到属于我们自己的“常规设置”
            s_tabButtonPrefab = Object.Instantiate(originalTabButtonObject, null, false);
            s_tabButtonPrefab.SetActive(false);
            Object.DontDestroyOnLoad(s_tabButtonPrefab);
            s_tabButtonPrefab.name = "NumericalStats_TabButton_Prefab";

            // 对现在这个常规设置文本小小的初始化一下
            if (s_tabButtonPrefab.TryGetComponent<TextMeshProUGUI>(out var text))
            {
                text.text = string.Empty;
            }

            var labelObject = s_tabButtonPrefab.transform.Find("Label")?.gameObject;
            if (labelObject is null) return;
            if(labelObject.TryGetComponent(out TextLocalizor t))
            {
                t.Key = "NumericalStats_ModOptionTitle";
                // Object.DestroyImmediate(t);
            }

            // 这样就可以了接下来搞定的是 OptionsPanel_TabButton.tab 指定的 gameobject
            // 2. 先从原版拿到 OptionsPanel_TabButton.tab 这个 GameObject，名称应该是 Common
            // 没有就玉石俱焚，我还玩尼玛
            GameObject originalTabPanel = GetTabButtonTabObjDelegate(originalTabButtonComp);
            if (originalTabPanel is null)
            {
                Debug.LogError("[NumericalStats] Failed to get original tab panel via reflection.");
                Object.Destroy(s_tabButtonPrefab);
                s_tabButtonPrefab = null;
                return;
            }

            // 直接拿到设置的下方面板，别急着清空，我们还要从里面拿一些 UI 控件
            var myTabPanelObject = Object.Instantiate(originalTabPanel, s_tabButtonPrefab.transform, true);
            myTabPanelObject.name = "NumericalStats_TabPanel";
            // 待会再杀光 myTabPanelObject 的 Children

            // 现在马上将原来的值指向我们新拿到的 GameObject
            var thisTabButton = s_tabButtonPrefab.GetComponent<OptionsPanel_TabButton>();
            SetTabButtonTabObjDelegate(thisTabButton, myTabPanelObject);

            // 3. 让我们继续拿，现在拿第一个 child：Canvas/MainMenuContainer/Menu/OptionsPanel/ScrollView/Viewport/Content/Common/UI_Language 这个 GameObject
            GameObject uiLanguageObject = myTabPanelObject.transform.GetChild(0)?.gameObject;
            if (!(uiLanguageObject is null))
            {
                s_optionUIEntryObject = Object.Instantiate(uiLanguageObject, null, true);
                Object.DontDestroyOnLoad(s_optionUIEntryObject);
                s_optionUIEntryObject.SetActive(false);
                s_optionUIEntryObject.name = "NumericalStats_OptionEntry_Prefab";

                // 开始拿 OptionsUIEntry_Dropdown
                if (s_optionUIEntryObject.TryGetComponent<OptionsUIEntry_Dropdown>(out var myDropdownComp))
                {
                    var getObjectDelegate = ReflectionHelper.CreateFieldGetter<OptionsUIEntry_Dropdown, TMP_Dropdown>("dropdown");
                    TMP_Dropdown thisTMPDropdown = getObjectDelegate(myDropdownComp);
                    s_optionUIDropDown = Object.Instantiate(thisTMPDropdown.gameObject, null, true);
                    s_optionUIDropDown.SetActive(false);
                    s_optionUIDropDown.name = "NumericalStats_OptionEntry_Dropdown";
                    Object.DontDestroyOnLoad(s_optionUIDropDown);
                    thisTMPDropdown.options.Clear();
                }

                if (s_optionUIEntryObject.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
                {
                    textMeshProUGUI.text = string.Empty;
                }

                // 清理这个选项条目预制件
                // s_optionUIEntryObject.transform.DestroyAllChildren();
                if (s_optionUIEntryObject.TryGetComponent<LanguageOptionsProvider>(out var compLanguageOptionsProvider))
                {
                    Object.DestroyImmediate(compLanguageOptionsProvider);
                }
            }
            else
            {
                Debug.LogWarning("[NumericalStats] Could not find a template option entry to create a prefab from.");
            }

            // 4. 最后，杀光一个不留
            myTabPanelObject.transform.DestroyAllChildren();

            Debug.Log("[NumericalStats] UI prefabs initialized successfully.");
        }

        public static GameObject GetOptionUIEntryInstance()
        {
            if (s_optionUIEntryObject is null)
            {
                Debug.LogError("[NumericalStats] OptionUIEntry prefab is null.");
                return null;
            }

            var instance = Object.Instantiate(s_optionUIEntryObject);
            instance.SetActive(true);
            return instance;
        }

        public static GameObject GetOptionUiDropDown()
        {
            if (s_optionUIDropDown is null)
            {
                Debug.LogError("[NumericalStats] OptionUIEntry prefab is null.");
                return null;
            }

            var instance = Object.Instantiate(s_optionUIDropDown);
            instance.SetActive(true);
            return instance;
        }
    }
}
