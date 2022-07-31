using System;
using UnityEngine;

public static class CameraExtension
{
	public static Matrix4x4 GetPerspectiveProjection(this Camera camera)
	{
		return camera.GetPerspectiveProjection(0f, 0f);
	}

	public static Matrix4x4 GetPerspectiveProjection(this Camera camera, float texelOffsetX, float texelOffsetY)
	{
		if (camera == null)
		{
			return Matrix4x4.identity;
		}
		float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView);
		float num2 = num * camera.aspect;
		float num3 = num2 / (0.5f * (float)camera.pixelWidth);
		float num4 = num / (0.5f * (float)camera.pixelHeight);
		float num5 = num3 * texelOffsetX;
		float num6 = num4 * texelOffsetY;
		float farClipPlane = camera.farClipPlane;
		float nearClipPlane = camera.nearClipPlane;
		float left = (num5 - num2) * nearClipPlane;
		float right = (num5 + num2) * nearClipPlane;
		float bottom = (num6 - num) * nearClipPlane;
		float top = (num6 + num) * nearClipPlane;
		return Matrix4x4Extension.GetPerspectiveProjection(left, right, bottom, top, nearClipPlane, farClipPlane);
	}

	public static Vector4 GetPerspectiveProjectionCornerRay(this Camera camera)
	{
		return camera.GetPerspectiveProjectionCornerRay(0f, 0f);
	}

	public static Vector4 GetPerspectiveProjectionCornerRay(this Camera camera, float texelOffsetX, float texelOffsetY)
	{
		if (camera == null)
		{
			return Vector4.zero;
		}
		float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView);
		float num2 = num * camera.aspect;
		float num3 = num2 / (0.5f * (float)camera.pixelWidth);
		float num4 = num / (0.5f * (float)camera.pixelHeight);
		float z = num3 * texelOffsetX;
		float w = num4 * texelOffsetY;
		return new Vector4(num2, num, z, w);
	}
}
