using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace GDLibrary.Tests
{
    [TestClass()]
    public class ColorParametersTests
    {
        [TestMethod()]
        public void ColorParametersTest()
        {
            ColorParameters c = new ColorParameters(Color.White, 1);
            Assert.IsNotNull(c);

            Assert.AreEqual(Color.White, c.Color);
            Assert.AreEqual(1, c.Alpha);

            ColorParameters d = c;
            Assert.AreSame(c, d);

            ColorParameters e = new ColorParameters(Color.White, 1);
            Assert.AreEqual(c, e);
           // Assert.AreSame(c, e);
        }

        [TestMethod()]
        public void ResetTest()
        {
            ColorParameters c = new ColorParameters(Color.White, 1);
            Assert.AreEqual(Color.White, c.Color);
            c.Color = Color.Red;
            Assert.AreEqual(Color.Red, c.Color);
            c.Reset();
            Assert.AreEqual(Color.White, c.Color);

        }

        [TestMethod()]
        public void CloneTest()
        {
            //deep clone test
            ColorParameters c = new ColorParameters(Color.White, 1);

            //ColorParameters clone = (ColorParameters)c.Clone();

            ColorParameters clone = c.Clone() as ColorParameters;
            if(clone != null)
            {
                Assert.AreEqual(clone, c);
                Assert.AreNotSame(clone, c);

            }
        }
    }
}