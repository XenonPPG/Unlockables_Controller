# Unlockables Controller

This script allows you to easily create unlockable items for your mod. It offers real-time, fast catalog updates. Locked items are displayed with a lock icon, and their descriptions inform users how to unlock them.

## Features
- **Real-time updates**: Changes to the catalog are applied instantly.
- **User-friendly interface**: Locked items have clear indicators and guidance on unlocking.
- **Customizable locked sprites**: Set individual locked sprites for items, or use a default sprite.

---

## Tutorial

### Step 1: Entry Point
Place the entry point anywhere in the `void Main` method of your mod.

#### Code Example:
```csharp
var unlockables = UnlockablesController.Main;
unlockables.modTag = "some mod tag"; // Prevents conflicts between mods
unlockables.lockedSprite = ModAPI.LoadSprite("sprites/Locked.png"); // Sets the default sprite for locked items
```

### Step 2: Registering Unlockables
Mark items as unlockable to enable the system to manage them. You can now set an individual locked sprite for each item. This code should be added after the entry point.

#### Method:
```csharp
public void RegisterUnlockable(string assetName, string howToUnlock = "This item has not been unlocked yet!", Sprite individualLockedSprite = null)
```

- **Parameters:**
  - `assetName`: The name of the item to register.
  - `howToUnlock`: A description of how to unlock the item (default: "This item has not been unlocked yet!").
  - `individualLockedSprite`: A custom sprite for the locked item (default: `null`).

#### Example:
```csharp
unlockables.RegisterUnlockable("Super Brick", "\n<b>How to unlock:</b>\nThrow epic apple at normal brick");
unlockables.RegisterUnlockable("Mega Brick", "Unknown!", ModAPI.LoadSprite("sprites/MegaBrickLocked.png"));
```

### Step 3: Unlocking Items
Use the `Unlock` method anywhere in your mod to unlock an item.

#### Method:
```csharp
public void Unlock(string assetName, bool notify = true)
```

- **Parameters:**
  - `assetName`: The name of the item to unlock.
  - `notify`: If `true`, a notification will pop up, and the category will switch to the unlocked item's category (default: `true`).

#### Example:
```csharp
UnlockablesController.Main.Unlock("Super Brick");
UnlockablesController.Main.Unlock("Mega Brick", false);
```

---

## Documentation

### Fields and Properties

#### `public static UnlockablesController Main`
- **Description**: Provides global access to the main instance of the `UnlockablesController`.
- **Usage**: Use this field to call methods and access properties from anywhere in your mod.

#### `public List<Unlockable> unlockables => _unlockables`
- **Description**: A list of all registered unlockable items.
- **Usage**: Use this property to iterate through or inspect unlockable items in your mod.

#### `public Sprite lockedSprite`
- **Description**: The default sprite used to represent locked items.
- **Usage**: Assign a custom sprite to this field to customize the default locked item appearance.

### Methods

#### `public void RegisterUnlockable(string assetName, string howToUnlock = "This item has not been unlocked yet!", Sprite individualLockedSprite = null)`
- **Description**: Registers an item as unlockable.
- **Parameters:**
  - `assetName`: The name of the item to register.
  - `howToUnlock`: Description of how to unlock the item.
  - `individualLockedSprite`: A custom sprite for this locked item.

#### `public void Lock(string assetName, string howToUnlock = "This item has not been unlocked yet!", Sprite individualLockedSprite = null)`
- **Description**: Locks an item, making it inaccessible until unlocked.
- **Parameters:**
  - `assetName`: The name of the item to lock.
  - `howToUnlock`: Description of how to unlock the item.
  - `individualLockedSprite`: A custom sprite for this locked item.

#### `public void Unlock(string assetName, bool notify = true)`
- **Description**: Unlocks an item, making it available for use.
- **Parameters:**
  - `assetName`: The name of the item to unlock.
  - `notify`: Whether to display a notification when the item is unlocked.

#### `public bool IsLocked(string assetName)`
- **Description**: Checks if an item is locked.
- **Parameters:**
  - `assetName`: The name of the item to check.
- **Returns**: `true` if the item is locked, otherwise `false`.

#### `public void ResetUnlockables(bool notify = true)`
- **Description**: Resets all unlockables to their locked state.
- **Parameters:**
  - `notify`: Whether to notify the user about the reset.

#### `public void UnlockAll(bool notify = true)`
- **Description**: Unlocks all registered items.
- **Parameters:**
  - `notify`: Whether to notify the user about the unlock.

#### `public void DeleteKey(string assetName)`
- **Description**: Deletes the key associated with an item, effectively resetting its unlock state.
- **Parameters:**
  - `assetName`: The name of the item whose key should be deleted.

#### `public void DeleteKeys()`
- **Description**: Deletes all keys, resetting the state of all unlockables.
