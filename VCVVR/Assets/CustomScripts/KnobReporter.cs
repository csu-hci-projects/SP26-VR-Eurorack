using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KnobReporter : MonoBehaviour
{
    [Header("Visual")]
    public Transform knobVisual;

    [Header("Rotation Axis")]
    // Which local axis is the spin axis? For a front-panel knob pointing toward
    // the player, this is usually Vector3.forward (0,0,1). Change if needed.
    public Vector3 localTwistAxis = Vector3.forward;

    [Header("Rotation Limits (degrees)")]
    public float minAngle = -150f;
    public float maxAngle = 150f;

    [Header("Output")]
    public float Value01 { get; private set; }

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    // Cached at grab time
    private Quaternion startHandRot;
    private float startKnobAngle;
    private Quaternion visualRestRot;   // knobVisual rotation with angle = 0

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.trackPosition = false;
        grab.trackRotation = false;
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        var interactor = grab.interactorsSelecting[0];
        startHandRot = interactor.transform.rotation;

        // Current knob angle around its twist axis
        startKnobAngle = CurrentKnobAngle();

        // The "zero-angle" rotation of the visual: strip out the current twist
        visualRestRot = knobVisual.localRotation *
                        Quaternion.AngleAxis(-startKnobAngle, localTwistAxis);
    }

    void OnRelease(SelectExitEventArgs args) { }

    void Update()
    {
        if (!grab.isSelected) return;

        var interactor = grab.interactorsSelecting[0];

        // World-space twist axis
        Vector3 worldAxis = transform.TransformDirection(localTwistAxis);

        // How much has the hand rotated around our twist axis since grab?
        Quaternion delta = interactor.transform.rotation * Quaternion.Inverse(startHandRot);
        float deltaAngle = SignedAngleAroundAxis(delta, worldAxis);

        float newAngle = Mathf.Clamp(startKnobAngle + deltaAngle, minAngle, maxAngle);

        // Apply: rest pose + twist
        knobVisual.localRotation = visualRestRot *
                                   Quaternion.AngleAxis(newAngle, localTwistAxis);

        Value01 = Mathf.InverseLerp(minAngle, maxAngle, newAngle);
    }

    // Extract the current twist angle of knobVisual around localTwistAxis
    float CurrentKnobAngle()
    {
        // Project the current local rotation onto the twist axis
        Vector3 perpendicular = Vector3.Cross(localTwistAxis,
                                    Vector3.up.sqrMagnitude > 0.001f &&
                                    Vector3.Cross(localTwistAxis, Vector3.up).sqrMagnitude > 0.001f
                                    ? Vector3.up : Vector3.right);

        Vector3 rotated = knobVisual.localRotation * perpendicular;
        // Flatten back to the plane perpendicular to twist axis
        rotated -= Vector3.Dot(rotated, localTwistAxis) * localTwistAxis;

        return Vector3.SignedAngle(perpendicular, rotated, localTwistAxis);
    }

    // Signed angle of a quaternion rotation projected onto a world-space axis
    static float SignedAngleAroundAxis(Quaternion rot, Vector3 axis)
    {
        // Decompose rot into swing * twist, extract twist angle
        Vector3 rotAxis = new Vector3(rot.x, rot.y, rot.z);
        Vector3 projected = Vector3.Project(rotAxis, axis);
        Quaternion twist = new Quaternion(projected.x, projected.y, projected.z, rot.w);

        if (twist == Quaternion.identity)
            return 0f;

        twist = twist.normalized; // normalize after projection
        float angle = 2f * Mathf.Acos(Mathf.Clamp(twist.w, -1f, 1f)) * Mathf.Rad2Deg;

        // Sign: check if projected axis points with or against our axis
        if (Vector3.Dot(projected, axis) < 0f)
            angle = -angle;

        return angle;
    }
}