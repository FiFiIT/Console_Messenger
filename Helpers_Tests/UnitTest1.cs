using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Helpers_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void ConvertTimeStamp()
        {
            string tmp = "1551871257458";
            double timestamp = double.Parse(tmp);

            var time = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            time = time.AddMilliseconds(timestamp);

            Assert.IsTrue(time > new DateTime(2019, 3, 6));

        }
    }
}
