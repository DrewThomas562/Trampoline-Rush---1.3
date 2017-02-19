using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Declarations
[Serializable]
public partial class cPhysicalObject : cActor {
    // Movement coroutine handling
    new protected Rigidbody2D rigidbody;
    private delegate IEnumerator delegateMoveType(Vector2 VECTOR, float MODIFIER);
    private event delegateMoveType selectedMoveType;
    Coroutine cofxnMoveType;
    bool bIsMoving;

    protected override void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
        selectedMoveType = MoveTypeQuadratic;
        base.Start();
    }
}

// Movement scripts
public partial class cPhysicalObject {
    protected override void OnDestroy() {
        base.OnDestroy();
        bIsMoving = false;
        if ( cofxnMoveType != null ) { StopCoroutine(cofxnMoveType); }
    }

    protected void StartMove(Vector2 TRANSLATION_VECTOR, float MODIFIER) {
        float x, y;
        x = Mathf.Round(transform.position.x / 0.32f) * 0.32f;
        y = Mathf.Round(transform.position.y / 0.32f) * 0.32f;

        transform.position = new Vector2(x,y);

        bIsMoving = true;
        if (cofxnMoveType != null) {
            StopCoroutine(cofxnMoveType);
        }
        cofxnMoveType = StartCoroutine(selectedMoveType(TRANSLATION_VECTOR, MODIFIER));
    }
    protected IEnumerator MoveTypeInstant(Vector2 TRANSLATION, float DELAY) {
        Vector2 END = rigidbody.position + TRANSLATION;
        yield return new WaitForSeconds(DELAY);
        rigidbody.MovePosition(END);
        bIsMoving = false;
    }
    protected IEnumerator MoveTypeLinear(Vector2 TRANSLATION, float MOVE_TIME){
        float INVERSE_MOVE_TIME = 1f/MOVE_TIME;
        float START_TIME = Time.time;
        float END_TIME = START_TIME + MOVE_TIME;
        Vector2 START = rigidbody.position;
        Vector2 END = START + TRANSLATION;

        while (Time.time < END_TIME) {
            rigidbody.MovePosition(START + TRANSLATION * (Time.time-START_TIME)*INVERSE_MOVE_TIME);
            yield return null;
        }
        rigidbody.MovePosition(END);
        bIsMoving = false;
    }
    protected IEnumerator MoveTypeQuadratic(Vector2 TRANSLATION, float MOVE_TIME) {
        float SINE_MOD = 180f / MOVE_TIME;
        float START_TIME = Time.time;
        float END_TIME = START_TIME + MOVE_TIME;
        Vector2 START = rigidbody.position;
        Vector2 END = START + TRANSLATION;

        while (Time.time < END_TIME) {
            rigidbody.MovePosition(START + TRANSLATION * (Mathf.Sin(Mathf.Deg2Rad * (-90f + (Time.time - START_TIME) * SINE_MOD))+1f)/2f);
            yield return null;
        }
        rigidbody.MovePosition(END);
        bIsMoving = false;
    }
}

// AI inputs
public partial class cPhysicalObject {
    public Collider2D[] DetectObjects(Vector2 center) {
        Collider2D[] hitColliders = Physics2D.OverlapPointAll(center);
        return hitColliders;
    }
}
//    public bool CheckBlockingAdjacent(Vector2 TRANSLATION) {
//        Vector2 ADJACENT = (Vector2)transform.position + TRANSLATION;
//        Collider2D[] DETECTED_OBJECTS = DetectObjects(ADJACENT);
//
//        foreach ( Collider2D OTHER in DETECTED_OBJECTS ) {
//            cTile TILE_OTHER = OTHER.GetComponent<cTile>();
//            if ( TILE_OTHER != null ) {
//                if ( TILE_OTHER.CheckIfBlocked(GetComponent<cPhysicalObject>(), traversibleTerrain) ) {
//                    return true;
//                }
//            }
//        }
//        return false;
//    }