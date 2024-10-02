using UnityEngine;

namespace Utilities
{
	public abstract class Singleton<T> : MonoBehaviour where T : Component
	{
		protected static T Instance;

		protected virtual void Awake()
		{
			if (Instance == null)
				Instance = this as T;
			else if (Instance != this) DestroyImmediate(gameObject);
		}

		private void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}
	}
}
