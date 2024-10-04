using System.Collections.Generic;
using System.Linq;
using Network;
using UnityEngine;
using Utilities;

namespace Sound
{
	public class SoundManager : Singleton<SoundManager>
	{
		[SerializeField] private Sounds sounds;
		[SerializeField] private AudioSource musicSource;
		[SerializeField] private List<AudioSource> sfxSources;

		public static void PlaySfx(SfxMode sfx, SoundPriority priority)
		{
			if (NetworkManager.IsLoading) return;
			var source = Instance.sfxSources.Duplicate().Skip((int)priority)
				.SkipWhile(c => c.isPlaying).FirstOrDefault();
			source ??= Instance.sfxSources[(int)priority];
			source.PlayOneShot(Instance.sounds.effects[sfx]);
		}

		public static void PlayMusic(MusicMode music)
		{
			Instance.musicSource.Stop();
			Instance.musicSource.clip = Instance.sounds.musics[music];
			Instance.musicSource.Play();
		}
	}

	public enum SoundPriority
	{
		High,
		Medium,
		Low
	}
}
