using UnityEngine;
using UnityEngine.Audio;

public class EnemySoundHandler : MonoBehaviour
{
    public enum EnemyVoice { Male, Female }
    public enum EnemyType { Basic, Heavy, Archer, Medic, Speedster }

    [Header("Identity")]
    public EnemyVoice voice;
    public EnemyType type;

    [Header("Mixer")]
    public AudioMixerGroup mixerGroup;

    [Header("Footsteps")]
    public float footstepInterval = 0.4f;

    private AudioSource audioSource;
    private AudioClip attack, hit, death;
    private AudioClip stomp;
    private AudioClip arrowLoad;
    private AudioClip medicHeal;
    private AudioClip footstep;
    private float footstepTimer;
    private UnityEngine.AI.NavMeshAgent agent;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.outputAudioMixerGroup = mixerGroup;

        string gender = voice == EnemyVoice.Male ? "male" : "female";

        hit   = Resources.Load<AudioClip>($"Sounds/{gender}/Hit");
        death = Resources.Load<AudioClip>($"Sounds/{gender}/Death");

        switch (type)
        {
            case EnemyType.Basic:
                attack   = Resources.Load<AudioClip>($"Sounds/{gender}/Attack");
                footstep = Resources.Load<AudioClip>($"Sounds/{gender}/Footstep");
                break;

            case EnemyType.Heavy:
                attack   = Resources.Load<AudioClip>("Sounds/heavy/Attack");
                hit      = Resources.Load<AudioClip>("Sounds/heavy/Hit");
                death    = Resources.Load<AudioClip>("Sounds/heavy/Death");
                stomp    = Resources.Load<AudioClip>("Sounds/heavy/Stomp");
                footstep = Resources.Load<AudioClip>("Sounds/heavy/Footstep");
                break;

            case EnemyType.Archer:
                arrowLoad = Resources.Load<AudioClip>("Sounds/archer/ArrowLoad");
                footstep  = Resources.Load<AudioClip>($"Sounds/{gender}/Footstep");
                break;

            case EnemyType.Medic:
                medicHeal = Resources.Load<AudioClip>("Sounds/medic/Heal");
                footstep  = Resources.Load<AudioClip>($"Sounds/{gender}/Footstep");
                break;

            case EnemyType.Speedster:
                attack   = Resources.Load<AudioClip>($"Sounds/{gender}/Attack");
                footstep = Resources.Load<AudioClip>("Sounds/speedster/Footstep");
                break;
        }
    }

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null || footstep == null) return;

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
            footstepTimer = 0f;
        }
    }

    // Shared
    public void PlayAttackSound()    => audioSource.PlayOneShot(attack);
    public void PlayHitSound()       => audioSource.PlayOneShot(hit);
    public void PlayDeathSound()     => audioSource.PlayOneShot(death);

    // Heavy
    public void PlayStompSound()     => audioSource.PlayOneShot(stomp);

    // Archer
    public void PlayArrowLoadSound() => audioSource.PlayOneShot(arrowLoad);

    // Medic
    public void PlayHealSound()      => audioSource.PlayOneShot(medicHeal);
}