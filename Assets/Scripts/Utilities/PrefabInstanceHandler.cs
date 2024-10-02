using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utilities
{
	public class PrefabInstanceHandler<T> where T : Component
	{
		private const int INITIAL_LOADING = 5, LOADING_POWER = 5;

		private readonly Queue<T> _instances = new();
		private readonly int _maxInstances;
		private readonly T _instance;
		private readonly Transform _parent;

		private string _clearKey;

		private int Count => All.Count;

		public PrefabInstanceHandler(T instance, Transform parent, int maxInstances = int.MaxValue)
		{
			_instance = instance;
			_parent = parent;
			_maxInstances = maxInstances;
		}

		public List<T> All => _instances.Where(c => c.gameObject.activeSelf).ToList();

		public T GetInstance(bool isReverse = false)
		{
			var res = Count < _instances.Count || Count >= _maxInstances
				? _instances.Dequeue()
				: Object.Instantiate(_instance, _parent);

			_instances.Enqueue(res);
			res.gameObject.SetActive(true);
			if (isReverse) res.transform.SetAsFirstSibling();
			else res.transform.SetAsLastSibling();
			return res;
		}

		public void Spawn(int count, bool isReversed = false, Action<T> doForAll = null)
		{
			for (var i = 0; i < count; i++)
			{
				var res = GetInstance(isReversed);
				doForAll?.Invoke(res);
			}
		}

		public void SpawnAtLeast(int count)
		{
			count -= Count;
			for (var i = 0; i < count; i++) GetInstance();
		}

		public void Reorder() => All.ForEach(c => c.transform.SetAsLastSibling());

		public void RemoveInstances(int count)
		{
			for (var i = 0; i < count; i++) RemoveInstance();
		}

		public void RemoveInstance(Func<T, bool> condition = null)
		{
			if (condition == null)
			{
				if (Count == 0) return;
				_instances.First(c => c.gameObject.activeSelf).gameObject.SetActive(false);
			}
			else
			{
				var toRemove = All.FirstOrDefault(condition);
				if (!toRemove) return;

				toRemove.gameObject.SetActive(false);
				_instances.Requeue(t => !condition(t));
				_instances.EnqueueBack(toRemove);
			}
		}

		public void RemoveInstance(T target) => RemoveInstance(c => c == target);

		public void ClearInstances()
		{
			foreach (var chatMessage in _instances) chatMessage.gameObject.SetActive(false);
		}

		public void SoftClearInstances(Action onDone)
		{
			DisableSoftClear();
			_clearKey = _instances.SoftLoading(INITIAL_LOADING, LOADING_POWER, c => c.gameObject.SetActive(false),
				onDone);
		}

		private void DisableSoftClear() => Utility.DisableSoftLoading(_clearKey);
	}
}
