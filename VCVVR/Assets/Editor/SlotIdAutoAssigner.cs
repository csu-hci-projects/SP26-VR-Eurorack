using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SlotIdAutoAssigner : EditorWindow
{
    private Dictionary<string, string> nameToSlotId = new Dictionary<string, string>
    {
        { "Freq", "VCO_FREQ" },
        { "PW", "VCO_PW" },
        { "Vol", "OUT_VOL" },
        { "Volume", "OUT_VOL" },
        // Add more mappings as needed
    };

    private Dictionary<string, int> modulePrefixToId = new Dictionary<string, int>
    {
        { "VCO", 0 },
        { "OSC", 0 },
        { "AUDIO", 1 },
        { "OUT", 1 },
        { "VCF", 2 },
        { "MIX", 3 },
        { "ASDR", 4 },
        { "SEQ", 5 },
        { "DRUM", 6 },
        { "DIS", 7 },
        { "AM", 8 },
    };

    [MenuItem("Tools/Eurorack/Auto Assign Slot IDs")]
    public static void ShowWindow() =>
        GetWindow<SlotIdAutoAssigner>("Slot ID Assigner");

    void OnGUI()
    {
        GUILayout.Label("Auto Assign Slot IDs", EditorStyles.boldLabel);
        GUILayout.Label("This will set SlotId based on GameObject names containing keywords.");
        GUILayout.Label("Module mapping: VCO=0, AUDIO/OUT=1, VCF=2, MIX=3, ASDR=4, SEQ=5, DRUM=6, DIS=7, AM=8");
        GUILayout.Label("Port IDs use numeric parts from the name; otherwise they count up per module.");
        GUILayout.Label("If the name contains OUT, the jack is marked as output.");

        if (GUILayout.Button("Assign Slot IDs to Selected Sockets"))
            AssignSlotIdsToSelection();

        if (GUILayout.Button("Assign Jack Components to Selected Objects"))
            AssignJacksToSelection();

        if (GUILayout.Button("Assign Slot IDs and Jacks to Selected"))
            AssignSlotIdsAndJacksToSelection();
    }

    private static readonly Regex JackNamePattern =
        new Regex(@"Jack_M(\d+)_P(\d+)_(In|Out)", RegexOptions.IgnoreCase);

    void AssignSlotIdsToSelection()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            var socketSlot = obj.GetComponent<AuxSocketSlot>();
            if (socketSlot == null) continue;

            string slotId = GetSlotIdFromName(obj.name);
            if (string.IsNullOrEmpty(slotId))
            {
                // Try to get from Jack component
                var jack = obj.GetComponentInChildren<Jack>();
                if (jack != null)
                {
                    string direction = jack.isOutput ? "OUT" : "IN";
                    slotId = $"M{jack.moduleId}_P{jack.portId}_{direction}";
                }
            }

            if (!string.IsNullOrEmpty(slotId))
            {
                Undo.RegisterCompleteObjectUndo(socketSlot, "Assign Slot ID");
                var so = new SerializedObject(socketSlot);
                so.Update();
                so.FindProperty("slotId").stringValue = slotId;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(socketSlot);
                Debug.Log($"[Slot Assigner] Set {obj.name} SlotId to {slotId}");
            }
        }
    }

    void AssignJacksToSelection()
    {
        var portCounters = new Dictionary<int, int>();

        foreach (GameObject obj in Selection.gameObjects)
        {
            AssignJacksRecursively(obj, portCounters);
        }
    }

    void AssignJacksRecursively(GameObject obj, Dictionary<int, int> portCounters)
    {
        // First see if this object already explicitly names a Jack
        Match explicitJack = JackNamePattern.Match(obj.name);
        if (explicitJack.Success)
        {
            AssignJackFromExplicitName(obj, explicitJack);
        }
        else if (TryGetJackInfoFromName(obj.name, portCounters, out int moduleId, out int portId, out bool isOutput))
        {
            AssignJackProperties(obj, moduleId, portId, isOutput);
        }

        foreach (Transform child in obj.transform)
        {
            AssignJacksRecursively(child.gameObject, portCounters);
        }
    }

    void AssignJackFromExplicitName(GameObject obj, Match match)
    {
        Jack jack = obj.GetComponent<Jack>();
        if (jack == null)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Add Jack Component");
            jack = obj.AddComponent<Jack>();
        }

        int moduleId = int.Parse(match.Groups[1].Value);
        int portId = int.Parse(match.Groups[2].Value);
        bool isOutput = match.Groups[3].Value.Equals("Out", System.StringComparison.OrdinalIgnoreCase);

        AssignJackProperties(obj, moduleId, portId, isOutput);
    }

    bool TryGetJackInfoFromName(string name, Dictionary<int, int> portCounters, out int moduleId, out int portId, out bool isOutput)
    {
        moduleId = -1;
        portId = -1;
        isOutput = false;

        string upperName = name.ToUpperInvariant();
        if (!GetModuleIdFromName(upperName, out moduleId))
            return false;

        portId = ExtractPortId(upperName);
        if (portId < 0)
            portId = GetNextPortId(moduleId, portCounters);

        isOutput = upperName.Contains("OUT");
        return true;
    }

    bool GetModuleIdFromName(string upperName, out int moduleId)
    {
        foreach (var kvp in modulePrefixToId)
        {
            string prefix = kvp.Key + "_";
            if (upperName.StartsWith(prefix) || upperName.StartsWith(kvp.Key))
            {
                moduleId = kvp.Value;
                return true;
            }
        }

        moduleId = -1;
        return false;
    }

    int ExtractPortId(string upperName)
    {
        var digitMatch = Regex.Match(upperName, @"(\d+)");
        if (digitMatch.Success && int.TryParse(digitMatch.Value, out int parsed))
            return parsed;

        if (upperName.Contains("_L") || upperName.EndsWith("L"))
            return 0;
        if (upperName.Contains("_R") || upperName.EndsWith("R"))
            return 1;

        if (upperName.Contains("LEFT"))
            return 0;
        if (upperName.Contains("RIGHT"))
            return 1;

        return -1;
    }

    int GetNextPortId(int moduleId, Dictionary<int, int> portCounters)
    {
        if (!portCounters.TryGetValue(moduleId, out int nextId))
            nextId = 0;

        portCounters[moduleId] = nextId + 1;
        return nextId;
    }

    void AssignJackProperties(GameObject obj, int moduleId, int portId, bool isOutput)
    {
        Jack jack = obj.GetComponent<Jack>();
        if (jack == null)
        {
            Undo.RegisterCompleteObjectUndo(obj, "Add Jack Component");
            jack = obj.AddComponent<Jack>();
        }

        var jackSo = new SerializedObject(jack);
        jackSo.Update();
        jackSo.FindProperty("moduleId").intValue = moduleId;
        jackSo.FindProperty("portId").intValue = portId;
        jackSo.FindProperty("isOutput").boolValue = isOutput;
        jackSo.ApplyModifiedProperties();

        EditorUtility.SetDirty(jack);
        Debug.Log($"[Slot Assigner] Jack assigned on '{obj.name}': M{moduleId} P{portId} {(isOutput ? "Out" : "In")}");
    }

    void AssignSlotIdsAndJacksToSelection()
    {
        AssignSlotIdsToSelection();
        AssignJacksToSelection();
    }

    string GetSlotIdFromName(string name)
    {
        string upperName = name.ToUpperInvariant();

        // Direct slot ID for socket names that already use the IN/OUT suffix convention.
        if (upperName.EndsWith("_IN") || upperName.EndsWith("_OUT"))
            return upperName;

        // Accept already-formatted socket names that use uppercase letters, numbers,
        // underscores, slashes, or hyphens and look like module ports.
        if (LooksLikeSlotName(upperName))
            return upperName;

        foreach (var kvp in nameToSlotId)
        {
            if (upperName.Contains(kvp.Key.ToUpperInvariant()))
                return kvp.Value;
        }

        return null;
    }

    bool LooksLikeSlotName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            return false;

        // Reject obvious generic GameObject names
        if (name.StartsWith("CUBE") || name.StartsWith("TEXT") || name.StartsWith("SPHERE") || name.StartsWith("CAMERA"))
            return false;

        bool hasModulePrefix = name.StartsWith("VCF_") || name.StartsWith("VCO_") || name.StartsWith("AM_") ||
                               name.StartsWith("MIX_") || name.StartsWith("ASDR_") || name.StartsWith("SEQ_") ||
                               name.StartsWith("DRUM_") || name.StartsWith("DIS_") ||
                               name.StartsWith("OUT_") || name.StartsWith("IN_") || name.StartsWith("OSC_");

        if (!hasModulePrefix)
            return false;

        foreach (char c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '/' && c != '-')
                return false;
        }

        return true;
    }
}