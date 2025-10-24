using Duckov.Options;
using SodaCraft.Localizations;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class HealthBarNumericalDisplayOptionProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetShowNumericalHealth(isEnabled);
            int valueToSave = ModSettings.ShowNumericalHealth ? 1 : 0;
            OptionsManager.Save(Key, valueToSave);
        }

        public override string Key => "ShowNumericalHealth";
    }
}
