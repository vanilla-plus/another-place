using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Vanilla.Math {
	
	public enum InterpolationType
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInAndOut,
		Bounce
	}
	
	public class VanillaMath {
		/// ----------------------------------------------------------------------------------------------------------/
		/// Float Clamps
		/// ----------------------------------------------------------------------------------------------------------/

		public static (float, bool) ClampTest(float input, float max)
		{
			return (Mathf.Clamp(input, 0.0f, max), WithinRange(input, max));
		}
		
		public static (float, bool) ClampTest(float input, float min, float max)
		{
			return (Mathf.Clamp(input, min, max), WithinRange(input, min, max));
		}
		
		/// ----------------------------------------------------------------------------------------------------------/
		/// Float Wraps
		/// ----------------------------------------------------------------------------------------------------------/

		public static float Wrap(float input, float max) 
		{
			return (Mathf.Clamp(input - Mathf.Floor(input / max) * max, 0.0f, max));
		}
		
		public static (float, bool) WrapTest(float input, float max)
		{
			return (Mathf.Clamp(input - Mathf.Floor(input / max) * max, 0.0f, max), WithinRange(input, max));
		}
		
		public static float WrapRange(float input, float min, float max) 
		{
			return (Mathf.Clamp(input - Mathf.Floor(input / max) * max, min, max));
		}
		
		public static (float, bool) WrapRangeTest(float input, float min, float max)
		{
			return (Mathf.Clamp(input - Mathf.Floor(input / max) * max, min, max), WithinRange(input, min, max));
		}
		
		/// ----------------------------------------------------------------------------------------------------------/
		/// Int Clamps
		/// ----------------------------------------------------------------------------------------------------------/

		public static (int, bool) ClampTest(int input, int max)
		{
			return (Mathf.Clamp(input, 0, max), WithinRange(input, max));
		}
		
		public static (int, bool) ClampTest(int input, int min, int max)
		{
			return (Mathf.Clamp(input, min, max), WithinRange(input, min, max));
		}
		
		/// ----------------------------------------------------------------------------------------------------------/
		/// Int Wraps
		/// ----------------------------------------------------------------------------------------------------------/
		
		public static int Wrap(int input, int max) 
		{
			return Mathf.Clamp(input - input / max * max, 0, max);
		}
		
		public static (int, bool) WrapTest(int input, int max)
		{
			return (Mathf.Clamp(input - input / max * max, 0, max), WithinRange(input, max));
		}
		
		// The old method. Does it have any advantages?
		public static int WrapOld(int input, int max) 
		{
			int output = input - input / max * max;

			return output < 0 ? output + max : output;
		}
		
		public static int WrapRange(int input, int min, int max) 
		{
			return Mathf.Clamp(input - input / max * max, min, max);
		}

		public static (int, bool) WrapRangeTest(int input, int min, int max)
		{
			return (Mathf.Clamp(input - input / max * max, min, max), WithinRange(input, min, max));
		}
		
		// The old method. Does it have any advantages?
		public static int WrapRangeOld(int input, int min, int max)
		{
			int e = max - min;

			int output = input - input / e * e;

			return (output < 0 ? output + e : output) + min;
		}

		/// ----------------------------------------------------------------------------------------------------------/
		/// Range Tests
		/// ----------------------------------------------------------------------------------------------------------/

		public static bool WithinRange(float input, float max)
		{
			return input < 0.0f || input > max;
		} 
		
		public static bool WithinRange(float input, float min, float max)
		{
			return input < min || input > max;
		}
		
		public static bool WithinRange(int input, int max)
		{
			return input < 0 || input > max;
		} 
		
		public static bool WithinRange(int input, int min, int max)
		{
			return input < min || input > max;
		}
		
		/// <summary>
		/// Returns true if input, divided by division, leaves no remainder. For example, input of 10 and divider of 3 would have a remainder of 1 and thus return false. An input of 10 and divider of 5 would return true.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="division"></param>
		/// <returns></returns>
		public static bool IsDivisableBy(float input, float divider) {
			return (input % divider == 0);
		}

		/// <summary>
		/// Returns true if the input value is odd. However, this function is currently untested. It also only works for an int, so consider rounding off if you need to check a float.
		/// </summary>
		public static bool ValueIsOdd(int input) {
			return (input & 1) == 1;
		}

		/// <summary>
		/// Returns true if the input value is even. However, this function is currently untested. It also only works for an int, so consider rounding off if you need to check a float.
		/// </summary>
		public static bool ValueIsEven(int input) {
			return (input & 1) == 0;
		}

//		// Tested - Works perfectly backwards and forwards
//		public static int WrapOld(int input, int length) {
//			int output = input - (input / length) * length;
//
//			return output < 0 ? output + length : output;
//		}
//
//		// Tested - Works perfectly backwards and forwards - but is it quicker to cache e or not?
//		public static int IntRepeatRange(int input, int min, int max) {
//			int e = max - min;
//
//			int output = input - (input / e) * e;
//
//			return (output < 0 ? output + e : output) + min;
//		}

		#region Float Interpolation

		/// <summary>
		/// Pass in a normalized float and an interpolation type and you'll receive the corresponding modification.
		/// </summary>
		/// <param name="i">A normalized float (between 0-1). This would ideally be iterated over time in a coroutine.</param>
		/// <param name="interpolation">What kind of interpolation do we want to apply to i?</param>
		/// <returns></returns>
		public static float Interpolate(float i, InterpolationType interpolation) {
			switch (interpolation) {
				case InterpolationType.Linear:
					return i;
				case InterpolationType.EaseIn:
					return Coserp(i);
				case InterpolationType.EaseOut:
					return Sinerp(i);
				case InterpolationType.EaseInAndOut:
					return Hermite(i);
				case InterpolationType.Bounce:
					return Bounce(i);
				default:
					return i;
			}
		}

		public static float SharpHermite(float i)
		{
			return i * i * i * (i * (6f * i - 15f) + 10f);
		}
		
		/// <summary>
		/// An ease-in-and-out formula for a normalized float. Modulate your standard 'i' time normal with this to get a smooth ease-in-and-out effect.
		/// </summary>
		public static float Hermite(float i) {
			return i * i * (3.0f - 2.0f * i);
		}

		/// <summary>
		/// An ease-out formula for a normalized float. Modulate your standard 'i' time normal with this to get a smooth ease-out effect.
		/// </summary>
		public static float Sinerp(float i) {
			return Mathf.Sin(i * Mathf.PI * 0.5f);
		}

		/// <summary>
		/// An ease-in formula for a normalized float. Modulate your standard 'i' time normal with this to get a smooth ease-in effect.
		/// </summary>
		public static float Coserp(float i) {
			return 1.0f - Mathf.Cos(i * Mathf.PI * 0.5f);
		}

		public static float Wobble(float start, float end, float i) {
			i = (Mathf.Sin(i * Mathf.PI * (0.2f + 2.5f * i * i * i)) * Mathf.Pow(1f - i, 2.2f) + i) * (1f + (1.2f * (1f - i)));
			return start + (end - start) * i;
		}

		public static Vector2 Wobble(Vector2 start, Vector2 end, float value) {
			return new Vector2(Berp(start.x, end.x, value), Berp(start.y, end.y, value));
		}

		public static Vector3 Wobble(Vector3 start, Vector3 end, float value) {
			return new Vector3(Berp(start.x, end.x, value), Berp(start.y, end.y, value), Berp(start.z, end.z, value));
		}

		//Ease in out
		public static float Hermite(float start, float end, float value) {
			return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
		}

		#endregion

		public static Vector2 Hermite(Vector2 start, Vector2 end, float value) {
			return new Vector2(Hermite(start.x, end.x, value), Hermite(start.y, end.y, value));
		}

		public static Vector3 Hermite(Vector3 start, Vector3 end, float value) {
			return new Vector3(Hermite(start.x, end.x, value), Hermite(start.y, end.y, value), Hermite(start.z, end.z, value));
		}

		//Ease out
		public static float Sinerp(float start, float end, float value) {
			return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * 0.5f));
		}

		public static Vector2 Sinerp(Vector2 start, Vector2 end, float value) {
			return new Vector2(Mathf.Lerp(start.x, end.x, Mathf.Sin(value * Mathf.PI * 0.5f)), Mathf.Lerp(start.y, end.y, Mathf.Sin(value * Mathf.PI * 0.5f)));
		}

		public static Vector3 Sinerp(Vector3 start, Vector3 end, float value) {
			return new Vector3(Mathf.Lerp(start.x, end.x, Mathf.Sin(value * Mathf.PI * 0.5f)), Mathf.Lerp(start.y, end.y, Mathf.Sin(value * Mathf.PI * 0.5f)), Mathf.Lerp(start.z, end.z, Mathf.Sin(value * Mathf.PI * 0.5f)));
		}
		//Ease in
		public static float Coserp(float start, float end, float value) {
			return Mathf.Lerp(start, end, 1.0f - Mathf.Cos(value * Mathf.PI * 0.5f));
		}

		public static Vector2 Coserp(Vector2 start, Vector2 end, float value) {
			return new Vector2(Coserp(start.x, end.x, value), Coserp(start.y, end.y, value));
		}

		public static Vector3 Coserp(Vector3 start, Vector3 end, float value) {
			return new Vector3(Coserp(start.x, end.x, value), Coserp(start.y, end.y, value), Coserp(start.z, end.z, value));
		}

		public static float Berp(float start, float end, float value) {
			value = Mathf.Clamp01(value);
			value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}

		public static Vector2 Berp(Vector2 start, Vector2 end, float value) {
			return new Vector2(Berp(start.x, end.x, value), Berp(start.y, end.y, value));
		}

		public static Vector3 Berp(Vector3 start, Vector3 end, float value) {
			return new Vector3(Berp(start.x, end.x, value), Berp(start.y, end.y, value), Berp(start.z, end.z, value));
		}

		//Like lerp with ease in ease out
		public static float SmoothStep(float x, float min, float max) {
			x = Mathf.Clamp(x, min, max);
			float v1 = (x - min) / (max - min);
			float v2 = (x - min) / (max - min);
			return -2 * v1 * v1 * v1 + 3 * v2 * v2;
		}

		public static float SinWave() {
			return Mathf.Sin(Time.time);
		}

		public static float SinWave(float rate) {
			return Mathf.Sin(Time.time * rate);
		}

		public static Vector2 SmoothStep(Vector2 vec, float min, float max) {
			return new Vector2(SmoothStep(vec.x, min, max), SmoothStep(vec.y, min, max));
		}

		public static Vector3 SmoothStep(Vector3 vec, float min, float max) {
			return new Vector3(SmoothStep(vec.x, min, max), SmoothStep(vec.y, min, max), SmoothStep(vec.z, min, max));
		}

		public static float Lerp(float start, float end, float value) {
			return ((1.0f - value) * start) + (value * end);
		}

		public static Vector3 NearestPoint(Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			Vector3 lineDirection = Vector3.Normalize(lineEnd - lineStart);
			float closestPoint = Vector3.Dot((point - lineStart), lineDirection);
			return lineStart + (closestPoint * lineDirection);
		}

		public static Vector3 NearestPointStrict(Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
			Vector3 fullDirection = lineEnd - lineStart;
			Vector3 lineDirection = Vector3.Normalize(fullDirection);
			float closestPoint = Vector3.Dot((point - lineStart), lineDirection);
			return lineStart + (Mathf.Clamp(closestPoint, 0.0f, Vector3.Magnitude(fullDirection)) * lineDirection);
		}

		//Bounce
		public static float Bounce(float x) {
			return Mathf.Abs(Mathf.Sin(6.28f * (x + 1f) * (x + 1f)) * (1f - x));
		}

		public static Vector2 Bounce(Vector2 vec) {
			return new Vector2(Bounce(vec.x), Bounce(vec.y));
		}

		public static Vector3 Bounce(Vector3 vec) {
			return new Vector3(Bounce(vec.x), Bounce(vec.y), Bounce(vec.z));
		}

		// test for value that is near specified float (due to floating point inprecision)
		// all thanks to Opless for this!
		public static bool Approx(float val, float about, float range) {
			return ((Mathf.Abs(val - about) < range));
		}

		// test if a Vector3 is close to another Vector3 (due to floating point inprecision)
		// compares the square of the distance to the square of the range as this 
		// avoids calculating a square root which is much slower than squaring the range
		public static bool Approx(Vector3 val, Vector3 about, float range) {
			return ((val - about).sqrMagnitude < range * range);
		}

		/*
		  * CLerp - Circular Lerp - is like lerp but handles the wraparound from 0 to 360.
		  * This is useful when interpolating eulerAngles and the object
		  * crosses the 0/360 boundary.  The standard Lerp function causes the object
		  * to rotate in the wrong direction and looks stupid. Clerp fixes that.
		  */
		public static float Clerp(float start, float end, float value) {
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) / 2.0f);//half the distance between min and max
			float retval = 0.0f;
			float diff = 0.0f;

			if ((end - start) < -half) {
				diff = ((max - start) + end) * value;
				retval = start + diff;
			} else if ((end - start) > half) {
				diff = -((max - end) + start) * value;
				retval = start + diff;
			} else
				retval = start + (end - start) * value;

			// Debug.Log("Start: "  + start + "   End: " + end + "  Value: " + value + "  Half: " + half + "  Diff: " + diff + "  Retval: " + retval);
			return retval;
		}
		
		public static float Vector2ToRotation(float x, float y) {
			return Mathf.Atan2(x, y) * Mathf.Rad2Deg;
		}
		
		public static float Vector2ToRotation(Vector2 input) {
			return Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
		}

		/*--------------------------------------------------------------------------------------
		//  Ready-to-go routines
		//------------------------------------------------------------------------------------*/

		public static IEnumerator MoveLerp(Transform t, Vector3 start, Vector3 end, float seconds, bool localMode = false) {
			float i = 0.0f;
			float rate = 1.0f / seconds;

			if (localMode) {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.localPosition = Vector3.Lerp(start, end, i);

					yield return null;
				}
			} else {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.position = Vector3.Lerp(start, end, i);

					yield return null;
				}
			}
		}

		// Ease out
		public static IEnumerator MoveSinerp(Transform t, Vector3 start, Vector3 end, float seconds, bool localMode = false) {
			float i = 0.0f;
			float rate = 1.0f / seconds;

			if (localMode) {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.localPosition = Vector3.Lerp(start, end, Mathf.Sin(i * Mathf.PI * 0.5f));

					yield return null;
				}
			} else {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.position = Vector3.Lerp(start, end, Mathf.Sin(i * Mathf.PI * 0.5f));

					yield return null;
				}
			}
		}

		// Ease in
		public static IEnumerator MoveCoserp(Transform t, Vector3 start, Vector3 end, float seconds, bool localMode = false) {
			float i = 0.0f;
			float rate = 1.0f / seconds;

			if (localMode) {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.localPosition = Vector3.Lerp(start, end, 1.0f - Mathf.Cos(i * Mathf.PI * 0.5f));

					yield return null;
				}
			} else {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.position = Vector3.Lerp(start, end, 1.0f - Mathf.Cos(i * Mathf.PI * 0.5f));

					yield return null;
				}
			}

		}

		// Ease in/out
		public static IEnumerator MoveHermite(Transform t, Vector3 start, Vector3 end, float seconds, bool localMode = false) {
			float i = 0.0f;
			float rate = 1.0f / seconds;

			if (localMode) {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.localPosition = Vector3.Lerp(start, end, Hermite(i));

					yield return null;
				}
			} else {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.position = Vector3.Lerp(start, end, Hermite(i));

					yield return null;
				}
			}
		}

		// Bounce
		public static IEnumerator MoveBounce(Transform t, float seconds, float amount, bool localMode = false) {
			float i = 0.0f;
			float rate = 1.0f / seconds;

			if (localMode) {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.localPosition = new Vector3(0, Bounce(i), 0);

					yield return null;
				}
			} else {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					t.position = new Vector3(0, Bounce(i), 0);

					yield return null;
				}
			}
		}

		// Make something bounce away and slow down, like its been hit
		public static IEnumerator BouncingSinerpMove(Transform t, Vector3 start, Vector3 end, float seconds, bool localMode = false) {
			float i = 0.0f;
			float rate = 1.0f / seconds;

			if (localMode) {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					float bX = Mathf.Lerp(start.x, end.x, Sinerp(i));
					float bY = Bounce(i);
					float bZ = Mathf.Lerp(start.z, end.z, Sinerp(i));

					t.localPosition = new Vector3(bX, bY, bZ);

					yield return null;
				}
			} else {
				while (i < 1.0f) {
					i += Time.deltaTime * rate;

					float bX = Mathf.Lerp(start.x, end.x, Sinerp(i));
					float bY = Bounce(i);
					float bZ = Mathf.Lerp(start.z, end.z, Sinerp(i));

					t.position = new Vector3(bX, bY, bZ);

					yield return null;
				}
			}
		}

		/// <summary>
		/// Returns the input value rounded to the nearest of value 'nearest'. Unlike most iterations of this formula, this one allows for negative input/output.
		/// </summary>
		/// <returns></returns>
		public static float RoundToNearest(float input, float nearest) {
			return (Mathf.Sign(input) == -1) ? -(Mathf.Round(Mathf.Abs(input) / nearest) * nearest) : (Mathf.Round(Mathf.Abs(input) / nearest) * nearest);
		}

		public static float Normalize(float input, float maximumValue) {
			return Mathf.Clamp01(input / maximumValue);
		}

		public static int GetFromIntRangeFloor(int min, int max, float normal) {
			return Mathf.FloorToInt(Mathf.Lerp(min, max, normal));
		}

		public static int GetFromIntRangeRound(int min, int max, float normal) {
			return Mathf.RoundToInt(Mathf.Lerp(min, max, normal));
		}

		public static int GetFromIntRangeCeil(int min, int max, float normal) {
			return Mathf.CeilToInt(Mathf.Lerp(min, max, normal));
		}
	}
}

