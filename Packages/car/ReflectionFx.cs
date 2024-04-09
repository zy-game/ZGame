using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Effects/ReflectionFx")]
public class ReflectionFx : MonoBehaviour
{
	public Transform[] reflectiveObjects;

	public LayerMask reflectionMask;

	public Material[] reflectiveMaterials;

	private Transform reflectiveSurfaceHeight;

	public Shader replacementShader;

	public bool HighQuality = true;

	public Color clearColor = Color.black;

	public string reflectionSampler = "_ReflectionTex";

	public string ReplaceTag = "";

	public bool CustomNormal = true;

	public Vector3 Normal = Vector3.up;

	public float clipPlaneOffset = 0.07f;

	private Vector3 oldpos = Vector3.zero;

	private Camera reflectionCamera;

	private Dictionary<Camera, bool> helperCameras;

	private Texture[] initialReflectionTextures;

	public void Start()
	{
		Texture[] array = new Texture2D[reflectiveMaterials.Length];
		Texture[] array2 = (initialReflectionTextures = array);
		for (int i = 0; i < reflectiveMaterials.Length; i++)
		{
			if (reflectiveMaterials[i].HasProperty(reflectionSampler))
			{
				initialReflectionTextures[i] = reflectiveMaterials[i].GetTexture(reflectionSampler);
			}
		}
		if (!SystemInfo.supportsRenderTextures)
		{
			base.enabled = false;
		}
	}

	public void OnDisable()
	{
		if (initialReflectionTextures == null)
		{
			return;
		}
		for (int i = 0; i < reflectiveMaterials.Length; i++)
		{
			if (reflectiveMaterials[i].HasProperty(reflectionSampler))
			{
				reflectiveMaterials[i].SetTexture(reflectionSampler, initialReflectionTextures[i]);
			}
		}
	}

	public void OnDestroy()
	{
		if (reflectiveMaterials != null)
		{
			for (int i = 0; i < reflectiveMaterials.Length; i++)
			{
				Material material = reflectiveMaterials[i];
				if (material.HasProperty(reflectionSampler))
				{
					material.SetTexture(reflectionSampler, null);
				}
			}
		}
		if ((bool)reflectionCamera)
		{
			if ((bool)reflectionCamera.targetTexture)
			{
				reflectionCamera.targetTexture.Release();
				Object.DestroyImmediate(reflectionCamera.targetTexture);
			}
			reflectionCamera.targetTexture = null;
		}
		reflectionCamera = null;
		reflectiveMaterials = null;
		helperCameras = null;
		initialReflectionTextures = null;
	}

	private Camera CreateReflectionCameraFor(Camera cam)
	{
		string text = base.gameObject.name + "Reflection" + cam.name;
		GameObject gameObject = GameObject.Find(text);
		if (!gameObject)
		{
			gameObject = new GameObject(text, typeof(Camera));
		}
		if (!gameObject.GetComponent(typeof(Camera)))
		{
			gameObject.AddComponent(typeof(Camera));
		}
		Camera component = gameObject.GetComponent<Camera>();
		component.backgroundColor = clearColor;
		component.clearFlags = CameraClearFlags.Color;
		SetStandardCameraParameter(component, reflectionMask);
		if (!component.targetTexture)
		{
			component.targetTexture = CreateTextureFor(cam);
		}
		return component;
	}

	private void SetStandardCameraParameter(Camera cam, LayerMask mask)
	{
		cam.backgroundColor = Color.black;
		cam.enabled = false;
		cam.cullingMask = reflectionMask;
	}

	private RenderTexture CreateTextureFor(Camera cam)
	{
		RenderTextureFormat format = RenderTextureFormat.RGB565;
		if (!SystemInfo.SupportsRenderTextureFormat(format))
		{
			format = RenderTextureFormat.Default;
		}
		float num = (HighQuality ? 0.75f : 0.5f);
		Debug.LogWarning("  -----此处不确定是 调用Camera.pixelWidth 还是 Camera.scalePixelWidth 哪一个 ----   ReflectionFx.cs in CarLogic.Dll  ");
		return new RenderTexture(Mathf.FloorToInt((float)cam.pixelWidth * num), Mathf.FloorToInt((float)cam.pixelHeight * num), 24, format);
	}

	public void RenderHelpCameras(Camera currentCam)
	{
		if (helperCameras == null)
		{
			helperCameras = new Dictionary<Camera, bool>();
		}
		if (!helperCameras.ContainsKey(currentCam))
		{
			helperCameras.Add(currentCam, value: false);
		}
		if (helperCameras[currentCam])
		{
			return;
		}
		if (!reflectionCamera)
		{
			reflectionCamera = CreateReflectionCameraFor(currentCam);
			for (int i = 0; i < reflectiveMaterials.Length; i++)
			{
				Material material = reflectiveMaterials[i];
				if (material.HasProperty(reflectionSampler))
				{
					material.SetTexture(reflectionSampler, reflectionCamera.targetTexture);
				}
			}
		}
		RenderReflectionFor(currentCam, reflectionCamera);
		helperCameras[currentCam] = true;
	}

