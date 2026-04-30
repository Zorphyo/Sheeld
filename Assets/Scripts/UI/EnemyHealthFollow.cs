using UnityEngine;

public class EnemyHealthFollow : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position;

        // Face camera
        transform.forward = Camera.main.transform.forward;
    }
}
