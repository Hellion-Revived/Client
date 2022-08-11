using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Playdead/FrustumJitter")]
public class FrustumJitter : MonoBehaviour
{
	public enum Pattern
	{
		Still = 0,
		Uniform2 = 1,
		Uniform4 = 2,
		Uniform4_Helix = 3,
		Uniform4_DoubleHelix = 4,
		SkewButterfly = 5,
		Rotated4 = 6,
		Rotated4_Helix = 7,
		Rotated4_Helix2 = 8,
		Poisson10 = 9,
		Pentagram = 10,
		Halton_2_3_X8 = 11,
		Halton_2_3_X16 = 12,
		Halton_2_3_X32 = 13,
		Halton_2_3_X256 = 14,
		MotionPerp2 = 15
	}

	private static float[] points_Still;

	private static float[] points_Uniform2;

	private static float[] points_Uniform4;

	private static float[] points_Uniform4_Helix;

	private static float[] points_Uniform4_DoubleHelix;

	private static float[] points_SkewButterfly;

	private static float[] points_Rotated4;

	private static float[] points_Rotated4_Helix;

	private static float[] points_Rotated4_Helix2;

	private static float[] points_Poisson10;

	private static float[] points_Pentagram;

	private static float[] points_Halton_2_3_x8;

	private static float[] points_Halton_2_3_x16;

	private static float[] points_Halton_2_3_x32;

	private static float[] points_Halton_2_3_x256;

	private static float[] points_MotionPerp2;

	private static bool _initialized;

	private Vector3 focalMotionPos = Vector3.zero;

	private Vector3 focalMotionDir = Vector3.right;

	public Pattern pattern = Pattern.Halton_2_3_X16;

	public float patternScale = 1f;

	public Vector4 activeSample = Vector4.zero;

	public int activeIndex = -1;

	static FrustumJitter()
	{
		points_Still = new float[2] { 0.5f, 0.5f };
		points_Uniform2 = new float[4] { -0.25f, -0.25f, 0.25f, 0.25f };
		points_Uniform4 = new float[8] { -0.25f, -0.25f, 0.25f, -0.25f, 0.25f, 0.25f, -0.25f, 0.25f };
		points_Uniform4_Helix = new float[8] { -0.25f, -0.25f, 0.25f, 0.25f, 0.25f, -0.25f, -0.25f, 0.25f };
		points_Uniform4_DoubleHelix = new float[16]
		{
			-0.25f, -0.25f, 0.25f, 0.25f, 0.25f, -0.25f, -0.25f, 0.25f, -0.25f, -0.25f,
			0.25f, -0.25f, -0.25f, 0.25f, 0.25f, 0.25f
		};
		points_SkewButterfly = new float[8] { -0.25f, -0.25f, 0.25f, 0.25f, 0.125f, -0.125f, -0.125f, 0.125f };
		points_Rotated4 = new float[8] { -0.125f, -0.375f, 0.375f, -0.125f, 0.125f, 0.375f, -0.375f, 0.125f };
		points_Rotated4_Helix = new float[8] { -0.125f, -0.375f, 0.125f, 0.375f, 0.375f, -0.125f, -0.375f, 0.125f };
		points_Rotated4_Helix2 = new float[8] { -0.125f, -0.375f, 0.125f, 0.375f, -0.375f, 0.125f, 0.375f, -0.125f };
		points_Poisson10 = new float[20]
		{
			-0.0419899f, 0.16386227f, -0.17274007f, 0.14753993f, 0.12460955f, 0.2077493f, 0.043075375f, -0.009706758f, -0.15193167f, -0.015033968f,
			0.16401598f, 0.060019f, 0.20087093f, -0.12024225f, 0.08359135f, -0.18251757f, -0.1195988f, -0.14001325f, -0.0309703f, -0.24158497f
		};
		points_Pentagram = new float[10] { 0f, 0.2628655f, -0.1545085f, -0.2126625f, 0.25f, 0.08123f, -0.25f, 0.08123f, 0.1545085f, -0.2126625f };
		points_Halton_2_3_x8 = new float[16];
		points_Halton_2_3_x16 = new float[32];
		points_Halton_2_3_x32 = new float[64];
		points_Halton_2_3_x256 = new float[512];
		points_MotionPerp2 = new float[4] { 0f, -0.25f, 0f, 0.25f };
		_initialized = false;
		if (!_initialized)
		{
			_initialized = true;
			TransformPattern(theta: (float)Math.PI / 180f * (0.5f * Vector2.Angle(to: new Vector2(points_Pentagram[0] - points_Pentagram[2], points_Pentagram[1] - points_Pentagram[3]), from: new Vector2(0f, 1f))), seq: points_Pentagram, scale: 1f);
			InitializeHalton_2_3(points_Halton_2_3_x8);
			InitializeHalton_2_3(points_Halton_2_3_x16);
			InitializeHalton_2_3(points_Halton_2_3_x32);
			InitializeHalton_2_3(points_Halton_2_3_x256);
		}
	}

