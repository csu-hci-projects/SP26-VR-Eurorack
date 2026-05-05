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
            SetupKnobs();
    }

    void SetupKnobs()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Setup Knob");

            // 1. Collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                col = obj.AddComponent<SphereCollider>();
            col.isTrigger = false;

            // 2. Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 0.05f;
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

            // 3. XRGrabInteractable
            var grab = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null)
                grab = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;

            // 4. KnobReporter
            KnobReporter reporter = obj.GetComponent<KnobReporter>();
            if (reporter == null)
                reporter = obj.AddComponent<KnobReporter>();

            // 5. Assign fields via SerializedObject so Unity serializes + tracks undo
            SerializedObject so = new SerializedObject(reporter);
            so.Update();

            // knobVisual — self if no children, else first child
            Transform visual = obj.transform.childCount > 0
                ? obj.transform.GetChild(0)
                : obj.transform;          // fallback: rotate the root itself

            if (obj.transform.childCount == 0)
                Debug.LogWarning($"{obj.name} has no child — knobVisual set to root transform.");

            so.FindProperty("knobVisual").objectReferenceValue = visual;
            so.FindProperty("minAngle").floatValue = -150f;
            so.FindProperty("maxAngle").floatValue = 150f;

            // localTwistAxis default (forward = Z)
            so.FindProperty("localTwistAxis").vector3Value = Vector3.forward;

            so.ApplyModifiedProperties();          // writes + marks dirty in one call
            EditorUtility.SetDirty(reporter);      // belt-and-suspenders for prefab scenes

            Debug.Log($"[Knob Setup] {obj.name} → knobVisual = {visual.name}");
        }
    }
}