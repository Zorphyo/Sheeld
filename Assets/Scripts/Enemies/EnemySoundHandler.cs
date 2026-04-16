using UnityEngine;
using UnityEngine.Audio;

public class EnemySoundHandler : MonoBehaviour
{
    public enum EnemyVoice { Male, Female }

    [Header("Voice")]
    public EnemyVoice voice;

    [Header("Mixer")]
    public AudioMixerGroup mixerGroup;

    private AudioSource audioSource;
    private AudioClip attack, hit, death;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.outputAudioMixerGroup = mixerGroup;

        if (voice == EnemyVoice.Male)
        {
            attack = Resources.Load<AudioClip>("Sounds/male/Attack");
            hit    = Resources.Load<AudioClip>("Sounds/male/Hit");
            death  = Resources.Load<AudioClip>("Sounds/male/Death");
        }
        else
        {
            attack = Resources.Load<AudioClip>("Sounds/female/Attack");
            hit    = Resources.Load<AudioClip>("Sounds/female/Hit");
            death  = Resources.Load<AudioClip>("Sounds/female/Death");
        }
    }

    public void PlayAttackSound() => audioSource.PlayOneShot(attack);
    public void PlayHitSound()    => audioSource.PlayOneShot(hit);
    public void PlayDeathSound()  => audioSource.PlayOneShot(death);
}