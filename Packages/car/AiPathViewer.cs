using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Demo/Ai Path Viewer")]
public class AiPathViewer : AbstractView
{
	public List<CarPoint> pathList;

	public List<ItemToggle> tgs;

	public bool DrawPath = true;

	public int MapId;

	public int Track;

	public int AiId;

	public uint AiSequence = 1u;

	private Vector3 vt1;

	private Vector3 vt2;

	private Vector3 cu = new Vector3(2.5f, 2.5f, 2.5f);

	private Color toggleColor = new Color(1f, 0f, 0f, 0.8f);

	public CarRecor recorder = new CarRecor();

	public string AiData => $"a_{MapId:D}Underline{Track:D}Underline{AiId:D}";

	private void OnDrawGizmosSelected()
	{
		if (!DrawPath)
		{
			return;
		}
		float num = 0f;
		ItemToggle itemToggle = null;
		List<CarPoint> list = pathList;
		if (list != null)
		{
			int num2 = 0;
			int num3 = ((tgs != null) ? tgs.Count : 0);
			for (int i = 0; i < list.Count - 1; i++)
			{
				CarPoint carPoint = list[i];
				CarPoint carPoint2 = list[i + 1];
				vt1.Set(carPoint.pos.x, carPoint.pos.y, carPoint.pos.z);
				vt2.Set(carPoint2.pos.x, carPoint2.pos.y, carPoint2.pos.z);
				Gizmos.color = Color.white;
				Gizmos.DrawLine(vt1, vt2);
				Gizmos.color = Color.black;
				Gizmos.DrawWireSphere(carPoint.pos.V3, 0.2f);
				num += carPoint.DeltaTime;
				if (num3 > 0 && num2 < num3)
				{
					itemToggle = tgs[num2];
					if (itemToggle.time >= num)
					{
						float num4 = num - itemToggle.time;
						Gizmos.color = toggleColor;
						Gizmos.DrawCube(Vector3.Lerp(vt1, vt2, num4 / carPoint.DeltaTime), cu);
						num2++;
					}
				}
			}
		}
		if (tgs != null)
		{
			for (int j = 0; j < tgs.Count; j++)
			{
				ItemToggle itemToggle2 = tgs[j];
				Gizmos.color = Color.yellow;
				Gizmos.DrawSphere(itemToggle2.pos, 1f);
			}
		}
	}
}
