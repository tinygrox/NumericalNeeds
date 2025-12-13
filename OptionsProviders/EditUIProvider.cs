using Duckov.Options;

namespace tinygrox.DuckovMods.NumericalStats.OptionsProviders
{
    public class EditUIProvider: TheModOptionsProviderBase
    {
        public override void Set(int index)
        {
            bool isEnabled = (index == 0);
            ModSettings.SetEditUI(isEnabled);
            OptionsManager.Save(Key, ModSettings.EditUI);
        }

        public override string Key => "NumericalStats_EditUI";
    }
}
