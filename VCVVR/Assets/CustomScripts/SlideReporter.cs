using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SliderReporter : MonoBehaviour
{
    [Header("Visual")]
    public Transform sliderVisual;

    [Header("Slide Limits (world units along slide axis)")]
    public float minZ = -0.05f;
    public float maxZ =  0.05f;

    [Header("Output")]
    public float Value01 { get; private set; }

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private float   startHandProjected;      // hand pos projected onto slide axis (world)
    private Vector3 startVisualWorldPos;     // visual world pos at grab time
    private Vector3 slideAxisWorld;          // world-space slide direction

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

        // World-space axis the slider moves along (local Z of the visual)
        slideAxisWorld = sliderVisual.up;

        // Project hand onto that axis
        startHandProjected  = Vector3.Dot(interactor.transform.position, slideAxisWorld);

        // Store visual's world position — no local space, no scale issues
        startVisualWorldPos = sliderVisual.position;
    }

    void OnRelease(SelectExitEventArgs args) { }

    void Update()
    {
        if (!grab.isSelected) return;

        var interactor = grab.interactorsSelecting[0];

        float currentHandProjected = Vector3.Dot(interactor.transform.position, slideAxisWorld);
        float delta = currentHandProjected - startHandProjected;

        // Clamp in world units relative to start
        float clampedDelta = Mathf.Clamp(delta, minZ, maxZ);

        // Move visual in world space — completely bypasses scale/local space issues
        sliderVisual.position = startVisualWorldPos + slideAxisWorld * clampedDelta;

        Value01 = Mathf.InverseLerp(minZ, maxZ, clampedDelta);
    }
}