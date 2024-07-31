using Terraria;
using Terraria.ModLoader;

namespace LPAutoSummon.src
{
    public class MinionUtil
    {
        // Get the total number of used up minion slot
        public static float CountMinionSlotsUsed()
        {
            float minionSlotsUsed = 0f;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Main.myPlayer && proj.minion)
                {
                    minionSlotsUsed += proj.minionSlots;
                }
            }

            return minionSlotsUsed;
        }

        // Remove all the minions that the palyer has summoned
        public static void RemoveAllSummonMinions()
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == Main.myPlayer && proj.minion)
                {
                    proj.Kill();
                }
            }
        }

        // Return true if the item is a summoner and not a sentry
        public static bool CheckItemSummonsMinions(Item item)
        {
            return item.DamageType == DamageClass.Summon && !item.sentry ? true : false;
        }
    }

    public class MinionColumnItems
    {
        public int Length { get; } = 5;

        private Item[] previousMinionSlotItem;
        public MinionColumnItems()
        {
            previousMinionSlotItem = new Item[Length];
            for (int i = 0; i < Length; i++)
            {
                previousMinionSlotItem[i] = new Item(); // Initialize each slot with a new Item
            }
        }

        public bool CheckItemChange(int minionColumnIndex)
        {
            // Set calling player
            Player player = Main.player[Main.myPlayer];

            for (int i = 0; i < Length; ++i)
            {
                if (previousMinionSlotItem[i] != player.inventory[minionColumnIndex + 10 * i] &&
                   (MinionUtil.CheckItemSummonsMinions(previousMinionSlotItem[i]) 
                   || MinionUtil.CheckItemSummonsMinions(player.inventory[minionColumnIndex + 10 * i])))
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateItems(int minionColumnIndex)
        {
            // Set calling player
            Player player = Main.player[Main.myPlayer];

            for (int i = 0; i < Length; ++i)
            {
                previousMinionSlotItem[i] = player.inventory[minionColumnIndex + 10 * i];
            }
        }
    }
}
