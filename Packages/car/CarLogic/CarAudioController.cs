using System;
using UnityEngine;

namespace CarLogic
{
	internal class CarAudioController : ControllerBase
	{
		private CarState carState;

		private AudioSource sourceEngine;

		private AudioSource sourceGas;

		private AudioSource sourceLGas;

		private AudioSource sourceDrift;

		private float speedRatioBuf = 0.05f;

		private float[] noiseLevels = new float[4] { 0f, 0.1f, 0.4f, 0.8f };

		private int curEngineLevel;

		public void Init(CarState state)
		{
			carState = state;
		}

		private void setUpAudioClips()
		{
		}

		public override void OnActiveChange(bool active)
		{
			base.OnActiveChange(active);
			if (carState == null || !(carState.view != null))
			{
				return;
			}
			AbstractView view = carState.view;
			view.OnUpdate = (Action)Delegate.Remove(view.OnUpdate, new Action(onUpdate));
			CarCallBack callBacks = carState.CallBacks;
			callBacks.OnGas = (Action<CarEventState>)Delegate.Remove(callBacks.OnGas, new Action<CarEventState>(onGas));
			CarCallBack callBacks2 = carState.CallBacks;
			callBacks2.OnAirGroundGas = (Action<CarEventState>)Delegate.Remove(callBacks2.OnAirGroundGas, new Action<CarEventState>(OnAirGroundGas));
			CarCallBack callBacks3 = carState.CallBacks;
			callBacks3.OnDrift = (Action<CarEventState>)Delegate.Remove(callBacks3.OnDrift, new Action<CarEventState>(onDrift));
			if (!active)
			{
				return;
			}
			view.OnUpdate = (Action)Delegate.Combine(view.OnUpdate, new Action(onUpdate));
			CarCallBack callBacks4 = carState.CallBacks;
			callBacks4.OnGas = (Action<CarEventState>)Delegate.Combine(callBacks4.OnGas, new Action<CarEventState>(onGas));
			CarCallBack callBacks5 = carState.CallBacks;
			callBacks5.OnAirGroundGas = (Action<CarEventState>)Delegate.Combine(callBacks5.OnAirGroundGas, new Action<CarEventState>(OnAirGroundGas));
			CarCallBack callBacks6 = carState.CallBacks;
			callBacks6.OnDrift = (Action<CarEventState>)Delegate.Combine(callBacks6.OnDrift, new Action<CarEventState>(onDrift));
			if (carState != null && carState.transform != null)
			{
				sourceEngine = carState.transform.gameObject.AddComponent<AudioSource>();
				Debug.Log("Modify by Sbin :   The Car's EngineSound only use one clip and setup the AudioSource's pitch  ");
				RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
				if (activeInstance != null)
				{
					sourceEngine.clip = activeInstance.Sound_yinqing1;
					sourceEngine.loop = true;
					sourceEngine.pitch = 0.5f;
					sourceEngine.spatialBlend = 1f;
					sourceEngine.Play();
				}
				sourceDrift = carState.transform.gameObject.AddComponent<AudioSource>();
				sourceDrift.spatialBlend = 1f;
				sourceDrift.minDistance = 4f;
				sourceGas = carState.view.ExSource;
				sourceGas.spatialBlend = 1f;
				sourceLGas = carState.view.ExEffectSource;
				sourceLGas.spatialBlend = 1f;
			}
		}

		private void onGas(CarEventState e)
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
			if (activeInstance != null && e == CarEventState.EVENT_BEGIN)
			{
				if (carState.N2State.Level == 1)
				{
					sourceGas.PlayOneShot(activeInstance.Sound_HSpeed);
				}
				else if (carState.N2State.Level > 1)
				{
					sourceLGas.PlayOneShot(activeInstance.Sound_LSpeed);
				}
			}
		}

		private void OnAirGroundGas(CarEventState e)
		{
			if (RaceAudioManager.ActiveInstance != null && e == CarEventState.EVENT_BEGIN)
			{
				sourceLGas.PlayOneShot(RaceAudioManager.ActiveInstance.Sound_LSpeed);
			}
		}

		public void SetDriftVolume(bool on)
		{
			if (!(sourceDrift == null))
			{
				if (on)
				{
					sourceDrift.volume = 1f;
				}
				else
				{
					sourceDrift.volume = 0f;
				}
			}
		}

