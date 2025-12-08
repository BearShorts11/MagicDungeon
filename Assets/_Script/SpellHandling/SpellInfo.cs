using UnityEngine;

public enum TargetingType
{
    PointOrEnemy, //Able to be cast on either ground or enemy
    EnemyOnly, //IGNORE GROUND; Hard target enemy (for spells that I only want interacting with an enemy
    GroundOnly, //IGNORE ENEMY; Ground based spells (spell meant for ground; can be used for summons, AoE, placables)
    Self //IMMEDIATE TO SELF; Spells that apply to the user; on-click -> apply
}
[CreateAssetMenu(menuName = "Spell/SpellInfo")]
public class SpellInfo : ScriptableObject
{
    public string spellName;

    public TargetingType targetingType;

    //allow or don't allow cast of spell if target is outside of range
    //kind of pointless since I do a check in logic scripts but keeping for now, might re-use
    public float maxRange = 10f;
    public bool clampToMaxRange = true;

    [Header("Resource Cost")]

    public int manaCost;
    public float cooldown;

    public SpellLogic logic;
}
