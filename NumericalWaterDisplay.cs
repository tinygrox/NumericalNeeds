using System;
using Duckov.UI;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class NumericalWaterDisplay: MonoBehaviour
    {
        private CharacterMainControl _characterMainControl;
        private TextMeshProUGUI _waterCurrentText;
        private TextMeshProUGUI _waterMaxText;
        private WaterHUD _waterHUD;

        private void Awake()
        {
            _waterHUD ??= GetComponent<WaterHUD>();
            if (_waterHUD is null)
            {
                Debug.LogError("[NumericalWaterDisplay] No WaterHUD component found on the GameObject.");
            }
            else
            {
                SetupValueText();
            }
        }

        private void SetupValueText()
        {
            var containerGo = new GameObject("NumericalWaterHUD_Container");
            containerGo.transform.SetParent(transform, false);
            var containerRect = containerGo.AddComponent<RectTransform>();
            containerGo.transform.localScale = Vector3.one;

            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.anchoredPosition = new Vector2(0f, 5f);

            var layoutGroup = containerGo.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.spacing = -2f;

            var contentFitter = containerGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            _waterCurrentText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            _waterCurrentText.gameObject.name = "CurrentValueText";
            _waterCurrentText.alignment = TextAlignmentOptions.Right;
            _waterCurrentText.fontSize = 20f;
            _waterCurrentText.gameObject.SetActive(true);

            TextMeshProUGUI slashText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            slashText.gameObject.name = "SlashText";
            slashText.alignment = TextAlignmentOptions.Center;
            slashText.text = "/";
            slashText.fontSize = 20f;
            slashText.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 100); // 给斜杠一个固定窄宽度
            slashText.gameObject.SetActive(true);

            _waterMaxText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            _waterMaxText.gameObject.name = "MaxValueText";
            _waterMaxText.alignment = TextAlignmentOptions.Left;
            _waterMaxText.fontSize = 20f;
            _waterMaxText.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (LevelManager.Instance?.MainCharacter is null) return;

            _characterMainControl ??= LevelManager.Instance.MainCharacter;

            if (!(_waterCurrentText is null) && !(_waterMaxText is null))
            {
                float currentWater = _characterMainControl.CurrentWater;
                float maxWater = _characterMainControl.MaxWater;
                _waterCurrentText.text = $"{currentWater:F1}";
                _waterMaxText.text = $"{maxWater:F0}";
            }
        }
    }
}
