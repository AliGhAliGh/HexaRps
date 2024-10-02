using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Utilities
{
	public class MainThread : Singleton<MainThread>
	{
		private static readonly Queue<Action> ExecutionQueue = new();

		public void Update()
		{
			lock (ExecutionQueue)
				while (ExecutionQueue.Count > 0)
					ExecutionQueue.Dequeue().Invoke();
		}

		private static void Enqueue(IEnumerator action)
		{
			lock (ExecutionQueue)
				ExecutionQueue.Enqueue(() => Instance.StartCoroutine(action));
		}

		public static void Enqueue(Action action) => Enqueue(ActionWrapper(action));

		public static Task EnqueueAsync(Action action)
		{
			var tcs = new TaskCompletionSource<bool>();

			Enqueue(ActionWrapper(WrappedAction));
			return tcs.Task;

			void WrappedAction()
			{
				try
				{
					action();
					tcs.TrySetResult(true);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}
		}


		private static IEnumerator ActionWrapper(Action a)
		{
			a();
			yield return null;
		}


		protected override void Awake()
		{
			base.Awake();
			if (Instance == this)
				DontDestroyOnLoad(gameObject);
		}
	}
}
