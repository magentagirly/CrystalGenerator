using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CrystalGenerator))]
public class CrystalSaver : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CrystalGenerator gen = (CrystalGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Crystal", GUILayout.Height(30)))
        {
            gen.Generate();
        }

        if (GUILayout.Button("Save Mesh as Asset", GUILayout.Height(30)))
        {
            Mesh mesh = gen.GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarning("Сначала сгенерируйте кристалл.");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Сохранить меш кристалла",
                "Crystal_" + System.Guid.NewGuid().ToString().Substring(0, 8),
                "asset",
                "Выберите папку для сохранения"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("Меш сохранён: " + path);
            }
        }
    }
}