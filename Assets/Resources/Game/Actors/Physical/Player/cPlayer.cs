using UnityEngine;
using System.Collections;

public partial class cPlayer : cPhysicalObject {
    [Header("BOUNCE TARGETS")]
    [SerializeField] private cAmbulance ambulance;
    [SerializeField] private cSWAT swat;
    [Space(10)]

    [Header("PLAYER PROPERTIES")]
    [SerializeField] private float defaultStunTimer;
    [SerializeField] private float runSpeed;
    private float stunTick;
    [Space(10)]

    [Header("ANIMATION")]
    private Animator animator;
    [SerializeField] private float idleMarginWidth;
    private sMinMax idleMargins;
    [Space(10)]

    [Header("MOUSE MOVEMENT")]
    [SerializeField] private bool bClampX;
    [SerializeField] private bool bClampY;
    [SerializeField] private sMinMax ClampX;
    [SerializeField] private sMinMax ClampY;
    private Vector3 mousePosition;

    private enum enumDirection {
        Left,
        Right
    };
    private enumDirection tiltDirection;

    protected override void Start() {
        base.Start();

        bool bFAILED_INITIALIZATION = false;
        if ( ambulance == null ) { bFAILED_INITIALIZATION = true; print("ERROR: The Player is not connected to and instance of the Ambulance car to bounce civilians."); }
        if ( swat == null ) { bFAILED_INITIALIZATION = true; print("ERROR: The Player is not connected to and instance of the SWAT car to bounce crooks."); }

        if ( bFAILED_INITIALIZATION ) { Destroy(gameObject); print("ERROR: Player destroyed. The player must have connections to an Ambulance and SWAT car."); }

        animator = GetComponent<Animator>();
    }
    public override IEnumerator PostBeginPlay() {
        yield return new WaitForEndOfFrame();
        GotoIdle();
        yield break;
    }

    public void FailedObjective() { Stun(); }
    public void Stun( float SECONDS = -1f ) {
        if ( SECONDS < 0 ) { SECONDS = defaultStunTimer; }
        stunTick = SECONDS;
        animator.SetTrigger("Trigger_Dizzy");
    }
    public void GotoIdle() {
        if ( tiltDirection == enumDirection.Left ) { animator.SetTrigger("Trigger_IdleLeft"); }
        else { animator.SetTrigger("Trigger_IdleRight"); }

        // Set up idle margins
        idleMargins.min = transform.position.x - idleMarginWidth*0.5f;
        idleMargins.max = transform.position.x + idleMarginWidth*0.5f;
    }
    public void ChangeTilt() {
        if ( tiltDirection == enumDirection.Left ) { tiltDirection = enumDirection.Right; }
        else { tiltDirection = enumDirection.Left; }

        if ( stunTick <= 0 ) { GotoIdle(); }
    }

