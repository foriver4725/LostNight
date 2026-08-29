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

            var modelPresenter = new LostItemModelPresenter();
            var umbrella = new GameObject("Starry Umbrella").transform; umbrella.position = new Vector3(0, 1.1f, 0);
            var canopy = UmbrellaCanopy(umbrella);
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI * .25f;
                var rim = new Vector3(Mathf.Sin(a) * 1.5f, -.01f, Mathf.Cos(a) * 1.5f);
                CylinderBetween(umbrella, "Rib", new Vector3(0, .5f, 0), rim, .012f, new Color(.65f, .72f, .75f));
            }
            CylinderBetween(umbrella, "Shaft", new Vector3(0, .64f, 0), new Vector3(0, -1.42f, 0), .035f, new Color(.72f, .75f, .72f));
            var handlePoints = new[]
            {
                new Vector3(0, -1.4f, 0), new Vector3(.02f, -1.58f, 0), new Vector3(.11f, -1.74f, 0),
                new Vector3(.27f, -1.82f, 0), new Vector3(.44f, -1.76f, 0), new Vector3(.51f, -1.61f, 0)
            };
            for (var i = 0; i < handlePoints.Length - 1; i++)
                CylinderBetween(umbrella, "Curved Handle", handlePoints[i], handlePoints[i + 1], .065f, new Color(.28f, .16f, .08f));
            Primitive(PrimitiveType.Sphere, "Top Cap", umbrella, new Vector3(0, .57f, 0), Vector3.one * .12f, new Color(.72f, .75f, .72f));
            for (var i = 0; i < 24; i++)
            {
                var a = i * 2.399f; var radius = .35f + i % 7 * .15f;
                var height = .5f * (1f - Mathf.Pow(radius / 1.5f, 1.45f)) + .025f;
                Primitive(PrimitiveType.Sphere, "Star", umbrella, new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius), Vector3.one * (i % 5 == 0 ? .055f : .028f), new Color(.75f, .9f, 1f));
            }
            var hotspots = new[]
            {
                Hotspot(umbrella, "傘布の光", new Vector3(-1.05f, .12f, -1.18f)),
                Hotspot(umbrella, "留め具の光", new Vector3(1.42f, -.12f, .68f)),
                Hotspot(umbrella, "柄の光", new Vector3(.28f, -1.82f, -.08f))
            };
            modelPresenter.Register(LostItemModelKind.Umbrella, umbrella, hotspots);
            RegisterPropModels(modelPresenter);

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
            var audioService = new GameObject("Audio Service").AddComponent<LostNightAudio>(); audioService.Initialize();
            var controller = new GameObject("Lost Night Game Controller").AddComponent<LostItemMockController>();
            controller.Initialize(modelPresenter, view, audioService);
        }

        private static Image Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color) { var i = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>(); i.transform.SetParent(parent, false); Stretch(i.rectTransform, min, max, new Vector2(8, 8)); i.color = color; return i; }
        private static Text Label(Transform parent, string value, Vector2 min, Vector2 max, int size, Color color, TextAnchor anchor) { var t = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<Text>(); t.transform.SetParent(parent, false); Stretch(t.rectTransform, min, max, Vector2.zero); t.font = font; t.text = value; t.fontSize = size; t.color = color; t.alignment = anchor; t.resizeTextForBestFit = true; t.resizeTextMinSize = 14; t.resizeTextMaxSize = size; return t; }
        private static Button Action(Transform parent, string value, Vector2 min, Vector2 max, Color color) { var i = Panel(parent, value + " Button", min, max, color); var b = i.gameObject.AddComponent<Button>(); Label(i.transform, value, Vector2.zero, Vector2.one, 34, new Color(.95f, .9f, .78f), TextAnchor.MiddleCenter); return b; }
        private static GameObject Root(Transform parent, string name) { var root = new GameObject(name, typeof(RectTransform)); root.transform.SetParent(parent, false); Stretch(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero); return root; }
        private static Transform Hotspot(Transform parent, string name, Vector3 position) { var go = Primitive(PrimitiveType.Sphere, name, parent, position, Vector3.one * .2f, new Color(.2f, .9f, 1f)); var material = go.GetComponent<Renderer>().material; material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", new Color(.15f, 1.2f, 1.6f)); return go.transform; }
        private static void Style(GameObject target, float metallic, float smoothness, Color emission = default) { var material = target.GetComponent<Renderer>().material; material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness); if (emission == default) return; material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", emission); }
        private static GameObject UmbrellaCanopy(Transform parent)
        {
            const int segments = 32; const int rings = 5; const float radius = 1.55f;
            var vertices = new Vector3[1 + segments * rings]; var triangles = new int[segments * 3 + segments * (rings - 1) * 6];
            vertices[0] = new Vector3(0, .52f, 0);
            for (var ring = 1; ring <= rings; ring++)
            for (var segment = 0; segment < segments; segment++)
            {
                var t = ring / (float)rings; var angle = segment * Mathf.PI * 2f / segments;
                var scallop = ring == rings ? 1f - .055f * Mathf.Abs(Mathf.Sin(angle * 4f)) : 1f;
                var r = radius * t * scallop; var y = .52f * (1f - Mathf.Pow(t, 1.45f));
                vertices[1 + (ring - 1) * segments + segment] = new Vector3(Mathf.Sin(angle) * r, y, Mathf.Cos(angle) * r);
            }
            var triangle = 0;
            for (var segment = 0; segment < segments; segment++)
            {
                var next = (segment + 1) % segments;
                triangles[triangle++] = 0; triangles[triangle++] = 1 + segment; triangles[triangle++] = 1 + next;
            }
            for (var ring = 1; ring < rings; ring++)
            for (var segment = 0; segment < segments; segment++)
            {
                var next = (segment + 1) % segments; var inner = 1 + (ring - 1) * segments; var outer = inner + segments;
                triangles[triangle++] = inner + segment; triangles[triangle++] = outer + segment; triangles[triangle++] = outer + next;
                triangles[triangle++] = inner + segment; triangles[triangle++] = outer + next; triangles[triangle++] = inner + next;
            }
            var mesh = new Mesh { name = "Scalloped Umbrella Canopy", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var go = new GameObject("Night Sky Canopy", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            go.transform.SetParent(parent, false); go.GetComponent<MeshFilter>().sharedMesh = mesh; go.GetComponent<MeshCollider>().sharedMesh = mesh;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var canopyMaterial = new Material(shader) { color = new Color(.018f, .09f, .22f), doubleSidedGI = true };
            canopyMaterial.SetFloat("_Cull", 0f); go.GetComponent<MeshRenderer>().sharedMaterial = canopyMaterial;
            var innerTriangles = (int[])triangles.Clone();
            for (var i = 0; i < innerTriangles.Length; i += 3)
                (innerTriangles[i + 1], innerTriangles[i + 2]) = (innerTriangles[i + 2], innerTriangles[i + 1]);
            var innerMesh = new Mesh { name = "Umbrella Inner Lining", vertices = vertices, triangles = innerTriangles };
            innerMesh.RecalculateNormals(); innerMesh.RecalculateBounds();
            var lining = new GameObject("Inner Lining", typeof(MeshFilter), typeof(MeshRenderer)); lining.transform.SetParent(go.transform, false);
            lining.transform.localPosition = Vector3.down * .012f; lining.GetComponent<MeshFilter>().sharedMesh = innerMesh;
            var liningMaterial = new Material(shader) { color = new Color(.055f, .16f, .28f) };
            liningMaterial.SetFloat("_Metallic", .05f); liningMaterial.SetFloat("_Smoothness", .48f);
            lining.GetComponent<MeshRenderer>().sharedMaterial = liningMaterial;
            Style(go, .12f, .78f, new Color(.004f, .025f, .08f)); return go;
        }
        private static GameObject CylinderBetween(Transform parent, string name, Vector3 start, Vector3 end, float radius, Color color)
        {
            var direction = end - start; var go = Primitive(PrimitiveType.Cylinder, name, parent, (start + end) * .5f,
                new Vector3(radius, direction.magnitude * .5f, radius), color, Quaternion.FromToRotation(Vector3.up, direction));
            Style(go, .65f, .72f); return go;
        }
        private static void RegisterPropModels(LostItemModelPresenter presenter)
        {
            CreatePropModel(presenter, LostItemModelKind.Glove, "Warm Glove", new Color(.34f, .12f, .08f));
            CreatePropModel(presenter, LostItemModelKind.Pass, "Vanishing Pass", new Color(.22f, .48f, .55f));
            CreatePropModel(presenter, LostItemModelKind.Bottle, "Rain Bottle", new Color(.12f, .32f, .38f));
            CreatePropModel(presenter, LostItemModelKind.Wristwatch, "Delayed Wristwatch", new Color(.3f, .24f, .16f));
            CreatePropModel(presenter, LostItemModelKind.Scarf, "Calling Scarf", new Color(.52f, .07f, .06f));
            CreatePropModel(presenter, LostItemModelKind.Shoe, "Sea Shoe", new Color(.2f, .09f, .045f));
            CreatePropModel(presenter, LostItemModelKind.Recorder, "Voice Recorder", new Color(.16f, .18f, .19f));
            CreatePropModel(presenter, LostItemModelKind.Lunchbox, "Cold Lunchbox", new Color(.5f, .24f, .2f));
            CreatePropModel(presenter, LostItemModelKind.Book, "Growing Book", new Color(.08f, .22f, .34f));
            CreatePropModel(presenter, LostItemModelKind.Mirror, "Moonless Mirror", new Color(.2f, .38f, .46f));
            CreatePropModel(presenter, LostItemModelKind.PocketWatch, "Reverse Pocket Watch", new Color(.55f, .38f, .12f));
            CreatePropModel(presenter, LostItemModelKind.Jar, "Footstep Jar", new Color(.18f, .34f, .42f));
        }

        private static void CreatePropModel(LostItemModelPresenter presenter, LostItemModelKind kind, string name, Color color)
        {
            var root = new GameObject(name).transform; root.position = new Vector3(0, 1.05f, 0);
            Vector3[] points;
            switch (kind)
            {
                case LostItemModelKind.Glove:
                    Primitive(PrimitiveType.Cube, "Palm", root, Vector3.zero, new Vector3(1.05f, .28f, 1.2f), color);
                    for (var i = 0; i < 4; i++) Primitive(PrimitiveType.Capsule, "Finger", root, new Vector3(-.42f + i * .28f, .02f, .82f), new Vector3(.18f, .48f + i * .04f, .18f), color, Quaternion.Euler(90, 0, 0));
                    Primitive(PrimitiveType.Capsule, "Thumb", root, new Vector3(.64f, 0, .15f), new Vector3(.2f, .45f, .2f), color, Quaternion.Euler(55, 0, -35));
                    points = new[] { new Vector3(-.38f, -.18f, -.45f), new Vector3(.62f, .05f, .2f), new Vector3(.12f, .1f, .92f) }; break;
                case LostItemModelKind.Pass:
                    Primitive(PrimitiveType.Cube, "Card", root, Vector3.zero, new Vector3(2.1f, .1f, 1.2f), color);
                    Primitive(PrimitiveType.Cube, "Magnetic Strip", root, new Vector3(0, -.07f, -.35f), new Vector3(1.7f, .025f, .18f), new Color(.04f, .04f, .05f));
                    points = new[] { new Vector3(-.72f, .08f, .3f), new Vector3(.02f, .08f, -.3f), new Vector3(.72f, .08f, .28f) }; break;
                case LostItemModelKind.Bottle:
                    Primitive(PrimitiveType.Cylinder, "Body", root, new Vector3(0, 0, 0), new Vector3(.62f, 1.05f, .62f), color);
                    Primitive(PrimitiveType.Cylinder, "Cap", root, new Vector3(0, 1.18f, 0), new Vector3(.45f, .16f, .45f), new Color(.08f, .22f, .28f));
                    points = new[] { new Vector3(-.58f, .45f, -.15f), new Vector3(.46f, -.25f, -.32f), new Vector3(.1f, 1.35f, 0) }; break;
                case LostItemModelKind.Wristwatch:
                    Primitive(PrimitiveType.Cube, "Upper Strap", root, new Vector3(0, .95f, .08f), new Vector3(.48f, 1.15f, .12f), color);
                    Primitive(PrimitiveType.Cube, "Lower Strap", root, new Vector3(0, -.95f, .08f), new Vector3(.48f, 1.15f, .12f), color);
                    Primitive(PrimitiveType.Cylinder, "Watch Face", root, Vector3.zero, new Vector3(.78f, .18f, .78f), new Color(.72f, .68f, .52f), Quaternion.Euler(90, 0, 0));
                    points = new[] { new Vector3(-.48f, .3f, -.08f), new Vector3(.45f, -.22f, -.08f), new Vector3(0, .98f, -.08f) }; break;
                case LostItemModelKind.Scarf:
                    for (var i = 0; i < 7; i++) Primitive(PrimitiveType.Cube, "Fabric", root, new Vector3(-1.25f + i * .4f, Mathf.Sin(i * .9f) * .32f, 0), new Vector3(.48f, .12f, .48f), color, Quaternion.Euler(0, i * 8f, Mathf.Cos(i) * 12f));
                    points = new[] { new Vector3(-1.18f, .05f, -.28f), new Vector3(.05f, -.28f, -.28f), new Vector3(1.18f, -.12f, -.28f) }; break;
                case LostItemModelKind.Shoe:
                    Primitive(PrimitiveType.Cube, "Sole", root, new Vector3(0, -.45f, 0), new Vector3(1.2f, .22f, 2.25f), new Color(.06f, .05f, .045f));
                    Primitive(PrimitiveType.Capsule, "Upper", root, new Vector3(0, .12f, .18f), new Vector3(.82f, .85f, 1.15f), color, Quaternion.Euler(90, 0, 0));
                    points = new[] { new Vector3(-.52f, -.55f, -.65f), new Vector3(.42f, .2f, -.42f), new Vector3(0, -.48f, .92f) }; break;
                case LostItemModelKind.Recorder:
                    Primitive(PrimitiveType.Cube, "Recorder Body", root, Vector3.zero, new Vector3(1.8f, .65f, 1.18f), color);
                    Primitive(PrimitiveType.Cylinder, "Left Reel", root, new Vector3(-.48f, .38f, -.2f), new Vector3(.32f, .08f, .32f), new Color(.72f, .62f, .3f));
                    Primitive(PrimitiveType.Cylinder, "Right Reel", root, new Vector3(.48f, .38f, -.2f), new Vector3(.32f, .08f, .32f), new Color(.72f, .62f, .3f));
                    points = new[] { new Vector3(-.62f, .36f, -.48f), new Vector3(.62f, .36f, -.48f), new Vector3(0, -.35f, -.55f) }; break;
                case LostItemModelKind.Lunchbox:
                    Primitive(PrimitiveType.Cube, "Box", root, new Vector3(0, -.15f, 0), new Vector3(1.9f, .75f, 1.25f), color);
                    Primitive(PrimitiveType.Cube, "Lid", root, new Vector3(0, .35f, 0), new Vector3(2.02f, .18f, 1.36f), new Color(.66f, .42f, .3f));
                    points = new[] { new Vector3(-.72f, .48f, -.3f), new Vector3(.7f, -.3f, -.52f), new Vector3(0, .48f, .4f) }; break;
                case LostItemModelKind.Book:
                    Primitive(PrimitiveType.Cube, "Pages", root, Vector3.zero, new Vector3(1.75f, .38f, 1.25f), new Color(.78f, .72f, .58f));
                    Primitive(PrimitiveType.Cube, "Top Cover", root, new Vector3(0, .25f, 0), new Vector3(1.9f, .1f, 1.38f), color);
                    Primitive(PrimitiveType.Cube, "Bottom Cover", root, new Vector3(0, -.25f, 0), new Vector3(1.9f, .1f, 1.38f), color);
                    points = new[] { new Vector3(-.72f, .32f, -.42f), new Vector3(.66f, 0, -.62f), new Vector3(.05f, .3f, .52f) }; break;
                case LostItemModelKind.Mirror:
                    Primitive(PrimitiveType.Cylinder, "Mirror", root, new Vector3(0, .35f, 0), new Vector3(1.0f, .14f, 1.0f), new Color(.55f, .78f, .82f), Quaternion.Euler(90, 0, 0));
                    CylinderBetween(root, "Mirror Handle", new Vector3(0, -.42f, 0), new Vector3(0, -1.35f, 0), .12f, color);
                    points = new[] { new Vector3(-.72f, .6f, -.15f), new Vector3(.65f, .22f, -.15f), new Vector3(0, -1.15f, -.15f) }; break;
                case LostItemModelKind.PocketWatch:
                    Primitive(PrimitiveType.Cylinder, "Watch", root, Vector3.zero, new Vector3(.92f, .18f, .92f), color, Quaternion.Euler(90, 0, 0));
                    Primitive(PrimitiveType.Cylinder, "Crown", root, new Vector3(0, 1.02f, 0), new Vector3(.18f, .18f, .18f), color);
                    for (var i = 0; i < 7; i++) Primitive(PrimitiveType.Sphere, "Chain", root, new Vector3(.25f + i * .18f, .95f + Mathf.Sin(i * .55f) * .22f, 0), Vector3.one * .1f, color);
                    points = new[] { new Vector3(-.58f, .42f, -.18f), new Vector3(.58f, -.28f, -.18f), new Vector3(.85f, 1.05f, -.1f) }; break;
                default:
                    Primitive(PrimitiveType.Cylinder, "Glass Jar", root, Vector3.zero, new Vector3(.78f, .9f, .78f), color);
                    Primitive(PrimitiveType.Cylinder, "Purple Stopper", root, new Vector3(0, 1.02f, 0), new Vector3(.46f, .18f, .46f), new Color(.38f, .12f, .52f));
                    Primitive(PrimitiveType.Sphere, "Stone", root, new Vector3(.18f, -.72f, -.3f), Vector3.one * .22f, new Color(.32f, .3f, .27f));
                    points = new[] { new Vector3(-.62f, .22f, -.35f), new Vector3(.18f, -.72f, -.48f), new Vector3(.15f, 1.15f, -.15f) }; break;
            }
            var hotspots = new[] { Hotspot(root, "調査ポイント 1", points[0]), Hotspot(root, "調査ポイント 2", points[1]), Hotspot(root, "調査ポイント 3", points[2]) };
            presenter.Register(kind, root, hotspots);
        }
        private static void Stretch(RectTransform r, Vector2 min, Vector2 max, Vector2 pad) { r.anchorMin = min; r.anchorMax = max; r.offsetMin = pad; r.offsetMax = -pad; }
        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 pos, Vector3 scale, Color color, Quaternion rotation = default) { var go = GameObject.CreatePrimitive(type); go.name = name; if (parent) go.transform.SetParent(parent, false); go.transform.localPosition = pos; go.transform.localScale = scale; go.transform.localRotation = rotation == default ? Quaternion.identity : rotation; var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); go.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color }; return go; }
    }
}
