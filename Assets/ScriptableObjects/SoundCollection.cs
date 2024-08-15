using UnityEngine;

[CreateAssetMenu(fileName = "SoundCollection", menuName = "Scriptable Objects/SoundCollection")]
public class SoundCollection : ScriptableObject
{
    public AudioClip[] DishServed;
    public AudioClip[] DishRequested;
    public AudioClip[] WrongDishServed;
    public AudioClip[] Chop;
    public AudioClip[] Foostep;
    public AudioClip[] ObjectDrop;
    public AudioClip[] ObjectPickup;
    public AudioClip StoveSizzle;
    public AudioClip[] Trash;
    public AudioClip[] Warning;

}
