using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
struct sMinMax {
    public float min;
    public float max;

    public sMinMax(float MIN, float MAX) {
        min = MIN;
        max = MAX;
    }
}

public class cActor : MonoBehaviour {
    protected bool bExecutionPaused;
    Coroutine cofxnMovePattern;

    protected virtual void OnDestroy() {
        if ( cofxnMovePattern != null ) { StopCoroutine(cofxnMovePattern); }
    }
    protected virtual void Start() {
        StartCoroutine(PostBeginPlay());
    }

    public virtual IEnumerator PostBeginPlay() {
        yield return new WaitForEndOfFrame();
        // Do stuff here
        yield break;
    }

    public virtual cActor Spawn(cActor SPAWNED) {
        if ( SPAWNED == null ) return null;

        cActor INSTANCE = Instantiate(SPAWNED);
        SPAWNED.transform.position = transform.position;

        return INSTANCE;
    }

    public virtual void ExecutionPause() { bExecutionPaused = true; }
    public virtual void ExecutionResume() { bExecutionPaused = false; }
    public virtual void MakeMove() {
        if ( cofxnMovePattern != null ) { StopCoroutine(cofxnMovePattern); }
        cofxnMovePattern = StartCoroutine(RunMovePattern());
    }
    protected virtual IEnumerator RunMovePattern() {
        // If paused, do nothing until unpaused
        while ( bExecutionPaused ) { yield return null; }

        // Execute behavior patterns here...
        yield return null;

        // End turn
        yield break;
    }
}