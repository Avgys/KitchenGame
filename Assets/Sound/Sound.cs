using UnityEngine;
using UnityEngine.Audio;

public class Sound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject soundSource;
    private ITrigger soundTrigger;
    [SerializeField] private float secondsDelay;
    [SerializeField] private AudioClip[] clips;
    private float secondsDelayCurrent;
    private bool isPlaying;
    private float startVolume;

    private void Start()
    {
        soundTrigger = soundSource.GetComponent<ITrigger>();
        soundTrigger.EnableChanged += Source_StateChanged;

        startVolume = audioSource.volume;
    }

    private void Update()
    {
        secondsDelayCurrent -= Time.deltaTime;
        if (secondsDelayCurrent < 0 && isPlaying && !audioSource.isPlaying)
        {
            var clip = clips[Random.Range(0, clips.Length)];
            audioSource.clip = clip;
            audioSource.volume = startVolume * SoundManager.Instance.Volume;
            audioSource.Play();
            secondsDelayCurrent = secondsDelay / soundTrigger.PlayingSpeed;
        }

        if (!isPlaying)
            audioSource.Stop();
    }

    private void Source_StateChanged(bool obj) => isPlaying = obj;
}