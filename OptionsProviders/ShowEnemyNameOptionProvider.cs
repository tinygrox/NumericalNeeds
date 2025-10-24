using Duckov.Options;
using SodaCraft.Localizations;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class ShowEnemyNameOptionProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetShowShowEnemyName(isEnabled);
            // int valueToSave = ModSettings.ShowEnemyName ? 1 : 0;
            OptionsManager.Save(Key, ModSettings.ShowEnemyName);
        }

        public override string Key => "ShowEnemyName";
    }
}
