using System;
using UnityEngine;

namespace CarLogic
{
	[Serializable]
	public class CarEffectController : ControllerBase
	{
		public class AirShipEffectGroup
		{
			public Transform ExDriftDown;

			public Transform ExDriftBack;

			public Transform ExNormalDown;

			public Transform ExNormalBack;

			internal EffectToggleBase normalDownToggle;

			internal EffectToggleBase normalBackToggle;

			internal EffectToggleBase driftDownToggle;

			internal EffectToggleBase driftBackToggle;
		}

		private AbstractView view;

		private CarState state;

		private Rigidbody rigidbody;

		private Transform transform;

		public Transform ExSmallGasAirflow;

		public Transform ExHighSpeed;

		public Transform ExAcceleration;

		public Transform ExGas;

		public Transform ExGasAirflow;

		public Transform ExYGasAirflow;

		public Transform ExPGasAirflow;

		public Transform ExCupidGas;

		public Transform ExExhaust;

		public AirShipEffectGroup AirShipEffects;

		public EffectToggleBase RushEffect;

		public EffectToggleBase simpleGas;

		public EffectToggleBase SimpleN2Effect;

		public EffectToggleBase TranslateEffect;

		public EffectToggleBase RearLampEffect;

		private EffectToggleBase smallGasAirflowToggle;

		private EffectToggleBase hightSpeedToggle;

		private EffectToggleBase accelerateToggle;

		private EffectToggleBase gasToggle;

		private EffectToggleBase gasAirflowToggle;

		private EffectToggleBase yGasAirflowToggle;

		private EffectToggleBase pGasAirflowToggle;

		private EffectToggleBase cupidGasToggle;

		private EffectToggleBase exhaustToggle;

		private float targetBlur;

		private float blurTmp;

		public EffectToggleBase GasToggle
		{
			get
			{
				return gasToggle;
			}
			set
			{
				gasToggle = value;
			}
		}

		public EffectToggleBase GasAirflowToggle
		{
			get
			{
				return gasAirflowToggle;
			}
			set
			{
				gasAirflowToggle = value;
			}
		}

		public EffectToggleBase YGasAirflowToggle
		{
			get
			{
				return yGasAirflowToggle;
			}
			set
			{
				yGasAirflowToggle = value;
			}
		}

		public EffectToggleBase PGasAirflowToggle
		{
			get
			{
				return pGasAirflowToggle;
			}
			set
			{
				pGasAirflowToggle = value;
			}
		}

		public EffectToggleBase HightSpeedToggle
		{
			get
			{
				return hightSpeedToggle;
			}
			set
			{
				hightSpeedToggle = value;
			}
		}

