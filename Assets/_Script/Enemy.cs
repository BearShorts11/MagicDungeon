using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    enum EnemyState
    {
        idle,
        chase,
        attacking,
        stunned
    }


    [SerializeField]
    [Range(0f, 5f)]
    private float _speed;

    [SerializeField]
    private float _health;

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

    [SerializeField]
    private Transform playerTrans;
    private NavMeshAgent enemyNavAgent;
    private EnemyState state;
    
    private void Awake()
    {
        enemyNavAgent = GetComponent<NavMeshAgent>();
        state = EnemyState.idle;
        enemyNavAgent.speed = speed;
    }

    private void Update()
    {
        StateCheck();
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger, chasing");
            state = EnemyState.chase;
        }
    }

    private void StateCheck()
    {
        switch (state)
        {
            case EnemyState.idle:
                break;
                case EnemyState.chase:
                ChasePlayer();
                break;
        }
    }

    private void ChasePlayer() //Seperate method for chasing to allow for potential stunning in future
    {
        enemyNavAgent.SetDestination(playerTrans.position);   
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
