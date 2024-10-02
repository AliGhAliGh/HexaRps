namespace Utilities
{
    public abstract class RealSingleton<T> : Singleton<T> where T: RealSingleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                DontDestroyOnLoad(gameObject);
        }
    }
}