///--------------------------------------------------------------------------------------------------------------------------------------------------
/// Math2D
///--------------------------------------------------------------------------------------------------------------------------------------------------

namespace Vanilla.Math2D {
	public class VanillaMath2D {
		//------------------------------------------------------------------------------------------------------------------
		// Radians
		//------------------------------------------------------------------------------------------------------------------
		// Radians are essentially the normalized length of the circumference of a circle [e.g. the circle line itself].
		// 1 radian is also the length of a circles radius.
		// Pi is 3.1415 radians and is the length of half of a circles circumference. 2 of pi would be the whole circle!
		// Lots of angle-related math use radians.
		//------------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// This will return a vector2 that represents the direction between the two positional vectors provided.
		/// </summary>
		public static Vector2 GetDirection(Vector2 a, Vector2 b) {
			return b - a;
		}

		/// <summary>
		/// This will convert a given vector2 into radians.
		/// </summary>
		/// <param name="a">The direction vector. I have yet to test if this only works if the vector is normalized.</param>
		public static float DirectionVectorToRadians(Vector2 a) {
			return Mathf.Atan2(a.y, a.x);
		}

		/// <summary>
		/// This will convert a given directional Vector2 into an angle in degrees (0 - 360). This can be fed into the z axis of an euler to get a rotation.
		/// </summary>
		/// <param name="a">The direction vector. I have yet to test if this only works if the vector is normalized.</param>
		public static float DirectionVectorToDegrees(Vector2 a) {
			return Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg;
		}

