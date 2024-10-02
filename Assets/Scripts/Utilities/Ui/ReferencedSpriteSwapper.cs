using System.Linq;
using UnityEngine;

namespace Utilities.Ui
{
	public class ReferencedSpriteSwapper : SpriteSwapper
	{
		[SerializeField] private SpriteReference reference;

		protected override void CodeChanged(string value)
		{
			if (value == null)
			{
				if (isRenderer)
					myRenderer.enabled = false;
				else
					myImage.enabled = false;
				return;
			}

			Sprite swapper;
			if (!(swapper = reference.GetSprite(value)) &&
			    !(swapper = sprites.FirstOrDefault(c => c.code == value)?.sprite))
			{
				LogManager.ShowMessage(Color.red, $"ref swapper {name} code {value} not found!");
				return;
			}

			if (isRenderer)
			{
				myRenderer.sprite = swapper;
				myRenderer.enabled = myRenderer.sprite;
			}
			else
			{
				myImage.sprite = swapper;
				myImage.enabled = myImage.sprite;
			}
		}
	}
}
