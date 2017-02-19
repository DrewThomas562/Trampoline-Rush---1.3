using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cSWAT : cVehicle {
    private void OnTriggerEnter2D(Collider2D collision) {
        cFallingObject OTHER = collision.GetComponent<cFallingObject>();
        if ( OTHER == null ) { return; }
        else if ( OTHER.tag == "Crook" ) { scoreKeeper.GainScore(1); }
        else { //if ( OTHER.tag == "Civilian" ) {
            // Play sound here
            player.FailedObjective();
        }
        Destroy(OTHER.gameObject);
    }
}
