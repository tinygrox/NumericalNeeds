using Duckov.Options;
using SodaCraft.Localizations;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class ArmourStatsOptionProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetShowArmourStats(isEnabled);
            int valueToSave = ModSettings.ShowArmourStats ? 1 : 0;
            OptionsManager.Save(Key, valueToSave);
        }

        public override string Key => "ShowArmourStats";
    }
}