		/// <summary>
		/// This will convert a given angle in radians to a Vector2 position.
		/// </summary>
		/// <param name="radian">How many radians?</param>
		/// <returns></returns>
		public static Vector2 RadianToVector2(float radian) {
			return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
		}

		/// <summary>
		/// This will convert a given angle in degrees to a Vector2 position.
		/// </summary>
		/// <param name="radian">How many degrees?</param>
		/// <returns></returns>
		public static Vector2 DegreeToVector2(float degree) {
			return RadianToVector2(degree * Mathf.Deg2Rad);
		}
	}
}

///--------------------------------------------------------------------------------------------------------------------------------------------------
/// Math3D
///--------------------------------------------------------------------------------------------------------------------------------------------------

namespace Vanilla.Math3D {
	public static class VanillaMath3D {
		public enum Vector2ToVector3ProjectionFormat {
			XYo,
			XoY,
			YXo,
			YoX,
			oXY,
			oYX,
		}

		public enum Vector3ToVector2ProjectionFormat {
			XY,
			XZ,
			YX,
			YZ,
			ZX,
			ZY
		}

		///--------------------------------------------------------------------------------------
		///  Vector2 <> Vector3 Remapping
		///--------------------------------------------------------------------------------------

		public static Vector3 MapVector2ToVector3(Vector2 input, Vector2ToVector3ProjectionFormat projectionFormat) {
			switch (projectionFormat) {
				case Vector2ToVector3ProjectionFormat.XYo:
					return input;

				case Vector2ToVector3ProjectionFormat.XoY:
					return new Vector3(input.x, 0, input.y);

				case Vector2ToVector3ProjectionFormat.YXo:
					return new Vector3(input.y, input.x, 0);

				case Vector2ToVector3ProjectionFormat.YoX:
					return new Vector3(input.y, 0, input.x);

				case Vector2ToVector3ProjectionFormat.oXY:
					return new Vector3(0, input.x, input.y);

				case Vector2ToVector3ProjectionFormat.oYX:
					return new Vector3(0, input.y, input.x);
				default:
					return input;
			}
		}