    private void Update() {
        // Update trampoline tilt direction
        if ( Input.GetMouseButtonDown(0) ) { ChangeTilt(); }

        if ( stunTick > 0 ) {
            stunTick -= Time.deltaTime;
            if ( stunTick <= 0 ) { GotoIdle(); }
            return;
        }

        // Update mouse position
        mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        if ( bClampX ) { mousePosition.x = Mathf.Clamp(mousePosition.x, ClampX.min, ClampX.max); }
        if ( bClampY ) { mousePosition.y = Mathf.Clamp(mousePosition.y, ClampY.min, ClampY.max); }

        // If the mouse is moving outside of a small "idle" margin, the player will run toward the mouse position
        if ( mousePosition.x < idleMargins.min || mousePosition.x > idleMargins.max ) {

            // Set up new idle margins
            if ( mousePosition.x < idleMargins.min ) {
                idleMargins.min = mousePosition.x;
                idleMargins.max = idleMargins.min + idleMarginWidth;
            }
            else {
                idleMargins.max = mousePosition.x;
                idleMargins.min = idleMargins.max - idleMarginWidth;
            }
        }

        // Move the player toward the idle margins
        bool bRunning=false;
        if ( transform.position.x > idleMargins.max ) { bRunning = true; transform.position = (Vector2)transform.position + new Vector2(-Mathf.Min(transform.position.x - idleMargins.max, runSpeed * Time.deltaTime), 0f); }
        else if ( transform.position.x < idleMargins.min ) { bRunning = true; transform.position = (Vector2)transform.position + new Vector2(Mathf.Min(idleMargins.min - transform.position.x, runSpeed * Time.deltaTime), 0f); }
        // Trigger running animation
        if ( bRunning ) {
            if ( tiltDirection == enumDirection.Left ) {
                if ( !animator.GetCurrentAnimatorStateInfo(0).IsName("Player_RunLeft") ) { animator.SetTrigger("Trigger_RunLeft"); }
            }
            else { // tiltDirection == enumDirection.Right
                if ( !animator.GetCurrentAnimatorStateInfo(0).IsName("Player_RunRight") ) { animator.SetTrigger("Trigger_RunRight"); }
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        cFallingObject OTHER = collision.GetComponent<cFallingObject>();
        if ( OTHER == null ) { return; }
        else if ( OTHER.tag == "Crook" || OTHER.tag == "Civilian" ) {
            Rigidbody2D OTHER_RIGIDBODY = OTHER.GetComponent<Rigidbody2D>();
            if ( OTHER_RIGIDBODY.velocity.y > 0 ) { return; }

            // Depending on the trampoline's tilt direction, set velocity so the objective goes either to the SWAT or Ambulance
            float NEW_VELOCITY_X;
            float NEW_VELOCITY_Y = Mathf.Abs(OTHER_RIGIDBODY.velocity.y) * UnityEngine.Random.Range(0.7f, 0.8f);
            if ( tiltDirection == enumDirection.Left ) {
                NEW_VELOCITY_X = -(swat.transform.position.x - OTHER.transform.position.x) / (NEW_VELOCITY_Y / Physics2D.gravity.y);
                OTHER_RIGIDBODY.angularVelocity += UnityEngine.Random.Range(90f, 180f);
            } else {
                NEW_VELOCITY_X = -(ambulance.transform.position.x - OTHER.transform.position.x) / (NEW_VELOCITY_Y / Physics2D.gravity.y);
                OTHER_RIGIDBODY.angularVelocity -= UnityEngine.Random.Range(90f, 180f);
            }
            OTHER_RIGIDBODY.velocity = new Vector2(NEW_VELOCITY_X * 0.5f, NEW_VELOCITY_Y);
        }
    }
}
    //: cPawn {
    //    public Texture healthIcon;
    //    public Rect healthIconDisplay = new Rect(Screen.width-64, 0, 32, 32);
    //    public Rect healthValueDisplay = new Rect(Screen.width-32, 0, 32, 32);

    //    protected override void Start() {
    //        base.Start();
    //    }
    //
    //    private void OnGUI() {
    //        healthIconDisplay.position = new Vector2(Screen.width - 64f, 0);
    //        healthValueDisplay.position = new Vector2(Screen.width - 32f, 0);
    //        GUI.DrawTexture(healthIconDisplay, healthIcon, ScaleMode.StretchToFill, true, 1.0f);
    //        GUI.Label(healthValueDisplay, "" + health);
    //    }

    //    protected override IEnumerator RunMovePattern() {
    //
    //        // Do not run calculations until the player is done moving
    //        while ( bIsMoving ) { yield return null; }
    //
    //        // If the player is dead, go ahead and skip their turn
    //        if ( health < 1 ) {
    //            // End of turn
    //            bTurnInProgress = false;
    //            yield break;
    //        }
    //
    //        // Now, wait for the player to make a legal move into an empty nearby space
    //        bool bMOVE_BLOCKED;
    //        do {
    //            int HORIZONTAL;
    //            int VERTICAL;
    //            bool TRIED_BOMB = false;
    //
    //            // First, grab the player's directional input. Cannot move diagonally.
    //            do {
    //                VERTICAL = (int)(Input.GetAxisRaw("Vertical"));
    //                HORIZONTAL = (int)(Input.GetAxisRaw("Horizontal"));
    //
    //                // If the player presses SPACE, skip turns
    //                if ( Input.GetKey(KeyCode.Space) ) {
    //                    // End of turn
    //                    bTurnInProgress = false;
    //                    yield break;
    //                }
    //                // If the player presses "F", try and activate a bomb
    //                else if ( Input.GetKeyDown(KeyCode.F) && !TRIED_BOMB ) {
    //                    TRIED_BOMB = true;
    //                    cItem BOMB_ITEM = inventory.FindItem<cPowderBomb>();
    //                    if ( BOMB_ITEM != null ) {
    //                        // If the player has at least one key, use it to unlock the door, and allow the player through
    //                        cPowderBomb BOMB = BOMB_ITEM.GetComponent<cPowderBomb>();
    //                        if ( BOMB.charges > 0 ) {
    //                            BOMB.Activate();
    //                            yield return new WaitForSeconds(moveSpeed);
    //
    //                            // End of turn
    //                            bTurnInProgress = false;
    //                            yield break;
    //                        }
    //                    }
    //                    print("You have no bombs!");
    //                }
    //                yield return null;
    //            } while ( HORIZONTAL == 0 && VERTICAL == 0 );
    //            if ( HORIZONTAL != 0 ) { VERTICAL = 0; }
    //
    //            // With player's input, determine if objects in selected adjacent cell are blocking. 
    //            // Only one blocking object is required to count as blocking
    //            Vector2 DESIRED_MOVE = new Vector2(HORIZONTAL, VERTICAL) * 0.32f;
    //            bMOVE_BLOCKED = CheckBlockingAdjacent(DESIRED_MOVE);
    //
    //            // If there were no blocking objects in the adjacent cell, the player can move there.
    //            if ( !bMOVE_BLOCKED ) {
    //                StartMove(DESIRED_MOVE, moveSpeed);
    //                while ( bIsMoving ) { yield return null; }
    //
    //                // End of turn
    //                bTurnInProgress = false;
    //                yield break;
    //            }
    //
    //            // Repeat loop until the player makes a move into an open tile
    //            yield return null;
    //        } while (bMOVE_BLOCKED);
    //    }


    //    public override void TakeDamage(cPhysicalObject INSTIGATOR, int DAMAGE, enumDamageType TYPE = enumDamageType.Normal, Vector2 DIRECTION = default(Vector2)) {
    //        health -= DAMAGE;
    //        print("OUCH! " + INSTIGATOR.name + " hit you for "+DAMAGE+" damage!");
    //        if ( health <= 0 ) {
    //            StartCoroutine(Died(INSTIGATOR));
    //        }
    //    }

    //    public virtual IEnumerator Died(cPhysicalObject INSTIGATOR) {
    //        print("You have been killed by "+INSTIGATOR.name);
    //        yield return new WaitForSeconds(3f);
    //        Application.LoadLevel(Application.loadedLevel);
    //        yield break;
    //    }

    //    virtual protected void OnTriggerEnter2D(Collider2D OTHER) {
    //        cItem ITEM_OTHER = OTHER.GetComponent<cItem>();
    //        if (ITEM_OTHER != null) {
    //            inventory.PickUp(ITEM_OTHER);
    //        }
    //    }