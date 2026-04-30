using UnityEngine;

public class KnobReporter : MonoBehaviour
{
    [SerializeField] Transform knobVisual;     // the part that rotates
    [SerializeField] Vector3 localAxis = Vector3.up;
    [SerializeField] float minAngle = -135f;
    [SerializeField] float maxAngle = 135f;

    public float Angle { get; private set; }      // degrees
    public float Value01 { get; private set; }    // 0..1

    void Update()
    {
        // Get rotation around axis in local space
        var euler = knobVisual.localEulerAngles;

        // Example for Y axis knob: convert 0..360 to -180..180
        float raw = euler.y;
        if (raw > 180f) raw -= 360f;

        Angle = Mathf.Clamp(raw, minAngle, maxAngle);
        Value01 = Mathf.InverseLerp(minAngle, maxAngle, Angle);

        // Example “rotation number”
        // Debug.Log($"Angle={Angle:0.0}°, Value={Value01:0.000}");
    }
}