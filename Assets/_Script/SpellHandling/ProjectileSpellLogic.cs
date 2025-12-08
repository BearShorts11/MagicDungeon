using UnityEngine;

[CreateAssetMenu(menuName = "Spell/Logic/Projectile")]
public class ProjectileSpellLogic : SpellLogic
{
    [SerializeField]
    private SpellInfo spellInfo;

    [Header("Spell Options")]
    public float projectileSpeed;
    public float impactDamage = 10f;
    
    public bool isAoE = false;
    public float aoeRange = 2f;
    public float AoEDamage = 15f;

    public bool isHoming = false;
    public bool enemyHardLock = false; //Used in projectile script
    public float homingStrength = 0.2f;
    [Tooltip("How wide, in a cone, will this projectile home onto a target if homing.")]
    public float homingAngle;

    public float lifeTime = 2f;
    public GameObject projectilePrefab; // Can contain a particle system, line renderer etc.

    public override bool CanCast(SpellContext ctx) //Doesn't currently allow for enemy spell casters
    {
        //Debug.Log("Checking Can Cast");
        if (ctx == null) return false; //Did the player make SpellContext for the spell
        if (Player.instance.mana < ctx.spellInfo.manaCost) //check player's mana if they can cast the Player's currentSpell 
        {
            PlayerUI.s.AddMessage("Not Enough Mana!");
            return false;
        }
        if (!IsRangeValid(ctx)) return false;

        return true;
    }

    public override void Cast(SpellContext ctx)
    {
        if (ctx == null) return;
        ctx.spellInfo = spellInfo;

        Player.instance.mana -= ctx.spellInfo.manaCost;

        //Spawn projectile facing the target point from caster
        Vector3 spawnPos = ctx.spellCaster.transform.position;
        //teavels either to center of enemy or ground
        Vector3 destination = (ctx.target != null) ? ctx.target.transform.position : ctx.targetPoint;
        //used to point projectile toward destination
        Quaternion rot = Quaternion.LookRotation( (destination - spawnPos).normalized );

        var go = Instantiate(projectilePrefab, spawnPos, rot);

        var projectile = go.GetComponent<Projectile>();
        projectile.Init(ctx, this);

        projectile.Invoke(nameof(projectile.Die), lifeTime);
    }

    /// <summary>
    /// Checks projectile's distance to target
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    protected virtual bool IsRangeValid(SpellContext ctx)
    {

        switch (ctx.spellInfo.targetingType)
        {
            case TargetingType.PointOrEnemy: //doesn't matter if it's an enemy or just ground; moves forward through code
                break;

            case TargetingType.EnemyOnly:
                if (ctx.target == null)
                {
                    PlayerUI.s.AddMessage("No Enemy Clicked or Out of Range!");
                    return false;
                }
                else
                {
                    ctx.target.GetComponent<Enemy>();
                }
                break;
        }

        return ctx.distanceToPoint <= ctx.spellInfo.maxRange; //is the target's pos less than or equal to the spell's max range
    }
}
