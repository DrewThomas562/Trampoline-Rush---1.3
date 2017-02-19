using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class cActorWrapper {
    public List<cActor> spawnpoints;
}

/// <summary>
/// In Trampoline Rush, the Spawner is given the task of determining when and where falling hazards and objectives will appear...
/// during the course of the game. The Spawner controls most of the difficulty of the game, being in control of spawn timers...
/// and density of spawns within spawning columns.
/// The Spawner must be able to start waves, pause while the player is looking through menus, and stop at the end of waves.
/// </summary>

public class cSpawner : cActor {
    // In Trampoline Rush, delays are needed to prevent single spawnpoint columns from getting too cluttered with objects...
    // so spawnpoints can be grouped as sets - delays can prevent objects from overlapping too closely when spawning.
    // In Trampoline Rush, those sets are Windows vertical to each other.
    [Header("SPAWNPOINT SETS")]
    [SerializeField] private List<cActorWrapper> spawnpointSets;
    [SerializeField] private float spawnSetNextAvailableTimer;
    [SerializeField] private List<float> spawnSetNextAvailableTick;
    [Space(10)]

    // In Trampoline Rush, the difficulty curve depends mainly on the spawn timers of objectives, and hazards independently...
    // so the spawner uses two independent variable sets to separate the spawning behaviors of objectives, and hazards
    // Minimum and maximum spawn timers are dependent on the Spawner's level;
    [Header("DIFFICULTY")]
    [SerializeField] private int level;
    [SerializeField] private List<sMinMax> objectiveSpawnTimer;
    [SerializeField] private List<sMinMax> hazardSpawnTimer;
    [Space(10)]

    [Header("ACTORS")]
    [SerializeField] private List<cActor> objectives;
    [SerializeField] private List<cActor> hazards;
    private float objectiveSpawnTick;
    private float hazardSpawnTick;

    // Coroutine handle
    Coroutine cofxnSpawnerRoutine;

    // Cleanup for Spawner routine
    // And initial ticker setup
    protected override void OnDestroy() {
        if ( cofxnSpawnerRoutine != null ) { StopCoroutine(cofxnSpawnerRoutine); }
        base.OnDestroy();
    }
    protected override void Start() {
        bool bSPAWNER_FAILED_INITIALIZATION = false;

        // Check for spawnpoints
        for ( int i = 0; i < spawnpointSets.Count; i++ ) {
            for ( int j = 0; j < spawnpointSets[i].spawnpoints.Count; j++ ) {
                if ( spawnpointSets[i].spawnpoints[j] == null ) {
                    print("WARNING: Spawnpoint set " + i + " has an empty element. Removing element " + j);
                    spawnpointSets[i].spawnpoints.RemoveAt(j);
                }
            }
            if ( spawnpointSets[i].spawnpoints.Count < 1 ) {
                print("WARNING: Spawnpoint set " + i + " must have at least one spawnpoint. Removing element " + i);
                spawnpointSets.RemoveAt(i);
            }
        }
        if ( spawnpointSets.Count < 1 ) {
            print("ERROR: Spawner must have at least one spawnpoint set.");
            bSPAWNER_FAILED_INITIALIZATION = true;
        }

        // If no spawnpoints were found, destroy the spawner and request corrective action.
        if ( bSPAWNER_FAILED_INITIALIZATION ) {
            print("ERROR: Spawner failed to initialize. Please correct errors.");
            Destroy(this);
            return;
        }

        // Check to make sure there are objectives and hazards to spawn.
        for ( int i = 0; i < objectives.Count; i++ ) {
            if ( objectives[i] == null ) {
                print("WARNING: Spawner objective element " + i + " is empty. Removing element.");
                objectives.RemoveAt(i);
            }
        }
        if ( objectives.Count < 1 ) {
            print("WARNING: Spawner has no objective element to spawn.");
        }
        for ( int i = 0; i < hazards.Count; i++ ) {
            if ( objectives[i] == null ) {
                print("WARNING: Spawner hazard element " + i + " is empty. Removing element.");
                objectives.RemoveAt(i);
            }
        }
        if ( hazards.Count < 1 ) {
            print("WARNING: Spawner has no hazard element to spawn.");
        }

        // Initialize only if at least one objective or hazard can be spawned.
        if ( hazards.Count + objectives.Count < 1 ) {
            print("ERROR: Spawner failed to initialize. No elements to spawn. Please add hazards and objectives.");
            Destroy(this);
            return;
        }

        // Make sure spawn timers were set. Otherwise create default spawn times.
        if ( objectiveSpawnTimer.Count < 1 ) {
            objectiveSpawnTimer.Add(new sMinMax(0.5f, 1.0f));
            print("WARNING: Spawner has no objective spawn timer. Creating element with 0.5->1.0 seconds.");
        }
        if ( hazardSpawnTimer.Count < 1 ) {
            objectiveSpawnTimer.Add(new sMinMax(0.5f, 1.0f));
            print("WARNING: Spawner has no hazard spawn timer. Creating element with 0.5->1.0 seconds.");
        }

        // All spawnpoints are active at the start of the game.
        spawnSetNextAvailableTick = new List<float>();
        for ( int i = 0; i < spawnpointSets.Count; i++ ) { spawnSetNextAvailableTick.Add(0f); }

        // Set up all ticks to match a reasonable timer value before the game starts.
        // Otherwise objects will spawn the same frame that the Spawner routine begins.
        int SELECT_RATE = Mathf.Clamp(level, 0, objectiveSpawnTimer.Count - 1);
        objectiveSpawnTick = UnityEngine.Random.Range(objectiveSpawnTimer[SELECT_RATE].min, objectiveSpawnTimer[SELECT_RATE].max);
        SELECT_RATE = Mathf.Clamp(level, 0, hazardSpawnTimer.Count - 1);
        hazardSpawnTick = UnityEngine.Random.Range(hazardSpawnTimer[SELECT_RATE].min, hazardSpawnTimer[SELECT_RATE].max);

        StartSpawnerRoutine();
    }