		private void onDrift(CarEventState e)
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
			if (activeInstance != null)
			{
				switch (e)
				{
					case CarEventState.EVENT_BEGIN:
						sourceDrift.clip = getDriftAudio();
						sourceDrift.loop = true;
						sourceDrift.volume = 1f;
						sourceDrift.Play();
						break;
					default:
						sourceDrift.Stop();
						break;
					case CarEventState.EVENT_DOING:
						break;
				}
			}
		}

		private void onUpdate()
		{
			if (base.Active && sourceEngine != null)
			{
				sourceEngine.volume = 1f;
				updateEngineNoise();
			}
		}

		private void updateEngineNoise()
		{
			Debug.Log("Modify by Sbin :   The Car's EngineSound only use one clip and setup the AudioSource's pitch by SpeedRatio(Self) or LinearVelocity(AI) ");
			if (carState.view.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				sourceEngine.pitch = Mathf.Clamp(Mathf.Abs(carState.SpeedRatio) * 3f, 0.5f, 3f);
			}
			else
			{
				sourceEngine.pitch = Mathf.Clamp(Mathf.Abs(carState.LinearVelocity / 20f) * 3f, 0.5f, 3f);
			}
		}

		private AudioClip getDriftAudio()
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
			if (activeInstance != null)
			{
				AudioClip[] array = new AudioClip[3] { activeInstance.Sound_piaoyi1, activeInstance.Sound_piaoyi2, activeInstance.Sound_piaoyi3 };
				return array[UnityEngine.Random.Range(0, array.Length - 1)];
			}
			return null;
		}

		private void resetNoiseAudio()
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
			if (activeInstance != null)
			{
				AudioClip[] array = new AudioClip[4] { null, activeInstance.Sound_yinqing1, activeInstance.Sound_yinqing2, activeInstance.Sound_yinqing3 };
				sourceEngine.Stop();
				if (array[curEngineLevel] != null)
				{
					sourceEngine.clip = array[curEngineLevel];
					sourceEngine.loop = true;
					sourceEngine.Play();
				}
			}
		}

		private void playCharacterSound(AudioClip clip)
		{
			if (clip != null && carState.view.ExSource != null)
			{
				carState.view.ExSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
				carState.view.ExSource.PlayOneShot(clip);
			}
		}

		internal void PlayLaugth(bool isLady = false)
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
		}

		internal void PlayAngry(bool isLady = false)
		{
			if (RaceAudioManager.ActiveInstance != null)
			{
				playCharacterSound(isLady ? RaceAudioManager.ActiveInstance.Sound_angry_lady : RaceAudioManager.ActiveInstance.Sound_angry_man);
			}
		}

		internal void PlayHurt(bool isLady = false)
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
		}

		internal void PlaySpeedUp(bool isLady = false)
		{
			RaceAudioManager activeInstance = RaceAudioManager.ActiveInstance;
		}

		internal void PlayPraise(int lv, bool isLady = false)
		{
			if (RaceAudioManager.ActiveInstance != null)
			{
				AudioClip audioClip = null;
				AudioClip[] array = new AudioClip[3]
				{
					RaceAudioManager.ActiveInstance.Sound_good_man,
					RaceAudioManager.ActiveInstance.Sound_nice_man,
					RaceAudioManager.ActiveInstance.Sound_perfect_man
				};
				audioClip = array[lv % array.Length];
				playCharacterSound(audioClip);
			}
		}

		internal void PlayOnFly(bool isLady = false)
		{
			if (RaceAudioManager.ActiveInstance != null)
			{
				AudioClip[] array = null;
				int num = UnityEngine.Random.Range(0, 10);
				if (isLady)
				{
					array = new AudioClip[1] { RaceAudioManager.ActiveInstance.Sound_excitement_lady01 };
				}
				else
				{
					array = new AudioClip[1] { RaceAudioManager.ActiveInstance.Sound_excitement_man2 };
				}
			}
		}

		internal void PlayTired(bool isLady = false)
		{
			if (RaceAudioManager.ActiveInstance != null)
			{
				playCharacterSound(isLady ? RaceAudioManager.ActiveInstance.Sound_tired_lady : RaceAudioManager.ActiveInstance.Sound_tired_man);
			}
		}

		internal void StopEngineAudio()
		{
			if (sourceEngine != null)
			{
				sourceEngine.Stop();
			}
			if (sourceDrift != null)
			{
				sourceDrift.Stop();
				sourceDrift.clip = null;
				sourceDrift.enabled = false;
			}
		}

		internal void StopDriftAudio()
		{
			if (sourceDrift != null)
			{
				sourceDrift.Stop();
			}
		}

		internal void EnableEngineAudio()
		{
			if (sourceDrift != null)
			{
				sourceDrift.enabled = true;
			}
		}

		internal bool IsEnableDriftAudio()
		{
			if (!base.Active)
			{
				return false;
			}
			if (sourceDrift == null)
			{
				return false;
			}
			if (!sourceDrift.enabled)
			{
				return false;
			}
			if (sourceDrift.volume <= 0.001f)
			{
				return false;
			}
			return true;
		}
	}
}