		public static Vector3 MapVector2ToVector3(float x, float y, Vector2ToVector3ProjectionFormat projectionFormat) {
			switch (projectionFormat) {
				case Vector2ToVector3ProjectionFormat.XYo:
					return new Vector3(x,y,0);

				case Vector2ToVector3ProjectionFormat.XoY:
					return new Vector3(x, 0, y);

				case Vector2ToVector3ProjectionFormat.YXo:
					return new Vector3(y, x, 0);

				case Vector2ToVector3ProjectionFormat.YoX:
					return new Vector3(y, 0, x);

				case Vector2ToVector3ProjectionFormat.oXY:
					return new Vector3(0, x, y);

				case Vector2ToVector3ProjectionFormat.oYX:
					return new Vector3(0, y, x);
				default:
					return new Vector3(x, y, 0);
			}
		}

		public static Vector2 MapVector3ToVector2(Vector3 input, Vector3ToVector2ProjectionFormat projectionFormat) {
			switch (projectionFormat) {
				case Vector3ToVector2ProjectionFormat.XY:
					return input;

				case Vector3ToVector2ProjectionFormat.XZ:
					return new Vector2(input.x, input.z);

				case Vector3ToVector2ProjectionFormat.YX:
					return new Vector2(input.y, input.x);

				case Vector3ToVector2ProjectionFormat.YZ:
					return new Vector2(input.y, input.z);

				case Vector3ToVector2ProjectionFormat.ZX:
					return new Vector2(input.z, input.x);

				case Vector3ToVector2ProjectionFormat.ZY:
					return new Vector2(input.z, input.y);

				default:
					return input;
			}
		}

