using System;

public interface IProgress
{
    event Action<float> ProgressChanged;
}