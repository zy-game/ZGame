using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VerticalSkidmarks : MonoBehaviour
{
	public class VerticalMark
	{
		public Vector3 pos = Vector3.zero;

		public Vector3 hitNormal = Vector3.zero;

		public Vector3 normal = Vector3.zero;

		public Vector4 tangent = Vector4.zero;

		public Vector3 posl = Vector3.zero;

		public Vector3 posr = Vector3.zero;

		public float intensity;

		public float timer;

		public float frameTime;

		public float length;

		public int lastIndex = -1;

		public int frame;

		public void Reset()
		{
			pos = Vector3.zero;
			hitNormal = Vector3.zero;
			normal = Vector3.zero;
			tangent = Vector4.zero;
			posl = Vector3.zero;
			posr = Vector3.zero;
			intensity = 0f;
			timer = 0f;
			frameTime = 0f;
			length = 0f;
			lastIndex = -1;
			frame = 0;
		}
	}

	public int maxMarks = 20;

	public int minMarks = 10;

	public float markHeight = 0.275f;

	public float groundOffset = 0.02f;

	public float minDistance = 0.1f;

	public float markLength = 0.2f;

	public float MaxLength = 20f;

	public float showTime = 0.1f;

	public float StartRatio = 0.7f;

	public float HeightLerpScale = 1f;

	public float[] XOffsets = new float[2] { 0.15f, -0.15f };

	private static float[] uvOffs = new float[10] { 0.15f, -0.3f, 0.28f, 0.64f, -0.92f, -0.56f, -0.037f, 0.185f, 0.752f, -0.527f };

	private int numMarks;

	private float duration;

	private MeshFilter filter;

	private MeshRenderer rd;

	public VerticalMark[] skidmarks;

	private bool updated;

	private Mesh mesh;

	private Color w = Color.white;

	private Vector3[] vertices;

	private int[] triangles;

	private Vector2[] uvs;

	public void Awake()
	{
		vertices = new Vector3[maxMarks * 4];
		triangles = new int[maxMarks * 6];
		uvs = new Vector2[maxMarks * 4];
		skidmarks = new VerticalMark[maxMarks];
		for (int i = 0; i < maxMarks; i++)
		{
			skidmarks[i] = new VerticalMark();
		}
		filter = GetComponent<MeshFilter>();
		if (filter.mesh == null)
		{
			filter.mesh = new Mesh();
		}
		mesh = filter.mesh;
		Debug.Log(" --- VerticalSkidmarks in CarLogic.Dll ---  Modify by LiuShibin");
		rd = GetComponent<Renderer>() as MeshRenderer;
	}

	public void ResetAllSkidmarks()
	{
		if (skidmarks != null)
		{
			for (int i = 0; i < skidmarks.Length; i++)
			{
				skidmarks[i].Reset();
			}
		}
		if (vertices != null)
		{
			for (int j = 0; j < vertices.Length; j++)
			{
				vertices[j] = Vector3.zero;
			}
		}
		if (triangles != null)
		{
			for (int k = 0; k < triangles.Length; k++)
			{
				triangles[k] = 0;
			}
		}
		if (uvs != null)
		{
			for (int l = 0; l < uvs.Length; l++)
			{
				uvs[l] = Vector2.zero;
			}
		}
	}

	public virtual int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex, int xof)
	{
		if (intensity > 1f)
		{
			intensity = 1f;
		}
		if (intensity < 0f)
		{
			return -1;
		}
		VerticalMark verticalMark = skidmarks[numMarks % maxMarks];
		verticalMark.pos = pos + normal * groundOffset;
		verticalMark.hitNormal = normal;
		verticalMark.intensity = intensity;
		verticalMark.lastIndex = lastIndex;
		verticalMark.frame = numMarks + 1;
		verticalMark.timer = Time.time;
		if (lastIndex != -1)
		{
			VerticalMark verticalMark2 = skidmarks[lastIndex % maxMarks];
			Vector3 lhs = verticalMark.pos - verticalMark2.pos;
			Vector3 normalized = Vector3.Cross(lhs, normal).normalized;
			float num = XOffsets[xof % XOffsets.Length];
			verticalMark.posl = verticalMark.pos + verticalMark.hitNormal * groundOffset;
			verticalMark.posr = verticalMark.pos + verticalMark.hitNormal * markHeight;
			verticalMark.length = lhs.magnitude;
			if (verticalMark2.lastIndex == -1)
			{
				verticalMark2.tangent = verticalMark.tangent;
				verticalMark2.posl = verticalMark.pos + verticalMark.hitNormal * groundOffset;
				verticalMark2.posr = verticalMark.pos + verticalMark.hitNormal * markHeight;
			}
		}
		numMarks++;
		updated = true;
		duration = 0f;
		return numMarks - 1;
	}

	public void LateUpdate()
	{
		if (!updated)
		{
			return;
		}
		duration += Time.deltaTime;
		if (duration > showTime)
		{
			Stop();
			return;
		}
		mesh.Clear();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
			if (skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks)
			{
				num2++;
			}
		}
		num2 = Mathf.Max(minMarks, num2);
		float num3 = uvOffs[Time.frameCount % uvOffs.Length];
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float time = Time.time;
		int num8 = numMarks - 1;
		while (num < num2 && num8 >= 0)
		{
			int num9 = num8 % maxMarks;
			if (skidmarks[num9].lastIndex != -1 && skidmarks[num9].lastIndex > numMarks - maxMarks)
			{
				VerticalMark verticalMark = null;
				VerticalMark verticalMark2 = null;
				verticalMark = skidmarks[num9];
				verticalMark2 = skidmarks[verticalMark.lastIndex % maxMarks];
				float value = (float)num / (float)num2;
				num6 = Mathf.Sin(Mathf.Clamp01(value) * 180f * ((float)Math.PI / 180f)) * HeightLerpScale;
				value = (float)(num + 1) / (float)num2;
				num7 = Mathf.Sin(Mathf.Clamp01(value) * 180f * ((float)Math.PI / 180f)) * HeightLerpScale;
				num4 = num3;
				num5 = num4 + verticalMark.length / markLength;
				num3 = Mathf.Repeat(num5, 1f);
				vertices[num * 4] = verticalMark2.posl;
				vertices[num * 4 + 1] = Vector3.Lerp(verticalMark2.posl, verticalMark2.posr, num7);
				vertices[num * 4 + 2] = verticalMark.posl;
				vertices[num * 4 + 3] = Vector3.Lerp(verticalMark.posl, verticalMark.posr, num6);
				uvs[num * 4].Set(num5, 0f);
				uvs[num * 4 + 1].Set(num5, 1f);
				uvs[num * 4 + 2].Set(num4, 0f);
				uvs[num * 4 + 3].Set(num4, 1f);
				triangles[num * 6] = num * 4;
				triangles[num * 6 + 2] = num * 4 + 1;
				triangles[num * 6 + 1] = num * 4 + 2;
				triangles[num * 6 + 3] = num * 4 + 2;
				triangles[num * 6 + 5] = num * 4 + 1;
				triangles[num * 6 + 4] = num * 4 + 3;
				num++;
			}
			num8--;
		}
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;
	}

	public void LateUpdateBak()
	{
		if (!updated)
		{
			return;
		}
		duration += Time.deltaTime;
		if (duration > showTime)
		{
			Stop();
			return;
		}
		mesh.Clear();
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
			if (skidmarks[i].lastIndex != -1 && skidmarks[i].lastIndex > numMarks - maxMarks)
			{
				num2++;
			}
		}
		int[] array = new int[num2 * 6];
		float num3 = uvOffs[Time.frameCount % uvOffs.Length];
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float time = Time.time;
		for (int j = 0; j < numMarks && j < maxMarks; j++)
		{
			if (skidmarks[j].lastIndex != -1 && skidmarks[j].lastIndex > numMarks - maxMarks)
			{
				VerticalMark verticalMark = skidmarks[j];
				VerticalMark verticalMark2 = skidmarks[verticalMark.lastIndex % maxMarks];
				num6 = Mathf.Clamp01(Mathf.Lerp(0f, 0.95f, (time - verticalMark.timer) / showTime - 0.2f));
				num7 = Mathf.Clamp01(Mathf.Lerp(0f, 0.95f, (time - verticalMark2.timer) / showTime - 0.2f));
				num4 = num3;
				num5 = num4 + verticalMark.length / markLength;
				num3 = num5;
				num3 = Mathf.Repeat(num3, 1f);
				vertices[num * 4] = verticalMark2.posl;
				vertices[num * 4 + 1] = Vector3.Lerp(verticalMark2.posr, verticalMark2.posl, num7);
				vertices[num * 4 + 2] = verticalMark.posl;
				vertices[num * 4 + 3] = Vector3.Lerp(verticalMark.posr, verticalMark.posl, num6);
				uvs[num * 4].Set(num4, 0f);
				uvs[num * 4 + 1].Set(num4, 1f);
				uvs[num * 4 + 2].Set(num5, 0f);
				uvs[num * 4 + 3].Set(num5, 1f);
				array[num * 6] = num * 4;
				array[num * 6 + 2] = num * 4 + 1;
				array[num * 6 + 1] = num * 4 + 2;
				array[num * 6 + 3] = num * 4 + 2;
				array[num * 6 + 5] = num * 4 + 1;
				array[num * 6 + 4] = num * 4 + 3;
				num++;
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = array;
	}

	public void Stop()
	{
		updated = false;
		mesh.Clear();
	}

	public void ExtractNewObject()
	{
		if (!(mesh == null) && mesh.vertexCount != 0 && mesh != null)
		{
			GameObject gameObject = new GameObject("SkidMesh");
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = rd.material;
			meshRenderer.castShadows = false;
			meshRenderer.receiveShadows = false;
			meshFilter.mesh = mesh;
			mesh = new Mesh();
			filter.mesh = mesh;
			numMarks = 0;
		}
	}
}
