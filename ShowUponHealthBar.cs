using System;
using System.Collections;
using System.Reflection;
using Duckov.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class ShowUponHealthBar: Duckov.Modding.ModBehaviour
    {
        private static Func<Health, HealthBar> s_getActiveHealthBar;

        public static Func<Health, HealthBar> GetActiveHealthBar
        {
            get
            {
                if (s_getActiveHealthBar == null)
                {
                    HealthBarManager instance = HealthBarManager.Instance;

                    if (instance is null)
                    {
                        return null;
                    }

                    var methodInfo = typeof(HealthBarManager).GetMethod("GetActiveHealthBar", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (methodInfo is null)
                    {
                        return null;
                    }
                    s_getActiveHealthBar = (Func<Health, HealthBar>)Delegate.CreateDelegate(typeof(Func<Health, HealthBar>), instance, methodInfo);
                }

                return s_getActiveHealthBar;
            }
        }
        private void TagHealthBar(Health health)
        {
            StartCoroutine(TagHealthBarAfterDelay(health));
        }

        protected override void OnAfterSetup()
        {
            Health.OnRequestHealthBar += TagHealthBar;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        private IEnumerator TagHealthBarAfterDelay(Health health)
        {
            yield return null;

            if (health is null) yield break; // 如果在等待期间 health 失效了，就退出

            try
            {
                // 现在我们可以安全地调用了，因为 HealthBar 已经被创建出来了
                HealthBar healthBar = GetActiveHealthBar?.Invoke(health);

                if (!(healthBar is null) && healthBar.GetComponent<NumericalHealthDisplay>() is null)
                {
                    healthBar.gameObject.AddComponent<NumericalHealthDisplay>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[NumericalHealthDisplay] 延迟标记时发生异常: {e.Message}");
            }
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            s_getActiveHealthBar = null;
        }

        protected override void OnBeforeDeactivate()
        {
            Health.OnRequestHealthBar -= TagHealthBar;
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }
}
