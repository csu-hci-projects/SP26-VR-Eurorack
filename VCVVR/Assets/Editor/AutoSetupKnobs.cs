using UnityEngine;
using UnityEditor;


public class AutoSetupKnobs : EditorWindow
{
    [MenuItem("Tools/Eurorack/Auto‑Setup Knobs (KnobReporter)")]
    public static void ShowWindow()
    {
        GetWindow<AutoSetupKnobs>("Knob Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("Auto‑Configure Selected Knobs (KnobReporter)", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup Selected Knobs"))
        {
            SetupKnobs();
        }
    }

    void SetupKnobs()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Setup Knob");

            // 1. Collider (so it can be grabbed)
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                col = obj.AddComponent<SphereCollider>();
            col.isTrigger = false;

            // 2. Rigidbody (required for XR interaction)
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 0.05f;
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation; 
            // physics won't move it; we rotate visual only

            // 3. XRGrabInteractable
            UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null)
                grab = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;

            // 4. KnobReporter
            KnobReporter reporter = obj.GetComponent<KnobReporter>();
            if (reporter == null)
                reporter = obj.AddComponent<KnobReporter>();

            // 5. Auto‑assign knobVisual (first child)
            if (obj.transform.childCount > 0)
            {
                reporter.knobVisual = obj.transform.GetChild(0);
            }
            else
            {
                Debug.LogWarning($"{obj.name} has no child to use as knobVisual.");
            }

            // Optional: default ~300° total range
            reporter.minAngle = -150f;
            reporter.maxAngle = 150f;

            Debug.Log($"[Knob Setup] {obj.name} configured with KnobReporter.");
        }
    }
}
