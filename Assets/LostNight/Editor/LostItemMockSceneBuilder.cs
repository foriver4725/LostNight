using System.IO;
using LostNight;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LostNight.Editor
{
    public static class LostItemMockSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/LostItemCenterMock.unity";
        private static Font uiFont;

        [InitializeOnLoadMethod]
        private static void BuildFirstMockAutomatically()
        {
            if (!File.Exists(ScenePath)) EditorApplication.delayCall += Build;
        }

        [MenuItem("Lost Night/Build Lost Item Center Mock")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupEnvironment();
            var umbrella = CreateUmbrella();
            var controller = BuildUi(umbrella);
            Selection.activeObject = controller;
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"Created mock scene: {ScenePath}");
        }

        private static void SetupEnvironment()
        {
            RenderSettings.ambientLight = new Color(.07f, .11f, .14f);
            RenderSettings.ambientMode = AmbientMode.Flat;
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.SetPositionAndRotation(new Vector3(0, 2.15f, -7.8f), Quaternion.Euler(5, 0, 0));
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.015f, .025f, .035f);
            camera.fieldOfView = 48;

            var light = new GameObject("Desk Lamp").AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, .73f, .42f);
            light.intensity = 850;
            light.range = 12;
            light.spotAngle = 75;
            light.transform.SetPositionAndRotation(new Vector3(-2.5f, 4.5f, -2f), Quaternion.Euler(55, 25, 0));

            Cube("Back Wall", new Vector3(0, 2.2f, 2.3f), new Vector3(12, 5, .2f), new Color(.055f, .09f, .1f));
            Cube("Counter", new Vector3(0, -.35f, 0), new Vector3(11, .7f, 4.4f), new Color(.12f, .105f, .08f));
            Cube("Window", new Vector3(0, 2.5f, 2.15f), new Vector3(6.2f, 3.3f, .08f), new Color(.06f, .22f, .27f));
            for (var i = -1; i <= 1; i++)
            {
                var silhouette = Capsule($"申告者 {i + 2}", new Vector3(i * 1.45f, 1.55f, 1.75f), new Vector3(.6f, 1.5f, .35f), new Color(.025f, .04f, .05f));
                Sphere("Head", silhouette.transform, new Vector3(0, .95f, 0), Vector3.one * .48f, new Color(.02f, .03f, .04f));
            }
            Cube("Ticket Machine", new Vector3(3.8f, .45f, -.15f), new Vector3(1.7f, 1.4f, 1.25f), new Color(.42f, .39f, .31f));
        }

        private static Transform CreateUmbrella()
        {
            var root = new GameObject("Starry Umbrella").transform;
            root.position = new Vector3(0, 1.1f, 0);
            var canopy = Sphere("Night Sky Canopy", root, Vector3.zero, new Vector3(3.1f, .75f, 3.1f), new Color(.025f, .12f, .28f));
            var renderer = canopy.GetComponent<Renderer>();
            renderer.material.SetFloat("_Smoothness", .72f);
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * .25f;
                Cylinder("Rib", root, new Vector3(Mathf.Sin(a) * .78f, -.18f, Mathf.Cos(a) * .78f), new Vector3(.018f, 1.65f, .018f), new Color(.75f, .8f, .82f), Quaternion.Euler(90, -i * 45, 0));
            }
            Cylinder("Shaft", root, new Vector3(0, -.95f, 0), new Vector3(.045f, 1.65f, .045f), new Color(.82f, .82f, .76f), Quaternion.identity);
            for (var i = 0; i < 24; i++)
            {
                var a = i * 2.399f;
                var radius = .35f + (i % 7) * .15f;
                Sphere("Star", root, new Vector3(Mathf.Cos(a) * radius, .38f, Mathf.Sin(a) * radius), Vector3.one * (i % 5 == 0 ? .07f : .035f), new Color(.75f, .9f, 1f));
            }
            return root;
        }

        private static LostItemMockController BuildUi(Transform umbrella)
        {
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Hiragino Sans", "Yu Gothic", "Arial" }, 32);
            var canvas = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            Panel(canvas.transform, "Top Shade", new Vector2(0, .84f), Vector2.one, new Color(.01f, .02f, .025f, .9f));
            Text(canvas.transform, "終電忘れ物センター", new Vector2(.035f, .88f), new Vector2(.48f, .985f), 48, new Color(.9f, .82f, .65f), TextAnchor.MiddleLeft);
            var clock = Text(canvas.transform, "0:13", new Vector2(.82f, .89f), new Vector2(.965f, .98f), 48, new Color(1f, .42f, .16f), TextAnchor.MiddleRight);
            Text(canvas.transform, "終電済み　最後の電車は終了しました", new Vector2(.57f, .84f), new Vector2(.965f, .9f), 20, new Color(.7f, .62f, .47f), TextAnchor.MiddleRight);

            Panel(canvas.transform, "Memo Paper", new Vector2(.025f, .19f), new Vector2(.285f, .72f), new Color(.82f, .76f, .62f, .96f));
            var memo = Text(canvas.transform, "", new Vector2(.045f, .24f), new Vector2(.265f, .68f), 26, new Color(.13f, .12f, .09f), TextAnchor.UpperLeft);
            Text(canvas.transform, "本日の忘れ物\n透明傘　一本\n\n『忘れ物は、記憶のカケラです。』", new Vector2(.045f, .48f), new Vector2(.265f, .7f), 23, new Color(.18f, .14f, .09f), TextAnchor.UpperLeft);

            Panel(canvas.transform, "Claim Panel", new Vector2(.72f, .2f), new Vector2(.975f, .72f), new Color(.06f, .08f, .075f, .94f));
            Text(canvas.transform, "申告者 A　会社員\n『透明な傘です。普通の傘でした』\n\n申告者 B　子どもの影\n『持ち手に、かんだあとがある』", new Vector2(.745f, .25f), new Vector2(.95f, .67f), 25, new Color(.88f, .82f, .68f), TextAnchor.UpperLeft);

            var caseText = Text(canvas.transform, "案件 01", new Vector2(.31f, .72f), new Vector2(.69f, .78f), 23, new Color(.73f, .76f, .7f), TextAnchor.MiddleCenter);
            var message = Text(canvas.transform, "傘をドラッグして、持ち主の記憶を探してください", new Vector2(.25f, .13f), new Vector2(.75f, .19f), 24, new Color(.8f, .88f, .9f), TextAnchor.MiddleCenter);

            var record = Button(canvas.transform, "記録", new Vector2(.15f, .025f), new Vector2(.33f, .12f), new Color(.2f, .36f, .48f));
            var observe = Button(canvas.transform, "観察", new Vector2(.34f, .025f), new Vector2(.52f, .12f), new Color(.43f, .36f, .2f));
            var store = Button(canvas.transform, "保管", new Vector2(.53f, .025f), new Vector2(.71f, .12f), new Color(.22f, .4f, .28f));
            var returnButton = Button(canvas.transform, "返却", new Vector2(.72f, .025f), new Vector2(.9f, .12f), new Color(.5f, .22f, .18f));

            var controller = new GameObject("Lost Item Mock Controller").AddComponent<LostItemMockController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("itemRoot").objectReferenceValue = umbrella;
            serialized.FindProperty("clockText").objectReferenceValue = clock;
            serialized.FindProperty("caseText").objectReferenceValue = caseText;
            serialized.FindProperty("memoText").objectReferenceValue = memo;
            serialized.FindProperty("messageText").objectReferenceValue = message;
            serialized.FindProperty("recordButton").objectReferenceValue = record;
            serialized.FindProperty("observeButton").objectReferenceValue = observe;
            serialized.FindProperty("returnButton").objectReferenceValue = returnButton;
            serialized.FindProperty("storeButton").objectReferenceValue = store;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return controller;
        }

        private static Image Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false); Stretch(image.rectTransform, min, max, new Vector2(8, 8)); image.color = color; return image;
        }

        private static Text Text(Transform parent, string value, Vector2 min, Vector2 max, int size, Color color, TextAnchor anchor)
        {
            var text = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false); Stretch(text.rectTransform, min, max, Vector2.zero); text.font = uiFont; text.text = value; text.fontSize = size; text.color = color; text.alignment = anchor; text.resizeTextForBestFit = true; text.resizeTextMinSize = 14; text.resizeTextMaxSize = size; return text;
        }

        private static Button Button(Transform parent, string label, Vector2 min, Vector2 max, Color color)
        {
            var image = Panel(parent, label + " Button", min, max, color);
            var button = image.gameObject.AddComponent<Button>();
            Text(image.transform, label, Vector2.zero, Vector2.one, 34, new Color(.95f, .9f, .78f), TextAnchor.MiddleCenter);
            return button;
        }

        private static void Stretch(RectTransform rect, Vector2 min, Vector2 max, Vector2 padding) { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = padding; rect.offsetMax = -padding; }
        private static GameObject Cube(string name, Vector3 pos, Vector3 scale, Color color) => Primitive(PrimitiveType.Cube, name, null, pos, scale, color, Quaternion.identity);
        private static GameObject Capsule(string name, Vector3 pos, Vector3 scale, Color color) => Primitive(PrimitiveType.Capsule, name, null, pos, scale, color, Quaternion.identity);
        private static GameObject Sphere(string name, Transform parent, Vector3 pos, Vector3 scale, Color color) => Primitive(PrimitiveType.Sphere, name, parent, pos, scale, color, Quaternion.identity);
        private static GameObject Cylinder(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, Quaternion rotation) => Primitive(PrimitiveType.Cylinder, name, parent, pos, scale, color, rotation);
        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color, Quaternion rotation)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; if (parent) go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.transform.localRotation = rotation;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); var material = new Material(shader) { color = color }; go.GetComponent<Renderer>().sharedMaterial = material; return go;
        }
    }
}
