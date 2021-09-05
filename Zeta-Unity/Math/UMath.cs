using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Zeta.Generics;

namespace Zeta.Math.Unity
{
    public static class UArrayUtils
    {
        /// <summary>
        /// Returns a list of compents found on a list of monobehaviours.
        /// </summary>
        public static List<TSub> ObjectSubList<TSub, TArr>(List<TArr> arr) where TArr : MonoBehaviour
        {
            List<TSub> subs = new List<TSub>();

            foreach (TArr a in arr)
            {
                TSub sub = a.GetComponent<TSub>();
                if (sub != null)
                    subs.Add(sub);
            }

            return subs;
        }

        /// <summary>
        /// Returns an array of compents found on an array of monobehaviours.
        /// </summary>
        public static TSub[] ObjectSubArr<TSub, TArr>(TArr[] arr) where TArr : MonoBehaviour
        {
            List<TSub> subs = new List<TSub>();

            foreach (TArr a in arr)
            {
                TSub sub = a.GetComponent<TSub>();
                if (sub != null)
                    subs.Add(sub);
            }

            return subs.ToArray();
        }
    }

    public static class UVectorUtils
    {
        public static Vector2 RotateVector2(Vector2 v, float angle, AngleMode angleMode = AngleMode.DEGREES)
        {
            float trueAngle = angle * (angleMode == AngleMode.DEGREES ? Mathf.Deg2Rad : 1);
            return new Vector2(v.x * Mathf.Cos(trueAngle) - v.y * Mathf.Sin(trueAngle), v.x * Mathf.Sin(trueAngle) + v.y * Mathf.Cos(trueAngle));
        }
    }

    public static class UVectorExtensions
    {
        public static void Rotate(this ref Vector2 v, float angle, AngleMode mode = AngleMode.DEGREES) 
            => v = UVectorUtils.RotateVector2(v, angle, mode);

        public static Vector2 Rotated(this Vector2 v, float angle, AngleMode mode = AngleMode.DEGREES)
            => UVectorUtils.RotateVector2(v, angle, mode);
    }
}
