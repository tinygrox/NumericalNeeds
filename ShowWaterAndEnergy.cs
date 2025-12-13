using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using tinygrox.DuckovMods.NumericalStats.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class ShowWaterAndEnergy : Duckov.Modding.ModBehaviour
    {
        public static ShowWaterAndEnergy Instance { get; private set; }

        public event Action OnWaterStatsChanged;
        public event Action OnEnergyStatsChanged;

        private CharacterMainControl _characterMainControl;
        private TextMeshProUGUI _waterCurrentText;
        private TextMeshProUGUI _energyCurrentText;
        private TextMeshProUGUI _waterMaxText;
        private TextMeshProUGUI _energyMaxText;
        private TextMeshProUGUI _waterslashText;
        private TextMeshProUGUI _energyslashText;

        private GameObject _waterContainerGo;
        private GameObject _energyContainerGo;

        private DraggableUIElement _waterDraggable;
        private DraggableUIElement _energyDraggable;

        // private CancellationTokenSource _cts;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void AddTextToHUD()
        {
            // _cts?.Cancel();
            // _cts?.Dispose();
            // _cts = new CancellationTokenSource();

            (_waterCurrentText, _waterslashText, _waterMaxText, _waterContainerGo, _waterDraggable) = SetupStatDisplay<WaterHUD>(
                hud => hud.backgroundImage.transform,
                "NumericalNeeds_Water_Container",
                ModSettings.WaterDisplayPosition,
                ModSettings.SetWaterDisplayPosition
            );
            (_energyCurrentText, _energyslashText, _energyMaxText, _energyContainerGo, _energyDraggable) = SetupStatDisplay<EnergyHUD>(
                hud => hud.backgroundImage.transform,
                "NumericalNeeds_Energy_Container",
                ModSettings.EnergyDisplayPosition,
                ModSettings.SetEnergyDisplayPosition
            );

            OnSettingsChanged(ModSettings.ShowNumericalWaterAndEnergy);
            RefreshStatAsync().Forget();
        }

        private async UniTaskVoid RefreshStatAsync()
        {
            try
            {
                await UniTask.WaitUntil(() => LevelManager.Instance?.MainCharacter is not null);
                _characterMainControl = LevelManager.Instance.MainCharacter;

                // while (!token.IsCancellationRequested)
                // {
                //     FreshStat();
                //     await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                // }
                RefreshWaterStat();
                RefreshEnergyStat();
            }
            catch (OperationCanceledException)
            {
                // 啥也不干
            }
        }

        private void OnSettingsChanged(bool value)
        {
            _waterContainerGo?.SetActive(value);
            _energyContainerGo?.SetActive(value);
            if (value)
            {
                RefreshWaterStat();
                RefreshEnergyStat();
            }
        }

        // private void FreshStat()
        // {
        //     if (LevelManager.Instance?.MainCharacter is null || !ModSettings.ShowNumericalWaterAndEnergy) return;
        //
        //     _characterMainControl ??= LevelManager.Instance.MainCharacter;
        //
        //     if (!(_waterCurrentText is null) && !(_waterMaxText is null))
        //     {
        //         float currentWater = _characterMainControl.CurrentWater;
        //         float maxWater = _characterMainControl.MaxWater;
        //         _waterCurrentText.SetText(Mathf.Approximately(currentWater, maxWater) ? "{0:0}" : "{0:1}", currentWater);
        //         _waterMaxText.SetText("{0:0}",maxWater);
        //     }
        //
        //     if (!(_energyCurrentText is null) && !(_energyMaxText is null))
        //     {
        //         float currentEnergy = _characterMainControl.CurrentEnergy;
        //         float maxEnergy = _characterMainControl.MaxEnergy;
        //         _energyCurrentText.SetText(Mathf.Approximately(currentEnergy, maxEnergy) ? "{0:0}" : "{0:1}", currentEnergy);
        //         _energyMaxText.SetText("{0:0}", maxEnergy);
        //     }
        // }

        private void RefreshWaterStat()
        {
            if (_characterMainControl is null || !ModSettings.ShowNumericalWaterAndEnergy) return;

            if (!(_waterCurrentText is null) && !(_waterMaxText is null))
            {
                float currentWater = _characterMainControl.CurrentWater;
                float maxWater = _characterMainControl.MaxWater;
                _waterCurrentText.SetText(Mathf.Approximately(currentWater, maxWater) ? "{0:0}" : "{0:1}", currentWater);
                _waterMaxText.SetText("{0:0}", maxWater);
            }
        }

        private void RefreshEnergyStat()
        {
            if (_characterMainControl is null || !ModSettings.ShowNumericalWaterAndEnergy) return;

            if (!(_energyCurrentText is null) && !(_energyMaxText is null))
            {
                float currentEnergy = _characterMainControl.CurrentEnergy;
                float maxEnergy = _characterMainControl.MaxEnergy;
                _energyCurrentText.SetText(Mathf.Approximately(currentEnergy, maxEnergy) ? "{0:0}" : "{0:1}", currentEnergy);
                _energyMaxText.SetText("{0:0}", maxEnergy);
            }
        }

        private void OnEnable()
        {
            LevelManager.OnAfterLevelInitialized += AddTextToHUD;
            ModSettings.OnShowNumericalWaterAndEnergyChanged += OnSettingsChanged;
            OnWaterStatsChanged += RefreshWaterStat;
            OnEnergyStatsChanged += RefreshEnergyStat;
            OnSettingsChanged(ModSettings.ShowNumericalWaterAndEnergy);
        }

        private void OnDisable()
        {
            LevelManager.OnAfterLevelInitialized -= AddTextToHUD;
            ModSettings.OnShowNumericalWaterAndEnergyChanged -= OnSettingsChanged;
            OnWaterStatsChanged -= RefreshWaterStat;
            OnEnergyStatsChanged -= RefreshEnergyStat;
            // _cts?.Cancel();
            // _cts?.Dispose();
            // _cts = null;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private (TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI, GameObject, DraggableUIElement) SetupStatDisplay<T>(
            Func<T, Transform> getTextParentFromHUD,
            string containerName,
            Vector2 initialPosition,
            DraggableUIElement.SavePositionDelegate savePositionDelegate) where T : MonoBehaviour
        {
            T hudInstance = LevelManager.Instance.gameObject.GetComponentInChildren<T>(); // FindAnyObjectByType<T>();
            if (hudInstance is null)
            {
                Debug.LogError($"[NumericalNeeds]: No {typeof(T).Name} instance found!");
                return (null, null, null, null, null);
            }

            Transform hudElementTransform = getTextParentFromHUD(hudInstance);

            var containerGo = new GameObject(containerName);
            containerGo.transform.SetParent(hudElementTransform.parent.parent); // 抛弃两个傻逼 HUD 的 gameobject，自立门户
            var containerRect = containerGo.AddComponent<RectTransform>();
            containerGo.transform.localScale = Vector3.one;

            // containerRect.anchorMin = new Vector2(0.5f, 1f);
            // containerRect.anchorMax = new Vector2(0.5f, 1f);
            // containerRect.pivot = new Vector2(0.5f, 0f);

            // containerRect.anchoredPosition = new Vector2(0f, 5f);
            // containerRect.sizeDelta = new Vector2(100f, 32f);
            containerRect.anchoredPosition = initialPosition;

            var layoutGroup = containerGo.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.spacing = 0f;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);

            var contentFitter = containerGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            // contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            DraggableUIElement draggable = containerGo.AddComponent<DraggableUIElement>();
            draggable.Initialize(initialPosition);
            draggable.OnSavePositionRequested = savePositionDelegate;

            TextMeshProUGUI currentText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            currentText.gameObject.name = "CurrentValueText";
            currentText.alignment = TextAlignmentOptions.Right;
            currentText.fontSizeMin = 16f;
            currentText.fontSizeMax = 20f;
            currentText.enableAutoSizing = true;

            LayoutElement currentLayoutElement = currentText.gameObject.AddComponent<LayoutElement>();
            currentLayoutElement.flexibleWidth = 0; // 不允许弹性扩展
            currentLayoutElement.minWidth = 45f;
            currentLayoutElement.preferredWidth = 45f;
            currentLayoutElement.minHeight = 0;

            TextMeshProUGUI slashText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            slashText.gameObject.name = "SlashText";
            slashText.alignment = TextAlignmentOptions.Center;
            slashText.SetText("/");
            slashText.fontSizeMin = 12f;
            slashText.fontSizeMax = 18f;
            // slashText.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 100); // 给斜杠一个固定窄宽度
            slashText.enableAutoSizing = true;

            LayoutElement slashLayoutElement = slashText.gameObject.AddComponent<LayoutElement>();
            slashLayoutElement.flexibleWidth = 0;
            slashLayoutElement.minWidth = 15f;
            slashLayoutElement.preferredWidth = 15f;
            slashLayoutElement.minHeight = 0;

            TextMeshProUGUI maxText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            maxText.gameObject.name = "MaxValueText";
            maxText.alignment = TextAlignmentOptions.Left;
            maxText.fontSizeMin = 16f;
            maxText.fontSizeMax = 20f;
            maxText.enableAutoSizing = true;
            LayoutElement maxLayoutElement = maxText.gameObject.AddComponent<LayoutElement>();
            maxLayoutElement.flexibleWidth = 0; // 不允许弹性扩展
            maxLayoutElement.minWidth = 35f;
            maxLayoutElement.preferredWidth = 35f;
            maxLayoutElement.minHeight = 0;

            return (currentText, slashText, maxText, containerGo, draggable);
        }

        public void OnWaterStatsInvoke()
        {
            OnWaterStatsChanged?.Invoke();
        }

        public void OnEnergyStatsInvoke()
        {
            OnEnergyStatsChanged?.Invoke();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
