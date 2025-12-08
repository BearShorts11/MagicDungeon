using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Spell/Logic/Self")]
public class SelfSpellLogic : SpellLogic
{
    [SerializeField]
    private SpellInfo spellInfo;

    [Header("Cost to cast (SET MANA COST IN SPELL INFO TO 0).")]
    public int healthCost = 0;
    public int manaCost = 0;

    [Header("Gain from spell")]
    public int healthGain = 0;
    public int manaGain = 0;

    [Header("Temporary Buffs")]
    public float speedBoost = 0f;
    public float damageBoost = 0f;

    public float duration = 0f; // 0 is instant

    public float healthRegenModifier = 0f;
    public float manaRegenModifier = 0f;

    [SerializeField]
    private ParticleSystem particleEffect;
    public override bool CanCast(SpellContext ctx)
    {
        if (ctx == null) return false;
        if(Player.instance.health < healthCost) //check player's health if they can cast the Player's currentSpell 
        {
            PlayerUI.s.AddMessage("Not Enough Health!");
            return false;
        }
        if(Player.instance.mana < manaCost) //check player's mana if they can cast the Player's currentSpell 
        {
            PlayerUI.s.AddMessage("Not Enough Mana!");
            return false;
        }

        return true;
    }

    public override void Cast(SpellContext ctx)
    {
        if(ctx == null) return;

        ctx.spellInfo = spellInfo;

        // apply costs
        Player.instance.mana -= manaCost;
        Player.instance.health -= healthCost;

        // Apply gains
        Player.instance.mana += manaGain;
        Player.instance.health += healthGain;  

        if(particleEffect != null)
        {
            particleEffect.gameObject.SetActive(true);
            particleEffect.Play();
        }

        if (particleEffect != null)
        {
            // Instantiate the particle system at the player's position
            ParticleSystem effectInstance = Instantiate(
                particleEffect,
                Player.instance.transform.position,
                Quaternion.identity
            );

            // Play the particle effect
            effectInstance.Play();

            // Destroy the effect after its duration to avoid clutter
            Destroy(effectInstance.gameObject,
                effectInstance.main.duration + effectInstance.main.startLifetime.constantMax);
        }
    }
}
