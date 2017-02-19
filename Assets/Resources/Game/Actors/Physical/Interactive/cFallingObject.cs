using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cFallingObject : cPhysicalObject {
    private static float killZ = -3f;
    private static cPlayer player;

    protected override void Start() {
        base.Start();
        if ( player == null ) { player = FindObjectOfType<cPlayer>(); } 
    }

    private void Update() {
        if ( transform.position.y < killZ ) {
            if ( tag == "Crook" || tag == "Civilian" ) { player.FailedObjective(); }
            Destroy(gameObject);
        }
    }
}
