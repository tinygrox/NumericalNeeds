using Duckov.Options;
using SodaCraft.Localizations;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class TheModOptionsProviderBase: OptionsProviderBase
    {
        [LocalizationKey] private const string OnKey = "Options_On";

        [LocalizationKey] private const string OffKey = "Options_Off";
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
        }

        public override string Key => string.Empty;
    }
}
