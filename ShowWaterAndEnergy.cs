using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Duckov.Options;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class ShowWaterAndEnergy : Duckov.Modding.ModBehaviour
    {
        private CharacterMainControl _characterMainControl;
        private TextMeshProUGUI _waterCurrentText;
        private TextMeshProUGUI _energyCurrentText;
        private TextMeshProUGUI _waterMaxText;
        private TextMeshProUGUI _energyMaxText;
        private TextMeshProUGUI _waterslashText;
        private TextMeshProUGUI _energyslashText;

        private CancellationTokenSource _cts;

        private void AddTextToHUD()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            (_waterCurrentText, _waterslashText, _waterMaxText) = SetupStatDisplay<WaterHUD>(
                hud => hud.backgroundImage.transform,
                "NumericalNeeds_Water_Container"
            );
            (_energyCurrentText, _energyslashText, _energyMaxText) = SetupStatDisplay<EnergyHUD>(
                hud => hud.backgroundImage.transform,
                "NumericalNeeds_Energy_Container"
            );

            OnSettingsChanged(ModSettings.ShowNumericalWaterAndEnergy);
            FreshStatLoopAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid FreshStatLoopAsync(CancellationToken token)
        {
            try
            {
                await UniTask.WaitUntil(() => LevelManager.Instance?.MainCharacter != null, cancellationToken: token);
                _characterMainControl = LevelManager.Instance.MainCharacter;

                while (!token.IsCancellationRequested)
                {
                    FreshStat();
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnSettingsChanged(bool value)
        {
            SetTextMeshProUGUIActiveStatus(_waterCurrentText, value);
            SetTextMeshProUGUIActiveStatus(_waterslashText, value);
            SetTextMeshProUGUIActiveStatus(_waterMaxText, value);

            SetTextMeshProUGUIActiveStatus(_energyCurrentText, value);
            SetTextMeshProUGUIActiveStatus(_energyslashText, value);
            SetTextMeshProUGUIActiveStatus(_energyMaxText, value);
        }

        private void SetTextMeshProUGUIActiveStatus(TextMeshProUGUI textMeshProUGUI, bool value)
        {
            if (textMeshProUGUI)
                textMeshProUGUI.gameObject.SetActive(value);
        }

        private void FreshStat()
        {
            if (LevelManager.Instance?.MainCharacter is null || !ModSettings.ShowNumericalWaterAndEnergy) return;

            _characterMainControl ??= LevelManager.Instance.MainCharacter;

            if (!(_waterCurrentText is null) && !(_waterMaxText is null))
            {
                float currentWater = _characterMainControl.CurrentWater;
                float maxWater = _characterMainControl.MaxWater;
                _waterCurrentText.SetText(Mathf.Approximately(currentWater, maxWater) ? "{0:0}" : "{0:1}", currentWater);
                _waterMaxText.SetText("{0:0}",maxWater);
            }

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
            LevelManager.OnLevelInitialized += AddTextToHUD;
            ModSettings.OnShowNumericalWaterAndEnergyChanged += OnSettingsChanged;
            OnSettingsChanged(ModSettings.ShowNumericalWaterAndEnergy);
        }

        private void OnDisable()
        {
            LevelManager.OnLevelInitialized -= AddTextToHUD;
            ModSettings.OnShowNumericalWaterAndEnergyChanged -= OnSettingsChanged;
            // OnSettingsChanged(ModSettings.ShowNumericalWaterAndEnergy);
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private (TextMeshProUGUI, TextMeshProUGUI, TextMeshProUGUI) SetupStatDisplay<T>(Func<T, Transform> getTextParentFromHUD, string containerName) where T : MonoBehaviour
        {
            T hudInstance = FindObjectOfType<T>();
            if (hudInstance is null)
            {
                Debug.LogError($"NumericalNeeds Mod: No {typeof(T).Name} instance found!");
                return (null, null, null);
            }

            Transform hudElementTransform = getTextParentFromHUD(hudInstance);

            var containerGo = new GameObject(containerName);
            containerGo.transform.SetParent(hudElementTransform);
            var containerRect = containerGo.AddComponent<RectTransform>();
            containerGo.transform.localScale = Vector3.one;

            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 5f);
            containerRect.sizeDelta = new Vector2(100f, 32f);

            var layoutGroup = containerGo.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.spacing = 0f;

            var contentFitter = containerGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            TextMeshProUGUI currentText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            currentText.gameObject.name = "CurrentValueText";
            currentText.alignment = TextAlignmentOptions.Right;
            currentText.fontSizeMin = 20f;
            currentText.fontSizeMax = 32f;
            currentText.enableAutoSizing = true;

            TextMeshProUGUI slashText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            slashText.gameObject.name = "SlashText";
            slashText.alignment = TextAlignmentOptions.Center;
            slashText.SetText("/");
            slashText.fontSizeMin = 16f;
            slashText.fontSizeMax = 32f;
            slashText.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 100); // 给斜杠一个固定窄宽度
            slashText.enableAutoSizing = true;

            TextMeshProUGUI maxText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            maxText.gameObject.name = "MaxValueText";
            maxText.alignment = TextAlignmentOptions.Left;
            maxText.fontSizeMin = 20f;
            maxText.fontSizeMax = 32f;
            maxText.enableAutoSizing = true;

            return (currentText, slashText, maxText);
        }
    }
}
