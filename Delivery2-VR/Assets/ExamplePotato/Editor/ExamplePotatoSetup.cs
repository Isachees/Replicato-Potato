using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ExamplePotatoSetup
{
    const string BodyParts = "Assets/Models/BodyParts";
    const string MaterialsFolder = "Assets/ExamplePotato/Materials";

    [MenuItem("Tools/Patata/Crear pantalla de patata de ejemplo")]
    static void Create()
    {
        GameObject potato = AssetDatabase.LoadAssetAtPath<GameObject>(BodyParts + "/Potato.prefab");
        if (potato == null)
        {
            EditorUtility.DisplayDialog("Patata de ejemplo",
                "No encuentro " + BodyParts + "/Potato.prefab. ¿Se han movido los prefabs?", "OK");
            return;
        }

        GameObject go = new GameObject("ExamplePotatoScreen");
        Undo.RegisterCreatedObjectUndo(go, "Crear pantalla de patata de ejemplo");

        // detrás de la patata de la escena, a la vista del jugador
        PotatoHead head = Object.FindFirstObjectByType<PotatoHead>();
        Vector3 basePos = head != null ? head.transform.position : Vector3.zero;
        go.transform.position = basePos + new Vector3(0f, 2.0f, 2.0f);

        ExamplePotatoDisplay display = go.AddComponent<ExamplePotatoDisplay>();
        display.potatoPrefab = potato;
        display.eyePrefabs = LoadPrefabs("Eye");
        display.nosePrefabs = LoadPrefabs("Nose");
        display.mouthPrefabs = LoadPrefabs("Mouth");
        display.earPrefabs = LoadPrefabs("Ear");

        display.screenMaterial = GetOrCreateMaterial("ExamplePotatoScreen.mat",
            "Universal Render Pipeline/Unlit", "Unlit/Texture", Color.white);
        display.frameMaterial = GetOrCreateMaterial("ExamplePotatoFrame.mat",
            "Universal Render Pipeline/Lit", "Standard", new Color(0.08f, 0.08f, 0.09f));
        display.CreateScreenGeometry(display.screenMaterial, display.frameMaterial);

        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;
        Debug.Log("Pantalla creada: eyes " + display.eyePrefabs.Length + ", noses " + display.nosePrefabs.Length +
                  ", mouths " + display.mouthPrefabs.Length + ", ears " + display.earPrefabs.Length, go);
    }

    static GameObject[] LoadPrefabs(string part)
    {
        string folder = BodyParts + "/" + part;
        if (!AssetDatabase.IsValidFolder(folder)) return new GameObject[0];
        return AssetDatabase.FindAssets("t:Prefab", new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
            .Where(p => p != null)
            .ToArray();
    }

    static Material GetOrCreateMaterial(string fileName, string shaderName, string fallbackShader, Color color)
    {
        string path = MaterialsFolder + "/" + fileName;
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
        {
            Directory.CreateDirectory(MaterialsFolder);
            AssetDatabase.Refresh();
        }

        Shader shader = Shader.Find(shaderName);
        if (shader == null) shader = Shader.Find(fallbackShader);
        mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return mat;
    }
}
