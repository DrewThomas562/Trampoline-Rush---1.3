using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Game Manager for Trampoline Rush is the hub of central information for in-game actors.
/// Information such as difficulty, waves/levels completed, score, penalties, audio/graphics preferences
/// and the like are stored on a single accessible game object.
/// This Game Manager is also in charge of executing targeted commands at related actor groups in order to...
/// halt or resume routines, switch game and menu states, call a game reset, update actor information, etc.
/// </summary>

[Serializable]
public class cGameManager : cActor {
    //Static instance of GameManager which allows it to be accessed by any other script.
    public static cGameManager instance = null;

    [Header("GAME PROPERTIES")]
    [SerializeField] private cSpawner waveSpawner;
    [SerializeField] private List<float> gravityScale;
    private Vector2 defaultGravity;

    public int level;
    public int score;
    public List<int> scoreToNextLevel;

    protected override void Start() {
        base.Start();
        if ( instance == null ) { instance = this; }
        else { Destroy(this); }
        if ( waveSpawner == null ) { print("Game Controller was not linked to a wave spawner, or wave spawner was not found."); }
        if ( gravityScale.Count < 1 ) { gravityScale.Add(1.0f); }
        defaultGravity = Physics2D.gravity;
        Physics2D.gravity = defaultGravity * gravityScale[Mathf.Min(level,gravityScale.Count-1)];
    }
    public override IEnumerator PostBeginPlay() {
        yield return new WaitForEndOfFrame();
        waveSpawner = FindObjectOfType<cSpawner>();
        yield break;
    }

    public void GainScore(int SCORE) {
        score += SCORE;
        if ( level < scoreToNextLevel.Count ) {
            if ( score > scoreToNextLevel[level] ) { GainLevel(); }
        }
    }
    private void GainLevel() {
        level++;
        waveSpawner.SetDifficulty(level);
        Physics2D.gravity = defaultGravity * gravityScale[Mathf.Min(level, gravityScale.Count - 1)];
    }

    private void OnGUI() {
        GUI.Label(new Rect(0, 0, 32, 32), "Level:"+level);
        GUI.Label(new Rect(0, 32, 32, 32), "Score:" + score);
    }
}