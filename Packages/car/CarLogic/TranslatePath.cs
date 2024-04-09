using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarLogic
{
	[AddComponentMenu("CarLogic/Translate Path")]
	public class TranslatePath : MonoBehaviour
	{
		public float TotalLength;

		[HideInInspector]
		public List<TranslatePoint> PointList;

		public AnimationClip[] QteAnimations;

		private Transform me;

		private List<TranslatePoint> bufList = new List<TranslatePoint>();

		public void Awake()
		{
			me = base.transform;
			CheckPointlist();
			ResetBuf();
			TotalLength = CalTotalLength(PointList);
			setWeights();
			setNormals();
		}

		public void CheckPointlist()
		{
			if (PointList == null)
			{
				PointList = new List<TranslatePoint>();
			}
			if (PointList.Count < 1)
			{
				for (int i = 0; i < 5; i++)
				{
					TranslatePoint translatePoint = new TranslatePoint();
					translatePoint.Position = base.transform.TransformPoint((float)i * 0.25f * Vector3.forward);
					translatePoint.Rotation = base.transform.rotation;
					PointList.Add(translatePoint);
				}
			}
			else if (PointList.Count < 2)
			{
				for (int j = 0; j < 4; j++)
				{
					TranslatePoint translatePoint2 = new TranslatePoint();
					translatePoint2.Position = base.transform.TransformPoint((float)j * (1f / 3f) * Vector3.forward);
					translatePoint2.Rotation = base.transform.rotation;
					PointList.Add(translatePoint2);
				}
			}
		}

		private void setWeights()
		{
			if (PointList != null && PointList.Count >= 2)
			{
				if (TotalLength == 0f)
				{
					CalTotalLength(PointList);
				}
				float num = TotalLength / (float)(PointList.Count - 1);
				for (int i = 0; i < PointList.Count - 1; i++)
				{
					TranslatePoint translatePoint = PointList[i];
					TranslatePoint b = PointList[i + 1];
					translatePoint.Weight = CalLength(translatePoint, b) / num;
				}
			}
		}

		private void setNormals()
		{
			if (PointList != null)
			{
				Vector3 up = Vector3.up;
				for (int i = 0; i < PointList.Count; i++)
				{
					PointList[i].Normal = PointList[i].Rotation * up;
				}
			}
		}

		public void Interp(float t, TranslatePoint ret)
		{
			if (PointList != null && PointList.Count >= 2)
			{
				if (bufList.Count < PointList.Count + 2)
				{
					ResetBuf();
				}
				int num = bufList.Count - 3;
				int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
				float num3 = t * (float)num - (float)num2;
				if (num2 < 0 || bufList.Count <= num2 + 3)
				{
					Debug.LogError("TranslatePath.Interp illegal currPt, bufList.Count=" + bufList.Count + ", currPt=" + num2);
					return;
				}
				TranslatePoint translatePoint = bufList[num2];
				TranslatePoint translatePoint2 = bufList[num2 + 1];
				TranslatePoint translatePoint3 = bufList[num2 + 2];
				TranslatePoint translatePoint4 = bufList[num2 + 3];
				ret.Position = 0.5f * ((-translatePoint.Position + 3f * translatePoint2.Position - 3f * translatePoint3.Position + translatePoint4.Position) * (num3 * num3 * num3) + (2f * translatePoint.Position - 5f * translatePoint2.Position + 4f * translatePoint3.Position - translatePoint4.Position) * (num3 * num3) + (-translatePoint.Position + translatePoint3.Position) * num3 + 2f * translatePoint2.Position);
				ret.Rotation = Quaternion.Lerp(translatePoint2.Rotation, translatePoint3.Rotation, num3);
				ret.CameraOffset = 0.5f * ((-translatePoint.CameraOffset + 3f * translatePoint2.CameraOffset - 3f * translatePoint3.CameraOffset + translatePoint4.CameraOffset) * (num3 * num3 * num3) + (2f * translatePoint.CameraOffset - 5f * translatePoint2.CameraOffset + 4f * translatePoint3.CameraOffset - translatePoint4.CameraOffset) * (num3 * num3) + (-translatePoint.CameraOffset + translatePoint3.CameraOffset) * num3 + 2f * translatePoint2.CameraOffset);
			}
		}

		public int GetLerpAIndex(float t)
		{
			if (PointList == null || PointList.Count < 2)
			{
				return -1;
			}
			if (bufList.Count < PointList.Count + 2)
			{
				ResetBuf();
			}
			int num = bufList.Count - 3;
			return Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
		}

		public void ResetBuf()
		{
			BuildBuf(PointList, bufList);
		}

		public static float CalLength(TranslatePoint a, TranslatePoint b)
		{
			if (a == null || b == null)
			{
				return 0f;
			}
			Vector3[] path = new Vector3[2] { a.Position, b.Position };
			return PathLength(path);
		}

		public static float CalTotalLength(List<TranslatePoint> list)
		{
			if (list == null)
			{
				return 0f;
			}
			Vector3[] array = new Vector3[list.Count];
			for (int i = 0; i < array.Length; i++)
			{
				if (list[i] != null)
				{
					array[i] = list[i].Position;
				}
			}
			return PathLength(array);
		}

		public static Vector3 Interp(Vector3[] pts, float t)
		{
			int num = pts.Length - 3;
			int num2 = Mathf.Min(Mathf.FloorToInt(t * (float)num), num - 1);
			float num3 = t * (float)num - (float)num2;
			Vector3 vector = pts[num2];
			Vector3 vector2 = pts[num2 + 1];
			Vector3 vector3 = pts[num2 + 2];
			Vector3 vector4 = pts[num2 + 3];
			return 0.5f * ((-vector + 3f * vector2 - 3f * vector3 + vector4) * (num3 * num3 * num3) + (2f * vector - 5f * vector2 + 4f * vector3 - vector4) * (num3 * num3) + (-vector + vector3) * num3 + 2f * vector2);
		}

		public static void BuildBuf(List<TranslatePoint> src, List<TranslatePoint> buf)
		{
			buf.Clear();
			buf.Add(new TranslatePoint());
			buf.AddRange(src);
			buf.Add(new TranslatePoint());
			buf[0].Position = buf[1].Position + (buf[1].Position - buf[2].Position);
			buf[0].CameraOffset = buf[1].CameraOffset + (buf[1].CameraOffset - buf[2].CameraOffset);
			buf[0].Rotation = buf[1].Rotation;
			buf[buf.Count - 1].Position = buf[buf.Count - 2].Position + (buf[buf.Count - 2].Position - buf[buf.Count - 3].Position);
			buf[buf.Count - 1].CameraOffset = buf[buf.Count - 2].CameraOffset + (buf[buf.Count - 2].CameraOffset - buf[buf.Count - 3].CameraOffset);
			buf[buf.Count - 1].Rotation = buf[buf.Count - 2].Rotation;
			if (buf[1].Position == buf[buf.Count - 2].Position)
			{
				List<TranslatePoint> list = new List<TranslatePoint>(buf.Count);
				list.AddRange(buf);
				list[0].Position = list[list.Count - 3].Position;
				list[0].CameraOffset = list[list.Count - 3].CameraOffset;
				list[list.Count - 1].Position = list[2].Position;
				list[list.Count - 1].CameraOffset = list[2].CameraOffset;
				buf.Clear();
				buf.AddRange(list);
			}
		}

		public static Vector3[] PathControlPointGenerator(Vector3[] path)
		{
			int num = 2;
			Vector3[] array = new Vector3[path.Length + num];
			Array.Copy(path, 0, array, 1, path.Length);
			array[0] = array[1] + (array[1] - array[2]);
			array[array.Length - 1] = array[array.Length - 2] + (array[array.Length - 2] - array[array.Length - 3]);
			if (array[1] == array[array.Length - 2])
			{
				Vector3[] array2 = new Vector3[array.Length];
				Array.Copy(array, array2, array.Length);
				array2[0] = array2[array2.Length - 3];
				array2[array2.Length - 1] = array2[2];
				array = new Vector3[array2.Length];
				Array.Copy(array2, array, array2.Length);
			}
			return array;
		}

		public static float PathLength(Vector3[] path)
		{
			float num = 0f;
			Vector3[] pts = PathControlPointGenerator(path);
			Vector3 a = Interp(pts, 0f);
			int num2 = path.Length * 20;
			for (int i = 1; i <= num2; i++)
			{
				float t = (float)i / (float)num2;
				Vector3 vector = Interp(pts, t);
				num += Vector3.Distance(a, vector);
				a = vector;
			}
			return num;
		}
	}
}
