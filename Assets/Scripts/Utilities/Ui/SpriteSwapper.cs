using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

namespace Utilities.Ui
{
	public class SpriteSwapper : CodeSwitcher
	{
		[SerializeField] protected List<SwapSprite> sprites;
		[SerializeField] protected bool isRenderer;

		[HideIf(nameof(isRenderer)), SerializeField]
		protected Image myImage;

		[ShowIf(nameof(isRenderer)), SerializeField]
		protected SpriteRenderer myRenderer;

		protected override void CodeChanged(string value)
		{
			var swapper = sprites.FirstOrDefault(c => c.code == value);
			if (value != null && swapper == null)
			{
				LogManager.ShowMessage(Color.red, $"swapper {name} code {value} not found!");
				return;
			}

			if (isRenderer)
			{
				myRenderer.sprite = swapper?.sprite;
				myRenderer.enabled = value != null && myRenderer.sprite;
				myRenderer.transform.localScale =
					Vector3.one * (swapper == null || Math.Abs(swapper.scale) < .1f ? 1 : swapper.scale);
			}
			else
			{
				myImage.sprite = swapper?.sprite;
				myImage.enabled = value != null && myImage.sprite;
				myImage.transform.localScale =
					Vector3.one * (swapper == null || Math.Abs(swapper.scale) < .1f ? 1 : swapper.scale);
			}
		}

		public Sprite Sprite
		{
			set
			{
				Code = null;
				if (isRenderer)
				{
					myRenderer.enabled = value;
					myRenderer.sprite = value;
				}
				else
				{
					myImage.enabled = value;
					myImage.sprite = value;
				}
			}
		}
	}

	[Serializable]
	public class SwapSprite
	{
		public Sprite sprite;
		public string code;
		public float scale;
	}
}
