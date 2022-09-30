using System;
using UnityEngine;

namespace ProBuilder2.Common
{
	[Serializable]
	public class pb_UnwrapParameters
	{
		[Tooltip("Angle between neighbor triangles that will generate seam.")]
		[Range(1f, 180f)]
		public float hardAngle = 88f;

		[Tooltip("Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.")]
		[Range(1f, 64f)]
		public float packMargin = 4f;

		[Tooltip("Measured in percents. Angle error measures deviation of UV angles from geometry angles. Area error measures deviation of UV triangles area from geometry triangles if they were uniformly scaled.")]
		[Range(1f, 75f)]
		public float angleError = 8f;

		[Tooltip("Does... something.")]
		[Range(1f, 75f)]
		public float areaError = 15f;

		public void Reset()
		{
			hardAngle = 88f;
			packMargin = 4f;
			angleError = 8f;
			areaError = 15f;
		}
	}
}
