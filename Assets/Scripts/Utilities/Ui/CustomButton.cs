using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// using Sound;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

namespace Utilities.Ui
{
	[RequireComponent(typeof(Button))]
	public class CustomButton : MonoBehaviour
	{
		[SerializeField] private Button myButton;
		[SerializeField] private bool hasSoundEffect = true;
		[SerializeField] private bool hasGraphics = true;

		[ShowIf(nameof(hasGraphics)), SerializeField]
		private Color disabledColor = new(.79f, .79f, .79f, .5f), normalColor = new(1,1,1,1);

		[ShowIf(nameof(hasGraphics)), SerializeField]
		private List<Graphic> allGraphics;

		public void Select() => myButton.Select();

		public Action OnClick
		{
			set
			{
				myButton.onClick.RemoveAllListeners();
				myButton.onClick.AddListener(() => value?.Invoke());
				// if (hasSoundEffect)
				// 	myButton.onClick.AddListener(() => SoundManager.PlaySfx(SfxMode.Click, SoundPriority.Low));
			}
		}

		public bool Interactable
		{
			get => myButton.interactable;
			set
			{
				myButton.interactable = value;
				var targetColor = value ? normalColor : disabledColor;
				if (!hasGraphics)
				{
					myButton.image.color = myButton.colors.normalColor;
					return;
				}

				allGraphics.ForEach(g => g.color = new Color(g.color.r, g.color.g, g.color.b, targetColor.a));
				myButton.image.color = targetColor;
			}
		}

		public Func<Task> OnClickAsync
		{
			set
			{
				myButton.onClick.RemoveAllListeners();
				myButton.onClick.AddListener(() => value?.Invoke());
			}
		}

		public void Deactivate() => Interactable = false;
	}
}
