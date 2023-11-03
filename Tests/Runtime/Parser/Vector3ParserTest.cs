using System;
using NUnit.Framework;

namespace PocketGems.Parameters.Parser
{
    public class Vector3ParserTest
    {
        [Test]
        public void ParseVector3Int_Valid()
        {
            // positive values
            var vector = Vector3Parser.ParseVector3Int("1:2:3");
            Assert.AreEqual(1, vector.x);
            Assert.AreEqual(2, vector.y);
            Assert.AreEqual(3, vector.z);

            // neg values
            vector = Vector3Parser.ParseVector3Int("-1:-2:-3");
            Assert.AreEqual(-1, vector.x);
            Assert.AreEqual(-2, vector.y);
            Assert.AreEqual(-3, vector.z);

            // white spaces
            vector = Vector3Parser.ParseVector3Int($" {int.MaxValue} : {int.MinValue}:3 ");
            Assert.AreEqual(int.MaxValue, vector.x);
            Assert.AreEqual(int.MinValue, vector.y);
            Assert.AreEqual(3, vector.z);
        }

        [TestCase("")] // no values
        [TestCase("1:2:a")] // alphabetical value
        [TestCase("1:2:3.0")] // float value
        [TestCase("1:2")] // only 2 values instead of 3
        public void ParseVector3Int_InvalidFormat(string input)
        {
            Assert.Throws<FormatException>(() => Vector3Parser.ParseVector3Int(input));
        }

        [Test]
        public void ParseVector3Int_OutOfBounds()
        {
            var vectorStr = $"{(long)int.MaxValue + 1}:1:1";
            Assert.Throws<OverflowException>(() => Vector3Parser.ParseVector3Int(vectorStr));

            vectorStr = $"{(long)int.MinValue - 1}:1:1";
            Assert.Throws<OverflowException>(() => Vector3Parser.ParseVector3Int(vectorStr));
        }

        [Test]
        public void ParseVector3Int_InvalidNull()
        {
            Assert.Throws<NullReferenceException>(() => Vector3Parser.ParseVector3Int(null));
        }

        [Test]
        public void ParseVector3Float_Valid()
        {
            // positive values
            var vector = Vector3Parser.ParseVector3Float("1.1:2.5:3.2");
            Assert.AreEqual(1.1f, vector.x);
            Assert.AreEqual(2.5f, vector.y);
            Assert.AreEqual(3.2f, vector.z);

            // neg values
            vector = Vector3Parser.ParseVector3Float("-1.1:-2.5:-3.2");
            Assert.AreEqual(-1.1f, vector.x);
            Assert.AreEqual(-2.5f, vector.y);
            Assert.AreEqual(-3.2f, vector.z);

            // white spaces
            vector = Vector3Parser.ParseVector3Float(" 1.0 : 2:3 ");
            Assert.AreEqual(1f, vector.x);
            Assert.AreEqual(2f, vector.y);
            Assert.AreEqual(3f, vector.z);
        }

        [TestCase("")] // no values
        [TestCase("1:2:a")] // alphabetical value
        [TestCase("1:2")] // only 2 values instead of 3
        public void ParseVector3Float_InvalidFormat(string input)
        {
            Assert.Throws<FormatException>(() => Vector3Parser.ParseVector3Float(input));
        }

        [Test]
        public void ParseVector3Float_InvalidNull()
        {
            Assert.Throws<NullReferenceException>(() => Vector3Parser.ParseVector3Float(null));
        }

        [Test]
        public void ParseVector3Float_OutOfBounds()
        {
            var vectorStr = $"{(double)float.MaxValue * 2}:1:1";
            Assert.Throws<OverflowException>(() => Vector3Parser.ParseVector3Float(vectorStr));

            vectorStr = $"{(double)float.MinValue * 2}:1:1";
            Assert.Throws<OverflowException>(() => Vector3Parser.ParseVector3Float(vectorStr));
        }
    }
}
