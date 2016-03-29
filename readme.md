## What is it
Mod for stardew valley, built with SMAPI 0.39.2.
Basically what it does it provide a means of pooling all of your individual chests together when depositing. So for example you can return from mining, walk up to a single chest, throw in everything. The mod will then for example move the minerals to the mineral chest, the ore and bars to their chest, the food items to the food chest, etc. It essentially keys off of where your existing items are and adds additional items to existing stacks. It should also be noted that the mod will not take effect when moving less then a full stack (right click), this allows you to effectively override the mods auto-sorting at will.

## I want more detail
Breaking this up into steps:
* item added to chest, if there's an existing stack or the item does not exist in another chest (or all the other stacks are full), it's added normally.
* if the item exists in another chest and there's room for it, it gets moved to the other chest
* if there's only partial room the stack gets split and whatever overflows stays in the current chest

## Installation
* install SMAPI http://community.playstarbound.com/threads/stardew-modding-api-0-39-2.108375/
* download somehow (I'll probably add a zip at some point)
* place in mods folder

## Known bugs
* doesn't work on chests inside constructed buildings, barn, coop, etc. Can probably fix this, need motivation.
* fridge doesn't connect, not a bug strictly speaking but it's something I want.
* not terribly fond of the behavior when a stack is filled in the currently open chest. But I'm fairly sure I need either a "chestUpdated" or "beforeInventoryChanged" event to fix it consistently.

## Building it
* Presumably you should just be able to clone and run this... Open an issue if it doesn't work for some reason.