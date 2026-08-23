using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LostNight
{
    public static class LostItemMockRuntimeBootstrap
    {
        private static Font font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Build()
        {
            if (SceneManager.GetActiveScene().name != "LostItemCenterMock") return;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) Object.Destroy(root);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.07f, .11f, .14f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(.012f, .035f, .045f); RenderSettings.fogDensity = .018f;
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.transform.SetPositionAndRotation(new Vector3(0, 2.15f, -7.8f), Quaternion.Euler(5, 0, 0));
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.015f, .025f, .035f); camera.fieldOfView = 48;
            var light = new GameObject("Desk Lamp").AddComponent<Light>();
            light.type = LightType.Spot; light.color = new Color(1f, .73f, .42f); light.intensity = 850; light.range = 12; light.spotAngle = 75;
            light.transform.SetPositionAndRotation(new Vector3(-2.5f, 4.5f, -2f), Quaternion.Euler(55, 25, 0));

            Primitive(PrimitiveType.Cube, "Back Wall", null, new Vector3(0, 2.2f, 2.3f), new Vector3(12, 5, .2f), new Color(.055f, .09f, .1f));
            var counter = Primitive(PrimitiveType.Cube, "Counter", null, new Vector3(0, -.35f, 0), new Vector3(11, .7f, 4.4f), new Color(.12f, .105f, .08f));
            Style(counter, .05f, .72f);
            var window = Primitive(PrimitiveType.Cube, "Rainy Window", null, new Vector3(0, 2.5f, 2.15f), new Vector3(6.2f, 3.3f, .08f), new Color(.035f, .16f, .22f));
            Style(window, .22f, .92f, new Color(.01f, .12f, .18f));
            for (var i = 0; i < 28; i++)
            {
                var x = -2.9f + (i * 1.73f % 5.8f); var y = 1f + (i * .91f % 3f);
                var rain = Primitive(PrimitiveType.Cube, "Rain Streak", null, new Vector3(x, y, 2.02f), new Vector3(.012f, .18f + i % 4 * .06f, .012f), new Color(.25f, .7f, .82f));
                Style(rain, 0f, .35f, new Color(.03f, .35f, .48f));
            }
            for (var i = 0; i < 7; i++)
            {
                var wetTile = Primitive(PrimitiveType.Cube, "Wet Counter Reflection", null, new Vector3(-3.9f + i * 1.3f, .015f, -.2f), new Vector3(1.18f, .025f, 2.4f), new Color(.055f, .085f, .09f));
                Style(wetTile, .28f, .96f);
            }
            for (var i = -1; i <= 1; i++)
            {
                var body = Primitive(PrimitiveType.Capsule, $"申告者 {i + 2}", null, new Vector3(i * 1.45f, 1.55f, 1.75f), new Vector3(.6f, 1.5f, .35f), new Color(.025f, .04f, .05f));
                Primitive(PrimitiveType.Sphere, "Head", body.transform, new Vector3(0, .95f, 0), Vector3.one * .48f, new Color(.02f, .03f, .04f));
            }
            Primitive(PrimitiveType.Cube, "Ticket Machine", null, new Vector3(3.8f, .45f, -.15f), new Vector3(1.7f, 1.4f, 1.25f), new Color(.42f, .39f, .31f));

            var umbrella = new GameObject("Starry Umbrella").transform; umbrella.position = new Vector3(0, 1.1f, 0);
            var canopy = Primitive(PrimitiveType.Sphere, "Night Sky Canopy", umbrella, Vector3.zero, new Vector3(3.1f, .75f, 3.1f), new Color(.025f, .12f, .28f));
            canopy.GetComponent<Renderer>().material.SetFloat("_Smoothness", .72f);
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * .25f;
                Primitive(PrimitiveType.Cylinder, "Rib", umbrella, new Vector3(Mathf.Sin(a) * .78f, -.18f, Mathf.Cos(a) * .78f), new Vector3(.018f, 1.65f, .018f), new Color(.75f, .8f, .82f), Quaternion.Euler(90, -i * 45, 0));
            }
            Primitive(PrimitiveType.Cylinder, "Shaft", umbrella, new Vector3(0, -.95f, 0), new Vector3(.045f, 1.65f, .045f), new Color(.82f, .82f, .76f));
            for (var i = 0; i < 24; i++)
            {
                var a = i * 2.399f; var radius = .35f + i % 7 * .15f;
                Primitive(PrimitiveType.Sphere, "Star", umbrella, new Vector3(Mathf.Cos(a) * radius, .38f, Mathf.Sin(a) * radius), Vector3.one * (i % 5 == 0 ? .07f : .035f), new Color(.75f, .9f, 1f));
            }
            var hotspots = new[]
            {
                Hotspot(umbrella, "傘布の光", new Vector3(-1.05f, .12f, -1.18f)),
                Hotspot(umbrella, "留め具の光", new Vector3(1.42f, -.12f, .68f)),
                Hotspot(umbrella, "柄の光", new Vector3(.08f, -1.48f, -.08f))
            };

            font = Font.CreateDynamicFontFromOSFont(new[] { "Hiragino Sans", "Yu Gothic", "Arial" }, 32);
            var canvas = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var gameplay = Root(canvas.transform, "Gameplay Screen");
            Panel(gameplay.transform, "Top Shade", new Vector2(0, .84f), Vector2.one, new Color(.01f, .02f, .025f, .9f));
            Label(gameplay.transform, "終電忘れ物センター", new Vector2(.035f, .88f), new Vector2(.48f, .985f), 48, new Color(.9f, .82f, .65f), TextAnchor.MiddleLeft);
            var clock = Label(gameplay.transform, "残り 0:45", new Vector2(.78f, .89f), new Vector2(.965f, .98f), 42, new Color(1f, .42f, .16f), TextAnchor.MiddleRight);
            Label(gameplay.transform, "終電済み　最後の電車は終了しました", new Vector2(.57f, .84f), new Vector2(.965f, .9f), 20, new Color(.7f, .62f, .47f), TextAnchor.MiddleRight);
            Panel(gameplay.transform, "Memo Paper", new Vector2(.025f, .19f), new Vector2(.285f, .72f), new Color(.82f, .76f, .62f, .96f));
            var memo = Label(gameplay.transform, "", new Vector2(.045f, .24f), new Vector2(.265f, .48f), 26, new Color(.13f, .12f, .09f), TextAnchor.UpperLeft);
            var itemLabel = Label(gameplay.transform, "", new Vector2(.045f, .48f), new Vector2(.265f, .7f), 23, new Color(.18f, .14f, .09f), TextAnchor.UpperLeft);
            Panel(gameplay.transform, "Claim Panel", new Vector2(.72f, .2f), new Vector2(.975f, .72f), new Color(.06f, .08f, .075f, .94f));
            var claimLabel = Label(gameplay.transform, "", new Vector2(.74f, .34f), new Vector2(.955f, .68f), 24, new Color(.88f, .82f, .68f), TextAnchor.UpperLeft);
            var claimantA = Action(gameplay.transform, "申告者 A", new Vector2(.735f, .22f), new Vector2(.85f, .32f), new Color(.14f, .22f, .24f));
            var claimantB = Action(gameplay.transform, "申告者 B", new Vector2(.855f, .22f), new Vector2(.97f, .32f), new Color(.14f, .22f, .24f));
            var caseLabel = Label(gameplay.transform, "案件 01", new Vector2(.31f, .72f), new Vector2(.69f, .78f), 23, new Color(.73f, .76f, .7f), TextAnchor.MiddleCenter);
            var progress = Label(gameplay.transform, "", new Vector2(.60f, .73f), new Vector2(.97f, .79f), 21, new Color(.73f, .76f, .7f), TextAnchor.MiddleRight);
            var message = Label(gameplay.transform, "", new Vector2(.22f, .13f), new Vector2(.78f, .19f), 24, new Color(.8f, .88f, .9f), TextAnchor.MiddleCenter);
            var store = Action(gameplay.transform, "保管", new Vector2(.30f, .025f), new Vector2(.49f, .12f), new Color(.22f, .4f, .28f));
            var returnAction = Action(gameplay.transform, "返却", new Vector2(.51f, .025f), new Vector2(.70f, .12f), new Color(.5f, .22f, .18f));

            var title = Panel(canvas.transform, "Title Screen", Vector2.zero, Vector2.one, new Color(.005f, .012f, .018f, .9f)).gameObject;
            Label(title.transform, "終電\n忘れ物センター", new Vector2(.16f, .47f), new Vector2(.66f, .82f), 86, new Color(.92f, .83f, .66f), TextAnchor.MiddleLeft);
            Label(title.transform, "忘れ物は、<color=#E9B85F>記憶のカケラ</color>です。\n終電後の窓口で、<color=#7ED6E6>証言</color>と<color=#E57668>怪異</color>を照合せよ。", new Vector2(.17f, .34f), new Vector2(.7f, .49f), 28, new Color(.58f, .76f, .8f), TextAnchor.UpperLeft);
            var start = Action(title.transform, "業務を開始する", new Vector2(.17f, .18f), new Vector2(.43f, .29f), new Color(.38f, .18f, .1f));
            Label(title.transform, "5件正解で業務完了 / 誤判断3件で業務停止", new Vector2(.17f, .11f), new Vector2(.62f, .17f), 20, new Color(.7f, .65f, .55f), TextAnchor.MiddleLeft);

            var result = Panel(canvas.transform, "Case Result Screen", new Vector2(.22f, .19f), new Vector2(.78f, .78f), new Color(.025f, .04f, .045f, .97f)).gameObject;
            var resultTitle = Label(result.transform, "判定結果", new Vector2(.08f, .7f), new Vector2(.92f, .92f), 48, new Color(.95f, .72f, .4f), TextAnchor.MiddleCenter);
            var resultBody = Label(result.transform, "", new Vector2(.09f, .24f), new Vector2(.91f, .7f), 27, new Color(.86f, .84f, .75f), TextAnchor.MiddleCenter);
            var continueAction = Action(result.transform, "次へ", new Vector2(.32f, .07f), new Vector2(.68f, .2f), new Color(.34f, .26f, .12f));

            var ending = Panel(canvas.transform, "Ending Screen", Vector2.zero, Vector2.one, new Color(.005f, .012f, .018f, .96f)).gameObject;
            var endingTitle = Label(ending.transform, "業務完了", new Vector2(.2f, .62f), new Vector2(.8f, .82f), 72, new Color(.92f, .75f, .46f), TextAnchor.MiddleCenter);
            var endingBody = Label(ending.transform, "", new Vector2(.24f, .3f), new Vector2(.76f, .62f), 30, new Color(.82f, .84f, .78f), TextAnchor.MiddleCenter);
            var retry = Action(ending.transform, "もう一度", new Vector2(.28f, .16f), new Vector2(.48f, .26f), new Color(.32f, .24f, .1f));
            var titleAction = Action(ending.transform, "タイトルへ", new Vector2(.52f, .16f), new Vector2(.72f, .26f), new Color(.12f, .25f, .28f));

            var view = new GameObject("Screen View").AddComponent<LostNightScreenView>();
            view.Initialize(title, gameplay, result, ending, clock, caseLabel, memo, message, itemLabel, claimLabel, progress,
                resultTitle, resultBody, endingTitle, endingBody, start, claimantA, claimantB, returnAction, store,
                continueAction, retry, titleAction);
            var controller = new GameObject("Lost Night Game Controller").AddComponent<LostItemMockController>();
            controller.Initialize(umbrella, hotspots, view);
        }

        private static Image Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color) { var i = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>(); i.transform.SetParent(parent, false); Stretch(i.rectTransform, min, max, new Vector2(8, 8)); i.color = color; return i; }
        private static Text Label(Transform parent, string value, Vector2 min, Vector2 max, int size, Color color, TextAnchor anchor) { var t = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(parent, false); Stretch(t.rectTransform, min, max, Vector2.zero); t.font = font; t.text = value; t.fontSize = size; t.color = color; t.alignment = anchor; t.resizeTextForBestFit = true; t.resizeTextMinSize = 14; t.resizeTextMaxSize = size; return t; }
        private static Button Action(Transform parent, string value, Vector2 min, Vector2 max, Color color) { var i = Panel(parent, value + " Button", min, max, color); var b = i.gameObject.AddComponent<Button>(); Label(i.transform, value, Vector2.zero, Vector2.one, 34, new Color(.95f, .9f, .78f), TextAnchor.MiddleCenter); return b; }
        private static GameObject Root(Transform parent, string name) { var root = new GameObject(name, typeof(RectTransform)); root.transform.SetParent(parent, false); Stretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero); return root; }
        private static Transform Hotspot(Transform parent, string name, Vector3 position) { var go = Primitive(PrimitiveType.Sphere, name, parent, position, Vector3.one * .2f, new Color(.2f, .9f, 1f)); var material = go.GetComponent<Renderer>().material; material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", new Color(.15f, 1.2f, 1.6f)); return go.transform; }
        private static void Style(GameObject target, float metallic, float smoothness, Color emission = default) { var material = target.GetComponent<Renderer>().material; material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness); if (emission == default) return; material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", emission); }
        private static void Stretch(RectTransform r, Vector2 min, Vector2 max, Vector2 pad) { r.anchorMin = min; r.anchorMax = max; r.offsetMin = pad; r.offsetMax = -pad; }
        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color, Quaternion rotation = default) { var go = GameObject.CreatePrimitive(type); go.name = name; if (parent) go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.transform.localRotation = rotation == default ? Quaternion.identity : rotation; var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); go.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color }; return go; }
    }
}