		public void Init(AbstractView view, CarState state)
		{
			this.view = view;
			rigidbody = view.GetComponent<Rigidbody>();
			transform = view.transform;
			this.state = state;
			if (ExHighSpeed == null && state.CarPlayerType == PlayerType.PLAYER_SELF)
			{
				GameObject gameObject = GameObject.Find("gaosuxian");
				if ((bool)gameObject)
				{
					ExHighSpeed = gameObject.transform;
				}
			}
			if ((bool)RushEffect)
			{
				RushEffect.Init();
				RushEffect.Active = false;
			}
			if ((bool)ExSmallGasAirflow)
			{
				smallGasAirflowToggle = ExSmallGasAirflow.GetComponent<EffectToggleBase>();
				if (null == smallGasAirflowToggle)
				{
					smallGasAirflowToggle = ExSmallGasAirflow.gameObject.AddComponent<EffectToggleBase>();
				}
				smallGasAirflowToggle.Init();
				smallGasAirflowToggle.Active = false;
			}
			if ((bool)ExHighSpeed)
			{
				hightSpeedToggle = ExHighSpeed.GetComponent<EffectToggleBase>();
				if (hightSpeedToggle == null)
				{
					hightSpeedToggle = ExHighSpeed.gameObject.AddComponent<EffectToggleBase>();
				}
				hightSpeedToggle.Init();
				hightSpeedToggle.Active = false;
			}
			if ((bool)ExAcceleration)
			{
				accelerateToggle = ExAcceleration.GetComponent<EffectToggleBase>();
				if (accelerateToggle == null)
				{
					accelerateToggle = ExAcceleration.gameObject.AddComponent<EffectToggleBase>();
				}
				accelerateToggle.Init();
				accelerateToggle.Active = false;
			}
			if ((bool)ExGas && ExGas.GetComponent<EffectToggleFx>() == null)
			{
				gasToggle = ExGas.GetComponent<EffectToggleBase>();
				if (gasToggle == null)
				{
					gasToggle = ExGas.gameObject.AddComponent<EffectToggleBase>();
				}
				gasToggle.Init();
				gasToggle.Active = false;
			}
			if ((bool)ExGasAirflow)
			{
				gasAirflowToggle = ExGasAirflow.GetComponent<EffectToggleFx>();
				if (gasAirflowToggle == null)
				{
					gasAirflowToggle = ExGasAirflow.gameObject.AddComponent<EffectToggleFx>();
				}
				gasAirflowToggle.Init();
				gasAirflowToggle.Active = false;
			}
			if ((bool)ExYGasAirflow)
			{
				yGasAirflowToggle = ExYGasAirflow.GetComponent<EffectToggleFx>();
				if (yGasAirflowToggle == null)
				{
					yGasAirflowToggle = ExYGasAirflow.gameObject.AddComponent<EffectToggleFx>();
				}
				yGasAirflowToggle.Init();
				yGasAirflowToggle.Active = false;
			}
			if ((bool)ExPGasAirflow)
			{
				pGasAirflowToggle = ExPGasAirflow.GetComponent<EffectToggleFx>();
				if (pGasAirflowToggle == null)
				{
					pGasAirflowToggle = ExPGasAirflow.gameObject.AddComponent<EffectToggleFx>();
				}
				pGasAirflowToggle.Init();
				pGasAirflowToggle.Active = false;
			}
			if ((bool)ExCupidGas)
			{
				cupidGasToggle = ExCupidGas.GetComponent<EffectToggleBase>();
				if (cupidGasToggle == null)
				{
					cupidGasToggle = ExCupidGas.gameObject.AddComponent<EffectToggleBase>();
				}
				cupidGasToggle.Init();
				cupidGasToggle.Active = false;
			}
			if ((bool)ExExhaust)
			{
				exhaustToggle = ExExhaust.GetComponent<EffectToggleBase>();
				if (exhaustToggle == null)
				{
					exhaustToggle = ExExhaust.gameObject.AddComponent<EffectToggleBase>();
				}
				exhaustToggle.Init();
				exhaustToggle.Active = false;
			}
			if ((bool)SimpleN2Effect)
			{
				SimpleN2Effect.Active = false;
			}
			if ((bool)RearLampEffect)
			{
				RearLampEffect.Init();
				RearLampEffect.Active = false;
			}
			UpdateEffect();
		}

		private void setupToogle<T>(ref Transform ex, ref EffectToggleBase effect, bool isActive) where T : EffectToggleBase
		{
			if ((bool)ex)
			{
				effect = ex.GetComponent<T>();
				if (effect == null)
				{
					effect = ex.gameObject.AddComponent<T>();
				}
				effect.Init();
				effect.Active = isActive;
			}
		}

		internal void UpdateEffect()
		{
			if (AirShipEffects != null)
			{
				setupToogle<EffectToggleBase>(ref AirShipEffects.ExDriftBack, ref AirShipEffects.driftBackToggle, isActive: false);
				setupToogle<EffectToggleBase>(ref AirShipEffects.ExDriftDown, ref AirShipEffects.driftDownToggle, isActive: false);
				setupToogle<EffectToggleBase>(ref AirShipEffects.ExNormalBack, ref AirShipEffects.normalBackToggle, isActive: true);
				setupToogle<EffectToggleBase>(ref AirShipEffects.ExNormalDown, ref AirShipEffects.normalDownToggle, isActive: true);
			}
		}

