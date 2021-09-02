using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Zeta.Generics;

namespace Unit_Tests
{
    [TestClass]
    public class RoloTest
    {
        public float f1 { get; set; }
        float f2;

        [TestMethod]
        public void TestMethod1()
        {
            f2 = 0f;
            Rolo rolo = new Rolo();
            rolo.Push("Value", ref f2);
            [Assert]
        }
    }
}
