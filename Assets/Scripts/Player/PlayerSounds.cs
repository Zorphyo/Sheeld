using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public AudioClip shieldUp;
    public AudioClip shieldDown;
    public AudioClip shieldBash;
    public AudioClip shieldHit;
    public AudioClip hit;
    public AudioClip thrown;
    public AudioClip roll;
    public AudioClip footstep;

    Animator animator;
    private float lastFootStep;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        var footstepValue = animator.GetFloat("Footstep");

        if (lastFootStep > 0 && footstepValue < 0 || lastFootStep < 0 && footstepValue > 0)
        {
            AudioSource.PlayClipAtPoint(footstep, transform.position);
        }

        lastFootStep = footstepValue;
    }
}
