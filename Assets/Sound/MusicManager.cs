using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }
    [SerializeField] private AudioSource musicSource;
    private float startVolume;
    public float Volume { get; private set; } = 1f;


    private const string MUSIC_VOLUME = "MusicVolume";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        startVolume = musicSource.volume;
        Volume = PlayerPrefs.GetFloat(MUSIC_VOLUME, .5f);
        musicSource.volume = startVolume * Volume;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        Instance = null;
    }

    public void ChangeVolume()
    {
        Volume = (Volume + 0.1f) % 1.01f;
        musicSource.volume = startVolume * Volume;

        PlayerPrefs.SetFloat(MUSIC_VOLUME, Volume);
        PlayerPrefs.Save();
    }
}
