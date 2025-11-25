using System.Net;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player instance;


    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private NavMeshAgent playerNavAgent;
    [SerializeField]
    private InputAction leftClick;
    [SerializeField]
    private InputAction rightClick;

    int groundMask; //for ground only raycast
    int ignoreMask; //Assigned in awake for now; will need to update mask when a new scene is loaded
    int propMask; //prop mask on player to allow clicking behind objects
    int wallMask; //NOT IMPLEMENTED: would allow player to click through walls onto ground or an enemy
    int enemyMask; //testing
    int ignoreEnemyTriggerMask; //Specifically for enemy triggers getting hit by raycast, making EnemyOnly spell interactions a nightmare

    [Range(0, 3)] //Only one of four possible slots
    int currentSpellIndex = 0; //used for checking player's current list of spells
    
    [SerializeField]
    SpellInfo currentSpell;

    [SerializeField]
    private float _health;
    private float _healthRegenModifier;
    [SerializeField]
    private float _mana;
    private float _manaRegenModifier;

    public float health
    {
        get { return _health; }
        set { _health = value; }
    }

    public float healthRegenModifier
    {
        get { return _healthRegenModifier; }
        set { _healthRegenModifier = value; }
    }

    public float mana
    {
        get { return _mana; }
        set { _mana = value; }
    }

    public float manaRegenModifier
    {
        get { return _manaRegenModifier;  }
        set { _manaRegenModifier = value; }
    }

    private void Awake()
    {
        instance = this;

        //just in case
        if (playerNavAgent == null)
        {
            playerNavAgent = GetComponent<NavMeshAgent>();
        }

        enemyMask = LayerMask.GetMask("Enemy");

        groundMask = LayerMask.GetMask("Ground");
        propMask = LayerMask.GetMask("Props");
        wallMask = LayerMask.GetMask("Walls");
        ignoreEnemyTriggerMask = LayerMask.GetMask("Ignore Raycast");

        ignoreMask = ~propMask & ~wallMask & ~ignoreEnemyTriggerMask;
    }



    private void OnEnable()
    {
        leftClick.Enable();
        leftClick.performed += ClickMove;

        rightClick.Enable();
        rightClick.performed += ClickCast;
    }

    private void OnDisable()
    {
        leftClick.Disable();
        leftClick.performed -= ClickMove;

        rightClick.Disable();
        rightClick.performed -= ClickCast;
    }
    
    private void ClickMove(InputAction.CallbackContext context)
    {
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            playerNavAgent.SetDestination(hit.point);
            Debug.Log("Click has hit: " + hit.collider.name);
        }
    } //Movement
    
    private void ClickCast(InputAction.CallbackContext context)
    {
        Debug.Log("Successful rightClick, attempting to cast");   
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~propMask, QueryTriggerInteraction.Ignore))
        {
            SpellContext ctx = CreateContext(hit);
            if (currentSpell.logic.CanCast(ctx))
            {
                Debug.Log("Casting!");
                currentSpell.logic.Cast(ctx);
            }
        }
    }

    
    /// <summary>
    /// Assigns data into SpellContext for Cast in SpellLogic derived scripts.
    /// </summary>
    /// <param name="hit">RaycastHit for what the player clicked on.</param>
    /// <returns>SpellContext needed for Cast in SpellLogic derived scripts.</returns>
    private SpellContext CreateContext(RaycastHit hit)
    {
        SpellContext ctx = new SpellContext();

        ctx.spellCaster = this.gameObject;

        //First ray (done when first casting): hits topmost collider, no masks so this can be used for collisions, could also be ground but will be ignored in Spell logic unless ground
        RaycastHit hitAny = hit;

        //Second ray: Only check for a ground hit point
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hitGround;
        bool groundHit = Physics.Raycast(ray, out hitGround, Mathf.Infinity, groundMask);

        //Get current spell, get spell's targetingType and assign it's needed context (target and targetPoint; also needed for distance clamp)
        switch (currentSpell.targetingType)
        {
            case TargetingType.EnemyOnly:
                //Only assign target if an enemy was clicked (avoids clicking triggers on enemy)
                if (!hitAny.collider.isTrigger && hitAny.collider.CompareTag("Enemy"))
                {
                    Enemy enemy = hitAny.collider.GetComponentInParent<Enemy>();
                    if (enemy != null)
                    {
                        Debug.Log("EnemyOnly hit!\nSetting enemy as target!");
                        ctx.target = enemy.gameObject;
                        ctx.targetPoint = enemy.transform.position;
                    }
                }
                else
                {
                    ctx.target = null;
                }
                break;

            case TargetingType.GroundOnly:
                // Always ground
                ctx.target = null;
                ctx.targetPoint = hitGround.point;
                break;

            case TargetingType.Self:
                ctx.target = this.gameObject;
                ctx.targetPoint = transform.position;
                break;

            case TargetingType.PointOrEnemy:
                //Allows a spell to hit either a point or enemy
                if (hitAny.collider != null && hitAny.collider.CompareTag("Enemy"))
                {
                    Debug.Log("PointOrEnemy spell clicked an enemy!");
                    ctx.target = hitAny.collider.gameObject; // enemy clicked
                    ctx.targetPoint = hitAny.collider.gameObject.transform.position;
                }
                else
                {
                    ctx.target = null; // ground clicked
                    ctx.targetPoint = hitGround.point;
                }
                break;
        }

        ctx.distanceToPoint = Vector3.Distance(ctx.spellCaster.transform.position, ctx.targetPoint);

        ctx.spellInfo = currentSpell;

        return ctx;
    }
}
