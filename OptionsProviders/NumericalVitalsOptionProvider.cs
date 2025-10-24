using Duckov.Options;
using SodaCraft.Localizations;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class NumericalVitalsOptionProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetShowNumericalWaterAndEnergy(isEnabled);
            int valueToSave = ModSettings.ShowNumericalWaterAndEnergy ? 1 : 0;
            OptionsManager.Save(Key, valueToSave);
        }

        public override string Key => "ShowNumericalWaterAndEnergy";
    }
}