	private static void TransformPattern(float[] seq, float theta, float scale)
	{
		float num = Mathf.Cos(theta);
		float num2 = Mathf.Sin(theta);
		int num3 = 0;
		int num4 = 1;
		int num5 = seq.Length;
		while (num3 != num5)
		{
			float num6 = scale * seq[num3];
			float num7 = scale * seq[num4];
			seq[num3] = num6 * num - num7 * num2;
			seq[num4] = num6 * num2 + num7 * num;
			num3 += 2;
			num4 += 2;
		}
	}

	private static float HaltonSeq(int prime, int index = 1)
	{
		float num = 0f;
		float num2 = 1f;
		for (int num3 = index; num3 > 0; num3 = (int)Mathf.Floor((float)num3 / (float)prime))
		{
			num2 /= (float)prime;
			num += num2 * (float)(num3 % prime);
		}
		return num;
	}

	private static void InitializeHalton_2_3(float[] seq)
	{
		int i = 0;
		for (int num = seq.Length / 2; i != num; i++)
		{
			float num2 = HaltonSeq(2, i + 1) - 0.5f;
			float num3 = HaltonSeq(3, i + 1) - 0.5f;
			seq[2 * i] = num2;
			seq[2 * i + 1] = num3;
		}
	}

	private static float[] AccessPointData(Pattern pattern)
	{
		switch (pattern)
		{
		case Pattern.Still:
			return points_Still;
		case Pattern.Uniform2:
			return points_Uniform2;
		case Pattern.Uniform4:
			return points_Uniform4;
		case Pattern.Uniform4_Helix:
			return points_Uniform4_Helix;
		case Pattern.Uniform4_DoubleHelix:
			return points_Uniform4_DoubleHelix;
		case Pattern.SkewButterfly:
			return points_SkewButterfly;
		case Pattern.Rotated4:
			return points_Rotated4;
		case Pattern.Rotated4_Helix:
			return points_Rotated4_Helix;
		case Pattern.Rotated4_Helix2:
			return points_Rotated4_Helix2;
		case Pattern.Poisson10:
			return points_Poisson10;
		case Pattern.Pentagram:
			return points_Pentagram;
		case Pattern.Halton_2_3_X8:
			return points_Halton_2_3_x8;
		case Pattern.Halton_2_3_X16:
			return points_Halton_2_3_x16;
		case Pattern.Halton_2_3_X32:
			return points_Halton_2_3_x32;
		case Pattern.Halton_2_3_X256:
			return points_Halton_2_3_x256;
		case Pattern.MotionPerp2:
			return points_MotionPerp2;
		default:
			Debug.LogError("missing point distribution");
			return points_Halton_2_3_x16;
		}
	}

	public static int AccessLength(Pattern pattern)
	{
		return AccessPointData(pattern).Length / 2;
	}

	public Vector2 Sample(Pattern pattern, int index)
	{
		float[] array = AccessPointData(pattern);
		int num = array.Length / 2;
		int num2 = index % num;
		float x = patternScale * array[2 * num2];
		float y = patternScale * array[2 * num2 + 1];
		if (pattern != Pattern.MotionPerp2)
		{
			return new Vector2(x, y);
		}
		return new Vector2(x, y).Rotate(Vector2.right.SignedAngle(focalMotionDir));
	}

	private void OnPreCull()
	{
		Camera component = GetComponent<Camera>();
		if (component != null && !component.orthographic)
		{
			Vector3 vector = focalMotionPos;
			Vector3 vector2 = component.transform.TransformVector(component.nearClipPlane * Vector3.forward);
			Vector3 vector3 = component.worldToCameraMatrix * vector;
			Vector3 vector4 = component.worldToCameraMatrix * vector2;
			Vector3 vector5 = (vector4 - vector3).WithZ(0f);
			float magnitude = vector5.magnitude;
			if (magnitude != 0f)
			{
				Vector3 b = vector5 / magnitude;
				if (b.sqrMagnitude != 0f)
				{
					focalMotionPos = vector2;
					focalMotionDir = Vector3.Slerp(focalMotionDir, b, 0.2f);
				}
			}
			activeIndex++;
			activeIndex %= AccessLength(pattern);
			Vector2 vector6 = Sample(pattern, activeIndex);
			activeSample.z = activeSample.x;
			activeSample.w = activeSample.y;
			activeSample.x = vector6.x;
			activeSample.y = vector6.y;
			component.projectionMatrix = component.GetPerspectiveProjection(vector6.x, vector6.y);
		}
		else
		{
			activeSample = Vector4.zero;
			activeIndex = -1;
		}
	}

	private void OnDisable()
	{
		Camera component = GetComponent<Camera>();
		if (component != null)
		{
			component.ResetProjectionMatrix();
		}
		activeSample = Vector4.zero;
		activeIndex = -1;
	}
}
