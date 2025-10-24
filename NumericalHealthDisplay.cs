using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Duckov.Options;
using Duckov.UI;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace tinygrox.DuckovMods.NumericalStats
{
    // 这个是直接挂载在 gameObject 上面的脚本，直接用 MonoBehaviour 就好
    // 原理就是通过 Harmony 给每个挂载了 Healthbar 的 gameObject 都挂载此脚本，然后单独自动更新
    public class NumericalHealthDisplay: MonoBehaviour
    {
        private TextMeshProUGUI _valueText;
        private HealthBar _healthBar;
        private TextMeshProUGUI _nameText;
        private CharacterRandomPreset _characterRandomPreset;
        private Health _currentTarget;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            SetupValueText();
        }

        // 因为是在运行时通过 Harmony patch 添加的，所以不能保证所需的内容已经全部加载完毕
        private async UniTaskVoid WaiteForbarInitAsync(CancellationToken token)
        {
            try
            {
                await UniTask.WaitUntil(() => _healthBar.target != null, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[NumericalHealthDisplay] No waiting for the target.");
                return;
            }
            catch (TimeoutException)
            {
                Debug.Log("[NumericalHealthDisplay] Timeout for the target.");
                return;
            }

            if (token.IsCancellationRequested) return;

            _currentTarget = _healthBar.target;

            if (_currentTarget is null)
            {
                return;
            }

            if (!_currentTarget.IsDead)
            {
                _currentTarget.OnHealthChange.RemoveListener(UpdateHealthText);
                _currentTarget.OnHealthChange.AddListener(UpdateHealthText);

                UpdateHealthText();

                // TrySetNameText();
                GetCharaterDisplayName();
                ModSettings.OnShowNumericalHealthChanged -= SetValueTextActiveSelfToValue;
                ModSettings.OnShowNumericalHealthChanged += SetValueTextActiveSelfToValue;

                ModSettings.OnShowEnemyNameChanged -= SetNameChangedTextActiveSelfToValue;
                ModSettings.OnShowEnemyNameChanged += SetNameChangedTextActiveSelfToValue;
            }
        }
        private void OnEnable()
        {
            _healthBar ??= GetComponent<HealthBar>();
            if (_healthBar is null)
            {
                enabled = false;
                Debug.Log("[NumericalHealthDisplay] No HealthBar component found on the GameObject.");
                return;
            }

            _cts = new CancellationTokenSource();
            WaiteForbarInitAsync(_cts.Token).Forget();
            SetValueTextActiveSelfToValue(ModSettings.ShowNumericalHealth);
            SetNameChangedTextActiveSelfToValue(ModSettings.ShowEnemyName);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            ModSettings.OnShowNumericalHealthChanged -= SetValueTextActiveSelfToValue;
            ModSettings.OnShowEnemyNameChanged -= SetNameChangedTextActiveSelfToValue;

            if (_currentTarget is null || _currentTarget.IsDead || _currentTarget.IsMainCharacterHealth)
            {
                Debug.Log($"[NumericalHealthDisplayOnDestroy] _currentTarget is null({_currentTarget is null}) or _currentTarget.IsDead({_currentTarget?.IsDead}) or _currentTarget.IsMainCharacterHealth({_currentTarget?.IsMainCharacterHealth}).");
                return;
            }

            _currentTarget.OnHealthChange.RemoveListener(UpdateHealthText);
        }
        private void GetCharaterDisplayName()
        {
            _characterRandomPreset = _healthBar.target.TryGetCharacter()?.characterPreset;

            _nameText ??= GetComponentInChildren<TextMeshProUGUI>(true);
            if (_nameText is null)
            {
                Debug.Log("[NumericalHealthDisplay] No NameText component found on the GameObject.");
            }
            else if (!(_characterRandomPreset is null))
            {
                if (_characterRandomPreset.showName) return;
                // Debug.Log($"[NumericalHealthDisplay] NameText:{_nameText.name}:{_characterRandomPreset.DisplayName}!");
                _nameText.SetText(_characterRandomPreset.DisplayName);
                // _nameText.gameObject.transform.parent.gameObject.SetActive(true);
                _nameText.gameObject.SetActive(ModSettings.ShowEnemyName);
            }
        }

        private void SetupValueText()
        {
            var containerGo = new GameObject("NumericalHealthBar_Container");
            containerGo.transform.SetParent(transform, false);
            var containerRect = containerGo.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;

            // containerRect.sizeDelta = new Vector2(100f, 20f);
            _valueText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, containerGo.transform);
            _valueText.gameObject.name = "ValueText";
            _valueText.alignment = TextAlignmentOptions.Center;
            _valueText.fontSizeMin = 13f;
            _valueText.fontSizeMax = 16f;
            _valueText.enableAutoSizing = true;
            _valueText.fontStyle = FontStyles.Bold;
            _valueText.enableWordWrapping = false;
            // _valueText.gameObject.SetActive(true);
        }

        private void SetValueTextActiveSelfToValue(bool value)
        {
            _valueText?.gameObject.SetActive(value);
        }

        private void SetNameChangedTextActiveSelfToValue(bool value)
        {
            if (_nameText is null || _currentTarget is null || _currentTarget.IsMainCharacterHealth || _currentTarget.IsDead || _currentTarget.TryGetCharacter().characterPreset.showName) return;
            _nameText.gameObject.SetActive(value);
        }
        private void UpdateHealthText()
        {
            if (_currentTarget is null)
            {
                return;
            }

            float currentHealth = Mathf.CeilToInt(_currentTarget.CurrentHealth);
            float maxHealth = Mathf.CeilToInt(_currentTarget.MaxHealth);
            _valueText.SetText("{0}/{1}", currentHealth, maxHealth);
        }

        private void UpdateHealthText(Health health)
        {
            UpdateHealthText();
        }
    }
}
