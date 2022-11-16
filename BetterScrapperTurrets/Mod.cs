using PulsarModLoader;

namespace BetterScrapperTurrets
{
    public class Mod : PulsarMod
    {
        public override string Version => "1.0.0";

        public override string Author => "Dragon";

        public override string Name => "Better Scrapper Turrets";

        public override string LongDescription => "Removes cooldown between scrap drops, increases drop chance based on damage.";

        public override string HarmonyIdentifier()
        {
            return $"{Author}.{Name}";
        }
    }
}
