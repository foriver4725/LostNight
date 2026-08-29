using System;
using System.Collections.Generic;
using UnityEngine;

namespace LostNight
{
    public enum LostItemModelKind
    {
        Umbrella, Glove, Pass, Bottle, Wristwatch, Scarf, Shoe, Recorder, Lunchbox, Book, Mirror, PocketWatch, Jar
    }

    public readonly struct LostItemModelHandle
    {
        public Transform Root { get; }
        public Transform[] Hotspots { get; }
        public LostItemModelHandle(Transform root, Transform[] hotspots) { Root = root; Hotspots = hotspots; }
    }

    public sealed class LostItemModelPresenter : MonoBehaviour
    {
        [Serializable]
        private struct ModelEntry
        {
            public LostItemModelKind kind;
            public Transform root;
            public Transform[] hotspots;
        }

        [SerializeField] private List<ModelEntry> entries = new();
        private readonly Dictionary<LostItemModelKind, LostItemModelHandle> models = new();

        public void Register(LostItemModelKind kind, Transform root, Transform[] hotspots)
        {
            entries.Add(new ModelEntry { kind = kind, root = root, hotspots = hotspots });
            models[kind] = new LostItemModelHandle(root, hotspots); root.gameObject.SetActive(false);
        }

        public LostItemModelHandle Show(LostItemModelKind kind)
        {
            EnsureCache();
            HideAll();
            var model = models[kind];
            model.Root.gameObject.SetActive(true); model.Root.rotation = Quaternion.identity; model.Root.localScale = Vector3.one;
            return model;
        }

        public void HideAll()
        {
            EnsureCache();
            foreach (var model in models.Values) model.Root.gameObject.SetActive(false);
        }

        private void EnsureCache()
        {
            if (models.Count == entries.Count) return;
            models.Clear();
            foreach (var entry in entries) models[entry.kind] = new LostItemModelHandle(entry.root, entry.hotspots);
        }
    }
}