		internal void SetupSmallAirflowEffect(bool loadOnNull = false)
		{
			if (!(ExSmallGasAirflow == null))
			{
				return;
			}
			GameObject go = GameObject.Find("jiasuxian(Clone)");
			if ((bool)go)
			{
				ExSmallGasAirflow = go.transform;
			}
			else if (loadOnNull)
			{
				Camera ui = null;
				Camera[] allCameras = Camera.allCameras;
				int num = LayerMask.NameToLayer("UI");
				if (allCameras == null)
				{
					return;
				}
				for (int i = 0; i < allCameras.Length; i++)
				{
					if (allCameras[i].gameObject.layer == num)
					{
						ui = allCameras[i];
						break;
					}
				}
				ResCallback effectLoaded = delegate(UnityEngine.Object o)
				{
					if (!(o == null) && !(view == null) && !(ui == null))
					{
						go = UnityEngine.Object.Instantiate(o) as GameObject;
						ExSmallGasAirflow = go.transform;
						ExSmallGasAirflow.parent = ui.transform;
						ExSmallGasAirflow.localRotation = Quaternion.Euler(0f, 180f, 0f);
						ExSmallGasAirflow.localPosition = RaceConfig.HSEffectOffset;
						smallGasAirflowToggle = ExSmallGasAirflow.GetComponent<EffectToggleBase>();
						if (smallGasAirflowToggle == null)
						{
							smallGasAirflowToggle = ExSmallGasAirflow.gameObject.AddComponent<EffectToggleBase>();
						}
						smallGasAirflowToggle.Init();
						smallGasAirflowToggle.Active = false;
					}
				};
				if (ui == null)
				{
					Singleton<ResourceOffer>.Instance.Load(RaceConfig.UIRootPath, delegate(UnityEngine.Object rootPrefab)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(rootPrefab) as GameObject;
						ui = gameObject.GetComponentInChildren<Camera>();
						Singleton<ResourceOffer>.Instance.Load(RaceConfig.SimpleHSEffect2, effectLoaded);
					});
				}
				else
				{
					Singleton<ResourceOffer>.Instance.Load(RaceConfig.SimpleHSEffect2, effectLoaded);
				}
			}
			else
			{
				LogWarning("jiasuxian(Clone) not found.");
			}
		}