		///--------------------------------------------------------------------------------------
		///  Drawing 3D representations of 2D shapes
		///--------------------------------------------------------------------------------------

		/// <summary>
		/// This can be used within a 'for' loop to get Vector3s that progressively make up a circle.
		/// </summary>
		/// <param name="positionID">The position in a for loop. It doesn't need to be from a for loop, but this is the intended use-case.</param>
		/// <param name="angleSize">The angle to iterate by. It is best to pre-calculate this by dividing 360 by the for loop maximum.</param>
		/// <param name="radius">The radius of the circular output positions.</param>
		/// <example>
		/// int circleResolution = 64;
		/// 
		/// float angleSize = 360.0f / circleResolution;
		/// 
		/// for (int i = 0; i lessthan circleResolution; i++) {
		///		lineRenderer.SetPosition(i, DrawIterativeCircle(i, angleSize, 1.5f);
		/// }
		/// 
		/// // Draws a circle with a diameter of 3 world-units, facing upwards.
		/// </example>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawIterativeCircle(int positionID, float angleSize, float radius) {
			float angle = (positionID * angleSize) * Mathf.Deg2Rad;

			return new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
		}

		/// <summary>
		/// This can be used within a 'for' loop to get Vector3s that progressively make up a circle.
		/// </summary>
		/// <param name="positionID">The position in a for loop. It doesn't need to be from a for loop, but this is the intended use-case.</param>
		/// <param name="angleSize">The angle to iterate by. It is best to pre-calculate this by dividing 360 by the for loop maximum.</param>
		/// <param name="radius">The radius of the circular output positions.</param>
		/// <param name="projectionFormat">Which Vector3 co-ordinate format should the returned points come in?</param>
		/// <example>
		/// int circleResolution = 64;
		/// 
		/// float angleSize = 360.0f / circleResolution;
		/// 
		/// for (int i = 0; i lessthan circleResolution; i++) {
		///		lineRenderer.SetPosition(i, DrawIterativeCircle(i, angleSize, 0.5f, Vector2ToVector3ProjectionFormat.XYo);
		/// }
		/// 
		/// // Draws a circle with a diameter of 1 world-unit facing global forwards.
		/// </example>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawIterativeCircle(int positionID, float angleSize, float radius, Vector2ToVector3ProjectionFormat projectionFormat) {
			float angle = (positionID * angleSize) * Mathf.Deg2Rad;

