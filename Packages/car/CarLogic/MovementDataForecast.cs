using System.IO;
using UdpAdapterClient;
using UnityEngine;

namespace CarLogic
{
	public class MovementDataForecast : RacePlayerMsg
	{
		public long m_momentTime;

		public Vector3 m_curPosition;

		public Vector3 m_curAccel;

		public Quaternion m_curRotation;

		public Vector3 m_linearVelocity;

		public Vector3 m_angularVelocity;

		public Vector3 EularAngle = Vector3.zero;

		private Vector3 vNormal;

		public float RunDistance;

		public ushort Index;

		public ushort Delay;

		public float m_fDeltaTime;

		public ushort EfState;

		public byte[] Items = new byte[2];

		public float ScrapeValue;

		public ushort TranslateInfo;

		public float TranslateTime;

		public Vector3 Position = Vector3.zero;

		public Quaternion Rotation;

		public Vector3 AccelVelocity = new Vector3(0f, 0f, 0f);

		public float Speed;

		public float AngleSpeed;

		private Vector3 velocity;

		public float QueueLoseTime;

		private static Vector3 vtemp = Vector3.zero;

		public Vector3 Velocity
		{
			get
			{
				if (Speed <= 0f)
				{
					return Vector3.zero;
				}
				if (velocity != Vector3.zero)
				{
					return velocity;
				}
				return velocity = vNormal * Speed;
			}
			set
			{
				vNormal = value.normalized;
				Speed = value.magnitude;
				if (Speed <= 0.027f)
				{
					Speed = 0f;
				}
			}
		}

		internal int DriftStartSteer
		{
			get
			{
				if ((EfState & 0x10u) != 0)
				{
					return -1;
				}
				return 1;
			}
		}

		internal bool HasTranslateInfo => (EfState & 0x40) != 0;

		internal int TranslateStarterId => (TranslateInfo & 0xF8) >> 3;

		internal int TranslatePathIndex => TranslateInfo & 7;

		internal int TranslateQteOk => (TranslateInfo & 0x100) >> 8;

		internal int TranslateQteAnimationIndex => (TranslateInfo & 0xE00) >> 9;

		public MovementDataForecast(byte[] data)
			: base(data)
		{
		}

		public MovementDataForecast(Transform trans, ushort index, Vector3 accel, long worldId, float distance, float deltaTime = 1f, long netDelay = 0L)
			: base(0, 4, 0)
		{
			Index = index;
			if (trans != null)
			{
				m_momentTime = udp_time.GameNow() + 24;
				m_curRotation = trans.localRotation;
				m_linearVelocity = trans.GetComponent<Rigidbody>().velocity;
				AccelVelocity = accel;
				if (vtemp.Equals((object)Vector3.zero))
				{
					vtemp = Velocity;
				}
				else
				{
					vtemp = vtemp * 0.1f + Velocity * 0.9f;
				}
				float magnitude = vtemp.magnitude;
				float num = Mathf.Min(magnitude, 10f) / 10f;
				float num2 = 1f - Mathf.Min(Mathf.Abs(AngleSpeed), 40f) / 40f;
				float num3 = 0.24f * num * num2;
				Vector3 vector = 0.5f * AccelVelocity * num3 * num3;
				EularAngle.y += AngleSpeed * num3;
				if (EularAngle.x < 20f || EularAngle.x > 340f)
				{
					vector.y = 0f;
					m_curPosition = trans.localPosition + vtemp * num3 + vector;
				}
				else
				{
					m_curPosition = trans.localPosition;
				}
				m_angularVelocity = trans.GetComponent<Rigidbody>().angularVelocity;
			}
			Delay = (ushort)netDelay;
			WorldId = worldId;
			RunDistance = distance;
			m_fDeltaTime = deltaTime;
		}

		public MovementDataForecast(CarState state, ushort index, float deltaTime, long netDelay = 0L)
			: this(state.transform, index, state.TotalForces / state.view.carModel.CarWeight, state.view.PlayerInfo.WorldId, state.PastDistance, deltaTime, netDelay)
		{
			SetEfState(state);
			SetItems(state);
			SetScrapeValue(state);
		}

