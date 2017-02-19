using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cVehicle : cPhysicalObject {
    protected cGameManager scoreKeeper;
    protected cPlayer player;

    public override IEnumerator PostBeginPlay() {
        yield return new WaitForEndOfFrame();
        scoreKeeper = FindObjectOfType<cGameManager>();
        player = FindObjectOfType<cPlayer>();
        yield break;
    }
}
