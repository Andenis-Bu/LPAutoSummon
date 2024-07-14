using System;
using Terraria;
using Terraria.ModLoader;

namespace LPAutoSummon
{
    public class AutoSummon : ModPlayer
    {
        private int  previousMinionSlotIndex;   // Store the previous index of hotbar slot for minions
        private int  previousMinionCap;         // Store the previous max minions count
        private Item previousMinionSlotItem;    // Store the previous item in the hotbar slot for minions

        private bool hasSpawned;                // Flag to check if the player has respawned
        private bool isUsingMinionSlotItem;     // Flag to check if is using an item
        private bool hasUsedMinionSlotItem;     // Flag to check if the player has used an item

        private bool hasChangedMinionCap;       // Flag to check if minion capacity has increased
        private bool hasChangedMinionSlotItem;  // Flag to check if item in configured minion slot has changed

        // Initialize the mod player
        public override void Initialize()
        {
            base.Initialize();

            previousMinionSlotIndex = GetConfigMinionSlotIndex();
            previousMinionSlotItem = new Item();
            previousMinionCap = 0;
     
            hasSpawned = false;
            isUsingMinionSlotItem = false;
            hasUsedMinionSlotItem = false;

            hasChangedMinionCap = false;
            hasChangedMinionSlotItem = false;
        }

        // Get the index of inventory slot for minions
        private int GetConfigMinionSlotIndex()
        {
            return ModContent.GetInstance<ModConfigs>().MinionSlot - 1;
        }

        // Get the total number of used up minion slot
        private float CountMinionSlotsUsed()
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
        private void removeAllSummonMinions()
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
        private bool checkItemSummonsMinions(Item item)
        {
            return item.DamageType == DamageClass.Summon && !item.sentry ? true : false;
        }

        // Automatically summon minions
        private void SummonMinions()
        {
            // Check if the item in the configured slot is a valid summon weapon
            if (!checkItemSummonsMinions(Player.inventory[previousMinionSlotIndex]))
            {
                return; 
            }

            // Select configured minion slot item
            int previousSelectedItem = Player.selectedItem;
            Player.selectedItem = previousMinionSlotIndex;
      
            // Create instance of a projectile of the summoner weapon minion to get its slot size
            Projectile proj = new Projectile();
            proj.SetDefaults(Player.HeldItem.shoot);
            float minionSlotSize = proj.minionSlots != 0 ? proj.minionSlots : 1;

            float minionSlotsUsed = CountMinionSlotsUsed();

            // Loop through the max minions count to summon them
            while (minionSlotsUsed + minionSlotSize <= Player.maxMinions)
            {
                float previousScreenPositionX = Main.screenPosition.X; // Backup screen position X
                float previousScreenPositionY = Main.screenPosition.Y; // Backup screen position Y

                // Adjust screen position for summoning effect
                Main.screenPosition.X = Player.Center.X - Main.MouseScreen.X + Random.Shared.Next(0, 10) * 16 - 5 * 16;
                Main.screenPosition.Y = Player.Center.Y - Main.MouseScreen.Y + Random.Shared.Next(0, 10) * 16 - 5 * 16;

                // Back up values
                var previousControlUseItem = Player.controlUseItem;
                var previousMana = Player.HeldItem.mana;

                Player.HeldItem.mana = 0; // Prevent mana consumption

                // Trigger item use
                Player.controlUseItem = true;
                Player.itemAnimation = 0;
                Player.ItemCheck();
                Player.controlUseItem = false;
                
                // Wait until the item animation is finished
                while (Player.itemAnimation > 0)
                {
                    Player.ItemCheck();
                }

                // Restore values
                Player.controlUseItem = previousControlUseItem;
                Player.HeldItem.mana = previousMana; 

                Main.screenPosition.X = previousScreenPositionX; // Restore screen position X
                Main.screenPosition.Y = previousScreenPositionY; // Restore screen position Y

                minionSlotsUsed += minionSlotSize;
            }
            
            Player.selectedItem = previousSelectedItem;
        }

        // Handle player respawn
        public override void OnRespawn()
        {
            base.OnRespawn();

            hasSpawned = true;
        }

        public override void PreUpdate()
        {
            base.PreUpdate();

            // Check if the game is running on a dedicated server or the player instance is not the current player,
            if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            // Set flag and remove all minions if item in configured minion slot has changed 
            if (previousMinionSlotItem != Player.inventory[previousMinionSlotIndex])
            {
                hasChangedMinionSlotItem = true;
                removeAllSummonMinions();
            }

            // Set flag if conditions are met
            if (Player.selectedItem == previousMinionSlotIndex && // Check if the select item is configured minion slot item
                checkItemSummonsMinions(Player.HeldItem) && // Check if the held item is a minion summon item
                Player.controlUseItem) // Check if the player is using an item
            {
                if (!isUsingMinionSlotItem)
                {
                    isUsingMinionSlotItem = true;
                    hasUsedMinionSlotItem = true;

                    Player.itemAnimation = 0;
                }
            }
            else
            {
                isUsingMinionSlotItem = false;
            }
        }

        public override void PostUpdate()
        {
            base.PostUpdate();

            // Check if the game is running on a dedicated server or the player instance is not the current player,
            if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            // Set flag if minion capacity has increased
            if (previousMinionCap < Player.maxMinions)
            {
                hasChangedMinionCap = true;
            }

            // Summon minions if conditions are met
            if (hasSpawned || // Check if if the player has respawned
               hasChangedMinionSlotItem || // Check if the item in the configured minion slot has changed
               hasUsedMinionSlotItem || // Check if the item in the configured minion slot has been used
               hasChangedMinionCap) // Check if the max minions count has changed  
            {
                SummonMinions();

                // Reset flags
                hasSpawned = false;
                hasUsedMinionSlotItem = false;
                hasChangedMinionSlotItem = false;
                hasChangedMinionCap = false;    
            }

            // Update stored values
            previousMinionCap = Player.maxMinions;           
            previousMinionSlotItem = Player.inventory[previousMinionSlotIndex];
            previousMinionSlotIndex = GetConfigMinionSlotIndex(); 
        }
    }
}