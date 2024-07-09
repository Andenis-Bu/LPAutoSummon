using System;
using System.Threading.Channels;
using Terraria;
using Terraria.ModLoader;

namespace LPAutoSummon
{
    public class AutoSummon : ModPlayer
    {
        private int  previousMinionSlotIndex;   // Store the index of inventory slot for minions
        private int  previousMinionCap;         // Store the previous max minions count
        private Item previousMinionSlotItem;    // Store the previous item in the inventory slot for minions
        private bool hasSpawned;                // Flag to check if the player has respawned

        // Initialize the mod player
        public override void Initialize()
        {
            base.Initialize();

            previousMinionSlotIndex = GetConfigMinionSlotIndex();
            previousMinionSlotItem = null;
            previousMinionCap = 0;
     
            hasSpawned = false;
        }

        // Helper method to get the index of inventory slot for minions
        private int GetConfigMinionSlotIndex()
        {
            return ModContent.GetInstance<ModConfigs>().MinionSlot - 1;
        }

        // Automatically summon minions
        private void SummonMinions()
        {
            // Check if the item in the configured slot is a valid summon weapon
            if (Player.inventory[previousMinionSlotIndex].DamageType != DamageClass.Summon || Player.inventory[previousMinionSlotIndex].sentry)
            {
                return; // Exit if the item is not a valid summon weapon
            }

            int previousSelectedItem = Player.selectedItem;
            Player.selectedItem = previousMinionSlotIndex;

            // Loop through the max minions count to summon them
            for (int i = 0; i < Player.maxMinions; ++i)
            {
                float previousScreenPositionX = Main.screenPosition.X;
                float previousScreenPositionY = Main.screenPosition.Y;

                // Adjust screen position for summoning effect
                Main.screenPosition.X = Player.Center.X - Main.MouseScreen.X + Random.Shared.Next(0, 10) * 16 - 5 * 16;
                Main.screenPosition.Y = Player.Center.Y - Main.MouseScreen.Y + Random.Shared.Next(0, 10) * 16 - 5 * 16;

                int previousMana = Player.HeldItem.mana;
                Player.HeldItem.mana = 0; // Prevent mana consumption

                // Trigger item use
                Player.controlUseItem = true;
                Player.ItemCheck();
                Player.controlUseItem = false;

                // Wait until the item animation is finished
                while (Player.itemAnimation > 0)
                {
                    Player.ItemCheck();
                }

                Player.HeldItem.mana = previousMana; // Restore previous mana cost

                Main.screenPosition.X = previousScreenPositionX; // Restore screen position X
                Main.screenPosition.Y = previousScreenPositionY; // Restore screen position Y
            }

            Player.selectedItem = previousSelectedItem;
        }

        // Handle player respawn
        public override void OnRespawn()
        {
            base.OnRespawn();
            hasSpawned = true;
        }

        // Update method called every frame
        public override void PostUpdate()
        {
            base.PostUpdate();
            if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            {
                return;
            }

            if (previousMinionCap != Player.maxMinions || // Check if the max minions count has changed
                previousMinionSlotItem != Player.inventory[previousMinionSlotIndex] || // Check if the item in the configured minion slot has changed
                hasSpawned) // Check if if the player has respawned
            {
                SummonMinions(); // Summon minions if conditions are met
                hasSpawned = false;
            }

            // Update stored values
            previousMinionCap = Player.maxMinions;           
            previousMinionSlotItem = Player.inventory[previousMinionSlotIndex];
            previousMinionSlotIndex = GetConfigMinionSlotIndex();
        }
    }
}