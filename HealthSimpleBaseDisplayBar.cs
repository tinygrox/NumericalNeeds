using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.Utilities;
using FOW;
using UnityEngine;
using UnityEngine.Events;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class HealthSimpleBaseDisplayBar: MonoBehaviour
    {
        private HealthSimpleBase _healthSimpleBase;
        private Health _healthProxy;

        private static readonly Action<Health, float> s_setCurrentHealthDelegate = ReflectionHelper.CreateFieldSetter<Health, float>("_currentHealth");
        private static readonly Action<Health, int> s_setDefaultMaxHealthDelegate = ReflectionHelper.CreateFieldSetter<Health, int>("defaultMaxHealth");
        private static readonly Func<HealthBarManager, object[], PrefabPool<HealthBar>> s_getPrefabPoolMethod = ReflectionHelper.CreateMethodCaller<HealthBarManager, PrefabPool<HealthBar>>("get_PrefabPool");
        private static readonly Action<HealthBar, Gradient> s_setColorGradientDelegate = ReflectionHelper.CreateFieldSetter<HealthBar, Gradient>("colorOverAmount");

        private static readonly Func<HealthBarManager, PrefabPool<HealthBar>> s_getPrefabPoolProperty =
            instance => s_getPrefabPoolMethod(instance, null);

        private bool _isInitialized = false;
        private HealthBar _healthBarInstance;
        private CancellationTokenSource _hideHealthBarCts;
        private bool _isPermanentlyVisible = false;

        private float _healthBarDisplayDuration = 5f;

        private float _permanentDisplayThreshold = 0.3f;

        private void Awake()
        {
            _healthSimpleBase = GetComponent<HealthSimpleBase>();
            if (!_healthSimpleBase)
            {
                enabled = false;
            }
            _healthSimpleBase.OnHurtEvent += SyncHealthOnHurt;
            _healthSimpleBase.OnDeadEvent += SyncHealthOnDead;
        }

        private void Start()
        {
            InitializeWhenReady().Forget();
        }

        private async UniTask InitializeWhenReady()
        {
            await UniTask.WaitUntil(() => LevelManager.LevelInited && HealthBarManager.Instance != null, cancellationToken: this.GetCancellationTokenOnDestroy());
            if (!this || _isInitialized) return;
            // Debug.Log($"[{nameof(HealthSimpleBaseDisplayBar)}] Game is ready. Initializing health bar for {gameObject.name}");
            if(!_healthSimpleBase) return;

            if (!gameObject.TryGetComponent(out _healthProxy))
            {
                _healthProxy = gameObject.AddComponent<Health>();
            }

            s_setDefaultMaxHealthDelegate(_healthProxy, (int)_healthSimpleBase.maxHealthValue);
            s_setCurrentHealthDelegate(_healthProxy, _healthSimpleBase.HealthValue);

            _healthProxy.team = _healthSimpleBase.team;
            _healthProxy.hasSoul = false;
            _healthProxy.autoInit = false;
            _healthProxy.showHealthBar = true;

            _healthProxy.OnMaxHealthChange ??= new UnityEvent<Health>();
            _healthProxy.OnHealthChange ??= new UnityEvent<Health>();
            _healthProxy.OnHurtEvent ??= new UnityEvent<DamageInfo>();
            _healthProxy.OnDeadEvent ??= new UnityEvent<DamageInfo>();

            _healthProxy.RequestHealthBar();

            await UniTask.WaitForEndOfFrame(this);

            if(!this) return;

            _healthBarInstance = GetHealthBarForProxy(_healthProxy);

            if (_healthBarInstance)
            {
                _healthBarInstance.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);

                _healthBarInstance.gameObject.SetActive(false);
                // Debug.Log($"[{nameof(HealthSimpleBaseDisplayBar)}] HealthBar instance for {gameObject.name} found and hidden.");
            }
            else
            {
                Debug.LogWarning($"[{nameof(HealthSimpleBaseDisplayBar)}] Could not find HealthBar instance for {gameObject.name} after request.");
            }

            _isInitialized = true;
        }

        private HealthBar GetHealthBarForProxy(Health healthProxy)
        {
            if (HealthBarManager.Instance is null || healthProxy is null) return null;

            var pool = s_getPrefabPoolProperty(HealthBarManager.Instance);

            return pool?.ActiveEntries.FirstOrDefault(hb => hb.target == healthProxy);
        }

        private async UniTask HideHealthBarAfterDelay(CancellationToken cancellationToken)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_healthBarDisplayDuration), ignoreTimeScale: true, cancellationToken: cancellationToken);

                _healthBarInstance?.gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
            }
        }
        private void SyncHealthOnHurt(DamageInfo damageInfo)
        {
            if (!_healthProxy || !ModSettings.ShowHealthBarForObjects) return;

            s_setCurrentHealthDelegate(_healthProxy, _healthSimpleBase.HealthValue);

            _healthProxy.OnHealthChange?.Invoke(_healthProxy);
            _healthProxy.OnHurtEvent?.Invoke(damageInfo);
            if (_healthBarInstance is not null && !_healthBarInstance.gameObject.activeSelf)
            {
                _healthBarInstance.gameObject.SetActive(ModSettings.ShowHealthBarForObjects);
            }
            if (_isPermanentlyVisible) return;

            float healthPercentage = _healthSimpleBase.HealthValue / _healthSimpleBase.maxHealthValue;

            if (healthPercentage >= 0f && healthPercentage <= _permanentDisplayThreshold)
            {
                _isPermanentlyVisible = true;
                _hideHealthBarCts?.Cancel();
                // Debug.Log($"[{nameof(HealthSimpleBaseDisplayBar)}] Health is low ({healthPercentage * 100}%), health bar is now permanently visible for {gameObject.name}");
            }
            else
            {
                _hideHealthBarCts?.Cancel();
                if (_hideHealthBarCts == null || _hideHealthBarCts.IsCancellationRequested)
                {
                    _hideHealthBarCts?.Dispose();
                    _hideHealthBarCts = new CancellationTokenSource();
                }
                HideHealthBarAfterDelay(_hideHealthBarCts.Token).Forget();
            }
        }

        private void SyncHealthOnDead(DamageInfo damageInfo)
        {
            if(!_healthProxy) return;
            _hideHealthBarCts?.Cancel();
            // s_setCurrentHealthDelegate(_healthProxy, 0);
            _healthBarInstance = GetHealthBarForProxy(_healthProxy);
            _healthBarInstance?.gameObject.SetActive(false);
            _healthProxy.OnDeadEvent?.Invoke(damageInfo);
        }

        private void OnDestroy()
        {
            if (_healthSimpleBase)
            {
                _healthSimpleBase.OnHurtEvent -= SyncHealthOnHurt;
                _healthSimpleBase.OnDeadEvent -= SyncHealthOnDead;
            }
            _hideHealthBarCts?.Cancel();
            _hideHealthBarCts?.Dispose();
        }
    }
}
