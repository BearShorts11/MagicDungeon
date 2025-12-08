using UnityEngine;

[CreateAssetMenu(menuName = "Spell/Logic/Ground")]
public class GroundSpellLogic : SpellLogic
{
    [SerializeField]
    private SpellInfo spellInfo;

    [Header("Spell Options")]
    public bool spawnAoE = true; //if false, will summon a prefab instead
    public GameObject aoePrefab;

    public float aoeDuration = 5f;
    public float aoeDamagePerSecond = 10f;
    public float aoeRadius = 2f;

    public GameObject summonPrefab;

    public override bool CanCast(SpellContext ctx)
    {
        if (ctx == null) return false;
        if (Player.instance.mana < ctx.spellInfo.manaCost) return false;
        if (!IsRangeValid(ctx)) return false;

        return true;
    }

    public override void Cast(SpellContext ctx)
    {
        if (ctx == null) return;

        ctx.spellInfo = spellInfo;
        Player.instance.mana -= ctx.spellInfo.manaCost;

        Vector3 spawnPosition = ctx.targetPoint;

        if (spawnAoE && aoePrefab != null)
        {
            GameObject go = Instantiate(aoePrefab, spawnPosition, Quaternion.identity);
            var aoe = go.GetComponent<GroundAoE>();
            if (aoe != null)
            {
                aoe.Init(aoeDuration, aoeDamagePerSecond, aoeRadius);
            }
            else
            {
                Debug.LogWarning("GroundAoE component missing on AoE prefab!");
            }
        }
        else if (!spawnAoE && summonPrefab != null)
        {
            Instantiate(summonPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No prefab assigned for GroundSpell!");
        }
    }

    private bool IsRangeValid(SpellContext ctx)
    {
        return ctx.distanceToPoint <= ctx.spellInfo.maxRange;
    }
}
