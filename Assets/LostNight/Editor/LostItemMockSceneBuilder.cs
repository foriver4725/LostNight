using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LostNight.Editor
{
    public static class LostItemSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/LostItemCenter.unity";
        private const string GeneratedDirectory = "Assets/LostNight/Generated";
        private const string FontPath = "Assets/LostNight/ThirdParty/NotoSansJP/NotoSansJP.ttf";

        [MenuItem("Lost Night/Bake Lost Item Center Scene")]
        public static void Build()
        {
            PrepareGeneratedDirectory();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var font = LoadFontAsset();
            var selection = LostItemSceneFactory.BuildSceneContents(font);
            PersistGeneratedAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = selection;
            Debug.Log($"Baked production scene: {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            Build();
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        public static void ValidateFromCommandLine()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Require(Object.FindAnyObjectByType<LostItemMockController>() != null, "Game Controller");
            Require(Object.FindAnyObjectByType<LostItemModelPresenter>() != null, "Model Presenter");
            Require(Object.FindAnyObjectByType<LostNightScreenView>() != null, "Screen View");
            Require(Object.FindAnyObjectByType<LostNightAudio>() != null, "Audio Service");
            Require(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length == 1, "single AudioListener");
            var modelNames = new[] { "Starry Umbrella", "Warm Glove", "Vanishing Pass", "Rain Bottle", "Delayed Wristwatch",
                "Calling Scarf", "Sea Shoe", "Voice Recorder", "Cold Lunchbox", "Growing Book", "Moonless Mirror",
                "Reverse Pocket Watch", "Footstep Jar" };
            var allTransforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var modelName in modelNames)
            {
                Require(System.Array.Exists(allTransforms, transform => transform.name == modelName), modelName);
            }
            Debug.Log("Validated baked LostItemCenter scene: all runtime references and 13 models are present.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        private static void PrepareGeneratedDirectory()
        {
            if (AssetDatabase.IsValidFolder(GeneratedDirectory)) AssetDatabase.DeleteAsset(GeneratedDirectory);
            Directory.CreateDirectory(GeneratedDirectory);
            AssetDatabase.Refresh();
        }

        private static Font LoadFontAsset()
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null)
            {
                throw new FileNotFoundException("The imported Japanese font is missing.", FontPath);
            }
            return font;
        }

        private static void PersistGeneratedAssets()
        {
            var materialIndex = 0;
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    var material = materials[i];
                    if (material == null || AssetDatabase.Contains(material)) continue;
                    material.name = $"Baked Material {materialIndex:000}";
                    AssetDatabase.CreateAsset(material, $"{GeneratedDirectory}/Material_{materialIndex++:000}.mat");
                }
                renderer.sharedMaterials = materials;
            }

            var meshIndex = 0;
            foreach (var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var mesh = filter.sharedMesh;
                if (mesh == null || AssetDatabase.Contains(mesh)) continue;
                mesh.name = $"Baked Mesh {meshIndex:000}";
                AssetDatabase.CreateAsset(mesh, $"{GeneratedDirectory}/Mesh_{meshIndex++:000}.asset");
            }
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new System.InvalidOperationException($"Baked scene is missing: {label}");
        }
    }
}
