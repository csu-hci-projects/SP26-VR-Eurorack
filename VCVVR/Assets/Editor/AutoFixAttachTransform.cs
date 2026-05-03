using UnityEngine;
using UnityEditor;


public class AutoFixAttachTransform : EditorWindow
{
    [MenuItem("Tools/Eurorack/Auto‑Fix Attach Transforms")]
    public static void ShowWindow()
    {
        GetWindow<AutoFixAttachTransform>("Attach Fixer");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto‑Fix Socket Attach Transforms", EditorStyles.boldLabel);

        if (GUILayout.Button("Fix Selected Sockets"))
        {
            FixSelectedSockets();
        }
    }

    void FixSelectedSockets()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (socket == null)
            {
                Debug.LogWarning($"{obj.name} has no XRSocketInteractor — skipping.");
                continue;
            }

            Undo.RegisterCompleteObjectUndo(obj, "Fix Attach Transform");

            // Create attach transform if missing
            Transform attach = socket.attachTransform;
            if (attach == null)
            {
                GameObject at = new GameObject("AttachTransform");
                at.transform.SetParent(obj.transform);
                attach = at.transform;
                socket.attachTransform = attach;
            }

            // Reset local position
            attach.localPosition = Vector3.zero;

            // Auto‑align rotation so plugs snap upright
            // Assumes socket forward = plug forward
            attach.localRotation = Quaternion.identity;

            // Optional: rotate 180° if your plugs face backward
            // attach.localRotation = Quaternion.Euler(0, 180, 0);

            Debug.Log($"[Attach Fixed] {obj.name} → {attach.localRotation.eulerAngles}");
        }
    }
}
