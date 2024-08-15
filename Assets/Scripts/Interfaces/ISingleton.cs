public interface ISingleton
{
    void Initialize();
}

public interface ISingleton<T> : ISingleton
{
    public static T Singleton { get; }
}
