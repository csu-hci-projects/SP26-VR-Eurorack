using UnityEngine;
using UnityEditor;

public class AutoSetupSliders : EditorWindow
{
    [MenuItem("Tools/Eurorack/Auto‑Setup Sliders (SliderReporter)")]
    public static void ShowWindow() =>
        GetWindow<AutoSetupSliders>("Slider Setup");

    void OnGUI()
    {
        GUILayout.Label("Auto‑Configure Selected Sliders (SliderReporter)", EditorStyles.boldLabel);
        if (GUILayout.Button("Setup Selected Sliders"))
            SetupSliders();
    }

    void SetupSliders()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Setup Slider");

            // 1. Collider
            Collider col = obj.GetComponent<Collider>();
            if (col == null)
                col = obj.AddComponent<BoxCollider>();
            col.isTrigger = false;

            // 2. Rigidbody
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
                rb = obj.AddComponent<Rigidbody>();
            rb.useGravity  = false;
            rb.mass        = 0.05f;
            rb.constraints = RigidbodyConstraints.FreezePosition |
                             RigidbodyConstraints.FreezeRotation;

            // 3. XRGrabInteractable
            var grab = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grab == null)
                grab = obj.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.movementType  = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.Kinematic;

            // 4. SliderReporter
            SliderReporter reporter = obj.GetComponent<SliderReporter>();
            if (reporter == null)
                reporter = obj.AddComponent<SliderReporter>();

            // 5. Assign via SerializedObject so it serializes properly
            SerializedObject so = new SerializedObject(reporter);
            so.Update();

            Transform visual = obj.transform.childCount > 0
                ? obj.transform.GetChild(0)
                : obj.transform;

            if (obj.transform.childCount == 0)
                Debug.LogWarning($"{obj.name} has no child — sliderVisual set to root transform.");

            so.FindProperty("sliderVisual").objectReferenceValue = visual;

            // Set minZ/maxZ to ±5cm — adjust in Inspector per slider
            so.FindProperty("minZ").floatValue = -0.05f;
            so.FindProperty("maxZ").floatValue =  0.05f;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(reporter);

            Debug.Log($"[Slider Setup] {obj.name} → sliderVisual = {visual.name}");
        }
    }
}