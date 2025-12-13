namespace tinygrox.DuckovMods.NumericalStats.HarmonyPatches
{
    public class CharacterMainControlPatches
    {
        public static void SetCurrentWaterPostfix()
        {
            ShowWaterAndEnergy.Instance?.OnWaterStatsInvoke();
        }
        public static void SetCurrentEnergyPostfix()
        {
            ShowWaterAndEnergy.Instance?.OnEnergyStatsInvoke();
        }
    }
}
