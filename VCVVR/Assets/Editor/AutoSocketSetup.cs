// Save to: Assets/Editor/AuxSocketSetup.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AuxSocketSetup : EditorWindow
{
    private const float OFFSET_DISTANCE = 0.0003005f;

    [MenuItem("Tools/Fix Aux Socket Attach Transforms")]
    public static void FixAllSockets()
    {
        AuxSocketSlot[] slots = GameObject.FindObjectsOfType<AuxSocketSlot>();
        int fixedCount = 0;

        foreach (AuxSocketSlot slot in slots)
        {
            XRSocketInteractor socket = slot.GetComponent<XRSocketInteractor>();
            if (socket == null) continue;

            Transform attachPoint = slot.transform.Find("AttachPoint");
            if (attachPoint == null)
            {
                GameObject go = new GameObject("AttachPoint");
                Undo.RegisterCreatedObjectUndo(go, "Create AttachPoint");
                go.transform.SetParent(slot.transform, false);
                attachPoint = go.transform;
            }

            Undo.RecordObject(attachPoint, "Fix Socket AttachPoint");

            // Reset rotation so it inherits parent orientation cleanly
            attachPoint.localRotation = Quaternion.identity;

            // Offset along the socket's world up axis (since X:-89.98 means
            // world up IS the socket's forward/outward direction)
            // Convert world offset back to local space so it works on any panel angle
            Vector3 worldOffset = slot.transform.up * OFFSET_DISTANCE;
            attachPoint.localPosition = slot.transform.InverseTransformVector(worldOffset);

            Undo.RecordObject(socket, "Assign Socket AttachPoint");
            socket.attachTransform = attachPoint;
            EditorUtility.SetDirty(slot.gameObject);
            fixedCount++;
        }

        Debug.Log("[AuxSocketSetup] Fixed " + fixedCount + " sockets.");
    }

    [MenuItem("Tools/Fix Aux Plug Attach Transforms")]
    public static void FixAllPlugs()
    {
        AuxPlugEndId[] plugEnds = GameObject.FindObjectsOfType<AuxPlugEndId>();
        int fixedCount = 0;

        foreach (AuxPlugEndId plugEnd in plugEnds)
        {
            Transform plugAttach = null;
            foreach (Transform child in plugEnd.transform)
            {
                if (child.name == "PlugAttach")
                {
                    plugAttach = child;
                    break;
                }
            }

            if (plugAttach == null)
            {
                Debug.LogWarning("[AuxSocketSetup] No PlugAttach on " + plugEnd.gameObject.name);
                continue;
            }

            Undo.RecordObject(plugAttach, "Fix PlugAttach Position");
            plugAttach.localPosition = new Vector3(0f, 0f, 0.0003105f);
            plugAttach.localRotation = Quaternion.identity;
            EditorUtility.SetDirty(plugAttach.gameObject);

            XRGrabInteractable interactable = plugEnd.GetComponent<XRGrabInteractable>();
            if (interactable == null)
                interactable = plugEnd.GetComponentInParent<XRGrabInteractable>();
            if (interactable == null) continue;

            Undo.RecordObject(interactable, "Assign Plug AttachPoint");
            interactable.attachTransform = plugAttach;
            EditorUtility.SetDirty(interactable.gameObject);
            fixedCount++;
        }

        Debug.Log("[AuxSocketSetup] Fixed " + fixedCount + " plug attach transforms.");
    }
}
#endif