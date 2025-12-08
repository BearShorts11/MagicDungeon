using System.Collections;
using UnityEngine;
using UnityEngine.AI;

//This ranged enemy is NOT the same as an enemy 
public class Enemy_Ranged : Enemy
{
    [Tooltip("Value used to run when player is within radius (blue)")][SerializeField]
    private float safeRadius = 5f; 

    [SerializeField]
    private GameObject projectilePrefab; //Projectiel to shoot
    [SerializeField]
    private Transform firePoint; //where projectile spawns

    private bool isRunAway = false;
    private Vector3 fleeTarget;
    private Vector3 playerPos;

    [Tooltip("How often, in seconds, the enemy calculates a new point to run away to.")][SerializeField] 
    private float runAwayRecalcTime = 1.5f; //How long to keep running before recalculating run away point
    private bool runAwayRunning = false;

    protected override void StateCheck()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // Check if player is too close
        if (distanceToPlayer < safeRadius)
        {
            isRunAway = true;

            // start Run Away routine if not already fleeing
            if (!runAwayRunning)
            {
                StartCoroutine(RunAway());
            }

            return;
        }
        else
        {
            isRunAway = false;
        }

        //normal ranged behavior
        switch (state)
        {
            case EnemyState.idle:
                if (distanceToPlayer <= detectionRadius)
                    state = EnemyState.chasing;
                break;

            case EnemyState.chasing:
                ChasePlayer();
                break;

            case EnemyState.attack:
                // Attack will be triggered via coroutine
                break;

            case EnemyState.cooldown:
                break;
        }
    }



    protected override void ChasePlayer()
    {
        if (isRunAway) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= attackRadius)
        {
            state = EnemyState.attack;
            StartCoroutine(Attack());
        }
        else
        {
            enemyNavAgent.SetDestination(player.transform.position);
        }
    }

    /// <summary>
    ///Creates a point opposite of the player to Run Away (RA) to.
    /// </summary>
    private void CalculateRAPoint()
    {
        //Pick random direction away from player
        Vector3 directionAway = (transform.position - player.transform.position).normalized; //gets mirror pos of player
        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)); //vec3 that will be used to modify where the enemy runs
        fleeTarget = transform.position + (directionAway + randomOffset).normalized * safeRadius;

        //enemyNavAgent.SetDestination(fleeTarget);
    }

    private IEnumerator RunAway()
    {
        runAwayRunning = true; //let state check know the enemy is busy running away, no state check possible during

        CalculateRAPoint();

        enemyNavAgent.SetDestination(fleeTarget);

        //wait while running but allow state to change
        float timer = 0f;
        while (timer < runAwayRecalcTime)
        {
            //stop early if player is no longer too close
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > safeRadius)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        // allow recalculation
        runAwayRunning = false;
    }

    protected override IEnumerator Attack()
    {
        enemyNavAgent.isStopped = true;

        if (projectilePrefab != null && firePoint != null)
        {
            // 1. Instantiate projectile
            GameObject projObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(player.transform.position - firePoint.position).normalized);

            // 2. Try to get projectile script
            EnemyProjectile proj = projObj.GetComponent<EnemyProjectile>();

            if (proj != null)
            {
                // 3. Send player's position (at moment of firing)
                proj.targetPos = player.transform.position;
                proj.damage = damage; //Pass through damage that this enemy will deal to projectile for interaaction
            }
            else
            {
                Debug.LogWarning("Projectile prefab missing EnemyProjectile script!");
            }
        }
        yield return new WaitForSeconds(attackTime);

        state = EnemyState.cooldown;
        StartCoroutine(Cooldown());
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, safeRadius);

        if (isRunAway)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(fleeTarget, Vector3.one);
        }
    }
}