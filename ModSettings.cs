using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Options;
using Duckov.Options.UI;
using Duckov.Utilities;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats
{
    public static class ModSettings
    {
        public static bool ShowNumericalHealth;

        public static bool ShowNumericalWaterAndEnergy;

        public static bool ShowArmourStats;

        public static bool ShowEnemyName;

        public static bool MustShowHealthBar;

        public static bool ShowHealthBarForObjects;

        public static bool EditUI;

        public static void SetEditUI(bool value)
        {
            SetBool(ref EditUI, value, v => OnEditUIChanged?.Invoke(v));
        }
        public static event Action<bool> OnEditUIChanged;
        public static event Action<bool> OnShowNumericalHealthChanged;
        public static event Action<bool> OnShowEnemyNameChanged;
        public static event Action<bool> OnShowNumericalWaterAndEnergyChanged;
        public static event Action<bool> OnShowArmourStatsChanged;
        public static event Action<bool> OnShowHealthBarForObjectsChanged;
        public static event Action<bool> OnMustShowHealthBarChanged;

        private static void SetBool(ref bool field, bool value, Action<bool> invoker)
        {
            if (field == value) return;
            field = value;
            invoker?.Invoke(value);
        }

        public static void SetShowNumericalHealth(bool value)
        {
            SetBool(ref ShowNumericalHealth, value, v => OnShowNumericalHealthChanged?.Invoke(v));
        }

        public static void SetShowShowEnemyName(bool value)
        {
            SetBool(ref ShowEnemyName, value, v => OnShowEnemyNameChanged?.Invoke(v));
        }

        public static void SetShowNumericalWaterAndEnergy(bool value)
        {
            SetBool(ref ShowNumericalWaterAndEnergy, value, v => OnShowNumericalWaterAndEnergyChanged?.Invoke(v));
        }

        public static void SetShowArmourStats(bool value)
        {
            SetBool(ref ShowArmourStats, value, v => OnShowArmourStatsChanged?.Invoke(v));
        }

        public static void SetMustShowHealthBar(bool value)
        {
            SetBool(ref MustShowHealthBar, value, v => OnMustShowHealthBarChanged?.Invoke(v));
        }
        public static void SetShowHealthBarForObjects(bool value)
        {
            SetBool(ref ShowHealthBarForObjects, value, v => OnShowHealthBarForObjectsChanged?.Invoke(v));
        }

        private static Vector2 s_waterDisplayPosition = new Vector2(-536f, -554f);

        public static Vector2 WaterDisplayPosition
        {
            get => s_waterDisplayPosition;
            set => SetVector2(ref s_waterDisplayPosition, value, OnWaterDisplayPositionChanged);
        }
        public static event Action<Vector2> OnWaterDisplayPositionChanged;
        private static Vector2 s_energyDisplayPosition = new Vector2(-421f, -554f);
        public static Vector2 EnergyDisplayPosition
        {
            get => s_energyDisplayPosition;
            private set => SetVector2(ref s_energyDisplayPosition, value, OnEnergyDisplayPositionChanged);
        }
        public static event Action<Vector2> OnEnergyDisplayPositionChanged;

        private static Vector2 s_armourStatsDisplayPosition = new Vector2(-775f, 90f);
        public static Vector2 ArmourStatsDisplayPosition
        {
            get => s_armourStatsDisplayPosition;
            private set => SetVector2(ref s_armourStatsDisplayPosition, value, OnArmourStatsDisplayPositionChanged);
        }
        public static event Action<Vector2> OnArmourStatsDisplayPositionChanged;

        private static void SetVector2(ref Vector2 field, Vector2 value, Action<Vector2> invoker)
        {
            if (field == value) return;
            field = value;
            invoker?.Invoke(value);
        }
        public static void SetArmourStatsDisplayPosition(Vector2 value)
        {
            SetVector2(ref s_armourStatsDisplayPosition, value, OnArmourStatsDisplayPositionChanged);
            // OptionsManager通常用于保存基本类型，这里将Vector2拆分为x和y
            OptionsManager.Save("NumericalStats_ArmourStatsDisplayPosX", value.x);
            OptionsManager.Save("NumericalStats_ArmourStatsDisplayPosY", value.y);
        }

        public static void SetWaterDisplayPosition(Vector2 value)
        {
            SetVector2(ref s_waterDisplayPosition, value, OnWaterDisplayPositionChanged);
            // OptionsManager通常用于保存基本类型，这里将Vector2拆分为x和y
            OptionsManager.Save("NumericalStats_WaterDisplayPosX", value.x);
            OptionsManager.Save("NumericalStats_WaterDisplayPosY", value.y);
        }

        public static void SetEnergyDisplayPosition(Vector2 value)
        {
            SetVector2(ref s_energyDisplayPosition, value, OnEnergyDisplayPositionChanged);
            OptionsManager.Save("NumericalStats_EnergyDisplayPosX", value.x);
            OptionsManager.Save("NumericalStats_EnergyDisplayPosY", value.y);
        }

        public static void LoadDisplayPositions()
        {
            // Debug.Log("[NumericalStats] 正在加载显示位置设置...");
            float waterPosX = OptionsManager.Load("NumericalStats_WaterDisplayPosX", s_waterDisplayPosition.x);
            float waterPosY = OptionsManager.Load("NumericalStats_WaterDisplayPosY", s_waterDisplayPosition.y);
            SetWaterDisplayPosition(new Vector2(waterPosX, waterPosY));
            // Debug.Log($"[NumericalStats] 水显示位置加载完成: ({waterPosX}, {waterPosY})");

            float energyPosX = OptionsManager.Load("NumericalStats_EnergyDisplayPosX", s_energyDisplayPosition.x);
            float energyPosY = OptionsManager.Load("NumericalStats_EnergyDisplayPosY", s_energyDisplayPosition.y);
            SetEnergyDisplayPosition(new Vector2(energyPosX, energyPosY));
            // Debug.Log($"[NumericalStats] 能量显示位置加载完成: ({energyPosX}, {energyPosY})");

            float armourStatsDisplayPosX = OptionsManager.Load("NumericalStats_ArmourStatsDisplayPosX", s_armourStatsDisplayPosition.x);
            float armourStatsDisplayPosY = OptionsManager.Load("NumericalStats_ArmourStatsDisplayPosY", s_armourStatsDisplayPosition.y);
            SetArmourStatsDisplayPosition(new Vector2(armourStatsDisplayPosX, armourStatsDisplayPosY));
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


                var newButtonObject = UnityEngine.Object.Instantiate(s_tabButtonPrefab);
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
            s_tabButtonPrefab = UnityEngine.Object.Instantiate(originalTabButtonObject, null, false);
            s_tabButtonPrefab.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(s_tabButtonPrefab);
            s_tabButtonPrefab.name = "NumericalStats_TabButton_Prefab";

            // 对现在这个常规设置文本小小的初始化一下
            if (s_tabButtonPrefab.TryGetComponent<TextMeshProUGUI>(out var text))
            {
                text.text = string.Empty;
            }

            var labelObject = s_tabButtonPrefab.transform.Find("Label")?.gameObject;
            if (labelObject is null) return;
            // 哈哈，这里直接设置 Mod 按钮的标题
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
                UnityEngine.Object.Destroy(s_tabButtonPrefab);
                s_tabButtonPrefab = null;
                return;
            }

            // 直接拿到设置的下方面板，别急着清空，我们还要从里面拿一些 UI 控件
            var myTabPanelObject = UnityEngine.Object.Instantiate(originalTabPanel, s_tabButtonPrefab.transform, true);
            myTabPanelObject.name = "NumericalStats_TabPanel";
            // 待会再杀光 myTabPanelObject 的 Children

            // 现在马上将原来的值指向我们新拿到的 GameObject
            var thisTabButton = s_tabButtonPrefab.GetComponent<OptionsPanel_TabButton>();
            SetTabButtonTabObjDelegate(thisTabButton, myTabPanelObject);

            // 3. 让我们继续拿，现在拿第一个 child：Canvas/MainMenuContainer/Menu/OptionsPanel/ScrollView/Viewport/Content/Common/UI_Language 这个 GameObject
            GameObject uiLanguageObject = myTabPanelObject.transform.GetChild(0)?.gameObject;
            if (!(uiLanguageObject is null))
            {
                s_optionUIEntryObject = UnityEngine.Object.Instantiate(uiLanguageObject, null, true);
                UnityEngine.Object.DontDestroyOnLoad(s_optionUIEntryObject);
                s_optionUIEntryObject.SetActive(false);
                s_optionUIEntryObject.name = "NumericalStats_OptionEntry_Prefab";

                // 开始拿 OptionsUIEntry_Dropdown
                if (s_optionUIEntryObject.TryGetComponent<OptionsUIEntry_Dropdown>(out var myDropdownComp))
                {
                    // 创建一个获取 OptionsUIEntry_Dropdown.dropdown 的委托
                    var getObjectDelegate = ReflectionHelper.CreateFieldGetter<OptionsUIEntry_Dropdown, TMP_Dropdown>("dropdown");
                    TMP_Dropdown thisTMPDropdown = getObjectDelegate(myDropdownComp);
                    s_optionUIDropDown = UnityEngine.Object.Instantiate(thisTMPDropdown.gameObject, null, true);
                    s_optionUIDropDown.SetActive(false);
                    s_optionUIDropDown.name = "NumericalStats_OptionEntry_Dropdown";
                    // 那个 Image 先关掉
                    var img = s_optionUIDropDown.transform.Find("Image");
                    img.gameObject.SetActive(false);

                    UnityEngine.Object.DontDestroyOnLoad(s_optionUIDropDown); // 用来复制的，你不能死
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
                    UnityEngine.Object.DestroyImmediate(compLanguageOptionsProvider);
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

            var instance = UnityEngine.Object.Instantiate(s_optionUIEntryObject);
            instance.SetActive(true);
            return instance;
        }

        // 虽然忘记用了
        public static GameObject GetOptionUiDropDown()
        {
            if (s_optionUIDropDown is null)
            {
                Debug.LogError("[NumericalStats] OptionUIEntry prefab is null.");
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(s_optionUIDropDown);
            instance.SetActive(true);
            return instance;
        }
    }
}