    // The Spawner passes a spawn command to actors listed as one of its spawnpoints.
    // Spawnpoint availability for use is determined by checking if its spawn set availability ticker is down.
    // From the available spawnpoint sets, the Spawner chooses a random set, and random spawnpoint within the set...
    // And PASSES a spawn command to that spawnpoint. 
    // Afterwards, the spawn set's availability ticker is set and that set cannot be used until it is down. 
    public virtual cActor Generate(cActor SPAWNED) {
        if (SPAWNED == null) { return null; }
        
        // Check for available columns. Note that a default list has a count of 1 with null or 0 entry.
        bool bCOLUMN_AVAILABLE=false;
        List<int> AVAILABLE_COLUMNS = new List<int>();

        for ( int i = 0; i < spawnpointSets.Count; i++ ) {
            if ( spawnSetNextAvailableTick[i] <= 0 ) {
                AVAILABLE_COLUMNS.Add(i);
                bCOLUMN_AVAILABLE = true;
            }
        }
        if ( !bCOLUMN_AVAILABLE ) { return null; }

        // Select a spawnpoint from any of the available columns
        int SELECTED_COLUMN = AVAILABLE_COLUMNS[UnityEngine.Random.Range(0, AVAILABLE_COLUMNS.Count)];
        int SELECTED_WINDOW = UnityEngine.Random.Range(0, spawnpointSets[SELECTED_COLUMN].spawnpoints.Count);

        cActor CHOSEN_SPAWNPOINT = spawnpointSets[SELECTED_COLUMN].spawnpoints[SELECTED_WINDOW];
        if ( CHOSEN_SPAWNPOINT == null ) { return null; }

        // Tell the spawnpoint ot spawn instance of the given actor
        // Set the column ticker to mark it as unavailable until ticked down
        cActor INSTANCE = CHOSEN_SPAWNPOINT.Spawn(SPAWNED);
        spawnSetNextAvailableTick[SELECTED_COLUMN] = spawnSetNextAvailableTimer;

        return INSTANCE;
    }

    // The Spawner will continue to step its spawn timers so long as its routine is in play.
    // This routine can be initiated and interrupted in-game via starting the game, pauses, resumes, and so on.
    public void SetDifficulty(int LEVEL) {
        if ( LEVEL < 0 ) {
            print("Cannot set difficulty below level 0. Setting difficulty to 0.");
            LEVEL = 0;
        }
        level = LEVEL;
    }
    public void StopSpawnerRoutine() {
        if ( cofxnSpawnerRoutine != null ) { StopCoroutine(cofxnSpawnerRoutine); }
    }
    public void StartSpawnerRoutine() {
        StopSpawnerRoutine();
        cofxnSpawnerRoutine = StartCoroutine(RunSpawnerRoutine());
    }
    IEnumerator RunSpawnerRoutine() {
        // Continue running routine until stopped by an external command
        while ( true ){

            // If the game is paused 
            while ( bExecutionPaused ) { yield return null; }

            // Update spawnpoint availability
            for ( int i = 0; i < spawnpointSets.Count; i++ ) {
                if ( spawnSetNextAvailableTick[i] > 0 ) { spawnSetNextAvailableTick[i] -= Time.deltaTime; }
            }

            // Spawn the objective at intervals
            // Yield steps to avoid rare case where both spawns are run in a single frame
            StepObjectiveSpawner();
            yield return null;
            StepHazardSpawner();

            yield return null;
        }
    }

    // Tick and Timer steppers. Each time these functions are called, spawning ticks move toward zero.
    // When a tick hits zero, a spawn is triggered, and the tick is reset.
    void StepObjectiveSpawner() {
        if ( objectiveSpawnTick > 0 ) { objectiveSpawnTick -= Time.deltaTime; }
        else {
            if ( objectives.Count > 0 ) { Generate(objectives[UnityEngine.Random.Range(0, objectives.Count)]); }
            int SELECT_RATE = Mathf.Min(level, objectiveSpawnTimer.Count - 1);
            objectiveSpawnTick = UnityEngine.Random.Range(objectiveSpawnTimer[SELECT_RATE].min, objectiveSpawnTimer[SELECT_RATE].max);
        }
    }
    void StepHazardSpawner() {
        if ( hazardSpawnTick > 0 ) { hazardSpawnTick -= Time.deltaTime; }
        else {
            if ( hazards.Count > 0 ) { Generate(hazards[UnityEngine.Random.Range(0, hazards.Count)]); }            
            int SELECT_RATE = Mathf.Min(level, hazardSpawnTimer.Count - 1);
            hazardSpawnTick = UnityEngine.Random.Range(hazardSpawnTimer[SELECT_RATE].min, hazardSpawnTimer[SELECT_RATE].max);
        }
    }
}
