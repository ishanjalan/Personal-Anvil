using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using Object = System.Object;
using Netcode;

namespace UsefulWorkbench
{
    public class ModEntry : Mod
    {
        private int leftClickXPos;
        private int leftClickYPos;

        public override void Entry(IModHelper helper)
        {
            /*helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.Display.MenuChanged += OnMenuChanged;*/
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                this.leftClickXPos = (int)e.Cursor.ScreenPixels.X;
                this.leftClickYPos = (int)e.Cursor.ScreenPixels.Y;
            }
            if (!e.Button.IsActionButton())
                return;
            Vector2 tile = Helper.Input.GetCursorPosition().Tile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if (obj is Workbench)
            {
                Game1.activeClickableMenu = new WorkbenchGeodeMenu();
            }
        }
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Re-send a left click to the geode menu if one is already not being broken, the player has the room and money for it, and the click was on the geode spot.
            if (e.IsMultipleOf(4) && this.Helper.Input.IsDown(SButton.MouseLeft) && Game1.activeClickableMenu is WorkbenchGeodeMenu menu)
            {
                bool clintNotBusy = menu.heldItem != null && (menu.heldItem.Name.Contains("Geode") || menu.heldItem.ParentSheetIndex == 275) && (Game1.player.Money >= 0 && menu.geodeAnimationTimer <= 0);
                bool playerHasRoom = Game1.player.freeSpotsInInventory() > 1 || (Game1.player.freeSpotsInInventory() == 1 && menu.heldItem != null && menu.heldItem.Stack == 1);

                if (clintNotBusy && playerHasRoom && menu.geodeSpot.containsPoint(this.leftClickXPos, this.leftClickYPos))
                {
                    menu.receiveLeftClick(this.leftClickXPos, this.leftClickYPos, false);
                }
            }
        }

    }
}