		public void GetEfState(CarState state)
		{
			state.N2State.Level = 3 & EfState;
			state.ApplyingSpecialType = SpecialType.None;
			if ((EfState & 0x40u) != 0)
			{
				state.ApplyingSpecialType = SpecialType.Translate;
			}
			if ((EfState & 0x80u) != 0)
			{
				state.ApplyingSpecialType = SpecialType.SpeedUp;
			}
			state.CurDriftState.Stage = (DriftStage)((EfState >> 8) & 7);
			state.N2State.GasType = (N2StateGasType)((EfState >> 11) & 7);
			state.Steer = (ushort)((EfState >> 14) & 3) switch
			{
				2 => -1, 
				0 => 0, 
				_ => 1, 
			};
		}

		public static void ClearEfState(CarState state)
		{
			state.N2State.Level = 0;
			state.CurDriftState.Stage = DriftStage.NONE;
		}

		private void SetEfState(CarState state)
		{
			int num = state.N2State.Level;
			if (num == 0 && state.AirGround.DoingGas)
			{
				num = 2;
			}
			ushort num2 = (ushort)((state.Steer != 0f) ? ((!(state.Steer < 0f)) ? 1u : 2u) : 0u);
			int num3 = 0;
			num3 |= 3 & num;
			num3 |= ((state.GroundHit != 0) ? 8 : 0);
			num3 |= ((state.CurDriftState.StartSteer < 0f) ? 16 : 0);
			num3 |= (state.view.ItController.ApplyingItem(RaceItemId.GROUP_GAS) ? 32 : 0);
			num3 |= ((state.ApplyingSpecialType == SpecialType.Translate) ? 64 : 0);
			num3 |= ((state.ApplyingSpecialType == SpecialType.SpeedUp) ? 128 : 0);
			num3 |= ((ushort)state.CurDriftState.Stage & 7) << 8;
			num3 |= ((ushort)state.N2State.GasType & 7) << 11;
			num3 |= (num2 & 3) << 14;
			EfState = (ushort)num3;
			if (HasTranslateInfo)
			{
				TranslateInfo = (ushort)(0xFFu & state.TranslateInfoByte);
				if (state.TransQTEok == 1 && state.TransQteAnimIndex >= 0)
				{
					TranslateInfo |= 256;
					TranslateInfo = (ushort)(TranslateInfo | (0xE00u & (uint)(state.TransQteAnimIndex << 9)));
				}
				TranslateTime = state.ApplyingSpecial.Duration;
			}
		}

		private void SetItems(CarState state)
		{
			for (int i = 0; i < state.Items.Count && i < Items.Length; i++)
			{
				if ((state.Items[i].ItemId > RaceItemId.NONE && state.Items[i].ItemId < RaceItemId.END) || state.Items[i].ItemId == RaceItemId.GasLevelTwo || state.Items[i].ItemId == RaceItemId.GasLevelThree)
				{
					Items[i] = (byte)state.Items[i].ItemId;
				}
				else
				{
					Items[i] = 0;
				}
			}
		}

		public void GetItems(CarState state)
		{
			state.Items.Clear();
			for (int i = 0; i < Items.Length; i++)
			{
				if ((Items[i] > 0 && Items[i] < 19) || Items[i] == 31 || Items[i] == 32)
				{
					state.view.AddItem(RaceItemFactory.BuildItemById((RaceItemId)Items[i]));
				}
			}
			if (state.CallBacks.OnItemsChange != null)
			{
				state.CallBacks.OnItemsChange(state);
			}
		}

		private void SetScrapeValue(CarState state)
		{
			ScrapeValue = state.view.N2Scraper.ScrapeValue;
		}

		public void GetScrapeValue(CarState state)
		{
			state.view.N2Scraper.SetScrapeValue(ScrapeValue);
			if (state.CallBacks.OnUpdateScrapeValue != null)
			{
				state.CallBacks.OnUpdateScrapeValue(state);
			}
		}

		internal bool ApplyingGroupGas()
		{
			return (EfState & 0x20) != 0;
		}

		internal bool IsGrounded()
		{
			return (EfState & 8) != 0;
		}

		internal float DeltaTime()
		{
			return m_fDeltaTime;
		}

		private int unitConvert(float d)
		{
			return (int)(d * 100f);
		}

		private float unitConvert(int d)
		{
			return (float)d / 100f;
		}

