using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [SerializeField]
    private Slider enemyHPSlider;

    float maxHP;
    Enemy enemy;
    void Start()
    {
        enemy = GetComponentInParent<Enemy>();
        //These are the same because we want our max hp to be decided by the initial health value
        maxHP = enemy.health;
        enemyHPSlider.maxValue = maxHP;
    }

    void Update()
    {
        UIUpdate();
        FaceCamera();
    }

    private void UIUpdate()
    {
        enemyHPSlider.value = enemy.health;
    }

    private void FaceCamera()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
