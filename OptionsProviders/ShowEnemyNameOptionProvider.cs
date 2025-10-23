using Duckov.Options;
using SodaCraft.Localizations;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class ShowEnemyNameOptionProvider: OptionsProviderBase
    {
        private void Awake()
        {
            LevelManager.OnLevelInitialized += RefreshOnLevelInited;
        }

        private void OnDestroy()
        {
            LevelManager.OnLevelInitialized -= RefreshOnLevelInited;
        }
        private void RefreshOnLevelInited()
        {
            int num = OptionsManager.Load(Key, 1);
            Set(num == 1 ? 0 : 1);
        }
        [LocalizationKey] private const string OnKey = "Options_On";

        [LocalizationKey] private const string OffKey = "Options_Off";

        public override string[] GetOptions()
        {
            return new[] { OnKey.ToPlainText(), OffKey.ToPlainText() };
        }

        public override string GetCurrentOption()
        {
            int toggle = OptionsManager.Load(Key, 1);
            return toggle == 1 ? OnKey.ToPlainText() : OffKey.ToPlainText();
        }

        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetShowShowEnemyName(isEnabled);
            int valueToSave = ModSettings.ShowEnemyName ? 1 : 0;
            OptionsManager.Save(Key, valueToSave);
        }

        public override string Key => "ShowEnemyName";
    }
}
