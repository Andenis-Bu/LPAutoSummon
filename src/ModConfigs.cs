using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace LPAutoSummon.src
{
    internal class ModConfigs : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(10)] // Default value is set to 10
        [Range(1, 10)] // Allows values from 1 to 10
        public int MinionSlot;

        public static ModConfigs Instance => ModContent.GetInstance<ModConfigs>();
    }
}
