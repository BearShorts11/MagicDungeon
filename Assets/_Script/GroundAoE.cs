using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

//ONLY DAMAGES; possibility for inclusion of status effects but would require refactoring
public class GroundAoE : MonoBehaviour
{
    private float duration;
    private float damagePerSecond;
    private float radius = 2f;

    [Header("Visual Elements")]
    public DecalProjector decal;
    public ParticleSystem areaParticles;

    /// <summary>
    /// Creates a ground-based AoE
    /// </summary>
    /// <param name="_duration">Duration of the spell.</param>
    /// <param name="_dps">Damage Per Second of the spell.</param>
    /// <param name="_radius">How large the AoE is.</param>
    public void Init(float _duration, float _dps, float _radius)
    {
        duration = _duration;
        damagePerSecond = _dps;
        radius = _radius;

        StartCoroutine(AoELoop());
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Update Decal Size
        if (decal != null)
        {
            decal.size = new Vector3(radius * 2f, radius * 2f, decal.size.z);
        }

        // Update Particle Shape
        if (areaParticles != null)
        {
            var shape = areaParticles.shape;
            shape.scale = new Vector3(radius * 2f, shape.scale.y, radius * 2f);
        }
    }

    private IEnumerator AoELoop()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            DealDamageTick();

            // wait 1 secnod between ticks
            yield return new WaitForSeconds(1f);

            elapsed += 1f;
        }

        Destroy(gameObject);
    }

    private void DealDamageTick()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerSecond);
                }
            }
        }
    }
}
