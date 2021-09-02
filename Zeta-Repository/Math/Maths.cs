using System;
using System.Collections.Generic;
using System.Text;
using Zeta.Generics;
using System.Linq;

namespace Zeta.Math
{
    public interface AlgebraicOperator<T>
    {
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
}
