using Duckov.Options;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class MustShowHealthBarProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetMustShowHealthBar(isEnabled);
            // int valueToSave = ModSettings.ShowEnemyName ? 1 : 0;
            OptionsManager.Save(Key, ModSettings.MustShowHealthBar);
        }

        public override string Key => "MustShowHealthBar";
    }
}
