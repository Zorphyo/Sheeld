using UnityEngine;
using UnityEngine.Audio;

public class EnemySoundHandler : MonoBehaviour
{
    public enum EnemyVoice { Male, Female }
    public enum EnemyType { Basic, Heavy, Archer, Medic }

    [Header("Identity")]
    public EnemyVoice voice;
    public EnemyType type;

    [Header("Mixer")]
    public AudioMixerGroup mixerGroup;

    private AudioSource audioSource;
    private AudioClip attack, hit, death;
    private AudioClip stomp;
    private AudioClip arrowLoad;
    private AudioClip medicHeal;

    [Header("Footsteps")]
public float footstepInterval = 0.4f; // time between steps, decrease for faster enemies

private float footstepTimer;
private AudioClip footstep;
private UnityEngine.AI.NavMeshAgent agent;

void Start()
{
    agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    string gender = voice == EnemyVoice.Male ? "male" : "female";
    footstep = Resources.Load<AudioClip>($"Sounds/{gender}/Footstep");
    Debug.Log($"[SoundHandler] Footstep loaded: {footstep != null}");
}

void Update()
{
    if (agent == null || footstep == null) return;

    // Only tick when actually moving
    if (agent.velocity.magnitude > 0.5f)
    {
        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            audioSource.PlayOneShot(footstep);
            footstepTimer = footstepInterval;
        }
    }
    else
    {
        footstepTimer = 0f; // reset so first step plays immediately on move
    }
}

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.outputAudioMixerGroup = mixerGroup;

        string gender = voice == EnemyVoice.Male ? "male" : "female";

        // Every enemy type gets hit and death
        hit = Resources.Load<AudioClip>($"Sounds/{gender}/Hit");
        death = Resources.Load<AudioClip>($"Sounds/{gender}/Death");

        // Type-specific sounds
        switch (type)
        {
            case EnemyType.Basic:
                attack = Resources.Load<AudioClip>($"Sounds/{gender}/Attack");
                break;

            case EnemyType.Heavy:
                attack = Resources.Load<AudioClip>($"Sounds/heavy/Attack");
                hit = Resources.Load<AudioClip>($"Sounds/heavy/Hit");
                death = Resources.Load<AudioClip>($"Sounds/heavy/Death");

                stomp = Resources.Load<AudioClip>("Sounds/heavy/Stomp");
                break;

            case EnemyType.Archer:
                arrowLoad = Resources.Load<AudioClip>("Sounds/archer/ArrowLoad");
                break;

            case EnemyType.Medic:
                medicHeal = Resources.Load<AudioClip>("Sounds/medic/Heal");
                break;
        }
    }

    // Shared
    public void PlayAttackSound() => audioSource.PlayOneShot(attack);
    public void PlayHitSound() => audioSource.PlayOneShot(hit);
    public void PlayDeathSound() => audioSource.PlayOneShot(death);

    // Heavy
    public void PlayStompSound() => audioSource.PlayOneShot(stomp);

    // Archer
    public void PlayArrowLoadSound() => audioSource.PlayOneShot(arrowLoad);

    // Medic
    public void PlayHealSound() => audioSource.PlayOneShot(medicHeal);
}