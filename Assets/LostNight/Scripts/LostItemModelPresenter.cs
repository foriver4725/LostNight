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

    public sealed class LostItemModelPresenter
    {
        private readonly Dictionary<LostItemModelKind, LostItemModelHandle> models = new();

        public void Register(LostItemModelKind kind, Transform root, Transform[] hotspots)
        {
            models.Add(kind, new LostItemModelHandle(root, hotspots)); root.gameObject.SetActive(false);
        }

        public LostItemModelHandle Show(LostItemModelKind kind)
        {
            HideAll();
            var model = models[kind];
            model.Root.gameObject.SetActive(true); model.Root.rotation = Quaternion.identity; model.Root.localScale = Vector3.one;
            return model;
        }

        public void HideAll()
        {
            foreach (var model in models.Values) model.Root.gameObject.SetActive(false);
        }
    }
}
