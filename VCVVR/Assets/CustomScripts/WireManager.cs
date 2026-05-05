using UnityEngine;

public class WireManager : MonoBehaviour {
    public ModularEngine engine;

    private Jack firstJack;

    public void ClickJack(Jack jack) {
        if (firstJack == null) {
            firstJack = jack;
            return;
        }

        if (firstJack.isOutput != jack.isOutput) {
            Jack outJack = firstJack.isOutput ? firstJack : jack;
            Jack inJack = firstJack.isOutput ? jack : firstJack;

            engine.Connect(
                outJack.moduleId,
                outJack.portId,
                inJack.moduleId,
                inJack.portId
            );
        }

        firstJack = null;
    }
    void Awake()
{
    AuxSocketSlot.WireManager = this;
}
public void DisconnectJack(Jack jack)
{
    // If this jack was half of a pending pair, clear it
    if (firstJack == jack)
    {
        firstJack = null;
        return;
    }

    // Otherwise find and remove all connections involving this jack
    if (jack.isOutput)
        engine.connections.RemoveAll(c => c.fromModule == jack.moduleId && c.fromPort == jack.portId);
    else
        engine.connections.RemoveAll(c => c.toModule == jack.moduleId && c.toPort == jack.portId);
}
}
