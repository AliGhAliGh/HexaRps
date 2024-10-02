namespace Utilities
{
    public abstract class RefresherSingleton<T> : RealSingleton<T> where T: RefresherSingleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            if (Instance == this)
                Refresh();
        }

        protected abstract void Refresh();
    }
}
