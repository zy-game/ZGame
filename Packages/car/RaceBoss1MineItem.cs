using CarLogic;
using UnityEngine;

public class RaceBoss1MineItem : RaceItemBase
{
	public static Int CurGroupIndex = -1;

	public static float[,,] PosGroup = new float[10, 3, 2]
	{
		{
			{ -2f, -2f },
			{ 2f, -2f },
			{ 0f, -5.464f }
		},
		{
			{ -2f, -5.464f },
			{ 2f, -5.464f },
			{ 0f, -2f }
		},
		{
			{ -2f, -2f },
			{ 2f, -6f },
			{ 0f, -4f }
		},
		{
			{ -2f, -6f },
			{ 2f, -2f },
			{ 0f, -4f }
		},
		{
			{ -2f, -4f },
			{ 2f, -4f },
			{ 0f, -4f }
		},
		{
			{ 0f, -2f },
			{ 0f, -4f },
			{ 0f, -6f }
		},
		{
			{ -1.732f, -2f },
			{ -1.732f, -6f },
			{ 1.732f, -4f }
		},
		{
			{ 1.732f, -2f },
			{ 1.732f, -6f },
			{ -1.732f, -4f }
		},
		{
			{ -2f, -4f },
			{ 0f, -6f },
			{ 2f, -2f }
		},
		{
			{ 2f, -4f },
			{ 0f, -6f },
			{ -2f, -2f }
		}
	};

	private const string objName = "TriggerBoss1Mine";

	public override RaceItemId ItemId => RaceItemId.MINE;

	public override void Toggle(ItemParams ps)
	{
		base.Toggle(ps);
		if (ps.user != null)
		{
			CarState user = ps.user;
			ushort instanceId = ps.instanceId;
			Singleton<ResourceOffer>.Instance.Load(RaceConfig.ItemMine, delegate(Object o)
			{
				if (!(o == null) && user != null && !(user.view == null))
				{
					CurGroupIndex = (int)(++CurGroupIndex) % PosGroup.GetLength(0);
					for (int i = 0; i < 3; i++)
					{
						GameObject gameObject = Object.Instantiate(o) as GameObject;
						Vector3 lastNormal = user.LastNormal;
						Vector3 vector = PosGroup[(int)CurGroupIndex, i, 0] * user.transform.right;
						Vector3 vector2 = PosGroup[(int)CurGroupIndex, i, 1] * user.transform.forward;
						Vector3 vector3 = user.transform.position + vector2 + vector;
						if (Physics.Raycast(vector3 + lastNormal * 3f, -lastNormal, out var hitInfo, 100f, 256))
						{
							vector3 = hitInfo.point;
							lastNormal = hitInfo.normal;
							gameObject.transform.position = vector3;
							gameObject.transform.rotation = Quaternion.LookRotation(user.transform.forward, lastNormal);
							gameObject.name = "TriggerBoss1Mine";
							SimpleData simpleData = gameObject.AddComponent<SimpleData>();
							TriggerData data = (TriggerData)(simpleData.UserData = new TriggerData
							{
								LayTime = Time.time,
								ItemId = ItemId,
								InstanceId = instanceId,
								User = user,
								TriggerObject = gameObject
							});
							saveToBuffer(data);
							addReleaseCallback(simpleData, data);
						}
						else
						{
							Object.DestroyImmediate(gameObject);
						}
					}
				}
			});
		}
		Break();
	}
}
