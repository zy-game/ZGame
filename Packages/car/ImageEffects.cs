using System;
using UnityEngine;

[AddComponentMenu("")]
public class ImageEffects
{
	private static Material[] m_BlitMaterials = new Material[6];

	public static Material GetBlitMaterial(CarImageEffectBlendMode mode)
	{
		if (m_BlitMaterials[(int)mode] != null)
		{
			return m_BlitMaterials[(int)mode];
		}
		m_BlitMaterials[0] = new Material("Shader \"BlitCopy\" {\n\tSubShader { Pass {\n \t\tZTest Always Cull Off ZWrite Off Fog { Mode Off }\n\t\tSetTexture [__RenderTex] { combine texture}\t}}\nFallback Off }");
		m_BlitMaterials[1] = new Material("Shader \"BlitMultiply\" {\n\tSubShader { Pass {\n\t\tBlend DstColor Zero\n \t\tZTest Always Cull Off ZWrite Off Fog { Mode Off }\n\t\tSetTexture [__RenderTex] { combine texture }\t}}\nFallback Off }");
		m_BlitMaterials[2] = new Material("Shader \"BlitMultiplyDouble\" {\n\tSubShader { Pass {\n\t\tBlend DstColor SrcColor\n \t\tZTest Always Cull Off ZWrite Off Fog { Mode Off }\n\t\tSetTexture [__RenderTex] { combine texture }\t}}\nFallback Off }");
		m_BlitMaterials[3] = new Material("Shader \"BlitAdd\" {\n\tSubShader { Pass {\n\t\tBlend One One\n \t\tZTest Always Cull Off ZWrite Off Fog { Mode Off }\n\t\tSetTexture [__RenderTex] { combine texture }\t}}\nFallback Off }");
		m_BlitMaterials[4] = new Material("Shader \"BlitAddSmooth\" {\n\tSubShader { Pass {\n\t\tBlend OneMinusDstColor One\n \t\tZTest Always Cull Off ZWrite Off Fog { Mode Off }\n\t\tSetTexture [__RenderTex] { combine texture }\t}}\nFallback Off }");
		m_BlitMaterials[5] = new Material("Shader \"BlitBlend\" {\n\tSubShader { Pass {\n\t\tBlend SrcAlpha OneMinusSrcAlpha\n \t\tZTest Always Cull Off ZWrite Off Fog { Mode Off }\n\t\tSetTexture [__RenderTex] { combine texture }\t}}\nFallback Off }");
		for (int i = 0; i < m_BlitMaterials.Length; i++)
		{
			m_BlitMaterials[i].hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable;
			m_BlitMaterials[i].shader.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor | HideFlags.NotEditable;
		}
		return m_BlitMaterials[(int)mode];
	}

	public static void Blit(RenderTexture source, RenderTexture dest, CarImageEffectBlendMode blendMode)
	{
		Blit(source, new Rect(0f, 0f, 1f, 1f), dest, new Rect(0f, 0f, 1f, 1f), blendMode);
	}

	public static void Blit(RenderTexture source, RenderTexture dest)
	{
		Blit(source, dest, CarImageEffectBlendMode.Copy);
	}

	public static void Blit(RenderTexture source, Rect sourceRect, RenderTexture dest, Rect destRect, CarImageEffectBlendMode blendMode)
	{
		RenderTexture.active = dest;
		source.SetGlobalShaderProperty("__RenderTex");
		bool invertY = source.texelSize.y < 0f;
		GL.PushMatrix();
		GL.LoadOrtho();
		Material blitMaterial = GetBlitMaterial(blendMode);
		for (int i = 0; i < blitMaterial.passCount; i++)
		{
			blitMaterial.SetPass(i);
			DrawQuad(invertY);
		}
		GL.PopMatrix();
	}

	public static void BlitWithMaterial(Material material, RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, material);
	}

	public static void RenderDistortion(Material material, RenderTexture source, RenderTexture destination, float angle, Vector2 center, Vector2 radius)
	{
		if (source.texelSize.y < 0f)
		{
			center.y = 1f - center.y;
			angle = 0f - angle;
		}
		Matrix4x4 value = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one);
		material.SetMatrix("_RotationMatrix", value);
		material.SetVector("_CenterRadius", new Vector4(center.x, center.y, radius.x, radius.y));
		material.SetFloat("_Angle", angle * ((float)Math.PI / 180f));
		Graphics.Blit(source, destination, material);
	}

	public static void DrawQuad(bool invertY)
	{
		GL.Begin(7);
		float y;
		float y2;
		if (invertY)
		{
			y = 1f;
			y2 = 0f;
		}
		else
		{
			y = 0f;
			y2 = 1f;
		}
		GL.TexCoord2(0f, y);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, y);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(1f, y2);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.TexCoord2(0f, y2);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.End();
	}

	public static void DrawGrid(int xSize, int ySize)
	{
		GL.Begin(7);
		float num = 1f / (float)xSize;
		float num2 = 1f / (float)ySize;
		for (int i = 0; i < xSize; i++)
		{
			for (int j = 0; j < ySize; j++)
			{
				GL.TexCoord2((float)j * num, (float)i * num2);
				GL.Vertex3((float)j * num, (float)i * num2, 0.1f);
				GL.TexCoord2((float)(j + 1) * num, (float)i * num2);
				GL.Vertex3((float)(j + 1) * num, (float)i * num2, 0.1f);
				GL.TexCoord2((float)(j + 1) * num, (float)(i + 1) * num2);
				GL.Vertex3((float)(j + 1) * num, (float)(i + 1) * num2, 0.1f);
				GL.TexCoord2((float)j * num, (float)(i + 1) * num2);
				GL.Vertex3((float)j * num, (float)(i + 1) * num2, 0.1f);
			}
		}
		GL.End();
	}
}
