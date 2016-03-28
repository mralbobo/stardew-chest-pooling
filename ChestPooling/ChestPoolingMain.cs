using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewModdingAPI;
using Newtonsoft.Json;
using System.IO;


//bugs while playing:
/*
    - adding an item to a chest that already has a partial in the open chest and in the remote chest, might duplicate. 
    Need to add a pre-check for the open chest to give it priority.
    - weirdness when item exists in multiple places, possibly only when stacks are mostly full


*/

namespace ChestPooling
{
    public class ChestPoolingMainClass : Mod
    {
        public override void Entry(params object[] objects)
        {
            //StardewModdingAPI.Events.PlayerEvents.InventoryChanged
            //StardewModdingAPI.Events.MenuEvents.MenuChanged
            //StardewModdingAPI.Events.LocationEvents.LocationObjectsChanged
            //StardewModdingAPI.Events.EventArgsInventoryChanged

            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.couldInventoryAcceptThisItem
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.couldInventoryAcceptThisObject
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.cupboard
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.dropItem
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.getAdjacentTiles
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.GetDropLocation
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.GetGrabTile
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.getIndexOfInventoryItem
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.hasItemInInventory
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.hasItemInList
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.hasItemOfType
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.hasItemWithNameThatContains
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.increaseBackpackSize
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.Items
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.removeItemFromInventory
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.toolBox
            //StardewModdingAPI.Entities.SPlayer.CurrentFarmer.warpFarmer

            StardewModdingAPI.Events.PlayerEvents.InventoryChanged += Event_InventoryChanged;

            StardewModdingAPI.Events.LocationEvents.CurrentLocationChanged += Event_CurrentLocationChanged;

            StardewModdingAPI.Events.GameEvents.LoadContent += Event_LoadContent;

        }

        static bool loaded = false;


