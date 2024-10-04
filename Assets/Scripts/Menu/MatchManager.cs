using Network;
using UnityEngine;
using Utilities;

namespace Menu
{
	public class MatchManager : Singleton<MatchManager>
	{
		[SerializeField] private MatchItem matchPrefab;
		[SerializeField] private Transform matchesParent;
		[SerializeField] private GameObject all;

		private PrefabInstanceHandler<MatchItem> _instanceHandler;

		private static PrefabInstanceHandler<MatchItem> InstanceHandler =>
			Instance ? Instance._instanceHandler ??= new(Instance.matchPrefab, Instance.matchesParent) : null;

		public static void Show()
		{
			Instance.all.SetActive(true);
			Refresh();
		}

		public static async void Refresh()
		{
			InstanceHandler.ClearInstances();
			foreach (var matchResult in await NetworkManager.GetMatches())
				InstanceHandler?.GetInstance().Init(matchResult);
		}
	}
}
