using UnityEngine;

//Projectile being a MonoBehaviour script allows it to copy a spell's (a scriptable object) data-
//and make run-time calculations

/// <summary>
/// Handles run-time projectile calculations.
/// </summary>
public class Projectile : MonoBehaviour
{
    private GameObject target;
    private Vector3 targetPoint;
    private float speed;
    
    private float impactDamage;
    
    private bool isAoE;
    private float aoeRange;
    private float aoeDamage;
    private Collider[] aoeHits = new Collider[30];

    private bool isHoming;
    private bool enemyHardLock;
    private float homingStrength;

    int propMask;

    private void Awake()
    {
        //exclude props from line of sight, larger props would be ground
        propMask = LayerMask.GetMask("Props");
    }

    //Gets all relevant data from context from Player->ProjectileSpellLogic (passes ctx and itself) ->Projectile
    public void Init(SpellContext ctx, ProjectileSpellLogic logic)
    {


        speed = logic.projectileSpeed;
        impactDamage = logic.impactDamage;
        
        isAoE = logic.isAoE;
        aoeRange = logic.aoeRange;
        aoeDamage = logic.AoEDamage;

        isHoming = logic.isHoming;
        enemyHardLock = logic.enemyHardLock;
        homingStrength = logic.homingStrength;

        targetPoint = ctx.targetPoint;

        switch (ctx.spellInfo.targetingType)
        {
            case TargetingType.EnemyOnly:

                //MUST have clicked an enemy
                target = ctx.target;
                Debug.Log($"Current target: {target.name} ");
                //Hard Lcok means: NEVER change target while moving to target
                //Soft Lock means: start with this target but allow target switching in FixedUpdate
            break;

            case TargetingType.PointOrEnemy:

                if (ctx.target != null)
                {
                    // clicked an enemy
                    target = ctx.target;
                }
                else
                {
                    //clicked ground get nearest enemy for homing (if homing is true)
                    target = FindClosestEnemy();
                }
            break;
        }

    }


    private void FixedUpdate()
    {
        // SOFT LOCK: reacquire nearest enemy while flying
        if (isHoming && !enemyHardLock)
        {
            var nearest = FindClosestEnemy();

            if (nearest != null)
                target = nearest;
        }

        //determine destination; if no target (ground) set to right, else set to left
        Vector3 destination = (target != null) ? target.transform.position : targetPoint;

        //turning
        Vector3 desiredDir = (destination - transform.position).normalized;

        if (isHoming && target != null)
        {
            desiredDir = Vector3.Lerp( transform.forward, desiredDir, homingStrength * Time.deltaTime ).normalized;
        }

        transform.position += speed * Time.deltaTime * desiredDir;

        if (Vector3.Distance(transform.position, destination) < 0.2f)
        {
            OnImpact();
        }
    }

    private void OnImpact() //Impact is by distance
    {
        //we're only looking for enemies, try allows failure
        if (target != null && target.TryGetComponent(out Enemy directHit))
        {
            directHit.TakeDamage(impactDamage);
        }

        if (isAoE)
        {
            //non alloc in the event of many AoE projectiles
            //count gets all colliders and populates aoeHits
            //int count = Physics.OverlapSphereNonAlloc(transform.position, aoeRange, aoeHits);
            //exclude props to exceeding array and Line of Sight being blocked by small/skinny props
            int count = Physics.OverlapSphereNonAlloc(transform.position, aoeRange, aoeHits, ~propMask);

            //run through aoeHits for any enemies in AoE
            for (int i = 0; i < count; i++)
            {
                //var to read value from aoeHits and check index var for enemy
                Collider colliderHit = aoeHits[i];

                if (colliderHit.TryGetComponent(out Enemy aoeEnemy))
                {
                    //check LoS based on projectile Pos and where the indexed enemy is
                    if (HasLineOfSight(transform.position, aoeEnemy.transform))
                    {
                        Debug.DrawLine(transform.position, aoeEnemy.transform.position, Color.green, 50f); //Big debug line in case of any modifiers in future
                        aoeEnemy.TakeDamage(aoeDamage);
                    }
                }


                aoeHits[i] = null; //either index is an enemy and took damage or not; remove from aoeHits for potential dupelicates
            }
        }

        Destroy(gameObject);
    }

    private bool HasLineOfSight(Vector3 origin, Transform targetTransform)
    {
        Vector3 direction = (targetTransform.position - origin).normalized;
        float distance = Vector3.Distance(origin, targetTransform.position);

        //Raycast that hits anything, uses above variables to create raycast to determine LoS
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            //if the first hit matches the target's transform, return true
            return hit.transform == targetTransform;
        }

        //Nothing hit, LOS is clear
        return true;
    }

    /// <summary>
    /// Gets distance between projectile and enemies in range to home
    /// </summary>
    /// <returns>Closest Enemy GameObject</returns>
    private GameObject FindClosestEnemy()
    {
        float bestDist = Mathf.Infinity;
        GameObject bestEnemy = null;
    
        Collider[] hits = Physics.OverlapSphere(transform.position, 50f);
    
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
    
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestEnemy = hit.gameObject;
                }
            }
        }
    
        return bestEnemy;
    }

    private void OnCollisionEnter(Collision coll)
    {
        if(coll != null)
        {
            //if we hit an enemy, try to get its script and apply damage
            //if no script, error but destroy regardless
            if (coll.gameObject.CompareTag("Enemy"))
            {
                if (coll.gameObject.TryGetComponent<Enemy>(out var enemy))
                {
                    enemy.TakeDamage(impactDamage);
                }
                else
                {
                    Debug.LogError("Enemy GameObject has no Enemy script!");
                }
                Destroy(gameObject);
            }
            else if (coll.gameObject.CompareTag("Ground"))
            {
                Destroy(gameObject);
            }
        }
    }
}


