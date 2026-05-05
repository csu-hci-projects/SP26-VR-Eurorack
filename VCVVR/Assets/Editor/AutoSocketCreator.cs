using UnityEngine;
using UnityEditor;
using System;
using System.Text.RegularExpressions;

public class AutoSocketCreator : EditorWindow
{
    private string interactionLayer = "Default";
    private string slotPrefix = "SOCKET_";
    private bool autoAssignJack = true;

    // Matches: Jack_M1_P3_Out / Jack_M2_P1_In (case-insensitive)
    private static readonly Regex JackNamePattern =
        new Regex(@"Jack_M(\d+)_P(\d+)_(In|Out)", RegexOptions.IgnoreCase);

    [MenuItem("Tools/Eurorack/Add Socket Components")]
    public static void ShowWindow() =>
        GetWindow<AutoSocketCreator>("Socket Creator");

    void OnGUI()
    {
        GUILayout.Label("Auto‑Add Socket Components", EditorStyles.boldLabel);

        interactionLayer = EditorGUILayout.TextField("Interaction Layer:", interactionLayer);
        slotPrefix       = EditorGUILayout.TextField("Slot ID Prefix:", slotPrefix);
        autoAssignJack   = EditorGUILayout.Toggle("Auto-Assign Jack (from name)", autoAssignJack);

        if (GUILayout.Button("Add Socket Components to Selected"))
            AddComponentsToSelection();
    }

    void AddComponentsToSelection()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Add Socket Components");

            // 1. AuxSocketSlot
            var socketSlot = obj.GetComponent<AuxSocketSlot>();
            if (socketSlot == null)
                socketSlot = obj.AddComponent<AuxSocketSlot>();

            var so = new SerializedObject(socketSlot);
            so.Update();
            so.FindProperty("slotId").stringValue = slotPrefix + obj.name.ToUpper();
            so.ApplyModifiedProperties();

            // 2. XRSocketInteractor
            var interactor = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (interactor == null)
                interactor = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

            // 3. Collider (trigger)
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                col = obj.AddComponent<SphereCollider>();
            col.isTrigger = true;

            // 4. Interaction Layer
            int layerIndex = LayerMask.NameToLayer(interactionLayer);
            if (layerIndex != -1)
                obj.layer = layerIndex;

            // 5. Jack — parse from name convention and assign in Editor
            if (autoAssignJack)
                TryAssignJack(obj);

            EditorUtility.SetDirty(obj);
            Debug.Log($"[Socket Creator] Setup complete: {obj.name}");
        }
    }

    void TryAssignJack(GameObject obj)
    {
        // Search the object and all children for a name matching Jack_M{n}_P{n}_{In|Out}
        foreach (Transform t in obj.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            Match m = JackNamePattern.Match(t.gameObject.name);
            if (!m.Success) continue;

            Jack jack = t.gameObject.GetComponent<Jack>();
            if (jack == null)
            {
                Undo.RegisterCompleteObjectUndo(t.gameObject, "Add Jack Component");
                jack = t.gameObject.AddComponent<Jack>();
            }

            var jackSo = new SerializedObject(jack);
            jackSo.Update();
            jackSo.FindProperty("moduleId").intValue  = int.Parse(m.Groups[1].Value);
            jackSo.FindProperty("portId").intValue    = int.Parse(m.Groups[2].Value);
            jackSo.FindProperty("isOutput").boolValue =
                m.Groups[3].Value.Equals("Out", StringComparison.OrdinalIgnoreCase);
            jackSo.ApplyModifiedProperties();

            EditorUtility.SetDirty(t.gameObject);
            Debug.Log($"[Socket Creator] Jack assigned on '{t.gameObject.name}': " +
                      $"M{jack.moduleId} P{jack.portId} {(jack.isOutput ? "Out" : "In")}");
            return; // one Jack per socket object is expected
        }

        // No match found — warn if the object name looks like it should have one
        if (obj.name.StartsWith("Jack", System.StringComparison.OrdinalIgnoreCase))
            Debug.LogWarning($"[Socket Creator] '{obj.name}' looks like a Jack but didn't match " +
                             $"pattern Jack_M{{n}}_P{{n}}_{{In|Out}} — skipping.");
    }
}