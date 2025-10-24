using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using Newtonsoft.Json;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace tinygrox.DuckovMods.NumericalStats
{
    public class ArmourStatsDisplay: Duckov.Modding.ModBehaviour
    {
        private CharacterMainControl _characterMainControl;

        private Image _armourImage;
        private Image _armourImageBackground;
        private Image _helmetImage;
        private Image _helmetImageBackground;
        private TextMeshProUGUI _helmetText;
        private TextMeshProUGUI _armourText;

        private GameObject _iconContainer;
        private readonly Color _lowDurabilityColor = new Color(0.76f, 0.21f, 0.09f); // "#C23616"
        private readonly Color _midDurabilityColor = new Color(0.88f, 0.69f, 0.17f); // "#E1B12C"
        private readonly Color _highDurabilityColor = new Color(0.8f, 0.8f, 0.8f); // "#CCCCCC"

        public Slot armorSlot;
        public Slot helmetSlot;

        private Item _armorItem;
        private Item _helmetItem;

        private static string GetLocalizationFilePath(string langName)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return assemblyDir != null ? Path.Combine(assemblyDir, "Localization", $"{langName}.json") : null;
        }

        private void LoadLanguageFile(SystemLanguage language)
        {
            string langName = language.ToString();
            Debug.Log($"[NumericalStats]Loading language file for: {langName}");
            string langFilePath = GetLocalizationFilePath(langName);
            if (!File.Exists(langFilePath))
            {
                langFilePath = GetLocalizationFilePath("English");

                if(!File.Exists(langFilePath)) return;
            }

            string jsonContent = File.ReadAllText(langFilePath);
            var localizedStrings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);

            foreach (var pair in localizedStrings)
            {
                LocalizationManager.SetOverrideText(pair.Key, pair.Value);
            }
        }

        private Color GetColorFromHex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
        }

        private void OnSettingsChanged(bool value)
        {
            if (_iconContainer)
            {
                _iconContainer.SetActive(value);
            }
        }

        private void Awake()
        {
            Debug.Log("[NumericalStats]ArmourStatsDisplay Awake");
            LoadLanguageFile(LocalizationManager.CurrentLanguage);
        }

        private void AddIconToHUD()
        {
            if (_iconContainer != null) return;

            GameObject hudObj = GameObject.Find("HP");

            if (hudObj is null) return;

            _iconContainer = new GameObject("ArmourStatsDisplay_Container");
            _iconContainer.transform.SetParent(hudObj.transform, false);
            var contanierRect = _iconContainer.AddComponent<RectTransform>();
            contanierRect.anchorMin = new Vector2(0, 1);
            contanierRect.anchorMax = new Vector2(1, 1);
            contanierRect.pivot = new Vector2(0.5f, 0);
            // contanierRect.anchoredPosition = new Vector2(0, 18);

            HorizontalLayoutGroup layoutGroup = _iconContainer.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childScaleHeight = false;
            layoutGroup.childScaleWidth = false;

            var contentSizeFitter = _iconContainer.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateArmourUnit("Helmet", out _helmetImage, out _helmetImageBackground, out _helmetText);
            CreateArmourUnit("Armour", out _armourImage, out _armourImageBackground, out _armourText);

            OnSettingsChanged(ModSettings.ShowArmourStats);
        }

        private void SetCharacterMainControl()
        {
            Debug.Log("[NumericalStats]ArmourStatsDisplay SetCharacterMainControl");
            _characterMainControl = LevelManager.Instance?.MainCharacter;
            if (_characterMainControl is null)
            {
                Debug.LogError("[NumericalStats] Character main control is null");
                return;
            }
            SlotSubscription();
            SetArmourStatus(_characterMainControl.GetHelmatItem(), _helmetImage, _helmetImageBackground, _helmetText);
            SetArmourStatus(_characterMainControl.GetArmorItem(), _armourImage, _armourImageBackground, _armourText);
        }

        private void CreateArmourUnit(string type, out Image image, out Image bg, out TextMeshProUGUI text)
        {
            GameObject unitContainer = new GameObject($"{type}UnitContainer");
            unitContainer.transform.SetParent(_iconContainer.transform, false);
            var unitContainerRect = unitContainer.AddComponent<RectTransform>();
            unitContainerRect.pivot = new Vector2(0.5f, 0f);
            unitContainerRect.sizeDelta = new Vector2(80f, 128f);

            VerticalLayoutGroup unitLayout = unitContainer.AddComponent<VerticalLayoutGroup>();
            unitLayout.spacing = 10f;
            unitLayout.padding = new RectOffset(5, 5, 5, 5);
            unitLayout.childAlignment = TextAnchor.LowerCenter;
            unitLayout.childControlWidth = false;
            unitLayout.childControlHeight = false;
            unitLayout.childForceExpandWidth = false;
            unitLayout.childForceExpandHeight = false;
            unitLayout.childScaleHeight = false;
            unitLayout.childScaleWidth = false;

            GameObject iconHolder = new GameObject($"{type}IconHolder");
            iconHolder.transform.SetParent(unitContainer.transform, false);
            var iconHolderRect = iconHolder.AddComponent<RectTransform>();
            iconHolderRect.sizeDelta = new Vector2(64, 64);
            iconHolderRect.pivot = new Vector2(0.5f, 0f);

            bg = new GameObject($"{type}Background").AddComponent<Image>();
            bg.transform.SetParent(iconHolder.transform, false);
            var bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            image = new GameObject($"{type}Icon").AddComponent<Image>();
            image.transform.SetParent(iconHolder.transform, false);
            var imageRect = image.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.sizeDelta = Vector2.zero;

            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (assemblyDir != null)
            {
                string spritePath = Path.Combine(assemblyDir, "Textures", "Icons", $"PictoIcon_{type}.png");
                image.sprite = LoadSprite(spritePath);
            }
            bg.sprite = image.sprite;

            image.fillMethod = Image.FillMethod.Vertical;
            image.type = Image.Type.Filled;
            image.fillOrigin = (int)Image.OriginVertical.Bottom;

            bg.fillMethod = Image.FillMethod.Vertical;
            bg.type = Image.Type.Filled;
            bg.fillOrigin = (int)Image.OriginVertical.Top;
            bg.color = _lowDurabilityColor;

            text = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI, unitContainer.transform,false);
            text.gameObject.name = $"{type}Text";

            text.fontSizeMin = 12;
            text.fontSizeMax = 20;
            text.verticalAlignment = VerticalAlignmentOptions.Bottom;
            text.horizontalAlignment = HorizontalAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.enableAutoSizing = true;
            text.enableWordWrapping = true;
            var textRect = text.GetComponent<RectTransform>();
            textRect.pivot = new Vector2(0.5f, 0f);
            textRect.sizeDelta = new Vector2(80f, 20f);
        }
        private Sprite LoadSprite(string path)
        {
            if(!File.Exists(path)) return null;

            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f));
        }

        public void SetArmourStatus(Item item , Image img, Image bg, TextMeshProUGUI text)
        {
            if(img is null || text is null || bg is null) return;

            if (item is null)
            {
                img.color = _lowDurabilityColor;
                img.fillAmount = 1;
                text.text = LocalizationManager.GetPlainText("NumericalStats_ArmourStatsDisplay_NoItem");
                return;
            }

            if (item != _characterMainControl.GetHelmatItem() && item != _characterMainControl.GetArmorItem()) return;

            float durability = Mathf.RoundToInt(item.Durability);
            float maxDurability = Mathf.RoundToInt(item.MaxDurabilityWithLoss);

            text.SetText("{0}/{1}",durability, maxDurability);

            if (maxDurability <= 0)
            {
                img.color = _lowDurabilityColor;
                text.color = _lowDurabilityColor;
                return;
            }
            float durabilityPercent = durability / maxDurability;
            img.fillAmount = durabilityPercent - item.DurabilityLoss;
            bg.fillAmount = item.DurabilityLoss;

            // 来点平滑变色
            Color finalcolor;
            if (durabilityPercent < 0.4f)
            {
                float t = durabilityPercent / 0.4f;
                finalcolor = Color.Lerp(_lowDurabilityColor, _midDurabilityColor, t);
                // img.color = _highDurabilityColor;
                // text.color = _highDurabilityColor;
            }
            else
            {
                float t = (durabilityPercent - 0.4f) / 0.6f;
                finalcolor = Color.Lerp(_midDurabilityColor, _highDurabilityColor, t);
            }

            img.color = finalcolor;
            text.color = finalcolor;
        }

        private void SlotSubscription()
        {
            armorSlot = _characterMainControl.GetSlot(CharacterEquipmentController.armorHash);
            if (armorSlot != null)
            {
                armorSlot.onSlotContentChanged += OnArmorSlotContentChanged;
            }
            else
            {
                Debug.LogError($"{nameof(SlotSubscription)} could not find an armor slot.");
            }
            helmetSlot = _characterMainControl.GetSlot(CharacterEquipmentController.helmatHash);
            if (helmetSlot != null)
            {
                helmetSlot.onSlotContentChanged += OnHelmetSlotContentChanged;
            }
            else
            {
                Debug.LogError($"{nameof(SlotSubscription)} could not find an helmet slot.");
            }
        }
        private void SlotUnSubscription()
        {
            if (armorSlot != null)
            {
                armorSlot.onSlotContentChanged -= OnArmorSlotContentChanged;
            }

            if (helmetSlot != null)
            {
                helmetSlot.onSlotContentChanged -= OnHelmetSlotContentChanged;
            }
        }

        // private void OnEnable()
        // {
        //     SlotSubscription();
        // }
        //
        // private void OnDisable()
        // {
        //     SlotUnSubscription();
        // }

        private void OnArmorSlotContentChanged(Slot s)
        {
            if(!_iconContainer) return;

            var curItem = _characterMainControl?.GetArmorItem();
            if (_armorItem == curItem)
            {
                SetArmourStatus(_armorItem, _armourImage, _armourImageBackground, _armourText);
                return;
            }

            if (_armorItem)
            {
                _armorItem.onDurabilityChanged -= OnArmorItemDurabilityChanged;
            }

            _armorItem = curItem;

            if (_armorItem)
            {
                _armorItem.onDurabilityChanged += OnArmorItemDurabilityChanged;
            }
            SetArmourStatus(_armorItem, _armourImage, _armourImageBackground, _armourText);
        }

        private void OnArmorItemDurabilityChanged(Item item)
        {
            // 以防万一
            if (item != _armorItem) return;

            SetArmourStatus(item, _armourImage, _armourImageBackground, _armourText);
        }
        private void OnHelmetSlotContentChanged(Slot slot)
        {
            if (!_iconContainer) return;
            var curItem = slot.Content;
            if (_helmetItem == curItem)
            {
                SetArmourStatus(_helmetItem, _helmetImage, _helmetImageBackground, _helmetText);
                return;
            }

            if (_helmetItem)
            {
                _helmetItem.onDurabilityChanged -= OnHelmetItemDurabilityChanged;
            }
            _helmetItem = curItem;
            if (_helmetItem)
            {
                _helmetItem.onDurabilityChanged += OnHelmetItemDurabilityChanged;
            }
            SetArmourStatus(_helmetItem, _helmetImage, _helmetImageBackground, _helmetText);
        }

        private void OnHelmetItemDurabilityChanged(Item item)
        {
            if(item != _helmetItem) return;

            SetArmourStatus(item, _helmetImage, _helmetImageBackground, _helmetText);
        }

        private void OnGameLanguageChanged(SystemLanguage newLanguage)
        {
            LoadLanguageFile(newLanguage);
        }

        private void OnDestroy()
        {
            OnBeforeDeactivate();
        }

        protected override void OnAfterSetup()
        {
            LevelManager.OnLevelBeginInitializing += AddIconToHUD;
            LevelManager.OnLevelInitialized += SetCharacterMainControl;
            LocalizationManager.OnSetLanguage += OnGameLanguageChanged;
            ModSettings.OnShowArmourStatsChanged += OnSettingsChanged;
        }

        protected override void OnBeforeDeactivate()
        {
            LevelManager.OnLevelBeginInitializing -= AddIconToHUD;
            LevelManager.OnLevelInitialized -= SetCharacterMainControl;
            LocalizationManager.OnSetLanguage -= OnGameLanguageChanged;
            ModSettings.OnShowArmourStatsChanged -= OnSettingsChanged;
            SlotUnSubscription();
            if (!(_iconContainer is null))
            {
                Destroy(_iconContainer);
                // _iconContainer.SetActive(false);
                _iconContainer = null;
            }
        }
    }
}
