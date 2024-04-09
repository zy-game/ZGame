using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Rendering/UV Optimizer")]
public class UVOptimizer : MonoBehaviour
{
	public List<int> ErrorUVPoints;

	private MeshFilter mf;

	public void Awake()
	{
		if (!mf)
		{
			mf = GetComponent<MeshFilter>();
		}
	}

	public void Start()
	{
		if ((bool)mf && (bool)mf.mesh)
		{
			Vector2[] uv = mf.mesh.uv;
			if (uv.Length < 1)
			{
				return;
			}
			calOffsets(uv);
			mf.mesh.uv = uv;
		}
		base.enabled = false;
	}

	private void calOffsets(Vector2[] uvs)
	{
		int num = uvs.Length;
		List<int> list = new List<int>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(i);
		}
		for (int j = 0; j < ErrorUVPoints.Count; j++)
		{
			if (ErrorUVPoints[j] < num && ErrorUVPoints[j] >= 0)
			{
				list[ErrorUVPoints[j]] = -1;
			}
		}
		calOffsets(uvs, ErrorUVPoints);
		calOffsets(uvs, list);
	}

	private static void calOffsets(Vector2[] uvs, List<int> ids)
	{
		if (uvs.Length == 0 || ids.Count == 0)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		int num3 = uvs.Length;
		int num4 = 0;
		for (int i = 0; i < ids.Count; i++)
		{
			int num5 = ids[i];
			if (num5 >= 0 && num5 < num3)
			{
				num += uvs[num5].x;
				num2 += uvs[num5].y;
				num4++;
			}
		}
		if (num4 == 0)
		{
			return;
		}
		float num6 = Mathf.Round(num / (float)num4);
		float num7 = Mathf.Round(num2 / (float)num4);
		for (int j = 0; j < ids.Count; j++)
		{
			int num8 = ids[j];
			if (num8 >= 0 && num8 < num3)
			{
				uvs[num8].Set(uvs[num8].x - num6, uvs[num8].y - num7);
			}
		}
	}
}
