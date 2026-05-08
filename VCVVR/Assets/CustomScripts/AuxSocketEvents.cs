using UnityEngine;
using System;

public static class AuxSocketSlotEvents
{
    public static Action<AuxCable, AuxSocketSlot, AuxEnd> OnPlugConnected;
    public static Action<AuxCable, AuxSocketSlot, AuxEnd> OnPlugDisconnected;
}