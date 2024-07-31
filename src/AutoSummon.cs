using System;
using Terraria;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

namespace LPAutoSummon.src
{
    public class AutoSummon : ModPlayer
    {
        private MinionColumnItems minionColumnItems;    

        private int  previousMinionColumnIndex;   // Store the previous index of hotbar slot for minions
        private int  previousMinionCap;         // Store the previous max minions count  

        private bool hasSpawned;                // Flag to check if the player has respawned
        private bool isUsingMinionSlotItem;     // Flag to check if is using an item
        private bool hasUsedMinionSlotItem;     // Flag to check if the player has used an item

        private bool hasChangedMinionCap;       // Flag to check if minion capacity has increased
        private bool hasChangedMinionSlotItem;  // Flag to check if item in configured minion slot has changed

        private bool shouldSummonMinions;       // Flag to check if minions should be summoned

        // Initialize the mod player
        public override void Initialize()
        {
            minionColumnItems = new MinionColumnItems();

            previousMinionColumnIndex = GetConfigMinionColumnIndex();   
            previousMinionCap = 0;
     
            hasSpawned = false;
            isUsingMinionSlotItem = false;
            hasUsedMinionSlotItem = false;

            hasChangedMinionCap = false;
            hasChangedMinionSlotItem = false;

            shouldSummonMinions = false;
        }

        // Get the index of inventory slot for minions
        private int GetConfigMinionColumnIndex()
        {
            return ModConfigs.Instance.MinionColumnIndex - 1;
        }

        // Automatically summon minions
        private void SummonMinions()
        {
            // Back up currently selected item index
            int previousSelectedItem = Player.selectedItem;

            // Iterate through every row of configured columns 
            for (int i = 0; i < minionColumnItems.Length; ++i)
            {
                // Calculate inventory slot index
                Player.selectedItem = previousMinionColumnIndex + i * 10;

                // Check if the item in the configured slot is a valid summon weapon
                if (!MinionUtil.CheckItemSummonsMinions(Player.HeldItem))
                    continue; 
      
                // Create instance of a projectile of the summoner weapon minion to get its slot size
                Projectile proj = new Projectile();
                proj.SetDefaults(Player.HeldItem.shoot);
                float minionSlotSize = proj.minionSlots != 0 ? proj.minionSlots : 1;

                float minionSlotsUsed = MinionUtil.CountMinionSlotsUsed();

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
                    var previousUseSound = Player.HeldItem.UseSound;
                    var previousMana = Player.HeldItem.mana;

                    if (ModConfigs.Instance.MuteSummonUseSounds)
                        Player.HeldItem.UseSound = null;
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
                    Player.HeldItem.UseSound = previousUseSound;
                    Player.HeldItem.mana = previousMana; 

                    Main.screenPosition.X = previousScreenPositionX; // Restore screen position X
                    Main.screenPosition.Y = previousScreenPositionY; // Restore screen position Y

                    minionSlotsUsed += minionSlotSize;
                }
            }

            Player.selectedItem = previousSelectedItem;
        }

        // Handle player respawn
        public override void OnRespawn()
        {
            hasSpawned = true;
        }

        public override void PreUpdate()
        {
            // Check if the game is running on a dedicated server or the player instance is not the current player,
            if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            // Set flag and remove all minions if item in configured minion slot has changed 
            if (minionColumnItems.CheckItemChange(previousMinionColumnIndex))
            {
                hasChangedMinionSlotItem = true;
            }

            // Set flag if conditions are met
            if (Player.selectedItem == previousMinionColumnIndex && // Check if the select item is configured minion slot item
                MinionUtil.CheckItemSummonsMinions(Player.HeldItem) && // Check if the held item is a minion summon item
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
            // Check if the game is running on a dedicated server or the player instance is not the current player,
            if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            // Summon minions if conditions are met
            if (shouldSummonMinions)
            {
                SummonMinions();

                // Reset flags
                shouldSummonMinions = false;
            }

            // Set flag if minion capacity has changed
            if (previousMinionCap != Player.maxMinions)
            {
                hasChangedMinionCap = true;
            }

            // Prepare to summon minions if conditions are met
            if (hasSpawned || // Check if if the player has respawned
               hasChangedMinionSlotItem || // Check if the item in the configured minion slot has changed
               hasUsedMinionSlotItem || // Check if the item in the configured minion slot has been used
               hasChangedMinionCap) // Check if the max minions count has changed  
            {
                shouldSummonMinions = true;
                MinionUtil.RemoveAllSummonMinions();

                // Reset flags
                hasSpawned = false;
                hasUsedMinionSlotItem = false;
                hasChangedMinionSlotItem = false;
                hasChangedMinionCap = false;    
            }

            // Update stored values
            previousMinionCap = Player.maxMinions;
            minionColumnItems.UpdateItems(previousMinionColumnIndex);
            previousMinionColumnIndex = GetConfigMinionColumnIndex(); 
        }
    }
}