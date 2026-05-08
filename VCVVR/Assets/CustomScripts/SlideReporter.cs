using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SliderReporter : MonoBehaviour
{
    [Header("Visual")]
    public Transform sliderVisual;

    [Header("Slide Limits (world units along slide axis)")]
    public float minZ = -0.05f;
    public float maxZ = 0.05f;

    [Header("Output")]
    public float Value01 { get; private set; } = 0.5f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    private float startHandProjected;
    private Vector3 startVisualWorldPos;
    private Vector3 slideAxisWorld;

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
        slideAxisWorld = sliderVisual.up;
        startHandProjected = Vector3.Dot(interactor.transform.position, slideAxisWorld);
        startVisualWorldPos = sliderVisual.position;
    }

    void OnRelease(SelectExitEventArgs args) { }

    void Update()
    {
        if (!grab.isSelected) return;

        var interactor = grab.interactorsSelecting[0];

        float currentHandProjected = Vector3.Dot(interactor.transform.position, slideAxisWorld);
        float delta = currentHandProjected - startHandProjected;
        float clampedDelta = Mathf.Clamp(delta, minZ, maxZ);

        sliderVisual.position = startVisualWorldPos + slideAxisWorld * clampedDelta;

        Value01 = Mathf.InverseLerp(minZ, maxZ, clampedDelta);
    }
}