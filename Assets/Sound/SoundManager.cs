using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundCollection sounds;
    [SerializeField] private AudioSource cameraAudioSource;
    private List<AudioClip> playingNow;

    private const string SFX_VOLUME = "SoundVolume";

    public float Volume { get; private set; } = 1f;

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playingNow = new();
        DeliveryManager.Instance.DishServed += DishServed;
        DeliveryManager.Instance.DishRequested += DishRequested;
        DeliveryManager.Instance.WrongDishServed += WrongDishServed;

        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;

        Player.DroppedItem += Player_DroppedItem;
        Player.PickedUpItem += Player_PickedUpItem;

        TrashBin.Used += TrashBin_Used;

        Volume = PlayerPrefs.GetFloat(SFX_VOLUME, .5f); 
    }

    private void TrashBin_Used(TrashBin obj)
        => PlaySoundAtPoint(sounds.Trash, obj.transform.position);

    private void Player_PickedUpItem(Player source)
        => PlaySoundAtPoint(sounds.ObjectPickup, source.transform.position);

    private void Player_DroppedItem(Player source)
        => PlaySoundAtPoint(sounds.ObjectDrop, source.transform.position);

    private void CuttingCounter_OnAnyCut(CuttingCounter counter)
        => PlaySoundAtPoint(sounds.Chop, counter.transform.position);

    private void WrongDishServed(DeliveryManager manager)
        => PlaySoundAtCamera(sounds.WrongDishServed);

    private void DishRequested(DeliveryManager manager, Dish dish)
    {
        var clip = dish.IsSpecial ? sounds.DishRequested[1] : sounds.DishRequested[0];
        PlayClip(clip);
    }

    private void DishServed(DeliveryManager manager, Dish dish)
        => PlaySoundAtCamera(sounds.DishServed);

    void PlaySoundAtCamera(AudioClip[] clipArray, float volumeModifier = 1f)
    {
        var clip = clipArray[Random.Range(0, clipArray.Length)];
        PlayClip(clip, default, volumeModifier);
    }

    void PlaySoundAtPoint(AudioClip[] clipArray, Vector3 position, float volumeModifier = 1f)
    {
        var clip = clipArray[Random.Range(0, clipArray.Length)];
        PlayClip(clip, position, volumeModifier * Volume);
    }

    void PlayClip(AudioClip clip, Vector3 position = default, float volumeModifier = 1f)
    {
        if (playingNow.Contains(clip))
            cameraAudioSource.Stop();
        else
            playingNow.Add(clip);

        if (position == default)
            cameraAudioSource.PlayOneShot(clip, volumeModifier * Volume);
        else
            AudioSource.PlayClipAtPoint(clip, position, volumeModifier * Volume);

        Task.Delay((int)clip.length * 1000)
            .ContinueWith((t) => playingNow.Remove(clip))            ;
    }

    internal void SetVolume()
    {
        Volume = (Volume + 0.1f) % 1f;

        PlayerPrefs.SetFloat(SFX_VOLUME, Volume);
        PlayerPrefs.Save();
    }

    internal void PlayCountdownSound()
    {
        PlaySoundAtCamera(sounds.Warning);
    }

    internal void PlayWarningSound(Vector3 position)
    {
        PlaySoundAtPoint(sounds.Warning, position, 5f);
    }
}
