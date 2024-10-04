using UnityEngine;
using VInspector;

namespace Sound
{
	[CreateAssetMenu(fileName = "Sounds", menuName = "ScriptableObjects/Sounds")]
	public class Sounds : ScriptableObject
	{
		public SerializedDictionary<SfxMode, AudioClip> effects;
		public SerializedDictionary<MusicMode, AudioClip> musics;
	}

	public enum SfxMode
	{
		Heal,
		Buy,
		Click,
		Rune,
		Dice,
		Warning,
		Information,
		Error,
		Door,
		PopupOpen,
		PopupClose,
		Walk
	}

	public enum MusicMode
	{
		Main
	}
}
