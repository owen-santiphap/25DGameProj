#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FieldFinder
{
    [MenuItem("Tools/Find 'projectilePrefab' Serialized Fields")]
    public static void FindProjectilePrefabFields()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            var components = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var comp in components)
            {
                if (comp == null) continue;

                SerializedObject so = new SerializedObject(comp);
                SerializedProperty prop = so.GetIterator();

                while (prop.NextVisible(true))
                {
                    if (prop.name == "projectilePrefab")
                    {
                        Debug.Log($"Found 'projectilePrefab' in prefab: {prefab.name}, script: {comp.GetType().Name}", prefab);
                    }
                }
            }
        }
    }
}
#endif
