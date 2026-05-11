using System;
using System.Collections.Generic;
using UnityEngine;

public class PatchRouter : MonoBehaviour
{
    public static PatchRouter Instance;

    public ModularEngine engine;

    private Dictionary<string, float> audioValues = new();
    private Dictionary<string, string> parent = new();
    private HashSet<(string, string)> connections = new();
    private Dictionary<string, HashSet<string>> adjacency = new();

    void Awake()
    {
        Instance = this;
    }

    public void ConnectSlots(string slotA, string slotB)
    {
        if (string.IsNullOrEmpty(slotA) || string.IsNullOrEmpty(slotB) || slotA == slotB)
            return;

        Debug.Log($"[PatchRouter] ConnectSlots {slotA} <-> {slotB}");

        var edge = NormalizePair(slotA, slotB);
        connections.Add(edge);
        EnsureAdjacent(slotA);
        EnsureAdjacent(slotB);
        adjacency[slotA].Add(slotB);
        adjacency[slotB].Add(slotA);
        RebuildNetworks();

        Debug.Log($"[PatchRouter] After connect: root({slotA})={GetRoot(slotA)} root({slotB})={GetRoot(slotB)}");
    }

    public void DisconnectSlots(string slotA, string slotB)
    {
        if (string.IsNullOrEmpty(slotA) || string.IsNullOrEmpty(slotB) || slotA == slotB)
            return;

        var edge = NormalizePair(slotA, slotB);
        connections.Remove(edge);

        if (adjacency.TryGetValue(slotA, out var neighborsA))
            neighborsA.Remove(slotB);
        if (adjacency.TryGetValue(slotB, out var neighborsB))
            neighborsB.Remove(slotA);

        RebuildNetworks();
    }

    public void SendValue(string slotId, float value)
    {
        string root = GetRoot(slotId);
        audioValues[root] = value;

        if (engine == null) return;
        switch (slotId)
        {
            case "VCO_FREQ": engine.SetVCOFreq(value * 2000f + 20f); break;
            case "VCO_PW":   engine.SetVCOPW(value);                 break;
            case "OUT_VOL":  engine.SetAudioVolume(value);            break;
        }
    }

    public float ReadValue(string slotId)
    {
        string root = GetRoot(slotId);
        return audioValues.TryGetValue(root, out float v) ? v : 0f;
    }

    // Public so Mixbus can check which network a slot belongs to
    public string GetRoot(string slotId)
    {
        if (!parent.TryGetValue(slotId, out string root))
            return slotId;

        if (root == slotId)
            return root;

        root = GetRoot(root);
        parent[slotId] = root;
        return root;
    }

    private (string, string) NormalizePair(string slotA, string slotB)
    {
        return string.Compare(slotA, slotB, StringComparison.Ordinal) <= 0
            ? (slotA, slotB)
            : (slotB, slotA);
    }

    private void EnsureSlot(string slotId)
    {
        if (!parent.ContainsKey(slotId))
            parent[slotId] = slotId;
    }

    private void EnsureAdjacent(string slotId)
    {
        if (!adjacency.ContainsKey(slotId))
            adjacency[slotId] = new HashSet<string>();
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