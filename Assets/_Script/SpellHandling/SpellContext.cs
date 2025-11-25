using UnityEngine;

//SpellContext exists as a separate script to allow for potential other users
public class SpellContext
{
    //GameObject that is casting spell,
    //used for origin and any assignments (XP, bonus mana from kill, etc.) STRETCH GOAL
    public GameObject spellCaster;

    //null if ground 
    public GameObject target;

    //Where the player clicked
    public Vector3 targetPoint; 
    // For spell distance clamp
    public float distanceToPoint; 

    //All info regarding the actual spell (Mana drain, health gained, damage, etc.)
    public SpellInfo spellInfo;
}
