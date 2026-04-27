using UnityEngine;

public class RagdollImpactDamage : MonoBehaviour
{
    public enum BodyPartType
    {
        Head,
        Torso,
        Arm,
        Leg,
        Other
    }

    public BodyPartType bodyPartType = BodyPartType.Other;

    public EnemyHealth healthManager;

    public float minimumDamageVelocity = 8f;
    public float damageMultiplier = 2f;
    public float damageCooldown = 0.2f;

    public bool onlyDamageWhenRagdolled = true;
    public EnemyRagdollController ragdollController;

    private float lastDamageTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - lastDamageTime < damageCooldown)
            return;

        if (onlyDamageWhenRagdolled && ragdollController != null && !ragdollController.isRagdolled)
            return;

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minimumDamageVelocity)
            return;

        float damage = (impactSpeed - minimumDamageVelocity) * damageMultiplier;

        if (bodyPartType == BodyPartType.Head)
            damage *= 2.5f;

        if (bodyPartType == BodyPartType.Arm || bodyPartType == BodyPartType.Leg)
            damage *= 0.6f;

        healthManager.TakeDamage(Mathf.RoundToInt(damage));

        lastDamageTime = Time.time;
    }
}