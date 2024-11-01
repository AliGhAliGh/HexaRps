using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;

namespace Utilities
{
	public class CoroutineRunner : RealSingleton<CoroutineRunner>
	{
		private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsMap = new();

		public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

		private static readonly List<Coroutine> Temporary = new();

		public static Coroutine Run(IEnumerator routine, bool isPermanent = false)
		{
			if (NetworkManager.IsLoading)
			{
				while (routine.MoveNext())
				{
					if (routine.Current is IEnumerator enumerator)
						Run(enumerator);
				}

				return null;
			}

			var res = Instance.StartCoroutine(routine);
			if (!isPermanent) Temporary.Add(res);
			return res;
		}

		public static IEnumerator Wait(float time, Action callback)
		{
			yield return GetWaitForSeconds(time);
			callback?.Invoke();
		}

		public static void StopAll() => Temporary.ForEach(c => Instance.StopCoroutine(c));

		public static void Stop(Coroutine coroutine) => Instance.StopCoroutine(coroutine);

		public static Coroutine DelayedRun(Action work) => Run(Wait(work));

		public static Coroutine WaitRun(float time, Action work, bool isPermanent = false) =>
			Run(Wait(time, work), isPermanent);

		private static IEnumerator Wait(Action callback)
		{
			yield return WaitForEndOfFrame;
			callback?.Invoke();
		}

		public static WaitForSeconds GetWaitForSeconds(float wait)
		{
			if (WaitForSecondsMap.TryGetValue(wait, out var seconds))
				return seconds;

			var res = new WaitForSeconds(wait);
			WaitForSecondsMap.Add(wait, res);
			return res;
		}
	}
}