			return MapVector2ToVector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, projectionFormat);
		}

		/// <summary>
		/// This can be used within a 'for' loop to get Vector3s that progressively make up a circle.
		/// </summary>
		/// <param name="positionID">The position in a for loop. It doesn't need to be from a for loop, but this is the intended use-case.</param>
		/// <param name="angleSize">The angle to iterate by. It is best to pre-calculate this by dividing 360 by the for loop maximum.</param>
		/// <param name="radius">The radius of the circular output positions.</param>
		/// <param name="projectionFormat">Which Vector3 co-ordinate format should the returned points come in?</param>
		/// <param name="clockwise">In which direction is the circle being drawn?</param>
		/// <example>
		/// int circleResolution = 64;
		/// 
		/// float angleSize = 360.0f / circleResolution;
		/// 
		/// for (int i = 0; i lessthan circleResolution; i++) {
		///		lineRenderer.SetPosition(i, DrawIterativeCircle(i, angleSize, 2.0f, Vector2ToVector3ProjectionFormat.XoY, false);
		/// }
		/// 
		/// // Draws a circle with a diameter of 4 world-units, counter-clockwise and facing upwards.
		/// </example>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawIterativeCircle(int positionID, float angleSize, float radius, Vector2ToVector3ProjectionFormat projectionFormat, bool clockwise) {
			float angle = clockwise ? -(positionID * angleSize) * Mathf.Deg2Rad : (positionID * angleSize) * Mathf.Deg2Rad;

			return MapVector2ToVector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, projectionFormat);
		}

		/// <summary>
		/// This can be used within a 'for' loop to get Vector3s that progressively make up a circle.
		/// </summary>
		/// <param name="positionID">The position in a for loop. It doesn't need to be from a for loop, but this is the intended use-case.</param>
		/// <param name="angleSize">The angle to iterate by. It is best to pre-calculate this by dividing 360 by the for loop maximum.</param>
		/// <param name="radius">The radius of the circular output positions.</param>
		/// <param name="projectionFormat">Which Vector3 co-ordinate format should the returned points come in?</param>
		/// <param name="clockwise">In which direction is the circle being drawn?</param>
		/// <param name="offset">How offset should the points be as a starting point?</param>
		/// <example>
		/// int circleResolution = 64;
		/// 
		/// float angleSize = 360.0f / circleResolution;
		/// 
		/// for (int i = 0; i lessthan circleResolution; i++) {
		///		lineRenderer.SetPosition(i, DrawIterativeCircle(i, angleSize, 2.0f, Vector2ToVector3ProjectionFormat.XoY, false);
		/// }
		/// 
		/// // Draws a circle with a diameter of 4 world-units, counter-clockwise and facing upwards.
		/// </example>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawIterativeCircle(int positionID, float angleSize, float radius, Vector2ToVector3ProjectionFormat projectionFormat, bool clockwise, float offset) {
			float angle = clockwise ? -((positionID * angleSize) + offset) * Mathf.Deg2Rad : ((positionID * angleSize) + offset) * Mathf.Deg2Rad;

			return MapVector2ToVector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, projectionFormat);
		}

		/// <summary>
		/// This can be used with a normal float value (0.0f - 1.0f) in order to successively return the points of a circle as a Vector3.
		/// </summary>
		/// <param name="normal">Between 0 and 1, which point on the circle do we want?</param>
		/// <param name="radius">How big should the circle be?</param>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawPerfectCircle(float normal, float radius) {
			float angle = Mathf.Lerp(0.0f, 360.0f, normal) * Mathf.Deg2Rad;

			return new Vector3(Mathf.Cos(angle) * radius, 0.0f, Mathf.Sin(angle) * radius);
		}

		/// <summary>
		/// This can be used with a normal float value (0.0f - 1.0f) in order to successively return the points of a circle as a Vector3.
		/// </summary>
		/// <param name="normal">Between 0 and 1, which point on the circle do we want?</param>
		/// <param name="radius">How big should the circle be?</param>
		/// <param name="projectionFormat">Which Vector3 co-ordinate format should the returned points come in?</param>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawPerfectCircle(float normal, float radius, Vector2ToVector3ProjectionFormat projectionFormat) {
			float angle = Mathf.Lerp(0.0f, 360.0f, normal) * Mathf.Deg2Rad;

			return MapVector2ToVector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, projectionFormat);
		}

		/// <summary>
		/// This can be used with a normal float value (0.0f - 1.0f) in order to successively return the points of a circle as a Vector3.
		/// </summary>
		/// <param name="normal">Between 0 and 1, which point on the circle do we want?</param>
		/// <param name="radius">How big should the circle be?</param>
		/// <param name="projectionFormat">Which Vector3 co-ordinate format should the returned points come in?</param>
		/// <param name="clockwise">In which direction is the circle being drawn?</param>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawPerfectCircle(float normal, float radius, Vector2ToVector3ProjectionFormat projectionFormat, bool clockwise) {
			float angle = clockwise ? -Mathf.Lerp(0.0f, 360.0f, normal) * Mathf.Deg2Rad : Mathf.Lerp(0.0f, 360.0f, normal) * Mathf.Deg2Rad;

			return MapVector2ToVector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, projectionFormat);
		}

		/// <summary>
		/// This can be used with a normal float value (0.0f - 1.0f) in order to successively return the points of a circle as a Vector3.
		/// </summary>
		/// <param name="normal">Between 0 and 1, which point on the circle do we want?</param>
		/// <param name="radius">How big should the circle be?</param>
		/// <param name="projectionFormat">Which Vector3 co-ordinate format should the returned points come in?</param>
		/// <param name="clockwise">In which direction is the circle being drawn?</param>
		/// <param name="offset">How offset should the points be as a starting point?</param>
		/// <returns>A Vector3 representation of a point on the circumference of a circle with the given radius.</returns>
		public static Vector3 DrawPerfectCircle(float normal, float radius, Vector2ToVector3ProjectionFormat projectionFormat, bool clockwise, float offset) {
			float angle = clockwise ? -(Mathf.Lerp(0.0f, 360.0f, normal) + offset) * Mathf.Deg2Rad : (Mathf.Lerp(0.0f, 360.0f, normal) + offset) * Mathf.Deg2Rad;

			return MapVector2ToVector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, projectionFormat);
		}

		public static int GetCirclePointIDFromNormal(float normal, int pointCount) {
			return Mathf.CeilToInt(Mathf.Lerp(0, pointCount, normal));
		}

		///--------------------------------------------------------------------------------------
		///  Vector Comparisons
		///--------------------------------------------------------------------------------------

		public static Vector3 GetDirection(Vector3 a, Vector3 b) {
			return b - a;
		}

		public static bool VectorIsPointingUp(Vector3 a, float marginOfErrorNormal) {
			marginOfErrorNormal = Mathf.Clamp01(marginOfErrorNormal);

			return Vector3.Dot(a, Vector3.up) > (1 - marginOfErrorNormal);
		}

		public static bool VectorIsPointingDown(Vector3 a, float marginOfErrorNormal) {
			marginOfErrorNormal = Mathf.Clamp01(marginOfErrorNormal);

			return Vector3.Dot(a, Vector3.up) > (-1 + marginOfErrorNormal);
		}

		public static bool VectorIsPointingHorizontally(Vector3 a, float marginOfErrorNormal) {
			marginOfErrorNormal = Mathf.Clamp01(marginOfErrorNormal);

			float dot = Vector3.Dot(a, Vector3.up);
			return dot > -marginOfErrorNormal && dot < marginOfErrorNormal;
		}

		/// <summary>
		/// Returns true if the dot product of the transforms forward direction and the direction from the transform to the target is greater than the given marginOfError. In other words, if target is in a cone of vision from t's forward direction. As with other dot calculations, margin of error should fall within 1 (perfect alignment) and -1 (owl vision) to be effective.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="target"></param>
		/// <param name="marginOfError"></param>
		/// <returns></returns>
		public static bool TransformCanSeePosition(Transform t, Vector3 target, float marginOfError) {
			return Vector3.Dot(t.forward, (target - t.position).normalized) >= marginOfError;
		}

		///--------------------------------------------------------------------------------------
		///  Grids and Snapping
		///--------------------------------------------------------------------------------------

		/// <summary>
		/// This function will return a vector snapped to the nearest point on a uniform grid using the given parameters.
		/// </summary>
		public static Vector3 RoundUniform(Vector3 v, float size) {
			return new Vector3(Mathf.Round(v.x / size) * size, Mathf.Round(v.y / size) * size, Mathf.Round(v.z / size) * size);
		}

		/// <summary>
		/// This function will return a vector snapped to the nearest point on a dynamic non-uniform grid using the given parameters.
		/// </summary>
		public static Vector3 RoundDynamic(Vector3 v, float xSize, float ySize, float zSize) {
			return new Vector3(Mathf.Round(v.x / xSize) * xSize, Mathf.Round(v.y / ySize) * ySize, Mathf.Round(v.z / zSize) * zSize);
		}

		///--------------------------------------------------------------------------------------
		///  Quaternion Control
		///--------------------------------------------------------------------------------------

		/// <summary>
		/// This function will return a quaternion equal to the difference between inputs a and b
		/// </summary>
		public static Quaternion GetQuaternionDifference(Quaternion a, Quaternion b) {
			return a * Quaternion.Inverse(b);
		}

		///--------------------------------------------------------------------------------------
		///  Parabolas
		///--------------------------------------------------------------------------------------

		/// <summary>
		/// This function will return a specific point on a parabola based on timeNormal (0-1)
		/// </summary>
		/// <param name="start">The starting position of the parabola</param>
		/// <param name="end">The ending positon of the parabola</param>
		/// <param name="height">The height of the parabola</param>
		/// <param name="timeNormal">Which point in the parabola are we getting? 0 being the start position and 1 being end position.</param>
		/// <returns></returns>
		public static Vector3 GetPositionInParabola(Vector3 start, Vector3 end, float height, float timeNormal) {
			timeNormal = Mathf.Clamp01(timeNormal);

			Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

			Vector3 mid = Vector3.Lerp(start, end, timeNormal);

			return new Vector3(mid.x, f(timeNormal) + Mathf.Lerp(start.y, end.y, timeNormal), mid.z);
		}

		/// <summary>
		/// This function returns an entire array of Vector3s that make up a parabola using the given parameters.
		/// </summary>
		/// <param name="start">The starting position of the parabola</param>
		/// <param name="end">The ending positon of the parabola</param>
		/// <param name="height">The height of the parabola</param>
		/// <param name="normalTimeStep">How quickly should we iterate through the normal for calculations? This value should be between 0-1, and the smaller it is, the more points will be calculated.</param>
		/// <returns></returns>
		public static Vector3[] GetParabolaPositions(Vector3 start, Vector3 end, float height, float normalTimeStep) {
			normalTimeStep = Mathf.Clamp(normalTimeStep, 0.001f, 1.0f);

			List<Vector3> points = new List<Vector3>();

			float i = 0;

			while (i < 1.0f) {
				Func<float, float> f = x => -4 * height * x * x + 4 * height * x;

				Vector3 mid = Vector3.Lerp(start, end, i);

				points.Add(new Vector3(mid.x, f(i) + Mathf.Lerp(start.y, end.y, i), mid.z));

				i += normalTimeStep;
			}

			return points.ToArray();
		}

		/// <summary>
		/// Returns a Vector3 smoothed out between a and b over time multipled by the power of the distance between them. The effect is that output will be exaggerated based on the distance, so small distances will happen very slowly but big distances will happen very quickly.
		/// </summary>
		/// <param name="a">The 'from' vector.</param>
		/// <param name="b">The 'to' vector.</param>
		/// <returns></returns>
		public static Vector3 SmoothByDifference(Vector3 a, Vector3 b) {
			return Vector3.MoveTowards(a, b, Time.deltaTime * Mathf.Pow(Vector3.Distance(a, b) * 10, 2));
		}
	}
}
