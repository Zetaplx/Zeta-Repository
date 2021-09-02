using System;
using UnityEngine;
using Zeta.Generics;

namespace Zeta.Unity
{
    public static class BasicZetaUnityUpgrades
    {
        public static void SetVX(this Rigidbody2D rb, float value) => rb.velocity = new Vector2(value, rb.velocity.y);
        public static void SetVY(this Rigidbody2D rb, float value) => rb.velocity = new Vector2(rb.velocity.x, value);

        public static void AddForceWithinLimiter(this Rigidbody2D rb, ILimiter limiter, Vector2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            if(limiter.Within(rb.velocity + force * (mode == ForceMode2D.Force ? Time.fixedDeltaTime : 1)))
            {
                rb.AddForce(force, mode);
            }
        }
        public static void AddForceToLimiter(this Rigidbody2D rb, ILimiter limiter, Vector2 force, ForceMode2D mode = ForceMode2D.Force)
        {
            if (limiter.Within(rb.velocity))
            {
                rb.AddForce(force, mode);
            }
            else
            {
                rb.velocity = limiter.Normalize(rb.velocity + force * (mode == ForceMode2D.Force ? Time.fixedDeltaTime : 1));
            }
        }
    }

    public interface ILimiter
    {
        public bool Within(Vector2 value);
        public Vector2 Normalize(Vector2 value);
    }
}
