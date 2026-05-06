using System;
using System.Collections.Generic;
using UnityEngine;

public class PatchRouter : MonoBehaviour
{
    public static PatchRouter Instance;

    // Wire this in the Inspector to your ModularEngine GameObject
    public ModularEngine engine;

    private Dictionary<string, float> audioValues = new();
    private Dictionary<string, string> parent = new();
    private HashSet<(string, string)> connections = new();

    void Awake()
    {
        Instance = this;
    }

    public void ConnectSlots(string slotA, string slotB)
    {
        if (string.IsNullOrEmpty(slotA) || string.IsNullOrEmpty(slotB) || slotA == slotB)
            return;

        connections.Add(NormalizePair(slotA, slotB));
        RebuildNetworks();
    }

    public void DisconnectSlots(string slotA, string slotB)
    {
        if (string.IsNullOrEmpty(slotA) || string.IsNullOrEmpty(slotB) || slotA == slotB)
            return;

        connections.Remove(NormalizePair(slotA, slotB));
        RebuildNetworks();
    }

    public void SendValue(string slotId, float value)
    {
        string root = GetRoot(slotId);
        audioValues[root] = value;

        // Bridge into DSP engine — slotId drives known parameters
        if (engine == null) return;
        switch (slotId)
        {
            case "VCO_FREQ": engine.SetVCOFreq(value * 2000f + 20f); break; // 0‑1 → 20‑2020 Hz
            case "VCO_PW":   engine.SetVCOPW(value);                 break;
            case "OUT_VOL":  engine.SetAudioVolume(value);            break;
        }
    }

    public float ReadValue(string slotId)
    {
        string root = GetRoot(slotId);
        return audioValues.TryGetValue(root, out float v) ? v : 0f;
    }

    private (string, string) NormalizePair(string slotA, string slotB)
    {
        return string.Compare(slotA, slotB, StringComparison.Ordinal) <= 0
            ? (slotA, slotB)
            : (slotB, slotA);
    }

    private string GetRoot(string slotId)
    {
        if (!parent.TryGetValue(slotId, out string root))
            return slotId;

        if (root == slotId)
            return root;

        root = GetRoot(root);
        parent[slotId] = root;
        return root;
    }

    private void EnsureSlot(string slotId)
    {
        if (!parent.ContainsKey(slotId))
            parent[slotId] = slotId;
    }

    private void Union(string a, string b)
    {
        string rootA = GetRoot(a);
        string rootB = GetRoot(b);
        if (rootA == rootB) return;
        parent[rootB] = rootA;
    }

    private void RebuildNetworks()
    {
        parent.Clear();

        foreach (var connection in connections)
        {
            EnsureSlot(connection.Item1);
            EnsureSlot(connection.Item2);
            Union(connection.Item1, connection.Item2);
        }

        var existingKeys = new List<string>(audioValues.Keys);
        foreach (var slotId in existingKeys)
            EnsureSlot(slotId);

        var newAudioValues = new Dictionary<string, float>();
        foreach (var kvp in audioValues)
        {
            string root = GetRoot(kvp.Key);
            newAudioValues[root] = kvp.Value;
        }

        audioValues = newAudioValues;
    }
}