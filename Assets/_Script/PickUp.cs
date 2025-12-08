using UnityEngine;

public class PickUp : MonoBehaviour
{
    public enum PickupType
    {
        Health,
        Mana,
        Combo
    }

    [Header("Gains from Pick-Up")]
    public PickupType type = PickupType.Health;
    public int amount = 10; // Amount of health or mana to give player

    [Header("Particle effect on pickup")]
    public ParticleSystem pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        //hceck if the colliding object is the player
        if (other.gameObject.CompareTag("Player"))
        {
            // apply the value to player
            switch (type)
            {
                case PickupType.Health:
                    Player.instance.health += amount;
                    PlayerUI.s.AddMessage("Health: +" + amount);
                    break;
                case PickupType.Mana:
                    Player.instance.mana += amount;
                    PlayerUI.s.AddMessage("Mana: +" + amount);
                    break;
                case PickupType.Combo:
                    Player.instance.health += amount;
                    Player.instance.mana += amount;
                    PlayerUI.s.AddMessage("Mana and Health: +" + amount);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
