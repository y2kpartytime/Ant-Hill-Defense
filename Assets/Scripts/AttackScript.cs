using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public int attackDamage = 1;
    public float attackRange = 0.8f;
    public float attackCooldown = 1f;

    private GameObject targetEnemy;
    private float attackTimer;

    void Update()
    {
        attackTimer -= Time.deltaTime;

        if (targetEnemy == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            targetEnemy.transform.position
        );

        if (distance <= attackRange)
        {
            Attack();
        }
    }

    public void SetTarget(GameObject enemy)
    {
        targetEnemy = enemy;
    }

    void Attack()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        Health health = targetEnemy.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(attackDamage);
            Debug.Log(gameObject.name + " attacked " + targetEnemy.name);
        }
    }
}