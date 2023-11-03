using System;
using NUnit.Framework;

namespace PocketGems.Parameters.Parser
{
    public class StringDictParserTest
    {
        [Test]
        public void ParseIntInt_Valid()
        {
            var dict = StringDictParser.ParseIntInt("");
            Assert.AreEqual(0, dict.Count);

            dict = StringDictParser.ParseIntInt("1:23");
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(23, dict[1]);

            dict = StringDictParser.ParseIntInt("1:3|2:4");
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(3, dict[1]);
            Assert.AreEqual(4, dict[2]);
        }

        [TestCase("abc:3|2:4")] // string key
        [TestCase("1.0:3|2:4")] // float key
        [TestCase("1.0:3|2:4")] // float key
        [TestCase("1:3.0|2:4")] // float value
        [TestCase("ab:32:4")] // multiple : delimiters
        [TestCase("1:3|")] // missing last key-value
        public void ParseIntInt_InvalidFormat(string input)
        {
            Assert.Throws<FormatException>(() => StringDictParser.ParseIntInt(input));
        }

        [Test]
        public void ParseIntInt_InvalidKeys()
        {
            Assert.Throws<ArgumentException>(() => StringDictParser.ParseIntInt("1:3|1:4"));
        }

        [Test]
        public void ParseStringInt_Valid()
        {
            var dict = StringDictParser.ParseStringInt("");
            Assert.AreEqual(0, dict.Count);

            dict = StringDictParser.ParseStringInt("abc:1");
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(1, dict["abc"]);

            dict = StringDictParser.ParseStringInt("123:1|1:2");
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(1, dict["123"]);
            Assert.AreEqual(2, dict["1"]);
        }

        [TestCase("abc:def|2:4")] // string value
        [TestCase("1:3.0|2:4")] // float value
        [TestCase("ab:32:4")] // multiple : delimiters
        [TestCase("1:3|")] // missing last key-value
        public void ParseStringInt_Invalid(string input)
        {
            Assert.Throws<FormatException>(() => StringDictParser.ParseStringInt(input));
        }

        [Test]
        public void ParseStringInt_InvalidKeys()
        {
            Assert.Throws<ArgumentException>(() => StringDictParser.ParseStringInt("1:3|1:4"));
        }

        [Test]
        public void ParseStringFloat_Valid()
        {
            var dict = StringDictParser.ParseStringFloat("");
            Assert.AreEqual(0, dict.Count);

            dict = StringDictParser.ParseStringFloat("abc:1");
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(1, dict["abc"]);

            dict = StringDictParser.ParseStringFloat("1:2.5|3:4");
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(2.5, dict["1"]);
            Assert.AreEqual(4, dict["3"]);
        }

        [TestCase("abc:def|2:4")] // string value
        [TestCase("ab:32:4")] // multiple : delimiters
        [TestCase("1:3|")] // missing last key-value
        public void ParseStringFloat_Invalid(string input)
        {
            Assert.Throws<FormatException>(() => StringDictParser.ParseStringFloat(input));
        }

        [Test]
        public void ParseStringFloat_InvalidKeys()
        {
            Assert.Throws<ArgumentException>(() => StringDictParser.ParseStringFloat("1:3|1:4"));
        }

        [Test]
        public void ParseStringString_Valid()
        {
            var dict = StringDictParser.ParseStringString("");
            Assert.AreEqual(0, dict.Count);

            dict = StringDictParser.ParseStringString("abc:def");
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("def", dict["abc"]);

            dict = StringDictParser.ParseStringString("abc:def|A:B");
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("def", dict["abc"]);
            Assert.AreEqual("B", dict["A"]);
        }

        [TestCase("ab:32:4")] // multiple : delimiters
        [TestCase("1:3|")] // missing last key-value
        public void ParseStringString_Invalid(string input)
        {
            Assert.Throws<FormatException>(() => StringDictParser.ParseStringString(input));
        }

        [Test]
        public void ParseStringString_InvalidKeys()
        {
            Assert.Throws<ArgumentException>(() => StringDictParser.ParseStringString("1:3|1:4"));
        }
    }
}
