using UnityEngine;
using System.Collections.Generic;

public class ModularEngine : MonoBehaviour
{
    public struct Connection
    {
        public int fromModule, fromPort;
        public int toModule,   toPort;
    }

    private VCO_DSP      vco;
    private AudioOutput_DSP audioOut;

    public List<Connection> connections = new();

    void Start()
    {
        float sr = AudioSettings.outputSampleRate;
        vco      = new VCO_DSP(sr);
        audioOut = new AudioOutput_DSP();

        // Default: connect VCO sin out (M0 P0) → AudioOut left (M1 P0)
        //          connect VCO sin out (M0 P0) → AudioOut right (M1 P1)
        // Remove these if you want patching to be fully manual
        Connect(0, 0, 1, 0);
        Connect(0, 0, 1, 1);
    }

    public void Connect(int fromModule, int fromPort, int toModule, int toPort)
    {
        // Avoid duplicate connections
        foreach (var c in connections)
            if (c.fromModule == fromModule && c.fromPort == fromPort &&
                c.toModule   == toModule   && c.toPort   == toPort)
                return;

        connections.Add(new Connection
        {
            fromModule = fromModule, fromPort = fromPort,
            toModule   = toModule,   toPort   = toPort
        });
        Debug.Log($"[ModularEngine] Connected M{fromModule}P{fromPort} → M{toModule}P{toPort}");
    }

    public void Disconnect(int fromModule, int fromPort, int toModule, int toPort)
    {
        connections.RemoveAll(c =>
            c.fromModule == fromModule && c.fromPort == fromPort &&
            c.toModule   == toModule   && c.toPort   == toPort);
        Debug.Log($"[ModularEngine] Disconnected M{fromModule}P{fromPort} → M{toModule}P{toPort}");
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        int frames = data.Length / channels;

        for (int i = 0; i < frames; i++)
        {
            // Advance DSP by one sample
            vco.ProcessBlock(1);

            // Route connections
            audioOut.SetInput(0, 0f);
            audioOut.SetInput(1, 0f);
            foreach (var c in connections)
            {
                float v = GetModuleOutput(c.fromModule, c.fromPort);
                SetModuleInput(c.toModule, c.toPort, v);
            }

            // Write to buffer
            data[i * channels]     = audioOut.GetLeft();
            if (channels > 1)
                data[i * channels + 1] = audioOut.GetRight();
        }
    }

    float GetModuleOutput(int moduleId, int portId)
    {
        if (moduleId == 0) return vco.GetOutput(portId);
        return 0f;
    }

    void SetModuleInput(int moduleId, int portId, float value)
    {
        if (moduleId == 1) audioOut.SetInput(portId, value);
    }

    public void SetVCOFreq(float hz) => vco.SetParam(0, hz);
    public void SetVCOPW(float pw)   => vco.SetParam(1, pw);
    public void SetAudioVolume(float v) => audioOut.SetParam(0, v);
}