	public void LateUpdate()
	{
		Transform transform = null;
		float num = float.PositiveInfinity;
		Vector3 position = Camera.main.transform.position;
		for (int i = 0; i < reflectiveObjects.Length; i++)
		{
			Transform transform2 = reflectiveObjects[i];
			if (transform2.GetComponent<Renderer>().isVisible)
			{
				float sqrMagnitude = (position - transform2.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					transform = transform2;
				}
			}
		}
		if ((bool)transform)
		{
			ObjectBeingRendered(transform, Camera.main);
			if (helperCameras != null)
			{
				helperCameras.Clear();
			}
		}
	}

	private void ObjectBeingRendered(Transform tr, Camera currentCam)
	{
		if (!(null == tr))
		{
			reflectiveSurfaceHeight = tr;
			RenderHelpCameras(currentCam);
		}
	}

	private void RenderReflectionFor(Camera cam, Camera reflectCamera)
	{
		if ((bool)reflectCamera)
		{
			SaneCameraSettings(reflectCamera);
			reflectCamera.backgroundColor = clearColor;
			GL.SetRevertBackfacing(revertBackFaces: true);
			Transform transform = reflectiveSurfaceHeight;
			Vector3 eulerAngles = cam.transform.eulerAngles;
			reflectCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
			reflectCamera.transform.position = cam.transform.position;
			Vector3 position = transform.transform.position;
			position.y = transform.position.y;
			Vector3 vector = (CustomNormal ? Normal : transform.transform.up);
			float w = 0f - Vector3.Dot(vector, position) - clipPlaneOffset;
			Vector4 plane = new Vector4(vector.x, vector.y, vector.z, w);
			Matrix4x4 zero = Matrix4x4.zero;
			zero = CalculateReflectionMatrix(zero, plane);
			oldpos = cam.transform.position;
			Vector3 position2 = zero.MultiplyPoint(oldpos);
			reflectCamera.worldToCameraMatrix = cam.worldToCameraMatrix * zero;
			Vector4 clipPlane = CameraSpacePlane(reflectCamera, position, vector, 1f);
			Matrix4x4 projectionMatrix = cam.projectionMatrix;
			Matrix4x4 matrix4x2 = (reflectCamera.projectionMatrix = CalculateObliqueMatrix(projectionMatrix, clipPlane));
			projectionMatrix = matrix4x2;
			reflectCamera.transform.position = position2;
			Vector3 eulerAngles2 = cam.transform.eulerAngles;
			reflectCamera.transform.eulerAngles = new Vector3(0f - eulerAngles2.x, eulerAngles2.y, eulerAngles2.z);
			reflectCamera.RenderWithShader(replacementShader, ReplaceTag);
			GL.SetRevertBackfacing(revertBackFaces: false);
		}
	}

	private void SaneCameraSettings(Camera helperCam)
	{
		helperCam.depthTextureMode = DepthTextureMode.None;
		helperCam.backgroundColor = Color.black;
		helperCam.clearFlags = CameraClearFlags.Color;
		helperCam.renderingPath = RenderingPath.Forward;
	}

	private static Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
	{
		Vector4 b = projection.inverse * new Vector4(sgn(clipPlane.x), sgn(clipPlane.y), 1f, 1f);
		Vector4 vector = clipPlane * (2f / Vector4.Dot(clipPlane, b));
		projection[2] = vector.x - projection[3];
		projection[6] = vector.y - projection[7];
		projection[10] = vector.z - projection[11];
		projection[14] = vector.w - projection[15];
		return projection;
	}

	private static Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMat, Vector4 plane)
	{
		reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
		reflectionMat.m01 = -2f * plane[0] * plane[1];
		reflectionMat.m02 = -2f * plane[0] * plane[2];
		reflectionMat.m03 = -2f * plane[3] * plane[0];
		reflectionMat.m10 = -2f * plane[1] * plane[0];
		reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
		reflectionMat.m12 = -2f * plane[1] * plane[2];
		reflectionMat.m13 = -2f * plane[3] * plane[1];
		reflectionMat.m20 = -2f * plane[2] * plane[0];
		reflectionMat.m21 = -2f * plane[2] * plane[1];
		reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
		reflectionMat.m23 = -2f * plane[3] * plane[2];
		reflectionMat.m30 = 0f;
		reflectionMat.m31 = 0f;
		reflectionMat.m32 = 0f;
		reflectionMat.m33 = 1f;
		return reflectionMat;
	}

	private static float sgn(float a)
	{
		if (a > 0f)
		{
			return 1f;
		}
		if (a < 0f)
		{
			return -1f;
		}
		return 0f;
	}

	private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
	{
		Vector3 point = pos + normal * clipPlaneOffset;
		Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
		Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
		Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
		return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
	}
}
