using Duckov.Options;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class ShowHealthBarForObjectsProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetShowHealthBarForObjects(isEnabled);
            OptionsManager.Save(Key, ModSettings.ShowHealthBarForObjects);
        }

        public override string Key => "ShowHealthBarForObjects";
    }
}
