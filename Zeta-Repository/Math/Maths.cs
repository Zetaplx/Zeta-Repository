using System;
using System.Collections.Generic;
using System.Text;
using Zeta.Generics;
using System.Linq;

namespace Zeta.Math
{
    public interface AlgebraicOperator<T>
    {
        public T Add(T t1, T t2);
        public T Mult(T t1, T t2);
        public T Inverse(T t);
        public T Negate(T t);
        public T Sqrt(T t);
        public T Sin(T t);
        public T Cos(T t);
        public T Atan(T y, T x);

        public T Unit { get; }
        public T Zero { get; }
        public int CompareTo(T t1, T t2);
    }

    public static class MathGenerics
    {
        public const float PI = (float) System.Math.PI;

        public static float Clamp(float value, float min, float max) => (value < min ? min : value > max ? max : value);
        public static int Clamp(int value, int min, int max) => (value < min ? min : value > max ? max : value);

        public static float Abs(float value) => (value > 0 ? value : -value);
        public static int Abs(int value) => (value > 0 ? value : -value);

        public static float Floor(float value) => (int)value;
        public static float Ceil(float value) => value > (int)value ? (int)value + 1 : value;

        public static float Lerp(float start, float end, float t, Func<float, float, float> distFunc = null)
        {
            distFunc ??= (a, b) => b - a;
            return start + distFunc(start, end) * t;
        }

        public static bool Within(float a, float b, float limit, Func<float, float, float> distFunc = null)
        {
            distFunc ??= (a, b) => Abs(b - a);
            return distFunc(a, b) < limit;
        }

        public static int Factorial(int n)
        {
            if (n < 0) throw new System.InvalidOperationException("Cannot find factorial of non-positive integers.");
            int val = 1;
            for(int i = n; i > 0; i++)
            {
                val *= i;
            }
            return val;
        }

        public static int Choose(int n, int r)
        {
            if (r < 1 || n < 1) throw new System.InvalidOperationException("N and R must be positive integers.");
            if (r > n) throw new System.InvalidOperationException("Cannot chose larger than set size");
            return Factorial(n) / (Factorial(r) * Factorial(n - 1));
        }

        public static float Max(params float[] arr)
        {
            if (arr.Length == 0) throw new System.InvalidOperationException("Cannot find max of empty array.");
            var val = arr[0];
            foreach(var v in arr)
            {
                if (v > val) val = v;
            }
            return val;
        }
        public static float Min(params float[] arr)
        {
            if (arr.Length == 0) throw new System.InvalidOperationException("Cannot find min of empty array.");
            var val = arr[0];
            foreach (var v in arr)
            {
                if (v < val) val = v;
            }
            return val;
        }

        public static int Max(params int[] arr)
        {
            if (arr.Length == 0) throw new System.InvalidOperationException("Cannot find max of empty array.");
            var val = arr[0];
            foreach (var v in arr)
            {
                if (v > val) val = v;
            }
            return val;
        }
        public static int Min(params int[] arr)
        {
            if (arr.Length == 0) throw new System.InvalidOperationException("Cannot find min of empty array.");
            var val = arr[0];
            foreach (var v in arr)
            {
                if (v < val) val = v;
            }
            return val;
        }
    }

    public enum AngleMode { RADIANS, DEGREES }

    public static class ArrayUtils
    {
        /// <summary>
        /// Returns a list of child type from a parent typed list.
        /// </summary>
        public static List<TSub> SubList<TSub, TArr>(List<TArr> arr) where TSub : TArr
        {
            return arr.OfType<TSub>().ToList();
        }

        /// <summary>
        /// Returns an array of child type from a parent typed array.
        /// </summary>
        public static TSub[] SubArr<TSub, TArr>(TArr[] arr) where TSub : TArr
        {
            return arr.OfType<TSub>().ToArray();
        }

        /// <summary>
        /// Returns true if <paramref name="index"/> is within the bounds of size of <paramref name="list"/>.
        /// </summary>
        public static bool ValidIndex<T>(int index, List<T> list)
        {
            return index >= 0 && index < list.Count;
        }

        /// <summary>
        /// Returns true if <paramref name="index"/> is within the bounds of size of <paramref name="arr"/>.
        /// </summary>
        public static bool ValidIndex<T>(int index, T[] arr)
        {
            return index >= 0 && index < arr.Length;
        }

        /// <summary>
        /// Returns the list value at <paramref name="index"/> if index is within bounds of size of <paramref name="list"/>.
        /// </summary>
        public static T ReturnValue<T>(int index, List<T> list) where T : class
        {
            return ValidIndex(index, list) ? list[index] : null;
        }

        /// <summary>
        /// Returns the array value at <paramref name="index"/> if index is within bounds of size of <paramref name="arr"/>.
        /// </summary>
        public static T ReturnValue<T>(int index, T[] arr) where T : class
        {
            return (ValidIndex(index, arr) ? arr[index] : null);
        }

        /// <summary>
        /// Returns the list value at <paramref name="index"/> clamped from 0 to <paramref name="list"/>.Count - 1
        /// </summary>
        public static T ReturnClampValue<T>(int index, List<T> list)
        {
            return list[MathGenerics.Clamp(index, 0, list.Count - 1)];
        }

