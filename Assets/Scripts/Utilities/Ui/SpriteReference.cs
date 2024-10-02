using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities.Ui
{
	[CreateAssetMenu(fileName = "Roles", menuName = "ScriptableObjects/SpriteRefrence")]
	public class SpriteReference : ScriptableObject
	{
		[SerializeField] private List<ReferenceSprite> sprites;

		public Sprite GetSprite(string code)
		{
			var res = sprites.FirstOrDefault(c => c.code == code);
			return res?.sprite;
		}
	}

	[Serializable]
	public class ReferenceSprite
	{
		public Sprite sprite;
		public string code;
	}
}
