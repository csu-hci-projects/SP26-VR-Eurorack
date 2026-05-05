using UnityEngine;
using UnityEditor;


public class AutoSocketCreator : EditorWindow
{
    private string interactionLayer = "Default";
    private string slotPrefix = "SOCKET_";

    [MenuItem("Tools/Eurorack/Add Socket Components")]
    public static void ShowWindow()
    {
        GetWindow<AutoSocketCreator>("Socket Creator");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto‑Add Socket Components", EditorStyles.boldLabel);

        interactionLayer = EditorGUILayout.TextField("Interaction Layer:", interactionLayer);
        slotPrefix = EditorGUILayout.TextField("Slot ID Prefix:", slotPrefix);

        if (GUILayout.Button("Add Socket Components to Selected"))
        {
            AddComponentsToSelection();
        }
    }

    void AddComponentsToSelection()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Add Socket Components");

            // 1. Add AuxSocketSlot
            var socketSlot = obj.GetComponent<AuxSocketSlot>();
            if (socketSlot == null)
                socketSlot = obj.AddComponent<AuxSocketSlot>();

            // Auto-generate Slot ID
            socketSlot.name = obj.name;
            var so = new SerializedObject(socketSlot);
            so.FindProperty("slotId").stringValue = slotPrefix + obj.name.ToUpper();
            so.ApplyModifiedProperties();

            // 2. Add XR Socket Interactor
            var interactor = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (interactor == null)
                interactor = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

            // 3. Add Collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                col = obj.AddComponent<SphereCollider>();

            col.isTrigger = true;

            // 4. Set Interaction Layer
            int layerIndex = LayerMask.NameToLayer(interactionLayer);
            if (layerIndex != -1)
                obj.layer = layerIndex;

            Debug.Log($"Socket setup complete on: {obj.name}");
        }
    }
}