        /// <summary>
        /// Returns the array value at <paramref name="index"/> clamped from 0 to <paramref name="arr"/>.Length - 1
        /// </summary>
        public static T ReturnClampValue<T>(int index, T[] arr)
        {
            return arr[MathGenerics.Clamp(index, 0, arr.Length - 1)];
        }

        /// <summary>
        /// Returns the list value at <paramref name="index"/> looped about 0 and <paramref name="list"/>.Count
        /// </summary>
        public static T ReturnLoopValue<T>(int index, List<T> list)
        {
            return list[index % list.Count];
        }

        /// <summary>
        /// Returns the array value at <paramref name="index"/> looped about 0 and <paramref name="arr"/>.Length
        /// </summary>
        public static T ReturnLoopValue<T>(int index, T[] arr)
        {
            return arr[index % arr.Length];
        }

        /// <summary>
        /// Calculates the nearest float to <paramref name="value"/> using a given distance function.
        /// </summary>
        public static float NearestValue(IEnumerable<float> floats, float value, Func<float, float, float> distance = null)
        {
            distance ??= (a, b) => MathGenerics.Abs(b - a);

            float nearest = 0f;
            float smallestDist = float.PositiveInfinity;
            foreach (float f in floats) if (MathGenerics.Abs(f - value) < smallestDist) { smallestDist = distance(value, f); nearest = f; }

            return nearest;
        }
    }

    public static class AngleUtils
    {
        /// <summary>
        /// Returns the angle opposite <paramref name="angle"/>. e.g. ReverseAngle(30) = 210
        /// </summary>
        public static float ReverseAngle(float angle, AngleMode mode = AngleMode.DEGREES)
        {
            return NormalizeAngle(angle + (mode == AngleMode.DEGREES ? 180 : mode == AngleMode.RADIANS ? MathGenerics.PI : 0f), 0, mode);
        }

        /// <summary>
        /// Returns an equivalent angle within half a rotation from <paramref name="center"/>.
        /// </summary>
        public static float NormalizeAngle(float angle, float center = 0f, AngleMode mode = AngleMode.DEGREES)
        {
            float a = angle;
            float max = mode == AngleMode.DEGREES ? 360 : mode == AngleMode.RADIANS ? MathGenerics.PI * 2f : 0f;

            a -= MathGenerics.Floor((a + center) / max) * max;
            return a;
        }

        /// <summary>
        /// Returns the angular distance between two angles. Positive distances are for CCW rotation, negative for CW rotations.
        /// </summary>
        public static float AngleDistance(float from, float to, AngleMode mode = AngleMode.DEGREES)
        {
            float fromSimplified = NormalizeAngle(from, 0, mode);
            float toSimplified = NormalizeAngle(to, 0, mode);

            if (toSimplified < fromSimplified)
            {
                return -AngleDistance(toSimplified, fromSimplified, mode);
            }

            float max = mode == AngleMode.DEGREES ? 360 : mode == AngleMode.RADIANS ? MathGenerics.PI * 2f : 0f;

            float del = toSimplified - fromSimplified;
            if (del <= max / 2f) return del;
            return -(max - (del));
        }

        /// <summary>
        /// Returns the nearest angle in a list to <paramref name="angle"/>.
        /// </summary>
        public static float NearestAngle(IEnumerable<float> angles, float angle, AngleMode mode = AngleMode.DEGREES)
        {
            return ArrayUtils.NearestValue(angles, angle, (a, b) => AngleDistance(a, b, mode));
        }

        /// <summary>
        /// Returns true if <paramref name="a1"/> and <paramref name="a2"/> are equivalent angles.
        /// </summary>
        public static bool EqualAngles(float a1, float a2)
        {
            return NormalizeAngle(a1) == NormalizeAngle(a2);
        }


        public static float AngleLerp(float startAngle, float endAngle, float t, AngleMode mode = AngleMode.DEGREES)
        {
            return MathGenerics.Lerp(startAngle, endAngle, t, (a, b) => AngleDistance(a, b, mode));
        }
    }

    public class FloatOperator : AlgebraicOperator<float>
    {
        public const float StandardPercision = 0.0005f;
        private float percision;

        public FloatOperator(float p = StandardPercision)
        {
            percision = p;
        }

        public float Add(float t1, float t2)
        {
            return t1 + t2;
        }
        public float Mult(float t1, float t2)
        {
            return t1 * t2;
        }
        public float Inverse(float t)
        {
            return 1/t;
        }
        public float Negate(float t)
        {
            return -t;
        }
        public float Sqrt(float t)
        {
            return (float)System.Math.Sqrt(t);
        }
        public float Sin(float t) => (float)System.Math.Sin(t);
        public float Cos(float t) => (float)System.Math.Cos(t);
        public float Atan(float y, float x) => (float)System.Math.Atan2(y, x);

        public float Unit { get { return 1; } }
        public float Zero { get { return 0; } }
        public int CompareTo(float t1, float t2)
        {
            if (t1 - t2 > 0.0005f) return 1;
            if (t1 - t2 < -0.0005f) return -1;
            return 0;
        }
    }
}
