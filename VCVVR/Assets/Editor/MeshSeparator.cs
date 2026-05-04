using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MeshSeparator : EditorWindow
{
    [MenuItem("Tools/Eurorack/Separate Mesh by Loose Parts")]
    public static void ShowWindow() =>
        GetWindow<MeshSeparator>("Mesh Separator");

    private float weldThreshold = 0.0001f;

    void OnGUI()
    {
        GUILayout.Label("Separate Combined Mesh into Loose Parts", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Select a GameObject with a MeshFilter. Each disconnected island " +
            "becomes its own GameObject. Weld threshold merges vertices that are " +
            "very close together before connectivity is checked.",
            MessageType.Info);

        weldThreshold = EditorGUILayout.FloatField("Weld Threshold", weldThreshold);

        if (GUILayout.Button("Separate Selected Object"))
            Separate();
    }

    void Separate()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null) { Debug.LogError("Nothing selected."); return; }

        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError($"{obj.name} has no MeshFilter/Mesh."); return;
        }

        Mesh src = mf.sharedMesh;
        Vector3[]  srcVerts   = src.vertices;
        Vector3[]  srcNormals = src.normals;
        Vector2[]  srcUVs     = src.uv;
        int[]      srcTris    = src.triangles;
        Material[] srcMats    = obj.GetComponent<MeshRenderer>()?.sharedMaterials;

        // --- Step 1: Weld vertices by position ---
        // Maps each vertex index to a canonical index (the first vert seen at that position)
        int[] weld = new int[srcVerts.Length];
        for (int i = 0; i < srcVerts.Length; i++)
        {
            weld[i] = i; // default to self
            for (int j = 0; j < i; j++)
            {
                if (Vector3.SqrMagnitude(srcVerts[i] - srcVerts[j]) < weldThreshold * weldThreshold)
                {
                    weld[i] = j;
                    break;
                }
            }
        }

        // --- Step 2: Union-Find on WELDED indices ---
        int[] parent = new int[srcVerts.Length];
        for (int i = 0; i < parent.Length; i++) parent[i] = i;

        int Find(int x)
        {
            while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
            return x;
        }
        void Union(int a, int b)
        {
            a = Find(a); b = Find(b);
            if (a != b) parent[a] = b;
        }

        for (int i = 0; i < srcTris.Length; i += 3)
        {
            // Union using welded indices so position-coincident verts connect islands
            Union(weld[srcTris[i]], weld[srcTris[i + 1]]);
            Union(weld[srcTris[i + 1]], weld[srcTris[i + 2]]);
        }

        // --- Step 3: Group ORIGINAL triangles by welded root ---
        Dictionary<int, List<int>> islands = new Dictionary<int, List<int>>();
        for (int i = 0; i < srcTris.Length; i += 3)
        {
            int root = Find(weld[srcTris[i]]);
            if (!islands.ContainsKey(root))
                islands[root] = new List<int>();
            islands[root].Add(srcTris[i]);
            islands[root].Add(srcTris[i + 1]);
            islands[root].Add(srcTris[i + 2]);
        }

        Debug.Log($"[Mesh Separator] Found {islands.Count} islands after welding.");

        if (islands.Count <= 1)
        {
            Debug.LogWarning($"{obj.name} has only one island after welding — nothing to separate.");
            return;
        }

        // --- Step 4: Build and save a mesh per island ---
        string assetFolder = "Assets/SeparatedMeshes";
        if (!AssetDatabase.IsValidFolder(assetFolder))
            AssetDatabase.CreateFolder("Assets", "SeparatedMeshes");

        int partIndex = 0;
        Undo.RegisterFullObjectHierarchyUndo(obj, "Separate Mesh");

        foreach (var island in islands)
        {
            List<int> tris = island.Value;

            Dictionary<int, int> remap  = new Dictionary<int, int>();
            List<Vector3>        newVerts   = new List<Vector3>();
            List<Vector3>        newNormals = new List<Vector3>();
            List<Vector2>        newUVs     = new List<Vector2>();
            List<int>            newTris    = new List<int>();

            foreach (int oldIdx in tris)
            {
                if (!remap.ContainsKey(oldIdx))
                {
                    remap[oldIdx] = newVerts.Count;
                    newVerts.Add(srcVerts[oldIdx]);
                    if (srcNormals.Length > 0) newNormals.Add(srcNormals[oldIdx]);
                    if (srcUVs.Length > 0)     newUVs.Add(srcUVs[oldIdx]);
                }
                newTris.Add(remap[oldIdx]);
            }

            Mesh newMesh      = new Mesh();
            newMesh.name      = $"{src.name}_part{partIndex}";
            newMesh.vertices  = newVerts.ToArray();
            if (newNormals.Count > 0) newMesh.normals = newNormals.ToArray();
            if (newUVs.Count > 0)     newMesh.uv      = newUVs.ToArray();
            newMesh.triangles = newTris.ToArray();
            newMesh.RecalculateBounds();
            if (newNormals.Count == 0) newMesh.RecalculateNormals();

            string meshPath = $"{assetFolder}/{obj.name}_part{partIndex}.asset";
            AssetDatabase.CreateAsset(newMesh, meshPath);

            GameObject child = new GameObject($"{obj.name}_slider{partIndex:D2}");
            Undo.RegisterCreatedObjectUndo(child, "Separate Mesh");
            child.transform.SetParent(obj.transform.parent, false);
            child.transform.localPosition = obj.transform.localPosition;
            child.transform.localRotation = obj.transform.localRotation;
            child.transform.localScale    = obj.transform.localScale;

            MeshFilter   childMF = child.AddComponent<MeshFilter>();
            childMF.sharedMesh   = newMesh;
            MeshRenderer childMR = child.AddComponent<MeshRenderer>();
            childMR.sharedMaterials = srcMats;

            partIndex++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Mesh Separator] Done — {islands.Count} parts saved to {assetFolder}.");
        obj.SetActive(false);
        Debug.Log($"Original '{obj.name}' disabled — delete once verified.");
    }
}