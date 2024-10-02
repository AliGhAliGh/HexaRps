using System;
using System.Collections.Generic;
using System.Linq;
// using Sound;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utilities
{
	public class PopupHandler : MonoBehaviour
	{
		[SerializeField] private Animator animator;
		[SerializeField] private List<UIBehaviour> disableList;
		[SerializeField] private List<PopupHandler> children;
		[SerializeField] private bool isFirstOpen;

		private static readonly int OpenHash = Animator.StringToHash("Open");
		private static readonly int CloseHash = Animator.StringToHash("Close");

		private bool _isOpen, _isStarted;

		public bool IsOpen
		{
			get
			{
				if (!_isStarted)
				{
					IsOpen = isFirstOpen;
					_isStarted = true;
				}

				return _isOpen;
			}
			private set => _isOpen = value;
		}

		private void OnDisable() => _isStarted = false;

		public virtual void Open() => OpenBool();

		public bool Open(IBack panel)
		{
			var success = OpenBool();
			return success && Push(panel);
		}

		public bool Close(IBack panel, Action onClose = null)
		{
			var success = CloseBool(onClose);
			if (success) Pop(panel);
			return success;
		}

		public virtual void Close() => CloseBool();

		public void Close(Action onClose) => CloseBool(onClose);

		private bool OpenBool()
		{
			if (IsOpen) return false;
			CoroutineRunner.WaitRun(0.1f, () => disableList.ForEach(c => c.enabled = false));
			CoroutineRunner.WaitRun(0.6f, () => disableList.ForEach(c => c.enabled = true));
			animator.SetTrigger(OpenHash);
			// SoundManager.PlaySfx(SfxMode.PopupOpen, SoundPriority.Medium);
			return IsOpen = true;
		}

		private bool CloseBool(Action onClose = null)
		{
			if (!IsOpen) return false;
			CoroutineRunner.WaitRun(children.Count(handler => handler.CloseBool()) * .6f, () =>
			{
				animator.SetTrigger(CloseHash);
				// SoundManager.PlaySfx(SfxMode.PopupClose, SoundPriority.Medium);
				CoroutineRunner.WaitRun(0.6f, onClose);
			}, true);
			IsOpen = false;
			return true;
		}

		private static void Pop(IBack back)
		{
			while (true)
			{
				var peek = UiManager.AllPages.TryPeek();
				UiManager.AllPages.Pop();
				if (peek == back) break;
			}
		}

		private static bool Push(IBack back)
		{
			if (UiManager.AllPages.TryPeek() == back) return false;
			UiManager.AllPages.Push(back);
			return true;
		}
	}

	public interface IBack
	{
		void Back();
	}
}
