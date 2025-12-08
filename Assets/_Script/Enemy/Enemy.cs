using Mono.Cecil.Cil;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    protected enum EnemyState
    {
        idle,
        chasing,
        attack,
        cooldown,
        stunned
    }


    [SerializeField]
    [Range(0f, 10f)]
    protected float _speed;

    [SerializeField]
    protected float _health;
    [SerializeField]
    protected float _impactDmg;

    [SerializeField]
    protected float _damage;

    [Tooltip("Chases forever when player is within this radius (Yellow)")][SerializeField]
    protected float detectionRadius = 10;
    [Tooltip("Attacks when player is within this radius (Red)")][SerializeField]
    protected float attackRadius = 2f;
    [SerializeField]
    protected float attackTime = 1f;
    [SerializeField]
    protected float cooldownTime = 1f;

    public float health
    {
        get { return _health; }
        set { _health = value; }
    }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    public float damage
    {
        get { return _damage; }
        set { _damage = value; }
    }

    public float impactDamage
    {
        get { return _impactDmg; }
        set { _impactDmg = value; }
    }

    protected Player player;

    protected NavMeshAgent enemyNavAgent;
    protected EnemyState state;
    
    protected virtual void Awake()
    {
        GameObject playerObject = GameObject.Find("Player");
        player = playerObject.GetComponent<Player>();

        enemyNavAgent = GetComponent<NavMeshAgent>();
        state = EnemyState.idle;
        enemyNavAgent.speed = speed;
    }

    protected virtual void Update()
    {
        StateCheck();
    }

    protected virtual void StateCheck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch (state)
        {
            case EnemyState.idle:
                // Detetcts if the player is within detetction radius.
                if (distanceToPlayer <= detectionRadius)
                {
                    state = EnemyState.chasing;
                }
                break;
            case EnemyState.chasing:
                ChasePlayer();
                break;
            case EnemyState.attack:
                break;
            case EnemyState.cooldown:
                break;
        }
    }

    protected virtual void ChasePlayer() //Seperate method for chasing to allow for potential stunning in future
    {
        enemyNavAgent.SetDestination(player.transform.position);
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackRadius)
        {
            state = EnemyState.attack;
            StartCoroutine(Attack());
        }
    }

    protected virtual IEnumerator Attack()
    {
        enemyNavAgent.isStopped = true;
        player.TakeDamage(damage);
        yield return new WaitForSeconds(attackTime);
        //could put damage here, recheck if player is within attack distance to see if they actually get damaged or not
        //aka play damage anim to give the player a chance to dodge?
        state = EnemyState.cooldown;
        StartCoroutine(Cooldown());
    }

    /// <summary>
    /// During cooldown, the enemy cannot attack but will chase the player.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        enemyNavAgent.isStopped = false;
        state = EnemyState.chasing;
    }

    public virtual void TakeDamage(float damage)
    {
        if(state == EnemyState.idle) //Enemy becomes aware of player when attacked
        {
            state = EnemyState.chasing;
        }
        health -= damage;
        if (health <= 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected virtual void OnDrawGizmos() //Allows for easier editor only editing
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
