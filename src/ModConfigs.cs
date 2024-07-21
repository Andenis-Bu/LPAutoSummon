using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace LPAutoSummon
{
    internal class ModConfigs : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Label("Hotbar Slot for Summoning Minions")]
        [Tooltip("Choose which hotbar slot to use for summoning minions (1-10)")]
        [DefaultValue(10)] // Default value is set to 10
        [Range(1, 10)] // Allows values from 1 to 10
        public int MinionSlot;
    }
}
