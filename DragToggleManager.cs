using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using tinygrox.DuckovMods.NumericalStats.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class DragToggleManager: MonoBehaviour
    {
        public static DragToggleManager Instance { get; private set; }

        private bool _isStopKeyHeld;
        private GameObject _inputBlockerObject;

        private readonly List<DraggableUIElement> _draggableElements = new List<DraggableUIElement>();

        private InputAction _gameStopAction;

        private static readonly Func<CharacterInputControl, object> s_getInputActionsObjectDelegate = ReflectionHelper.CreateFieldGetter<CharacterInputControl, object>("inputActions");
        private void Awake()
        {
            if (Instance is not null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _inputBlockerObject = new GameObject("ModDragInputBlocker");
            _inputBlockerObject.transform.SetParent(transform);
            _inputBlockerObject.SetActive(true);

            Debug.Log("[NumericalStats] DragToggleManager: Awake 阶段完成，订阅 LevelManager.OnAfterLevelInitialized 事件。");
            // 在 Awake 中订阅事件，等待游戏场景初始化完成
            SceneLoader.onFinishedLoadingScene += OnLevelInitialized;
        }

        private void OnEditUIChanged(bool value)
        {
            if (!SceneManager.GetActiveScene().name.Contains("Base")) return;

            // 懒加载：如果 _gameStopAction 为空（例如初始化时被跳过），尝试初始化
            if (_gameStopAction == null)
            {
                TryInitializeInput();
            }

            if (_gameStopAction == null) return;

            _gameStopAction.performed -= OnActionPerformed;
            _gameStopAction.canceled -= OnActionCanceled;
            if (value)
            {
                _gameStopAction.performed += OnActionPerformed;
                _gameStopAction.canceled += OnActionCanceled;
            }
        }

        private void OnLevelInitialized(SceneLoadingContext sceneLoadingContext)
        {
            if (!sceneLoadingContext.sceneName.Contains("Base"))
            {
                if (_gameStopAction != null)
                {
                    _gameStopAction.performed -= OnActionPerformed;
                    _gameStopAction.canceled -= OnActionCanceled;
                    _gameStopAction = null;
                }

                return;
            }

            // 场景加载完成后尝试初始化
            TryInitializeInput();
        }

        private void TryInitializeInput()
        {
            // 确保只执行一次输入初始化
            if (_gameStopAction != null)
            {
                // 如果已经初始化，刷新一下状态
                OnEditUIChanged(ModSettings.EditUI);
                return;
            }

            // 增强错误检查和日志记录
            if (CharacterInputControl.Instance is null)
            {
                // Debug.LogError("[NumericalStats] DragToggleManager: CharacterInputControl.Instance 未找到。拖拽模式将不会工作。请确保此管理器在 CharacterInputControl 之后初始化。");
                // enabled = false; // 暂时不禁用，允许后续重试
                return;
            }

            if (s_getInputActionsObjectDelegate is null)
            {
                // Debug.LogError("[NumericalStats] DragToggleManager: 无法创建 'inputActions' 字段的获取委托。请检查 ReflectionHelper 或字段名称。拖拽模式将不会工作。");
                enabled = false;
                return;
            }

            // 1. 通过委托获取 CharacterInputControl 实例中的私有 'inputActions' 对象 (类型为 object)
            object inputActionObject = s_getInputActionsObjectDelegate(CharacterInputControl.Instance);
            if(inputActionObject is null)
            {
                // Debug.LogError("[NumericalStats] DragToggleManager: 通过委托获取到的 CharacterInputControl.inputActions 为 null。拖拽模式将不会工作。");
                enabled = false;
                return;
            }

            // 2. 获取 inputActionObject 的运行时类型 (即 CharacterInputControl.InputActionReferences)
            Type inpuActionReferencesType = inputActionObject.GetType();

            // 3. 从该类型中查找名为 "StopAction" 的字段
            FieldInfo stopActionField = AccessTools.Field(inpuActionReferencesType, "StopAction");

            if (stopActionField is null)
            {
                // Debug.LogError($"[NumericalStats] DragToggleManager: 无法通过反射在 '{inpuActionReferencesType.Name}' 对象中找到 'StopAction' 字段。请检查字段名称。拖拽模式将不会工作。");
                enabled = false;
                return;
            }

            // 4. 获取 'StopAction' 字段的值并尝试转换为 InputAction 类型
            _gameStopAction = stopActionField.GetValue(inputActionObject) as InputAction;
            if(_gameStopAction is null)
            {
                // Debug.LogError("[NumericalStats] DragToggleManager: 'StopAction' 字段的值为 null 或不是 InputAction 类型。拖拽模式将不会工作。");
                enabled = false;
                return;
            }

            // Debug.Log("[NumericalStats] DragToggleManager: 输入动作初始化成功，通过反射使用游戏 CharacterInputControl 中的 'StopAction' 进行拖拽模式切换。");

            ModSettings.OnEditUIChanged -= OnEditUIChanged;
            ModSettings.OnEditUIChanged += OnEditUIChanged;

            // 初始化完成后，立即应用当前设置
            OnEditUIChanged(ModSettings.EditUI);
            UpdateDraggableElementsState();
        }

        private void UpdateDraggableElementsState()
        {
            if (_gameStopAction == null) return; // 避免在未初始化前调用

            _isStopKeyHeld = _gameStopAction.IsPressed();

            foreach (var draggable in _draggableElements)
            {
                if (draggable is not null)
                {
                    draggable.IsDraggingAllowed = _isStopKeyHeld;
                }
            }
            // Debug.Log($"[NumericalStats] DragToggleManager: UpdateDraggableElementsState 更新，拖拽模式状态为: {_isStopKeyHeld}.");

            if (_isStopKeyHeld)
            {
                InputManager.DisableInput(_inputBlockerObject);
            }
            else
            {
                InputManager.ActiveInput(_inputBlockerObject);
            }
        }

        private void OnEnable()
        {
            if (_gameStopAction != null)
            {
                _isStopKeyHeld = _gameStopAction.IsPressed();
                // Debug.Log($"[NumericalStats] DragToggleManager: OnEnable 检查，游戏 StopAction 初始状态为按下: {_isStopKeyHeld}.");
            }
            else
            {
                // 如果 _gameStopAction 为空，说明 Awake 中初始化失败或组件已被禁用
                _isStopKeyHeld = false;
                // Debug.LogWarning("[NumericalStats] DragToggleManager: _gameStopAction 为空，无法检查按键状态。");
            }

            // 应用拖拽模式到所有已注册的元素
            foreach (var draggable in _draggableElements)
            {
                if (draggable is not null)
                {
                    draggable.IsDraggingAllowed = _isStopKeyHeld;
                }
            }

        }
        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            if (!_isStopKeyHeld)
            {
                _isStopKeyHeld = true;
                // Debug.Log("[NumericalStats] DragToggleManager: Ctrl 键按下 (事件驱动)。启用拖拽模式。");
                ToggleDraggingMode(true);
            }
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            if (_isStopKeyHeld)
            {
                _isStopKeyHeld = false;
                // Debug.Log("[NumericalStats] DragToggleManager: Ctrl 键抬起 (事件驱动)。禁用拖拽模式。");
                ToggleDraggingMode(false);
            }
        }
        private void ToggleDraggingMode(bool enable)
        {
            foreach (var draggable in _draggableElements)
            {
                if (draggable is not null)
                {
                    draggable.IsDraggingAllowed = enable;
                }
            }

            if (enable)
            {
                InputManager.DisableInput(_inputBlockerObject);
            }
            else
            {
                InputManager.ActiveInput(_inputBlockerObject);
            }
        }

        public void RegisterDraggable(DraggableUIElement element)
        {
            if (element is not null && !_draggableElements.Contains(element))
            {
                _draggableElements.Add(element);
                element.IsDraggingAllowed = _isStopKeyHeld;
                // Debug.Log($"[NumericalStats] DragToggleManager: 注册了拖拽元素: {element.gameObject.name}. 当前注册数: {_draggableElements.Count}. IsDraggingAllowed 初始设置为: {_isCtrlHeld}.");
            }
        }

        public void UnregisterDraggable(DraggableUIElement element)
        {
            if (element is not null && _draggableElements.Contains(element))
            {
                _draggableElements.Remove(element);
                // Debug.Log($"[NumericalStats] DragToggleManager: 取消注册了拖拽元素: {element.gameObject.name}. 当前注册数: {_draggableElements.Count}.");
            }
        }

        private void OnDestroy()
        {
            // LevelManager.OnAfterLevelInitialized -= OnLevelInitialized;
            SceneLoader.onFinishedLoadingScene -= OnLevelInitialized;
            ModSettings.OnEditUIChanged -= OnEditUIChanged;
            if (_gameStopAction != null)
            {
                _gameStopAction.performed -= OnActionPerformed;
                _gameStopAction.canceled -= OnActionCanceled;
                // _gameStopAction.Dispose();
                _gameStopAction = null;
            }
            if (Instance == this)
            {
                if (_isStopKeyHeld)
                {
                    ToggleDraggingMode(false);
                }
                if (_inputBlockerObject != null)
                {
                    Destroy(_inputBlockerObject);
                }
                Instance = null;
            }
        }

    }
}
