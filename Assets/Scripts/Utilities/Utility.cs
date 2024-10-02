using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities
{
	public static class Utility
	{
		public static T AMin<T>(this IEnumerable<T> list, Func<T, T, bool> isLessThan)
		{
			T res = default;
			var isFirst = true;
			foreach (var item in list)
			{
				if (isFirst)
				{
					res = item;
					isFirst = false;
				}
				else if (isLessThan(item, res)) res = item;
			}

			return res;
		}

		#region Ienumerables

		public static void RemoveFirst<T>(this List<T> list, Func<T, bool> condition)
		{
			for (var i = 0; i < list.Count; i++)
				if (condition(list[i]))
				{
					list.RemoveAt(i);
					return;
				}
		}

		public static void EnqueueBack<T>(this Queue<T> me, T data)
		{
			var count = me.Count;
			me.Enqueue(data);
			for (var i = 0; i < count; i++) me.Enqueue(me.Dequeue());
		}

		public static void Requeue<T>(this Queue<T> me, Func<T, bool> condition)
		{
			var count = me.Count;
			for (var i = 0; i < count; i++)
			{
				var ins = me.Dequeue();
				if (condition(ins))
					me.Enqueue(ins);
			}
		}

		#endregion

		#region SoftLoading

		private static readonly Dictionary<string, bool> Loaders = new();

		public static void DisableSoftLoading(string key)
		{
			if (!string.IsNullOrEmpty(key) && Loaders.ContainsKey(key)) Loaders[key] = true;
		}

		private static string SoftLoading<T>(
			this IReadOnlyCollection<T> list, int initialLoading, int loadingPower, float delay,
			Action<T> load, Action onEnd = null, Action onInit = null, Action onKilled = null)
		{
			var key = Guid.NewGuid().ToString();
			Loaders[key] = false;
			list.Loader(initialLoading, loadingPower, load, onEnd, onKilled, key, delay);
			CoroutineRunner.DelayedRun(onInit);
			return key;
		}

		public static string SoftLoading<T>(
			this IReadOnlyCollection<T> list, int initialLoading, int loadingPower, Action<T> load,
			Action onEnd = null, Action onInit = null, Action onKilled = null) =>
			SoftLoading(list, initialLoading, loadingPower, 0, load, onEnd, onInit, onKilled);

		public static string SoftLoading<T>(
			this IReadOnlyCollection<T> list, float delay, Action<T> load,
			Action onEnd = null, Action onInit = null, Action onKilled = null) =>
			SoftLoading(list, 0, 1, delay, load, onEnd, onInit, onKilled);

		private static void Loader<T>(
			this IReadOnlyCollection<T> list, int initialLoading, int loadingPower, Action<T> load,
			Action onEnd, Action onKilled, string key, float delayTime)
		{
			if (delayTime == 0) CoroutineRunner.DelayedRun(Load);
			else CoroutineRunner.WaitRun(delayTime, Load);

			return;

			void Load()
			{
				if (Loaders[key])
				{
					Loaders.Remove(key);
					onKilled?.Invoke();
					return;
				}

				if (list.Count == 0)
				{
					onEnd?.Invoke();
					return;
				}

				list.Take(initialLoading).ForEach(load);
				list.Skip(initialLoading).ToList()
					.Loader(loadingPower, loadingPower, load, onEnd, onKilled, key, delayTime);
			}
		}

		#endregion

		public static void ForEach<T>(this IEnumerable<T> me, Action<T> action)
		{
			foreach (var data in me) action?.Invoke(data);
		}

		public static T GetRandom<T>(this List<T> data) => data[Random.Range(0, data.Count)];

		public static void ForEach<T>(this List<T> me, Action<T, int> action)
		{
			for (var i = 0; i < me.Count; i++) action?.Invoke(me[i], i);
		}

		public static IEnumerable<T> Duplicate<T>(this IEnumerable<T> data)
		{
			var res = data.ToList();
			res.AddRange(res);
			return res;
		}

		public static T TryPeek<T>(this Stack<T> me) where T : class => me.Count > 0 ? me.Peek() : null;

		public static int Remainder(this int data, int r) => (data + r) % r;

		public static T CastEnum<T>(this object me) where T : struct => Enum.Parse<T>(me.ToString());

		public static void Pop<T>(this Stack<T> me, T top) where T : IEquatable<T>
		{
			if (!me.Pop().Equals(top))
				throw new("top of Stack must equal!");
		}

		public static void RemoveAll<T>(this Stack<T> me, Predicate<T> condition)
		{
			var tempStack = new Stack<T>();
			while (me.TryPop(out var res)) tempStack.Push(res);
			while (tempStack.TryPop(out var res))
			{
				if (condition(res))
					me.Push(res);
			}
		}

		public static float ToAngle(this Vector2 me)
		{
			var angle = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), me);
			if (me.x < 0)
				angle = 360 - angle;
			return angle;
		}

		public static float ToAngle(this Vector3 me)
		{
			var angle = Vector3.Angle(new Vector3(0.0f, 1.0f, 0.0f), me);
			if (me.x < 0)
				angle = 360 - angle;
			return angle;
		}

		public static long ToTimeL(this float timeF) => (long)(timeF * 1000);

		public static float ToTimeF(this long timeL) => timeL / 1000f;

		public static bool IsInBoundaries(this float angle, int min, int max) =>
			min > max ? angle > min || angle < max : angle > min && angle < max;

		public static bool TryPop<T>(this List<T> list, out T res)
		{
			if (list.Count > 0)
			{
				res = list.First();
				list.Remove(res);
				return true;
			}

			res = default;
			return false;
		}

		public static Direction ToDirection(this float angle)
		{
			if (angle.IsInBoundaries(315, 45)) return Direction.Up;
			if (angle.IsInBoundaries(45, 135)) return Direction.Right;
			return angle.IsInBoundaries(135, 225) ? Direction.Down : Direction.Left;
		}

		public static Vector3 ToVector(this Direction direction) =>
			direction switch
			{
				Direction.Up => Vector2.up,
				Direction.Down => Vector2.down,
				Direction.Left => Vector2.left,
				Direction.Right => Vector2.right,
				_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
			};
	}

	public enum Direction
	{
		Up,
		Down,
		Right,
		Left
	}
}
