using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cAmbulance : cVehicle {
    private void OnTriggerEnter2D(Collider2D collision) {
        cFallingObject OTHER = collision.GetComponent<cFallingObject>();
        if ( OTHER == null ) { return; }
        else if ( OTHER.tag == "Civilian" ) { scoreKeeper.GainScore(1); }
        else { //if ( OTHER.tag == "Crook" ) {
            // Play sound here
            player.FailedObjective();
        }
        Destroy(OTHER.gameObject);
    }
}
