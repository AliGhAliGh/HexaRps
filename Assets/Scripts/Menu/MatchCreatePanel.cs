using Network;
using UnityEngine;
using Utilities;
using Utilities.Ui;

namespace Menu
{
	public class MatchCreatePanel : Singleton<MatchCreatePanel>, IBack
	{
		[SerializeField] private PopupHandler popup;
		[SerializeField] private CustomButton button;
		[SerializeField] private CustomField field;

		private void Start() => field.OnSubmit = button.OnClick = Click;

		private async void Click()
		{
			button.Interactable = false;
			await NetworkManager.CreateServerMatch(field.Text);
			Instance.Back();
			CoroutineRunner.WaitRun(1, MatchManager.Refresh);
		}

		public static void Show()
		{
			if (!Instance.popup.Open(Instance)) return;
			CoroutineRunner.WaitRun(.6f, Instance.field.Select);
			Instance.button.Interactable = true;
		}

		public void Back() => popup.Close(this);
	}
}
