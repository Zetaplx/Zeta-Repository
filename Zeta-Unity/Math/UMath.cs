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
}
