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
    private float homingAngle;

    int propMask;

    private void Awake()
    {
        //exclude props from line of sight, larger props would be ground
        propMask = LayerMask.GetMask("Props");
    }

    //Gets all relevant data from context from Player->ProjectileSpellLogic (passes ctx and itself) ->Projectile
    public void Init(SpellContext ctx, ProjectileSpellLogic logic)
    {
        //it occurs to me I could have just made two class variables and plug these in

        speed = logic.projectileSpeed;
        impactDamage = logic.impactDamage;

        isAoE = logic.isAoE;
        aoeRange = logic.aoeRange;
        aoeDamage = logic.AoEDamage;

        isHoming = logic.isHoming;
        enemyHardLock = logic.enemyHardLock;
        homingStrength = logic.homingStrength;
        homingAngle = logic.homingAngle;

        targetPoint = ctx.targetPoint;

        switch (ctx.spellInfo.targetingType)
        {
            case TargetingType.EnemyOnly:

                //MUST have clicked an enemy
                target = ctx.target;
                Debug.Log($"Current target: {target.name} "); //just making sure the enemy I clicked is actually assigned
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

        GameObject currentTarget = null;

        // Soft Homing (homes but not hard lock)
        if (isHoming && !enemyHardLock)
        {
            currentTarget = FindClosestEnemy();
        }

        // Hard Lock (homes and hard locks; checks target for enemy GO)
        else if (isHoming && enemyHardLock && target != null)
        {
            currentTarget = target;
        }

        // Not Homing (not homing / hard locking)
        else
        {
            currentTarget = null;  // use targetPoint instead
        }

        //where to go (if currentTarget isn't null, go to that pos, else go to target point aka ground)
        Vector3 destination = (currentTarget != null) ? currentTarget.transform.position : targetPoint;

        Vector3 desiredDir = (destination - transform.position).normalized;

        //only home turn if actively homing on something
        if (isHoming && currentTarget != null)
        {
            desiredDir = Vector3.Lerp(transform.forward, desiredDir, homingStrength * Time.deltaTime).normalized;
        }

        //Move forward
        transform.position += speed * Time.deltaTime * desiredDir;

        if (Vector3.Distance(transform.position, destination) < 0.2f)
        {
            OnImpact();
        }

    }

    private void OnImpact() //Impact is by distance
    {
        // we're only looking for enemies, try allows failure
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

    private bool HasLineOfSight(Vector3 origin, Transform targetTransform) //TO DO: do enemy layer
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

        float halfAngle = homingAngle * 0.5f;

        Collider[] hits = Physics.OverlapSphere(transform.position, 50f);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Enemy enemy))
            {
                Vector3 dirToEnemy = (hit.transform.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, hit.transform.position);

                float angle = Vector3.Angle(transform.forward, dirToEnemy);

                if (angle <= halfAngle)
                {
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestEnemy = hit.gameObject;
                    }
                }
            }
        }

        return bestEnemy;
    }

    private void OnCollisionEnter(Collision coll)
    {
        if (coll != null)
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

    public void Die()
    {
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float halfAngle = homingAngle * 0.5f;
        Vector3 forward = transform.forward; //local forward

        // left side of cone
        Quaternion leftRot = Quaternion.AngleAxis(-halfAngle, Vector3.up);
        Gizmos.DrawRay(transform.position, leftRot * forward * 5f);

        //right side of cone
        Quaternion rightRot = Quaternion.AngleAxis(halfAngle, Vector3.up);
        Gizmos.DrawRay(transform.position, rightRot * forward * 5f);

        // middle ray for better distinction
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, forward * 5f);
    }
}


