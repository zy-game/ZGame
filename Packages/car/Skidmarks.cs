using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Skidmarks : MonoBehaviour
{
	public class MarkSection
	{
		public Vector3 pos = Vector3.zero;

		public Vector3 normal = Vector3.zero;

		public Vector3 posl = Vector3.zero;

		public Vector3 posr = Vector3.zero;

		public float intensity;

		public int lastIndex;

		public float length;

		public void CopyTo(MarkSection des)
		{
			des.pos = pos;
			des.normal = normal;
			des.posl = posl;
			des.posr = posr;
			des.intensity = intensity;
			des.lastIndex = lastIndex;
			des.length = length;
		}
	}

	public int maxMarks = 1024;

	public float markWidth = 0.275f;

	public float markLength = 0.2f;

	public float groundOffset = 0.02f;

	public float minDistance = 0.1f;

	private int numMarks;

	private MeshFilter filter;

	private MeshRenderer rd;

	public MarkSection[] skidmarks;

	private bool updated;

	private Mesh meshA;

	private MarkSection tmpMark = new MarkSection();

	private Vector3[] vertices;

	private int[] triangles;

	private Vector2[] uvs;

	private Color[] colors;

	public void Awake()
	{
		vertices = new Vector3[maxMarks * 4 - 4];
		triangles = new int[maxMarks * 6];
		uvs = new Vector2[maxMarks * 4 - 4];
		colors = new Color[maxMarks * 4 - 4];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i].r = 1f;
			colors[i].g = 1f;
			colors[i].b = 1f;
			colors[i].a = 1f;
		}
		skidmarks = new MarkSection[maxMarks];
		for (int j = 0; j < maxMarks; j++)
		{
			skidmarks[j] = new MarkSection();
		}
		filter = GetComponent<MeshFilter>();
		filter.sharedMesh = buildMesh();
		meshA = filter.sharedMesh;
		rd = GetComponent<Renderer>() as MeshRenderer;
	}

	public virtual int AddSkidMark(Vector3 pos, Vector3 normal, float intensity, int lastIndex)
	{
		if (intensity > 1f)
		{
			intensity = 1f;
		}
		if (intensity < 0f)
		{
			return -1;
		}
		MarkSection markSection = skidmarks[numMarks % maxMarks];
		markSection.pos = pos + normal * groundOffset;
		markSection.intensity = intensity;
		markSection.lastIndex = lastIndex;
		if (lastIndex >= 0)
		{
			MarkSection markSection2 = skidmarks[lastIndex];
			Vector3 lhs = markSection.pos - markSection2.pos;
			if (lhs.sqrMagnitude < minDistance * minDistance)
			{
				return numMarks - 1;
			}
			Vector3 normalized = Vector3.Cross(lhs, normal).normalized;
			markSection.posl = markSection.pos + normalized * markWidth * 0.5f;
			markSection.posr = markSection.pos - normalized * markWidth * 0.5f;
			markSection.length = lhs.magnitude;
			if (markSection2.lastIndex == -1)
			{
				markSection2.posl = markSection.pos + normalized * markWidth * 0.5f;
				markSection2.posr = markSection.pos - normalized * markWidth * 0.5f;
			}
		}
		numMarks++;
		updated = true;
		BuildSkidmarks();
		if (numMarks < maxMarks)
		{
			return numMarks - 1;
		}
		return 0;
	}

	private void BuildSkidmarks()
	{
		if (!updated)
		{
			return;
		}
		updated = false;
		Mesh mesh = meshA;
		mesh.Clear();
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < numMarks && i < maxMarks; i++)
		{
			if (skidmarks[i].lastIndex >= 0)
			{
				MarkSection markSection = skidmarks[i];
				MarkSection markSection2 = skidmarks[markSection.lastIndex];
				num3 = num2;
				num4 = num3 + markSection.length / markLength;
				num2 = num4;
				vertices[num * 4] = markSection2.posl;
				vertices[num * 4 + 1] = markSection2.posr;
				vertices[num * 4 + 2] = markSection.posl;
				vertices[num * 4 + 3] = markSection.posr;
				uvs[num * 4].Set(0f, num3);
				uvs[num * 4 + 1].Set(1f, num3);
				uvs[num * 4 + 2].Set(0f, num4);
				uvs[num * 4 + 3].Set(1f, num4);
				colors[num * 4].a = markSection2.intensity;
				colors[num * 4 + 1].a = markSection2.intensity;
				colors[num * 4 + 2].a = markSection.intensity;
				colors[num * 4 + 3].a = markSection.intensity;
				triangles[num * 6] = num * 4;
				triangles[num * 6 + 2] = num * 4 + 1;
				triangles[num * 6 + 1] = num * 4 + 2;
				triangles[num * 6 + 3] = num * 4 + 2;
				triangles[num * 6 + 5] = num * 4 + 1;
				triangles[num * 6 + 4] = num * 4 + 3;
				num++;
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.colors = colors;
		mesh.triangles = triangles;
		filter.sharedMesh = mesh;
		if (numMarks >= maxMarks)
		{
			ExtractNewObject(clampLastMark: true);
		}
	}

	private Mesh buildMesh()
	{
		Mesh mesh = new Mesh();
		mesh.MarkDynamic();
		return mesh;
	}

	public void ExtractNewObject(bool clampLastMark = false)
	{
		if (filter == null || filter.sharedMesh == null || filter.sharedMesh.vertexCount == 0)
		{
			return;
		}
		GameObject gameObject = new GameObject("SkidMesh");
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = rd.material;
		meshRenderer.castShadows = false;
		meshRenderer.receiveShadows = false;
		meshFilter.sharedMesh = filter.sharedMesh;
		filter.sharedMesh = buildMesh();
		meshA = filter.sharedMesh;
		if (clampLastMark)
		{
			if (numMarks < 0 || numMarks > skidmarks.Length)
			{
				Debug.LogError("numMarks out of range numMarks=" + numMarks + ", skidmarks.Length=" + skidmarks.Length);
			}
			numMarks = Mathf.Clamp(numMarks, 0, skidmarks.Length);
			if (numMarks > 0)
			{
				skidmarks[numMarks - 1].CopyTo(skidmarks[0]);
				numMarks = 1;
			}
		}
		else
		{
			numMarks = 0;
		}
		skidmarks[0].lastIndex = -2;
	}
}
