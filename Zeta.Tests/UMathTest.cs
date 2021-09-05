using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using UnityEngine;
using Zeta.Math.Unity;

namespace Zeta.Tests
{
    
    
    public class UMathTest
    {

        [Fact]
        public void Test1 ()
        {
            Vector2 v = new Vector2(1, 0);
            v.Rotate(90);
            Assert.True((v - new Vector2(0, 1)).sqrMagnitude < 0.05f);
            v.Rotate(90);
            Assert.True((v - new Vector2(-1, 0)).sqrMagnitude < 0.05f);
            v.Rotate(90);
            Assert.True((v - new Vector2(0, -1)).sqrMagnitude < 0.05f);
            v.Rotate(90);
            Assert.True((v - new Vector2(1, 0)).sqrMagnitude < 0.05f);
            v.Rotate(-90);
            Assert.True((v - new Vector2(0, -1)).sqrMagnitude < 0.05f);
        }
    }
}