		private int angleConvert(float a)
		{
			return (int)(a * 10f);
		}

		private float angleConvert(int a)
		{
			return (float)a / 10f;
		}

		private int velConvert(float v)
		{
			return (int)(v * 1000f);
		}

		private float velConvert(int v)
		{
			return (float)v / 1000f;
		}

		protected override void writeData(BinaryWriter writer)
		{
			writer.Write(unitConvert(Position.x));
			writer.Write(unitConvert(Position.y));
			writer.Write(unitConvert(Position.z));
			writer.Write(unitConvert(RunDistance));
			writer.Write(Index);
			writer.Write(angleConvert(EularAngle.y));
			writer.Write(velConvert(vNormal.x));
			writer.Write(velConvert(vNormal.y));
			writer.Write(velConvert(vNormal.z));
			writer.Write(velConvert(Speed));
			writer.Write(velConvert(AngleSpeed));
			writer.Write(Delay);
			writer.Write(EfState);
			writer.Write(Items);
			writer.Write(ScrapeValue);
			writer.Write((double)m_fDeltaTime);
			writer.Write(angleConvert(EularAngle.x));
			writer.Write(angleConvert(EularAngle.z));
			if (HasTranslateInfo)
			{
				writer.Write(TranslateInfo);
				writer.Write((ushort)unitConvert(TranslateTime));
			}
			writer.Write(m_momentTime);
			writer.Write((double)m_curPosition.x);
			writer.Write((double)m_curPosition.y);
			writer.Write((double)m_curPosition.z);
			writer.Write((double)m_curRotation.x);
			writer.Write((double)m_curRotation.y);
			writer.Write((double)m_curRotation.z);
			writer.Write((double)m_curRotation.w);
			writer.Write((double)m_linearVelocity.x);
			writer.Write((double)m_linearVelocity.y);
			writer.Write((double)m_linearVelocity.z);
			writer.Write((double)m_angularVelocity.x);
			writer.Write((double)m_angularVelocity.y);
			writer.Write((double)m_angularVelocity.z);
		}

		protected override void readData(BinaryReader reader)
		{
			Position.x = unitConvert(reader.ReadInt32());
			Position.y = unitConvert(reader.ReadInt32());
			Position.z = unitConvert(reader.ReadInt32());
			RunDistance = unitConvert(reader.ReadInt32());
			Index = reader.ReadUInt16();
			EularAngle.y = angleConvert(reader.ReadInt32());
			vNormal.x = velConvert(reader.ReadInt32());
			vNormal.y = velConvert(reader.ReadInt32());
			vNormal.z = velConvert(reader.ReadInt32());
			Speed = velConvert(reader.ReadInt32());
			AngleSpeed = velConvert(reader.ReadInt32());
			Delay = reader.ReadUInt16();
			EfState = reader.ReadUInt16();
			Items = reader.ReadBytes(Items.Length);
			ScrapeValue = reader.ReadSingle();
			m_fDeltaTime = (float)reader.ReadDouble();
			EularAngle.x = angleConvert(reader.ReadInt32());
			EularAngle.z = angleConvert(reader.ReadInt32());
			if (HasTranslateInfo)
			{
				TranslateInfo = reader.ReadUInt16();
				TranslateTime = unitConvert(reader.ReadUInt16());
			}
			Rotation = Quaternion.Euler(EularAngle);
			m_momentTime = reader.ReadInt64();
			m_curPosition.x = (float)reader.ReadDouble();
			m_curPosition.y = (float)reader.ReadDouble();
			m_curPosition.z = (float)reader.ReadDouble();
			m_curRotation.x = (float)reader.ReadDouble();
			m_curRotation.y = (float)reader.ReadDouble();
			m_curRotation.z = (float)reader.ReadDouble();
			m_curRotation.w = (float)reader.ReadDouble();
			m_linearVelocity.x = (float)reader.ReadDouble();
			m_linearVelocity.y = (float)reader.ReadDouble();
			m_linearVelocity.z = (float)reader.ReadDouble();
			m_angularVelocity.x = (float)reader.ReadDouble();
			m_angularVelocity.y = (float)reader.ReadDouble();
			m_angularVelocity.z = (float)reader.ReadDouble();
		}
	}
}
