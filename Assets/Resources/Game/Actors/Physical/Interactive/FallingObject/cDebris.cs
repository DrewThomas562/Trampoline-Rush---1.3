using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cDebris : cFallingObject {
    private void OnTriggerEnter2D(Collider2D collision) {
        cPlayer OTHER = collision.GetComponent<cPlayer>();
        if ( OTHER == null ) { return; }
        else if ( OTHER.tag == "Player" ) {
            OTHER.GetComponent<cPlayer>().Stun();
            Destroy(gameObject);
        }
    }
}
