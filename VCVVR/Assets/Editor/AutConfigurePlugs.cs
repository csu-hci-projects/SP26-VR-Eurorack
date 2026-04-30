using UnityEngine;
using UnityEditor;


public class AutoConfigurePlugs : EditorWindow
{
    [MenuItem("Tools/Eurorack/Auto‑Configure Cable Plugs")]
    public static void ShowWindow()
    {
        GetWindow<AutoConfigurePlugs>("Plug Config");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Configure Selected Plugs"))
        {
            Configure();
        }
    }

    void Configure()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Configure Plug");

            // Add collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                col = obj.AddComponent<SphereCollider>();

            col.isTrigger = false;

            // Add rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = obj.AddComponent<Rigidbody>();

            rb.useGravity = true;
            rb.mass = 0.05f;

            // Add XR Grab Interactable
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null)
                grab = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            grab.trackPosition = true;
            grab.trackRotation = true;

            // Ensure AuxPlugEndId exists
            if (obj.GetComponent<AuxPlugEndId>() == null)
                obj.AddComponent<AuxPlugEndId>();

            Debug.Log($"Configured plug: {obj.name}");
        }
    }
}
