using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	public class RaceAudioManager : IInit
	{
		private Dictionary<string, AudioClip> _soundObjPath = new Dictionary<string, AudioClip>
		{
			{ "sound_yinqing1", null },
			{ "sound_yinqing2", null },
			{ "sound_yinqing3", null },
			{ "sound_gamestar_man", null },
			{ "sound_finish", null },
			{ "sound_finish_2", null },
			{ "sound_HSpeed", null },
			{ "sound_LSpeed", null },
			{ "sound_daoshu", null },
			{ "sound_daoshu_2", null },
			{ "sound_gameover", null },
			{ "sound_miaozhun", null },
			{ "sound_lost", null },
			{ "sound_piaoyi", null },
			{ "sound_hurt", null },
			{ "sound_eat", null },
			{ "sound_throw", null },
			{ "sound_laugth_man", null },
			{ "sound_laugth_lady", null },
			{ "sound_angry_man", null },
			{ "sound_angry_lady", null },
			{ "sound_tire_man", null },
			{ "sound_tire_lady", null },
			{ "sound_cheers", null },
			{ "sfx_passby_oncoming_car_01", null },
			{ "sfx_passby_oncoming_car_02", null },
			{ "sfx_passby_oncoming_car_03", null },
			{ "sound_piaoyi1", null },
			{ "sound_piaoyi2", null },
			{ "sound_piaoyi3", null },
			{ "sound_excitement_man1", null },
			{ "sound_excitement_man02", null },
			{ "sound_excitement_lady01", null },
			{ "sound_excitement_lady02", null },
			{ "sound_good_man", null },
			{ "sound_nice_man", null },
			{ "sound_perfect", null },
			{ "Final_Lap", null },
			{ "Final_Lap_1", null },
			{ "music_finish", null },
			{ "music_countdown", null },
			{ "sound_saichefeixiang", null },
			{ "sound_pengzhuang", null }
		};

		private Dictionary<string, AudioClip> _itemSoundObjPath = new Dictionary<string, AudioClip>
		{
			{ "sound_daodanfashe", null },
			{ "sound_daodanfly", null },
			{ "sound_daodanjizhong", null },
			{ "sound_mogui", null },
			{ "sound_shuicangying", null },
			{ "sound_feidie", null },
			{ "sound_duanlu", null },
			{ "sound_xiangjiaopi", null },
			{ "sound_tianshi", null },
			{ "sound_tianshifangyu", null },
			{ "sound_moshui", null },
			{ "sound_shuipao", null },
			{ "sound_shuipaojizhong", null },
			{ "sound_waterclick", null },
			{ "sound_hurt_man", null },
			{ "sound_hurt_lady", null },
			{ "sound_storm", null },
			{ "sound_hurt_storm", null },
			{ "sound_block", null },
			{ "sound_hurt_block", null },
			{ "airplane", null },
			{ "jiguang_gun", null }
		};

		private List<string> _soundKeys;

		private List<string> _itemSoundKeys;

		private static RaceAudioManager instance;

		private bool itemMode;

		public static Action OnAudioLoaded;

		public static Action<Action<Dictionary<string, AudioClip>, object>, object> AsyncLoadRaceAllAudioDelegate;

		private static bool loaded;

		public static RaceAudioManager ActiveInstance => instance;

		public static bool Loaded => loaded;

		public AudioClip[] Sound_finalLaps => new AudioClip[2] { Sound_finalLap_1, Sound_finalLap_2 };

		public AudioClip Sound_finalLap_1 => _soundObjPath["Final_Lap"];

		public AudioClip Sound_finalLap_2 => _soundObjPath["Final_Lap_1"];

		public AudioClip Sound_good_man => _soundObjPath["sound_good_man"];

		public AudioClip Sound_nice_man => _soundObjPath["sound_nice_man"];

		public AudioClip Sound_perfect_man => _soundObjPath["sound_perfect"];

		public AudioClip Sound_excitement_man1 => _soundObjPath["sound_excitement_man1"];

		public AudioClip Sound_excitement_man2 => _soundObjPath["sound_excitement_man02"];

		public AudioClip Sound_excitement_man3 => null;

		public AudioClip Sound_excitement_man4 => null;

		public AudioClip Sound_excitement_lady01 => _soundObjPath["sound_excitement_lady01"];

		public AudioClip Sound_excitement_lady02 => _soundObjPath["sound_excitement_lady02"];

		public AudioClip Sound_excitement_lady03 => null;

		public AudioClip Sound_excitement_lady04 => null;

		public AudioClip Sound_passby1 => _soundObjPath["sfx_passby_oncoming_car_01"];

		public AudioClip Sound_passby2 => _soundObjPath["sfx_passby_oncoming_car_02"];

		public AudioClip Sound_passby3 => _soundObjPath["sfx_passby_oncoming_car_03"];

		public AudioClip Sound_cheers => _soundObjPath["sound_cheers"];

		public AudioClip Music_countdown => _soundObjPath["music_countdown"];

		public AudioClip Sound_translate => _soundObjPath["sound_saichefeixiang"];

		public AudioClip Sound_crash => _soundObjPath["sound_pengzhuang"];

		public AudioClip Sound_yinqing3 => _soundObjPath["sound_yinqing3"];

		public AudioClip Sound_yinqing2 => _soundObjPath["sound_yinqing2"];

		public AudioClip Sound_yinqing1 => _soundObjPath["sound_yinqing1"];

		public AudioClip Music_finish => _soundObjPath["music_finish"];

		public AudioClip Sound_laugth_man => _soundObjPath["sound_laugth_man"];

		public AudioClip Sound_laugth_lady => _soundObjPath["sound_laugth_lady"];

		public AudioClip Sound_angry_man => _soundObjPath["sound_angry_man"];

		public AudioClip Sound_hurt_lady => _itemSoundObjPath["sound_hurt_lady"];

		public AudioClip Sound_hurt_man => _itemSoundObjPath["sound_hurt_man"];

		public AudioClip Sound_tired_lady => _soundObjPath["sound_tire_lady"];

		public AudioClip Sound_tired_man => _soundObjPath["sound_tire_man"];

		public AudioClip Sound_angry_lady => _soundObjPath["sound_angry_lady"];

		public AudioClip Sound_yinqing => null;

		public AudioClip Sound_start => _soundObjPath["sound_gamestar_man"];

		public AudioClip Sound_finish => _soundObjPath["sound_finish"];

		public AudioClip Sound_finish2 => _soundObjPath["sound_finish_2"];

		public AudioClip Sound_daoshu2 => _soundObjPath["sound_daoshu_2"];

		public AudioClip Sound_HSpeed => _soundObjPath["sound_HSpeed"];

		public AudioClip Sound_LSpeed => _soundObjPath["sound_LSpeed"];

		public AudioClip Sound_daoshu => _soundObjPath["sound_daoshu"];

		public AudioClip Sound_gameover => _soundObjPath["sound_gameover"];

		public AudioClip Sound_miaozhun => _soundObjPath["sound_miaozhun"];

		public AudioClip Sound_lost => _soundObjPath["sound_lost"];

		public AudioClip Sound_piaoyi => _soundObjPath["sound_piaoyi"];

		public AudioClip Sound_piaoyi1 => _soundObjPath["sound_piaoyi1"];

		public AudioClip Sound_piaoyi2 => _soundObjPath["sound_piaoyi2"];

		public AudioClip Sound_piaoyi3 => _soundObjPath["sound_piaoyi3"];

		public AudioClip Sound_hurt => _soundObjPath["sound_hurt"];

		public AudioClip Sound_eat => _soundObjPath["sound_eat"];

		public AudioClip Sound_throw => _soundObjPath["sound_throw"];

		public AudioClip Sound_daodanfashe => _itemSoundObjPath["sound_daodanfashe"];

		public AudioClip Sound_daodanfly => _itemSoundObjPath["sound_daodanfly"];

		public AudioClip Sound_daodanjizhong => _itemSoundObjPath["sound_daodanjizhong"];

		public AudioClip Sound_mogui => _itemSoundObjPath["sound_mogui"];

		public AudioClip Sound_shuicangying => _itemSoundObjPath["sound_shuicangying"];

		public AudioClip Sound_feidie => _itemSoundObjPath["sound_feidie"];

		public AudioClip Sound_duanlu => _itemSoundObjPath["sound_duanlu"];

		public AudioClip Sound_xiangjiaopi => _itemSoundObjPath["sound_xiangjiaopi"];

		public AudioClip Sound_tianshi => _itemSoundObjPath["sound_tianshi"];

		public AudioClip Sound_tianshifangyu => _itemSoundObjPath["sound_tianshifangyu"];

		public AudioClip Sound_moshui => _itemSoundObjPath["sound_moshui"];

		public AudioClip Sound_shuipao => _itemSoundObjPath["sound_shuipao"];

		public AudioClip Sound_shuipaojizhong => _itemSoundObjPath["sound_shuipaojizhong"];

		public AudioClip Sound_shuipaodianji => _itemSoundObjPath["sound_waterclick"];

		public AudioClip Sound_storm => _itemSoundObjPath["sound_storm"];

		public AudioClip Sound_hurt_storm => _itemSoundObjPath["sound_hurt_storm"];

		public AudioClip Sound_block => _itemSoundObjPath["sound_block"];

		public AudioClip Sound_hurt_block => _itemSoundObjPath["sound_hurt_block"];

		public AudioClip Sound_Airplane => _itemSoundObjPath["airplane"];

		public AudioClip Sound_Electric => _itemSoundObjPath["jiguang_gun"];

		public RaceAudioManager(bool isItemMode = false)
		{
			instance = this;
			itemMode = isItemMode;
			_soundKeys = new List<string>(_soundObjPath.Keys);
			_itemSoundKeys = new List<string>(_itemSoundObjPath.Keys);
		}

		public void Init()
		{
			loaded = false;
			loadSounds();
		}

		private void loadSounds()
		{
			if (AsyncLoadRaceAllAudioDelegate == null)
			{
				Debug.LogError("AsyncLoadRaceAllAudioDelegate is null");
				return;
			}
			Debug.LogWarning("<<<<<<<< Start load sounds>>>>>>>>");
			AsyncLoadRaceAllAudioDelegate(delegate(Dictionary<string, AudioClip> allClips, object userData)
			{
				if (allClips != null && allClips.Count > 0)
				{
					if (_soundKeys != null && _soundKeys.Count > 0)
					{
						for (int i = 0; i < _soundKeys.Count; i++)
						{
							if (allClips.ContainsKey(_soundKeys[i]))
							{
								_soundObjPath[_soundKeys[i]] = allClips[_soundKeys[i]];
							}
							else
							{
								Debug.LogError("do not contain sound name=" + _soundKeys[i]);
							}
						}
					}
					if (_itemSoundKeys != null && _itemSoundKeys.Count > 0)
					{
						for (int j = 0; j < _itemSoundKeys.Count; j++)
						{
							if (allClips.ContainsKey(_itemSoundKeys[j]))
							{
								_itemSoundObjPath[_itemSoundKeys[j]] = allClips[_itemSoundKeys[j]];
							}
						}
					}
				}
				else
				{
					Debug.LogError("allClips is null or count is 0");
				}
				Debug.LogWarning("<<<<<<<< Sound loaded >>>>>>>>");
				if (OnAudioLoaded != null)
				{
					OnAudioLoaded();
				}
			}, null);
		}

		public void UnInit()
		{
			if (instance == this)
			{
				instance = null;
			}
			for (int i = 0; i < _soundKeys.Count; i++)
			{
				if (_soundObjPath.ContainsKey(_soundKeys[i]) && null != _soundObjPath[_soundKeys[i]])
				{
					Resources.UnloadAsset(_soundObjPath[_soundKeys[i]]);
					_soundObjPath[_soundKeys[i]] = null;
				}
			}
			for (int j = 0; j < _itemSoundKeys.Count; j++)
			{
				if (_itemSoundObjPath.ContainsKey(_itemSoundKeys[j]) && null != _itemSoundObjPath[_itemSoundKeys[j]])
				{
					Resources.UnloadAsset(_itemSoundObjPath[_itemSoundKeys[j]]);
					_itemSoundObjPath[_itemSoundKeys[j]] = null;
				}
			}
		}
	}
}
