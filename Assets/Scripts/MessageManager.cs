using System;
using Network;
using Sound;
using TextHandlers;
using UnityEngine;
using Utilities;
using Utilities.Ui;
using static InGame.UI.Buttons;
using static InGame.UI.Headers;
using static InGame.UI.MessageType;
using static InGame.UI.Messaging;

namespace InGame.UI
{
	public class MessageManager : Singleton<MessageManager>
	{
		[SerializeField] private CustomButton yesButton, noButton, bg;
		[SerializeField] private LuiText messageText, titleText, yesButtonText, noButtonText;
		[SerializeField] private SpriteSwapper iconSwapper;
		[SerializeField] private PopupHandler popup;

		private static bool _isWaiting;

		public static void ShowWarning(Action onSuccess) =>
			ShowMessage(ARE_YOU_SURE, Warning, NOTICE,
				(onSuccess, null), (null, null), true);

		public static void Successful() => ShowMessage(SUCCESSFUL, Information);

		public static void Fatal() => ShowMessage(SOME_ERROR_HAPPENED, Error, ERROR);

		protected override void Awake()
		{
			base.Awake();
			if (Instance != this) return;
			bg.OnClick = () => Close();
		}

		public static void ShowBuyMessage(string code, int goldAmount, Action onBuy) =>
			ShowMessage(code, Warning, NOTE, (onBuy, BUY),
				(null, CANCEL), true, goldAmount.ToString());

		public static void ShowMessage(
			string messageText, MessageType messageType, string headerText = NOTE, params string[] parameters)
		{
			if (NetworkManager.IsLoading)
				return;
			Instance.bg.Interactable = true;
			Instance.yesButton.gameObject.SetActive(false);
			Instance.noButton.gameObject.SetActive(false);
			Instance.messageText.SetText(messageText, parameters);
			Instance.titleText.SetText(headerText);
			Instance.iconSwapper.gameObject.SetActive(messageType != None);
			Instance.iconSwapper.Code = messageType.ToString();
			Instance.titleText.gameObject.SetActive(headerText != null);
			if (!Instance.popup.IsOpen)
			{
				Instance.popup.Open();
				SoundManager.PlaySfx(messageType.CastEnum<SfxMode>(), SoundPriority.Medium);
			}
		}

		public static void ShowMessage(
			string messageText, MessageType messageType, string headerText,
			(Action onClick, string text) yesButton, params string[] parameters)
		{
			ShowMessage(messageText, messageType, headerText, parameters);
			Instance.bg.Interactable = false;
			Instance.yesButton.gameObject.SetActive(true);
			var rectTransform = (RectTransform)Instance.yesButton.transform;
			rectTransform.anchorMin = new Vector2(0.5f, 0);
			rectTransform.anchorMax = new Vector2(0.5f, 0);
			rectTransform.anchoredPosition = new Vector2(0, 40);
			Instance.yesButton.OnClick = () =>
			{
				yesButton.onClick?.Invoke();
				Close();
			};
			Instance.yesButtonText.SetText(yesButton.text ?? YES);
		}

		public static void ShowMessage(
			string messageText, MessageType messageType, string headerText,
			(Action onClick, string text) yesButton,
			(Action onClick, string text) noButton, bool hasBack = false,
			params string[] parameters)
		{
			ShowMessage(messageText, messageType, headerText, yesButton, parameters);
			Instance.bg.Interactable = hasBack;
			var rectTransform = (RectTransform)Instance.yesButton.transform;
			rectTransform.anchoredPosition = new Vector2(110, 40);
			Instance.noButton.gameObject.SetActive(true);
			Instance.noButton.OnClick = () =>
			{
				noButton.onClick?.Invoke();
				Close();
			};
			Instance.noButtonText.SetText(noButton.text ?? NO);
		}

		public static void ShowWait(bool isShow)
		{
			if (isShow == _isWaiting) return;
			_isWaiting = isShow;
			if (isShow)
			{
				ShowMessage(PLEASE_WAIT, Information);
				Instance.bg.Interactable = false;
			}
			else
				CoroutineRunner.DelayedRun(() => Close());
		}

		public static void ComingSoon() => ShowMessage(COMING_SOON, Warning);

		private static void Close(Action onClose = null)
		{
			if (NetworkManager.IsLoading) return;
			Instance.popup.Close(onClose);
		}
	}

	public static class Headers
	{
		public const string NOTICE = "Notice!";
		public const string NOTE = "Note!";
		public const string ERROR = "Error!";
	}

	public static class Buttons
	{
		public const string NO = "No";
		public const string CANCEL = "Cancel";
		public const string BUY = "Buy";
		public const string YES = "Yes";
	}

	public static class Messaging
	{
		public const string ARE_YOU_SURE = "AreYouSure";
		public const string COMING_SOON = "ComingSoon";
		public const string PLEASE_WAIT = "PleaseWait";
		public const string SOME_ERROR_HAPPENED = "SomeErrorHappened";
		public const string SUCCESSFUL = "Successful";
	}

	public enum MessageType
	{
		Information,
		Warning,
		Error,
		None
	}
}