        static void debugThing(object theObject, string descriptor = "")
        {
            String thing = JsonConvert.SerializeObject(theObject, Formatting.Indented,
            new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            File.WriteAllText("debug.json", thing);
            Console.WriteLine(descriptor + "\n"+ thing);
        }

        static void Event_CurrentLocationChanged(object sender, EventArgs e)
        {
            //loaded = true;

        }

        static void Event_LoadContent(object sender, EventArgs e)
        {
            loaded = true;

        }

        static void getChest(KeyValuePair<Vector2, StardewValley.Object> obj)
        {

        }

        static List<StardewValley.Objects.Chest> getChests()
        {
            if (!loaded) { return null; }
            if (StardewValley.Game1.currentLocation == null) { return null; }

            List<StardewValley.Objects.Chest> chestList = new List<StardewValley.Objects.Chest>();

            foreach (StardewValley.GameLocation location in StardewValley.Game1.locations)
            {
                foreach (KeyValuePair<Vector2, StardewValley.Object> farmObj in location.Objects)
                {
                    if (farmObj.Value is StardewValley.Objects.Chest)
                    {
                        chestList.Add(farmObj.Value as StardewValley.Objects.Chest);
                    }
                }
            }


            //still in progress...
            // this isn't picking up chests inside "built" buildings, like the barn
            // might as well make sure the actually operation works first though
            /*
            StardewValley.Farm farm = StardewValley.Game1.getFarm();
            if (farm != null) {

                foreach (StardewValley.Buildings.Building building in farm.buildings)
                {
                    if(building.)
                    foreach (KeyValuePair<Vector2, StardewValley.Object> farmObj in building.indoors.Objects)
                    {
                        if (farmObj.Value is StardewValley.Objects.Chest)
                        {
                            chestList.Add(farmObj.Value as StardewValley.Objects.Chest);
                        }
                    }
                }
            }
            */

            //Log.Info("chest list: " + chestList.Count);

            return chestList;

        }

        static StardewValley.Objects.Chest QueryChests(List<StardewValley.Objects.Chest> chestList, StardewValley.Item itemRemoved)
        {
            StardewValley.Objects.Chest openChest = null;
            StardewValley.Objects.Chest chestWithStack = null;
            StardewValley.Item itemToAddTo = null;
            bool hasFoundCurrentChest = false;

            if (StardewValley.Game1.activeClickableMenu is StardewValley.Menus.ItemGrabMenu)
            {
                StardewValley.Menus.ItemGrabMenu menu = StardewValley.Game1.activeClickableMenu as StardewValley.Menus.ItemGrabMenu;
                if (menu.behaviorOnItemGrab != null && menu.behaviorOnItemGrab.Target is StardewValley.Objects.Chest)
                {
                    openChest = menu.behaviorOnItemGrab.Target as StardewValley.Objects.Chest;
                    //Log.Info("open chest (other) first item: " + openChest.items.First().Name);
                }
            }

            //likely in some other menu
            if (openChest == null)
            {
                return null;
            }

            foreach (StardewValley.Objects.Chest chest in chestList)
            {
                //don't do anything special if it's in the current chest
                //might be only way to check if it's open...
                //this is actually a bit flawed, since simply standing near chests opens them, so by being near 2 chests you can duplicate shit
                /*
                if (chest.currentLidFrame == 135) {
                    if(chest == openChest)
                    {
                        Log.Info("matched");
                    }
                    else
                    {
                        Log.Info("didn't match");
                    }
                    openChest = chest;
                    continue;
                }
                */
                if(chest == openChest)
                {
                    hasFoundCurrentChest = true;
                    continue;
                }

                //found something, don't bother going any further
                //consider adding another check that completely bails if both the open and "withStack" chest is found
                if (chestWithStack != null) { continue; }
                
                foreach(StardewValley.Item item in chest.items)
                {
                    if (itemRemoved.canStackWith(item) && item.Stack < item.maximumStackSize())
                    {
                        chestWithStack = chest;
                        //newStackSize = item.Stack + itemRemoved.Stack;
                        itemToAddTo = item;

                        break;
                    }
                    
                }
            }

            //user probably just threw away the item
            //could probably remove this check as a "cheat" to allow remote deposit...
            if (openChest == null || !hasFoundCurrentChest)
            {
                return null;
            }

            if(chestWithStack != null)
            {
                if (openChest.items.Count > 0 && chestWithStack.items.Count > 0)
                {
                    Log.Info("open chest first item: " + openChest.items.First().Name);
                    Log.Info("target chest first item: " + chestWithStack.items.First().Name);
                }

                int newStackSize = newStackSize = itemToAddTo.Stack + itemRemoved.Stack;

                //resize it in the chest it was placed in
                if (newStackSize > itemRemoved.maximumStackSize())
                {
                    itemRemoved.Stack = newStackSize - itemRemoved.maximumStackSize();
                }
                //actually do things
                else
                {
                    itemToAddTo.addToStack(itemRemoved.Stack);
                    Log.Info(itemToAddTo.Name + " new size: " + newStackSize);
                    openChest.grabItemFromChest(itemRemoved, StardewModdingAPI.Entities.SPlayer.CurrentFarmer);
                }
            }

            return null;
        }

        //e is a thing that contains "Inventory", "Added" and "Removed" properties, not yet sure what object that corresponds to
        static void Event_InventoryChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }
            if(StardewValley.Game1.currentLocation == null) { return; }

            //the real event, might be necessary to determine what item was placed where
            StardewModdingAPI.Events.EventArgsInventoryChanged inventoryEvent = (StardewModdingAPI.Events.EventArgsInventoryChanged)e;

            if(inventoryEvent.Removed.Count == 0) { return; }

            List<StardewValley.Objects.Chest>  chestList = getChests();
            if (chestList == null) { return; }

            //now have (kind of) the list of chests, and the item that was just removed or updated from inventory, do something with that
            //for first pass, probably just ignore the update version (less then full stack)
            QueryChests(chestList, inventoryEvent.Removed.First().Item);

            //debugThing(inventoryEvent.QuantityChanged);
            /*
            StardewValley.Farm farm = StardewValley.Game1.getFarm();

            Log.Info(StardewValley.Game1.locations[0].Name);
            Log.Info(StardewValley.Game1.currentLocation.Name);

            if (farm == null) { return;  }

            int count = 0;
            foreach (KeyValuePair<Vector2, StardewValley.Object> keyPair in farm.Objects)
            {
                if(keyPair.Value is StardewValley.Objects.Chest)
                {
                    StardewValley.Objects.Chest chest = keyPair.Value as StardewValley.Objects.Chest;
                    //chest.addItem
                   // Log.Info("first item " + chest.items[0].Name);
                    count++;
                }
            }
            */

            //Log.Info(count + " chests");

            //StardewValley.Objects.Chest
            //StardewValley.Game1.getFarm().buildings
            //farm.openChest()
            //farm.openItemChest()
            //farm.Objects

            //Log.Info("inventoryEvent");
            //debugThing(inventoryEvent.Removed);


            //debugThing(farm.buildings);

        }
    }
}
