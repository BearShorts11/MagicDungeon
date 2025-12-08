using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public Vector3 targetPos;
    public float speed = 10f; //default value
    public float damage = 10f; //default value 

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        if (transform.position == targetPos) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Ranged attack dealing damage!");
            other.GetComponent<Player>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
