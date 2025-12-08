using System.Net;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player instance;


    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    public NavMeshAgent playerNavAgent;
    #region Input
    [SerializeField]
    private InputAction leftClick;
    [SerializeField]
    private InputAction rightClick;
    [SerializeField]
    private InputAction spell1Key;
    [SerializeField]
    private InputAction spell2Key;
    [SerializeField]
    private InputAction spell3Key;
    [SerializeField]
    private InputAction spell4Key;

    private System.Action<InputAction.CallbackContext> spell1Delegate;
    private System.Action<InputAction.CallbackContext> spell2Delegate;
    private System.Action<InputAction.CallbackContext> spell3Delegate;
    private System.Action<InputAction.CallbackContext> spell4Delegate;
    #endregion

    int groundMask; //for ground only raycast
    int ignoreMask; //Assigned in awake for now; will need to update mask when a new scene is loaded
    int propMask; //prop mask on player to allow clicking behind objects
    int wallMask; //NOT IMPLEMENTED: would allow player to click through walls onto ground or an enemy
    int enemyMask; //testing
    int ignoreEnemyTriggerMask; //Specifically for enemy triggers getting hit by raycast, making EnemyOnly spell interactions a nightmare

    [Range(0, 3)] //Only one of four possible slots
    int currentSpellIndex = 0; //used for checking player's current list of spells
    
    [SerializeField]
    public SpellInfo currentSpell;

    [SerializeField]
    private SpellInfo[] spellSlots = new SpellInfo[4]; //max of 4 spells

    #region Player Stats

    // Regen modifers set to 1 to act as percentages and allows base regen

    [SerializeField]
    private float _health;
    private float _healthRegenModifier = 1f;
    [SerializeField]
    private float _mana;
    private float _manaRegenModifier = 1f;

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

    [Header("Regen Settings")]

    [SerializeField] 
    private float baseHealthRegen = 1f;
    [SerializeField] 
    private float baseManaRegen = 1f;
    [SerializeField] 
    private float regenInterval = 2f;
    #endregion

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

        spell1Delegate = ctx => SelectSpell(0);
        spell2Delegate = ctx => SelectSpell(1);
        spell3Delegate = ctx => SelectSpell(2);
        spell4Delegate = ctx => SelectSpell(3);
    }
    //avoiding any stats not being initialized before running regentick
    private void Start()
    {
        InvokeRepeating(nameof(RegenTick), regenInterval, regenInterval);
    }

    private void OnEnable()
    {
        leftClick.Enable();
        leftClick.performed += ClickMove;

        rightClick.Enable();
        rightClick.performed += ClickCast;

        spell1Key.Enable();
        spell1Key.performed += spell1Delegate;
        spell2Key.Enable();
        spell2Key.performed += spell2Delegate;
        spell3Key.Enable();
        spell3Key.performed += spell3Delegate;
        spell4Key.Enable();
        spell4Key.performed += spell4Delegate;
    }

    private void OnDisable()
    {
        leftClick.Disable();
        leftClick.performed -= ClickMove;
        rightClick.Disable();
        rightClick.performed -= ClickCast;

        spell1Key.Disable();
        spell1Key.performed -= spell1Delegate;
        spell2Key.Disable(); 
        spell2Key.performed -= spell2Delegate;
        spell3Key.Disable(); 
        spell3Key.performed -= spell3Delegate;
        spell4Key.Disable();
        spell4Key.performed -= spell4Delegate;
    }

    private void ClickMove(InputAction.CallbackContext context)
    {
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Debug.DrawRay(ray.origin, ray.direction, Color.black);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask))
        {
            playerNavAgent.SetDestination(hit.point);
            Debug.Log("Click has hit: " + hit.collider.name);
        }
    } //Movement
    
    private void ClickCast(InputAction.CallbackContext context)
    {
        Ray ray = playerCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~propMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.black, 1f); //true click ray
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
        SpellContext ctx = new SpellContext()
        {
            spellCaster = this.gameObject
        };

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
                        //Debug.Log("EnemyOnly hit!\nSetting enemy as target!"); // Checking if I was actually hitting an enemy with targetType
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
       /* if (ctx.target != null && ctx.target.tag == "Enemy")
        {
            Debug.DrawLine(Camera.main.transform.position, ctx.target.transform.position, Color.red, 2f); //For enemies 
        }
        Debug.DrawLine(Camera.main.transform.position, ctx.targetPoint, Color.HSVToRGB(30f, 100f, 38.43f), 2f); //draws brown to ground
       */ //Only for checking type with ray

        ctx.distanceToPoint = Vector3.Distance(ctx.spellCaster.transform.position, ctx.targetPoint);

        ctx.spellInfo = currentSpell;

        return ctx;
    }
    /// <summary>
    /// Method for spell selecting by player
    /// </summary>
    /// <param name="index">spell index number</param>
    private void SelectSpell(int index)
    {
        if (index < 0 || index >= spellSlots.Length)
        {
            Debug.LogWarning("Invalid spell index " + index);
            return;
        }

        if (spellSlots[index] == null)
        {
            Debug.LogWarning($"No spell assigned to slot {index + 1}");
            return;
        }

        currentSpellIndex = index;
        currentSpell = spellSlots[index];

        Debug.Log($"Selected spell {index + 1}: {currentSpell.spellName}");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Taking {damage} damage!");
        instance.health -= damage;
        if(instance.health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        PlayerUI.s.AddMessage("You Died!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RegenTick()
    {
        // Health regen
        float healthGain = baseHealthRegen * healthRegenModifier;
        health = Mathf.Min(health + healthGain, 100f); //No overhealing unless explicitly overheal, not implemented, but regen should never go over 100

        // Mana regen
        float manaGain = baseManaRegen * manaRegenModifier;
        mana = Mathf.Min(mana + manaGain, 50f); //No over-mana in general
    }
}
