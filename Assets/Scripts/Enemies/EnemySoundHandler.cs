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
    public float basicFootstepInterval    = 0.4f;
    public float heavyFootstepInterval    = 0.7f;
    public float speedsterFootstepInterval = 0.25f;

    private AudioSource audioSource;
    private AudioClip attack, hit, death;
    private AudioClip stomp;
    private AudioClip arrowLoad;
    private AudioClip arrowShoot;
    private AudioClip medicHeal;
    private AudioClip footstep;
    private AudioClip swing;
    private AudioClip damage;

    private float footstepTimer;
    private float footstepInterval;
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
        //swing = Resources.Load<AudioClip>("Sounds/basicSwing");
        damage = Resources.Load<AudioClip>("Sounds/basicDamage");
        footstep = Resources.Load<AudioClip>($"Sounds/basicFootstep");
        
        switch (type)
        {
            case EnemyType.Basic:
                attack   = Resources.Load<AudioClip>($"Sounds/{gender}/Attack");
                swing    = Resources.Load<AudioClip>("Sounds/basicSwing");
                footstepInterval = basicFootstepInterval;
                break;

            case EnemyType.Heavy:
                attack   = Resources.Load<AudioClip>("Sounds/heavy/Attack");
                hit      = Resources.Load<AudioClip>("Sounds/heavy/Hit");
                death    = Resources.Load<AudioClip>("Sounds/heavy/Death");
                stomp    = Resources.Load<AudioClip>("Sounds/heavy/Stomp");
                swing = Resources.Load<AudioClip>("Sounds/heavy/swing");
                footstep = Resources.Load<AudioClip>("Sounds/heavy/Footstep");
                footstepInterval = heavyFootstepInterval;
                break;

            case EnemyType.Archer:
                arrowLoad = Resources.Load<AudioClip>("Sounds/archer/arrowLoad");
                arrowShoot= Resources.Load<AudioClip>("Sounds/archer/arrowShoot");
                break;

            case EnemyType.Medic:
                medicHeal = Resources.Load<AudioClip>("Sounds/medic/Heal");
                break;

            case EnemyType.Speedster:
                attack   = Resources.Load<AudioClip>($"Sounds/{gender}/Attack");
                swing    = Resources.Load<AudioClip>("Sounds/basicSwing");
                footstep = Resources.Load<AudioClip>("Sounds/speedster/Footstep");
                footstepInterval = speedsterFootstepInterval;
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
    public void PlayAttackSound() {
        audioSource.PlayOneShot(attack);
        audioSource.PlayOneShot(swing);
    }
    public void PlayHitSound() {
        audioSource.PlayOneShot(hit);
        audioSource.PlayOneShot(damage);
    } 
    public void PlayDeathSound()     => audioSource.PlayOneShot(death);
  
    // Heavy
    public void PlayStompSound()     => audioSource.PlayOneShot(stomp);

    // Archer
    public void PlayArrowLoadSound() => audioSource.PlayOneShot(arrowLoad);
    public void PlayArrowShootSound() => audioSource.PlayOneShot(arrowShoot);

    // Medic
    public void PlayHealSound()      => audioSource.PlayOneShot(medicHeal);
}