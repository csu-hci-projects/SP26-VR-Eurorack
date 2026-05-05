using UnityEngine;
using UnityEditor;


public class AutoFixPlugAttach : EditorWindow
{
    [MenuItem("Tools/Eurorack/Auto‑Fix Plug Attach")]
    public static void ShowWindow()
    {
        GetWindow<AutoFixPlugAttach>("Plug Attach Fixer");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto‑Fix Plug Attach Transforms", EditorStyles.boldLabel);

        if (GUILayout.Button("Fix Selected Plugs"))
        {
            FixPlugs();
        }
    }

    void FixPlugs()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null)
            {
                Debug.LogWarning($"{obj.name} has no XRGrabInteractable — skipping.");
                continue;
            }

            Undo.RegisterCompleteObjectUndo(obj, "Fix Plug Attach");

            // Create attach transform if missing
            Transform attach = grab.attachTransform;
            if (attach == null)
            {
                GameObject at = new GameObject("PlugAttach");
                at.transform.SetParent(obj.transform);
                attach = at.transform;
                grab.attachTransform = attach;
            }

            // Reset local position
            attach.localPosition = Vector3.zero;

            // Align so plug snaps upright
            // Forward = plug forward
            // Up = plug up
            attach.localRotation = Quaternion.identity;

            Debug.Log($"[Plug Attach Fixed] {obj.name} → {attach.localRotation.eulerAngles}");
        }
    }
}
