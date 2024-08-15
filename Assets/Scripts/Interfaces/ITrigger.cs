using System;


public interface ITrigger
{
    public event Action<bool> EnableChanged;
    public float PlayingSpeed { get; }
}