		internal void SetupHSEffect(bool loadOnNull = false)
		{
			if (!(ExHighSpeed == null))
			{
				return;
			}
			GameObject go = GameObject.Find("gaosuxian(Clone)");
			if ((bool)go)
			{
				ExHighSpeed = go.transform;
			}
			else if (loadOnNull)
			{
				Camera ui = null;
				Camera[] allCameras = Camera.allCameras;
				int num = LayerMask.NameToLayer("UI");
				if (allCameras == null)
				{
					return;
				}
				for (int i = 0; i < allCameras.Length; i++)
				{
					if (allCameras[i].gameObject.layer == num)
					{
						ui = allCameras[i];
						break;
					}
				}
				ResCallback effectLoaded = delegate(UnityEngine.Object o)
				{
					if (!(o == null) && !(view == null) && !(ui == null))
					{
						go = UnityEngine.Object.Instantiate(o) as GameObject;
						ExHighSpeed = go.transform;
						ExHighSpeed.parent = ui.transform;
						ExHighSpeed.localRotation = Quaternion.Euler(0f, 180f, 0f);
						ExHighSpeed.localPosition = RaceConfig.HSEffectOffset;
						hightSpeedToggle = ExHighSpeed.GetComponent<EffectToggleBase>();
						if (hightSpeedToggle == null)
						{
							hightSpeedToggle = ExHighSpeed.gameObject.AddComponent<EffectToggleBase>();
						}
						hightSpeedToggle.Init();
						hightSpeedToggle.Active = false;
					}
				};
				if (ui == null)
				{
					Singleton<ResourceOffer>.Instance.Load(RaceConfig.UIRootPath, delegate(UnityEngine.Object rootPrefab)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(rootPrefab) as GameObject;
						ui = gameObject.GetComponentInChildren<Camera>();
						Singleton<ResourceOffer>.Instance.Load(RaceConfig.SimpleN2Effect, effectLoaded);
					});
				}
				else
				{
					Singleton<ResourceOffer>.Instance.Load(RaceConfig.SimpleN2Effect, effectLoaded);
				}
			}
			else
			{
				LogWarning("gaosuxian(Clone) not found.");
			}
		}

		public void StopAllEffects()
		{
			if ((bool)smallGasAirflowToggle)
			{
				smallGasAirflowToggle.Active = false;
			}
			if ((bool)hightSpeedToggle)
			{
				hightSpeedToggle.Active = false;
			}
			if ((bool)accelerateToggle)
			{
				accelerateToggle.Active = false;
			}
			if ((bool)gasToggle)
			{
				gasToggle.Active = false;
			}
			if ((bool)gasAirflowToggle)
			{
				gasAirflowToggle.Active = false;
			}
			if ((bool)yGasAirflowToggle)
			{
				yGasAirflowToggle.Active = false;
			}
			if ((bool)pGasAirflowToggle)
			{
				pGasAirflowToggle.Active = false;
			}
			if ((bool)cupidGasToggle)
			{
				cupidGasToggle.Active = false;
			}
			if ((bool)exhaustToggle)
			{
				exhaustToggle.Active = false;
			}
			if (RushEffect != null)
			{
				RushEffect.Active = false;
			}
			if (simpleGas != null)
			{
				simpleGas.Active = false;
			}
			if (AirShipEffects != null)
			{
				if (AirShipEffects.driftBackToggle != null)
				{
					AirShipEffects.driftBackToggle.Active = false;
				}
				if (AirShipEffects.driftDownToggle != null)
				{
					AirShipEffects.driftDownToggle.Active = false;
				}
				if (AirShipEffects.normalBackToggle != null)
				{
					AirShipEffects.normalBackToggle.Active = false;
				}
				if (AirShipEffects.normalDownToggle != null)
				{
					AirShipEffects.normalDownToggle.Active = false;
				}
			}
		}

		public override void OnActiveChange(bool active)
		{
			if (active)
			{
				AbstractView abstractView = view;
				abstractView.OnUpdate = (Action)Delegate.Remove(abstractView.OnUpdate, new Action(OnUpdate));
				AbstractView abstractView2 = view;
				abstractView2.OnUpdate = (Action)Delegate.Combine(abstractView2.OnUpdate, new Action(OnUpdate));
			}
			else
			{
				AbstractView abstractView3 = view;
				abstractView3.OnUpdate = (Action)Delegate.Remove(abstractView3.OnUpdate, new Action(OnUpdate));
			}
		}

		public void OnUpdate()
		{
			if (base.Active && state != null)
			{
				checkSmallGasAirflow();
				checkHightSpeed();
				checkAccel();
				checkGas();
				checkExhaust();
				checkTranslation();
				checkRush();
				checkRearLamp();
				checkAirShipEffect();
			}
		}

		private void checkAirShipEffect()
		{
			if (AirShipEffects == null)
			{
				return;
			}
			if (AirShipEffects.normalBackToggle != null && !AirShipEffects.normalBackToggle.Active)
			{
				AirShipEffects.normalBackToggle.Active = true;
			}
			if (AirShipEffects.normalDownToggle != null && !AirShipEffects.normalDownToggle.Active)
			{
				AirShipEffects.normalDownToggle.Active = true;
			}
			if (AirShipEffects.driftBackToggle != null)
			{
				if (state.CurDriftState.Stage != 0 && !AirShipEffects.driftBackToggle.Active)
				{
					AirShipEffects.driftBackToggle.Active = true;
				}
				else if (state.CurDriftState.Stage == DriftStage.NONE && AirShipEffects.driftBackToggle.Active)
				{
					AirShipEffects.driftBackToggle.Active = false;
				}
			}
			if (AirShipEffects.driftDownToggle != null)
			{
				if (state.CurDriftState.Stage != 0 && !AirShipEffects.driftDownToggle.Active)
				{
					AirShipEffects.driftDownToggle.Active = true;
				}
				else if (state.CurDriftState.Stage == DriftStage.NONE && AirShipEffects.driftDownToggle.Active)
				{
					AirShipEffects.driftDownToggle.Active = false;
				}
			}
		}

		private void checkRearLamp()
		{
			if (!(RearLampEffect == null))
			{
				RearLampEffect.Active = state.CurDriftState.Stage != DriftStage.NONE;
			}
		}

		private void checkRush()
		{
			if (RushEffect != null)
			{
				bool flag = state.N2State.Level == 1;
				if (flag != RushEffect.Active)
				{
					RushEffect.Active = flag;
				}
			}
			if (SimpleN2Effect != null)
			{
				bool flag2 = state.N2State.Level >= 2;
				flag2 |= state.AirGround.DoingGas;
				if (flag2 != SimpleN2Effect.Active)
				{
					SimpleN2Effect.Active = flag2;
				}
			}
		}

		private void checkExhaust()
		{
			if (!(exhaustToggle == null))
			{
				bool flag = state.N2State.Level == 0;
				if (flag != exhaustToggle.Active)
				{
					exhaustToggle.Active = flag;
				}
			}
		}

		private void checkAccel()
		{
			if (!(accelerateToggle == null))
			{
				bool flag = state.N2State.Level > 0 && (state.CarPlayerType == PlayerType.PLAYER_SELF || Application.platform == RuntimePlatform.WindowsPlayer || Application.isEditor);
				if (flag != accelerateToggle.Active)
				{
					accelerateToggle.Active = flag;
				}
			}
		}

		private void checkGas()
		{
			if (gasToggle == null)
			{
				return;
			}
			bool flag = state.N2State.Level == 1;
			if (null != cupidGasToggle)
			{
				bool flag2 = flag & state.N2State.IsCupidGas;
				if (flag == cupidGasToggle.Active && flag == gasToggle.Active)
				{
					return;
				}
				if (flag2)
				{
					if (gasToggle.Active)
					{
						gasToggle.Active = false;
					}
					if (flag != cupidGasToggle.Active)
					{
						cupidGasToggle.Active = flag;
					}
					return;
				}
				if (cupidGasToggle.Active)
				{
					cupidGasToggle.Active = false;
				}
				if (flag != gasToggle.Active)
				{
					gasToggle.Active = flag;
				}
				checkLevelGas(flag);
			}
			else
			{
				if (flag != gasToggle.Active)
				{
					gasToggle.Active = flag;
				}
				checkLevelGas(flag);
			}
		}

		private void checkLevelGas(bool isOpen)
		{
			switch (state.N2State.GasType)
			{
				case N2StateGasType.LEVEL_TWO:
					if (gasAirflowToggle != null && gasAirflowToggle.Active)
					{
						gasAirflowToggle.Active = false;
					}
					if (yGasAirflowToggle != null && isOpen != yGasAirflowToggle.Active)
					{
						yGasAirflowToggle.Active = isOpen;
					}
					if (pGasAirflowToggle != null && pGasAirflowToggle.Active)
					{
						pGasAirflowToggle.Active = false;
					}
					break;
				case N2StateGasType.LEVEL_THREE:
					if (gasAirflowToggle != null && gasAirflowToggle.Active)
					{
						gasAirflowToggle.Active = false;
					}
					if (yGasAirflowToggle != null && yGasAirflowToggle.Active)
					{
						yGasAirflowToggle.Active = false;
					}
					if (pGasAirflowToggle != null && isOpen != pGasAirflowToggle.Active)
					{
						pGasAirflowToggle.Active = isOpen;
					}
					break;
				default:
					if ((bool)gasAirflowToggle && isOpen != gasAirflowToggle.Active)
					{
						gasAirflowToggle.Active = isOpen;
					}
					if ((bool)yGasAirflowToggle && yGasAirflowToggle.Active)
					{
						yGasAirflowToggle.Active = false;
					}
					if ((bool)pGasAirflowToggle && pGasAirflowToggle.Active)
					{
						pGasAirflowToggle.Active = false;
					}
					break;
			}
		}

		private void checkSmallGasAirflow()
		{
			if (smallGasAirflowToggle != null)
			{
				bool flag = RaceConfig.HighSpeedEffectOn && (state.N2State.Level == 2 || state.SpeedRatio > 0.8f);
				if (flag != smallGasAirflowToggle.Active)
				{
					smallGasAirflowToggle.Active = flag;
				}
			}
		}

		private void checkHightSpeed()
		{
			if (!(hightSpeedToggle == null))
			{
				bool flag = RaceConfig.HighSpeedEffectOn && state.N2State.Level == 1;
				if (flag != hightSpeedToggle.Active)
				{
					hightSpeedToggle.Active = flag;
				}
			}
		}

		private void checkTranslation()
		{
			if (!(TranslateEffect == null))
			{
				bool flag = state.ApplyingSpecialType == SpecialType.Translate;
				if (flag != TranslateEffect.Active)
				{
					TranslateEffect.Active = flag;
				}
			}
		}
	}
}
