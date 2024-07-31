using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace LPAutoSummon.src
{
    public class ModConfigs : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [DefaultValue(10)]
        [Range(1, 10)] 
        public int MinionColumnIndex { get; set; }

        [DefaultValue(false)]
        public bool MuteSummonUseSounds { get; set; }

        public static ModConfigs Instance => ModContent.GetInstance<ModConfigs>();
    }
}
