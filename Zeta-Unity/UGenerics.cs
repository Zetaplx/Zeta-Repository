using System;
using UnityEngine;
using Zeta.Generics;

namespace Zeta.Unity
{
    public static class BasicZetaUnityUpgrades
    {
        public static void SetVX(this Rigidbody2D rb, float value) => rb.velocity = new Vector2(value, rb.velocity.y);
        public static void SetVY(this Rigidbody2D rb, float value) => rb.velocity = new Vector2(rb.velocity.x, value);

        
    }
}
