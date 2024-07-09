using System;
using Terraria;
using Terraria.ModLoader;

namespace LPAutoSummon
{
    public class LPAutoSummon : ModPlayer
    {
        
        private Item previousItemInSlot10;  // Store the previous item in the 10th inventory slot
        private int previousMinionCap;      // Store the previous max minions count
        bool hasSpawned;                    // Flag to check if the player has respawned

        // Initialize the mod player
        public override void Initialize()
        {
            base.Initialize();

            previousItemInSlot10 = Player.inventory[9];
            previousMinionCap = 0;
            hasSpawned = false;
        }

        // Automatically summon minions
        private void AutoSummon()
        {
            // Check if the item in the 10th slot is not a summon weapon or is a sentry
            if (Player.inventory[9].DamageType != DamageClass.Summon || Player.inventory[9].sentry)
            {
                return; // Exit if the item is not a valid summon weapon
            }

            int previousSelectedItem = Player.selectedItem;
            Player.selectedItem = 9;

            // Loop through the max minions count to summon them
            for (int i = 0; i < Player.maxMinions; ++i)
            {
                // Save the current screen position
                float previousScreenPositionX = Main.screenPosition.X;
                float previousScreenPositionY = Main.screenPosition.Y;

                // Modify the screen position to a random position around the player
                Main.screenPosition.X = Player.Center.X - Main.MouseScreen.X + Random.Shared.Next(0, 10) * 16 - 5 * 16;
                Main.screenPosition.Y = Player.Center.Y - Main.MouseScreen.Y + Random.Shared.Next(0, 10) * 16 - 5 * 16;

                // Save the current mana cost of the held item
                int previousMana = Player.HeldItem.mana;

                // Set the mana cost of the held item to 0 to prevent mana consumption
                Player.HeldItem.mana = 0;

                // Trigger item use
                Player.controlUseItem = true;
                Player.ItemCheck();
                Player.controlUseItem = false;

                // Wait until the item animation is finished
                while (Player.itemAnimation > 0)
                {
                    Player.ItemCheck();
                }

                // Restore the previous mana cost of the held item
                Player.HeldItem.mana = previousMana;

                // Restore the previous screen position
                Main.screenPosition.X = previousScreenPositionX;
                Main.screenPosition.Y = previousScreenPositionY;
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
            // Check if the max minions count or the item in the 10th slot has changed, or if the player has respawned
            if (previousMinionCap != Player.maxMinions || previousItemInSlot10 != Player.inventory[9] || hasSpawned)
            {
                AutoSummon();
                hasSpawned = false;
            }

            previousMinionCap = Player.maxMinions;
            previousItemInSlot10 = Player.inventory[9];
        }
    }
}
