using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utilities
{
	public class LogManager : Singleton<LogManager>
	{
		[SerializeField] private bool isTest;
		[SerializeField] private bool disable;
		[SerializeField] private State state;
		[SerializeField] private int queueCount;
		[SerializeField] private Text messageTextPrefab;
		[SerializeField] private RectTransform textParent;

		private static Thread _mainThread;
		private static Text _inlineText;

		private static readonly Queue<Text> MessageQueue = new();
		private static readonly Queue<(string, Color?)> WaitingQueue = new();

		private int _ping;

		private void Start()
		{
			_mainThread = Thread.CurrentThread;
			DontDestroyOnLoad(gameObject);

			if (!disable)
				return;
			gameObject.SetActive(false);
			state = State.Console;
		}

		private void Update()
		{
			if (WaitingQueue.Count > 0)
			{
				WaitingQueue.ForEach(c => ShowMessage(c.Item1, c.Item2));
				WaitingQueue.Clear();
			}

			if (Input.GetKeyDown(KeyCode.J))
			{
				ShowMessage("name: " + EventSystem.current.currentSelectedGameObject?.name);
			}
		}

		private static bool IsMainThread() => Thread.CurrentThread == _mainThread;

		public static void ShowWarning(params object[] message)
		{
			foreach (var s in message)
				ShowMessage(s?.ToString(), new Color(.8f, .6f, .3f, 1));
		}

		public static void ShowError(params object[] message)
		{
			foreach (var s in message)
				ShowMessage(s?.ToString(), Color.red);
		}

		public static void InlineMessage(string message, string delim = " ")
		{
			if (!ValidateTextLog(message, null)) return;
			if (!_inlineText)
				_inlineText = Instantiate(Instance.messageTextPrefab, Instance.textParent);
			_inlineText.text += delim + message;
		}

		public static void ShowMessage(params object[] message)
		{
			foreach (var s in message)
				ShowMessage(s?.ToString());
		}

		public static void ShowMessage(Color color, params object[] message)
		{
			foreach (var s in message)
				ShowMessage(s?.ToString(), color);
		}

		public void Flush()
		{
			foreach (var text in MessageQueue) Destroy(text.gameObject);
			MessageQueue.Clear();
		}

		private static bool ValidateTextLog(string message, Color? color)
		{
			if (!Instance || string.IsNullOrEmpty(message)) return false;
			if (Instance.state is not State.Text)
			{
				Debug.Log(message);
				if (Instance.state is State.Console)
					return false;
			}

			if (!IsMainThread())
			{
				WaitingQueue.Enqueue(($"[{Thread.CurrentThread.Name}]: " + message, color));
				return false;
			}

			return true;
		}

		private static void ShowMessage(string message, Color? color = null)
		{
			if (!ValidateTextLog(message, color)) return;

			Text text;
			if (MessageQueue.Count == Instance.queueCount)
			{
				text = MessageQueue.Dequeue();
				text.transform.SetAsLastSibling();
			}
			else
				text = Instantiate(Instance.messageTextPrefab, Instance.textParent);

			MessageQueue.Enqueue(text);
			text.color = color ?? Color.white;
			text.text = message;
			CoroutineRunner.DelayedRun(() => LayoutRebuilder.ForceRebuildLayoutImmediate(Instance.textParent));
		}

		public void Show() => textParent.gameObject.SetActive(!textParent.gameObject.activeSelf);

		private enum State
		{
			Text,
			Console,
			[UsedImplicitly] Both
		}
	}
}
