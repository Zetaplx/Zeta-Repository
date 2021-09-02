using System;
using Xunit;
using Zeta.Generics;

namespace Zeta.Tests
{
    public class UnitTest1
    {
        

        [Fact]
        public void Test1()
        {
            Rolodex rolo = new Rolodex();
            TestCollectible(rolo);
        }

        void TestCollectible(Rolodex icoll)
        {
            int value = 1;
            icoll.Push("Value", -1);
            var test0 = icoll.Pull<int>("Value");
            Assert.True(test0 == -1, "Initial Field Test");

            value = -2;
            var test01 = icoll.Pull<int>("Value");
            Assert.True(test01 != value, "Initial Change Fail Test");

            icoll.Push("Value", -2);
            var test02 = icoll.Pull<int>("Value");
            Assert.True(test02 == -2);


            icoll.Push("Value", () => value, (d) => value = d);

            // Storage Test
            var test1 = icoll.Pull<int>("Value");
            Assert.True(value == -2, "Override Test");
            Assert.True(value == test1, "Storage Test");

            // Reassign Test
            icoll.Push("Value", 2);
            var test2 = icoll.Pull<int>("Value");
            Assert.True(value == test2, "Reassign Test");

            // Source Change Test
            value = 3;
            var test3 = icoll.Pull<int>("Value");
            Assert.True(value == test3, "Source Change Test");
        }
    }
}
