using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class UnlockablesController : MonoBehaviour
{
    //Use it to access the class from anywhere in your mod
    public static UnlockablesController Main
    {
        get
        {
            if (!_instance)
            {
                _instance = new GameObject("UNLOCKABLES CONTROLLER").AddComponent<UnlockablesController>();
            }
            return _instance;
        }
    }
    private static UnlockablesController _instance;

    //list of unlockables
    private List<Unlockable> _unlockables = new List<Unlockable>();
    public List<Unlockable> unlockables => _unlockables;

    private Scrollbar _scrollbar = UnityEngine.Object.FindObjectOfType<Scrollbar>();
    private List<SpawnableAsset> _allAssets;

    //mod tag to prevent conflict between mods
    public string modTag;
    //sprite that locked items will have
    public Sprite lockedSprite;

    public void Start()
    {
        if (modTag == null) throw new Exception("modTag is null");
        if (lockedSprite == null) throw new Exception("lockedSprite is null");
        StartCoroutine(Utils.NextFrameCoroutine(() =>
        {
            _allAssets = UnityEngine.Object.FindObjectsOfType<SpawnableAsset>().ToList();
            foreach (var unlockable in unlockables)
            {
                _UpdateCatalog(unlockable.originalName, false);
            }
        }));
    }

    /// <summary>
    /// Register item as unlockable
    /// </summary>
    /// <param name="assetName">SpawnableAsset</param>
    /// <param name="howToUnlock">Unlock instructions</param>
    /// <param name="individualLockedSprite">Override locked sprite</param>
    public void RegisterUnlockable(string assetName, string howToUnlock = "This item has not been unlocked yet!", Sprite individualLockedSprite = null)
    {
        _unlockables.Add(new Unlockable(assetName, howToUnlock, individualLockedSprite));
        if (_GetKey(assetName) == null)
        {
            _SetKey(assetName, false);
        }
    }

    /// <summary>
    /// Lock item. Unregistered items won't save
    /// </summary>
    /// <param name="assetName">SpawnableAsset</param>
    /// <param name="howToUnlock">Unlock instructions</param>
    /// <param name="individualLockedSprite">Override locked sprite</param>
    public void Lock(string assetName, string howToUnlock = "This item has not been unlocked yet!", Sprite individualLockedSprite = null)
    {
        if (CatalogBehaviour.Main.SelectedItem.name == assetName && _allAssets.Count > 0)
        {
            CatalogBehaviour.Main.SetItem(_allAssets.Where(item => !_unlockables.Select(u => u.spawnableAsset).Contains(item)).ToArray().PickRandom());
        }
        if (_GetKey(assetName) == null)
        {
            RegisterUnlockable(assetName, howToUnlock, individualLockedSprite);
            _UpdateCatalog(assetName, false);
        }
        else
        {
            _SetKey(assetName, false);
            var item = _unlockables[_unlockables.FindIndex(i => i.originalName == assetName)];
            item.howToUnlock = howToUnlock;
            if (individualLockedSprite) item.individualLockedSprite = individualLockedSprite;
            _UpdateCatalog(assetName, false);
        }
    }

    /// <summary>
    /// Unlock item
    /// </summary>
    /// <param name="assetName">SpawnableAsset</param>
    /// <param name="notify">Should notify and switch category?</param>
    public void Unlock(string assetName, bool notify = true)
    {
        if (IsLocked(assetName))
        {
            _SetKey(assetName, true);
            if (notify)
            {
                ModAPI.Notify($"<color=green><size=135%>Unlocked {assetName}!");
            }
            _UpdateCatalog(assetName, notify);
        }
    }

    /// <summary>
    /// Check if item is locked
    /// </summary>
    /// <param name="assetName">SpawnableAsset's name</param>
    /// <returns>Returns true if item is locked</returns>
    public bool IsLocked(string assetName)
    {
        return !_GetKey(assetName) ?? false;
    }

    /// <summary>
    /// Reset all unlockables back to locked
    /// </summary>
    /// <param name="notify">Should notify?</param>
    public void ResetUnlockables(bool notify = true)
    {
        foreach (var item in _unlockables)
        {
            Lock(item.originalName, item.howToUnlock, item.individualLockedSprite);
        }
        if (notify) ModAPI.Notify($"<color=red><size=175%>Unlockables were reset!");
    }

    /// <summary>
    /// Unlock all
    /// </summary>
    /// <param name="notify">Should notify?</param>
    public void UnlockAll(bool notify = true)
    {
        foreach (var item in unlockables)
        {
            Unlock(item.originalName, false);
        }
        if (notify) ModAPI.Notify("<color=green><size=175%>EVERYTHING UNLOCKED!!!");
    }

    /// <summary>
    /// Unlocks item and deletes key. Use for fixing mistakes, not as Unlock()
    /// </summary>
    /// <param name="assetName"></param>
    public void DeleteKey(string assetName)
    {
        PlayerPrefs.DeleteKey(_ToKey(assetName));
        ModAPI.Notify($"<color=orange>Key deleted: " + assetName);
        _UpdateCatalog(assetName, false);
    }

    /// <summary>
    /// Deletes all keys. Use if you want to reset the 
    /// </summary>
    public void DeleteKeys()
    {
        foreach (var item in _unlockables)
        {
            DeleteKey(item.originalName);
        }
        _unlockables = new List<Unlockable>();
        ModAPI.Notify("<color=orange><size=200%>KEYS DELETED");
    }

    //Update item's button
    private void _UpdateCatalog(string assetName, bool changeCategory)
    {
        var unlockable = unlockables.Find(u => u.originalName == assetName);
        unlockable.SetValues();

        var scrollBuffer = _scrollbar?.value ?? 1;
        var categoryBuffer = CatalogBehaviour.Main.SelectedCategory;
        CatalogBehaviour.Main.SetCategory(unlockable.spawnableAsset.Category);

        SpawnableAsset item;
        if (IsLocked(unlockable.originalName))
        {
            item = ScriptableObject.CreateInstance<SpawnableAsset>();
            item.name = "<color=orange>LOCKED: <color=white>" + unlockable.originalName;
            item.Description = unlockable.howToUnlock;
            item.ViewSprite = unlockable.individualLockedSprite ?? lockedSprite;
            item.Category = unlockable.spawnableAsset.Category;
        }
        else
        {
            item = unlockable.spawnableAsset;
        }

        var button = UnityEngine.Object.FindObjectsOfType<ItemButtonBehaviour>().ToList().Find(i => i.Item.name.Contains(unlockable.originalName));
        button.Item = item;
        button.Invoke("Start", 0);

        if (!changeCategory)
        {
            CatalogBehaviour.Main.SetCategory(categoryBuffer);
            StartCoroutine(Utils.NextFrameCoroutine(() =>
            {
                StartCoroutine(Utils.NextFrameCoroutine(() =>
                {
                    if (_scrollbar) _scrollbar.value = scrollBuffer;
                }));
            }));
        }
    }

    //key control
    private string _ToKey(string assetName)
    {
        return $"UNLOCKABLE {modTag} {assetName}";
    }
    private void _SetKey(string assetName, bool value)
    {
        PlayerPrefs.SetInt(_ToKey(assetName), value ? 1 : 0);
    }
    private bool? _GetKey(string assetName)
    {
        if (PlayerPrefs.HasKey(_ToKey(assetName)))
        {
            return PlayerPrefs.GetInt(_ToKey(assetName)) == 1;
        }
        return null;
    }


    public class Unlockable : MonoBehaviour
    {
        public SpawnableAsset spawnableAsset;
        public string originalName;
        public Sprite originalSprite;
        public Sprite individualLockedSprite;
        public string originalDescription;
        public string howToUnlock;

        private bool _updated = false;

        public Unlockable(string originalName, string howToUnlock, Sprite individualLockedSprite)
        {
            this.originalName = originalName;
            this.howToUnlock = howToUnlock;
            this.individualLockedSprite = individualLockedSprite;
        }
        public void SetValues()
        {
            if (!_updated)
            {
                this.spawnableAsset = ModAPI.FindSpawnable(this.originalName);
                this.originalSprite = this.spawnableAsset.ViewSprite;
                this.originalDescription = this.spawnableAsset.Description;
                _updated = true;
            }
        }
    }
}
