using Network;
using TextHandlers;
using TMPro;
using UnityEngine;
using Utilities.Ui;

namespace Menu
{
	public class MatchItem : MonoBehaviour
	{
		[SerializeField] private RtlText matchName;
		[SerializeField] private SpriteSwapper bg;
		[SerializeField] private CustomButton button;
		[SerializeField] private TextMeshProUGUI playersCount;

		public void Init(MatchResult match)
		{
			matchName.text = match.Name;
			bg.Code = match.IsJoined && match.IsOwner ? "Owner" : match.IsJoined ? "Joined" : "Normal";
			playersCount.text = match.Size.ToString();
			button.OnClick = Click;
			button.Interactable = !(match.IsOwner || match.IsJoined);
			return;

			async void Click() => await NetworkManager.JoinMatch(match.ID);
		}
	}
}
