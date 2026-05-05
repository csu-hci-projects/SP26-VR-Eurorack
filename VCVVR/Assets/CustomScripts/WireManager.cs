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
}
