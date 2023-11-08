using System;
using NUnit.Framework;
using UnityEngine;

namespace PocketGems.Parameters.LocalCSV
{
    public partial class CSVValueConversionTest
    {
        [Test]
        [TestCase(false, null)]
        [TestCase(false, "")]
        [TestCase(false, " ")]
        [TestCase(true, "my_id", "my_id")]
        [TestCase(true, " my_id ", "my_id")]
        [TestCase(true, "a", "a")]
        public void IdentifierFromString(bool success, string value, string expected = null)
        {
            if (success)
                Assert.AreEqual(expected, CSVValueConverter.Identifier.FromString(value));
            else
                Assert.Throws<Exception>(() => CSVValueConverter.Identifier.FromString(value));
        }


        [Test]
        [TestCase(1f, 1f, 1f, 1f, "255,255,255,255")]
        [TestCase(0f, .2f, .4f, .6f, "0,51,102,153")]
        public void ColorToString(float r, float g, float b, float a, string expected)
        {
            var color = new Color(r, g, b, a);
            Assert.AreEqual(expected, CSVValueConverter.Color.ToString(color));
        }

        [Test]
        [TestCase(true, "255,0,0,0", 1f, 0f, 0f, 0f)]
        [TestCase(true, "0,51,102,153", 0f, .2f, .4f, .6f)]
        [TestCase(true, "0, 51 , 102, 153 ,,", 0f, .2f, .4f, .6f)]
        [TestCase(true, " ")]
        [TestCase(true, "")]
        [TestCase(false, ",,,")]
        [TestCase(false, "a,0,0,0")]
        [TestCase(false, "0,0,0")]
        [TestCase(false, "0,0")]
        [TestCase(false, "0,")]
        public void ColorFromString(bool success, string value,
            float r = 0f, float g = 0f, float b = 0f, float a = 0f)
        {
            if (success)
            {
                var color = CSVValueConverter.Color.FromString(value);
                Assert.AreEqual(r, color.r);
                Assert.AreEqual(g, color.g);
                Assert.AreEqual(b, color.b);
                Assert.AreEqual(a, color.a);
            }
            else
            {
                Assert.Catch(() => CSVValueConverter.Color.FromString(value));
            }
        }

        [Test]
        [TestCase(10f, -5.5f, "10,-5.5")]
        [TestCase(0f, 100f, "0,100")]
        public void Vector2ToString(float x, float y, string expected)
        {
            var vector = new Vector2(x, y);
            Assert.AreEqual(expected, CSVValueConverter.Vector2.ToString(vector));
        }

        [Test]
        [TestCase(true, "10,-5.5", 10f, -5.5f)]
        [TestCase(true, "0,100", 0f, 100f)]
        [TestCase(true, " 0 , 100 , ", 0f, 100f)]
        [TestCase(true, "", 0f, 0f)]
        [TestCase(true, null, 0f, 0f)]
        [TestCase(false, "a,b")]
        [TestCase(false, ",")]
        [TestCase(false, "5")]
        public void Vector2FromString(bool success, string value, float x = 0f, float y = 0f)
        {
            if (success)
            {
                var vector = CSVValueConverter.Vector2.FromString(value);
                Assert.AreEqual(x, vector.x);
                Assert.AreEqual(y, vector.y);
            }
            else
            {
                Assert.Catch(() => CSVValueConverter.Vector2.FromString(value));
            }
        }

        [Test]
        [TestCase(10, -5, "10,-5")]
        [TestCase(0, 100, "0,100")]
        public void Vector2IntToString(int x, int y, string expected)
        {
            var vector = new Vector2Int(x, y);
            Assert.AreEqual(expected, CSVValueConverter.Vector2Int.ToString(vector));
        }

        [Test]
        [TestCase(true, "", new int[] { })]
        [TestCase(true, "1,2", new int[] { 1, 2 })]
        [TestCase(true, "1,2|3,4", new int[] { 1, 2, 3, 4 })]
        [TestCase(false, "a,b", null)]
        [TestCase(false, ",", null)]
        [TestCase(false, "1", null)]
        public void ArrayFuncMapperFromString(bool success, string value, int[] expected)
        {
            if (success)
            {
                const int valueCount = 2;
                var vectors = CSVValueConverter.ArrayFuncMapper<Vector2Int>.FromString(value, CSVValueConverter.Vector2Int.FromString);
                Assert.AreEqual(expected.Length / valueCount, vectors.Length);
                for (int i = 0; i < vectors.Length; i++)
                {
                    Assert.AreEqual(expected[i * valueCount], vectors[i].x);
                    Assert.AreEqual(expected[i * valueCount + 1], vectors[i].y);
                }
            }
            else
            {
                Assert.Catch(() => CSVValueConverter.ArrayFuncMapper<Vector2Int>.FromString(value, CSVValueConverter.Vector2Int.FromString));
            }
        }

        [Test]
        [TestCase(new int[] { }, "")]
        [TestCase(new int[] { 1, 2 }, "1,2")]
        [TestCase(new int[] { 1, 2, 3, 4 }, "1,2|3,4")]
        public void ArrayFuncMapperToString(int[] input, string expected)
        {
            const int valueCount = 2;
            var length = input.Length / valueCount;
            Vector2Int[] vectors = new Vector2Int[length];
            for (int i = 0; i < length; i++)
                vectors[i] = new Vector2Int(input[i * valueCount], input[i * valueCount + 1]);
            Assert.AreEqual(expected, CSVValueConverter.ArrayFuncMapper<Vector2Int>.ToString(vectors, CSVValueConverter.Vector2Int.ToString));
        }

        [Test]
        [TestCase(true, "10,-5", 10, -5)]
        [TestCase(true, "0,100", 0, 100)]
        [TestCase(true, " 0 , 100 , ", 0, 100)]
        [TestCase(true, "", 0, 0)]
        [TestCase(true, null, 0, 0)]
        [TestCase(false, "a,b")]
        [TestCase(false, ",")]
        [TestCase(false, "5")]
        public void Vector2IntFromString(bool success, string value, int x = 0, int y = 0)
        {
            if (success)
            {
                var vector = CSVValueConverter.Vector2Int.FromString(value);
                Assert.AreEqual(x, vector.x);
                Assert.AreEqual(y, vector.y);
            }
            else
            {
                Assert.Catch(() => CSVValueConverter.Vector2Int.FromString(value));
            }
        }

        [Test]
        [TestCase(10f, -5.5f, 0f, "10,-5.5,0")]
        [TestCase(0f, 100f, 1f, "0,100,1")]
        public void Vector3ToString(float x, float y, float z, string expected)
        {
            var vector = new Vector3(x, y, z);
            Assert.AreEqual(expected, CSVValueConverter.Vector3.ToString(vector));
        }

        [Test]
        [TestCase(true, "10,-5.5,0", 10f, -5.5f, 0f)]
        [TestCase(true, "0,100,1", 0f, 100f, 1)]
        [TestCase(true, " 0 , 100 , 1 , ,", 0f, 100f, 1f)]
        [TestCase(true, "", 0f, 0f, 0f)]
        [TestCase(true, null, 0f, 0f, 0f)]
        [TestCase(false, "a,b,c")]
        [TestCase(false, ",,")]
        [TestCase(false, "5")]
        public void Vector3FromString(bool success, string value, float x = 0f, float y = 0f, float z = 0f)
        {
            if (success)
            {
                var vector = CSVValueConverter.Vector3.FromString(value);
                Assert.AreEqual(x, vector.x);
                Assert.AreEqual(y, vector.y);
                Assert.AreEqual(z, vector.z);
            }
            else
            {
                Assert.Catch(() => CSVValueConverter.Vector3.FromString(value));
            }
        }

        [Test]
        [TestCase(10, -5, 0, "10,-5,0")]
        [TestCase(0, 100, 1, "0,100,1")]
        public void Vector3IntToString(int x, int y, int z, string expected)
        {
            var vector = new Vector3Int(x, y, z);
            Assert.AreEqual(expected, CSVValueConverter.Vector3Int.ToString(vector));
        }

        [Test]
        [TestCase(true, "10,-5,0", 10, -5, 0)]
        [TestCase(true, "0,100,1", 0, 100, 1)]
        [TestCase(true, " 0 , 100 , 1 , ,", 0, 100, 1)]
        [TestCase(true, "", 0, 0, 0)]
        [TestCase(true, null, 0, 0, 0)]
        [TestCase(false, "a,b,c")]
        [TestCase(false, ",,")]
        [TestCase(false, "5")]
        public void Vector3IntFromString(bool success, string value, int x = 0, int y = 0, int z = 0)
        {
            if (success)
            {
                var vector = CSVValueConverter.Vector3Int.FromString(value);
                Assert.AreEqual(x, vector.x);
                Assert.AreEqual(y, vector.y);
                Assert.AreEqual(z, vector.z);
            }
            else
            {
                Assert.Catch(() => CSVValueConverter.Vector3Int.FromString(value));
            }
        }


        [Test]
        [TestCase(TestEnum.Enum1, "Enum1")]
        [TestCase(TestEnum.Enum2, "Enum2")]
        [TestCase(TestEnum.Enum3, "Enum3")]
        public void EnumToString(TestEnum e, string expected)
        {
            Assert.AreEqual(expected, CSVValueConverter.Numeric<TestEnum>.ToString(e));
        }

        [Test]
        [TestCase(true, "Enum1", TestEnum.Enum1)]
        [TestCase(true, "Enum2", TestEnum.Enum2)]
        [TestCase(true, "Enum3", TestEnum.Enum3)]
        [TestCase(false, "blah")]
        [TestCase(false, "")]
        [TestCase(false, null)]
        public void EnumFromString(bool success, string value, TestEnum e = TestEnum.Enum1)
        {
            if (success)
                Assert.AreEqual(e, CSVValueConverter.Numeric<TestEnum>.FromString(value));
            else
                Assert.Catch(() => CSVValueConverter.Numeric<TestEnum>.FromString(value));
        }

        [Test]
        [TestCase(true, "1")]
        [TestCase(false, "0")]
        public void BoolToString(bool b, string expected)
        {
            Assert.AreEqual(expected, CSVValueConverter.Bool.ToString(b));
        }

        [Test]
        [TestCase(true, "1", true)]
        [TestCase(true, " true", true)]
        [TestCase(true, "TRUE ", true)]
        [TestCase(true, " True ", true)]
        [TestCase(true, null, false)]
        [TestCase(true, "", false)]
        [TestCase(true, " 0", false)]
        [TestCase(true, "false", false)]
        [TestCase(true, "False", false)]
        [TestCase(false, "blah")]
        [TestCase(false, "10")]
        public void BoolFromString(bool success, string value, bool expected = false)
        {
            if (success)
                Assert.AreEqual(expected, CSVValueConverter.Bool.FromString(value));
            else
                Assert.Catch(() => CSVValueConverter.Bool.FromString(value));
        }


        [Test]
        public void NumericT()
        {
            void Test<T>(T five, T negTwo, T zero, T upperBound, string upperString, T lowerBound, string lowerString)
            {
                // to string
                Assert.AreEqual("5", CSVValueConverter.Numeric<T>.ToString(five));
                Assert.AreEqual("-2", CSVValueConverter.Numeric<T>.ToString(negTwo));
                Assert.AreEqual("0", CSVValueConverter.Numeric<T>.ToString(zero));
                Assert.AreEqual(upperString, CSVValueConverter.Numeric<T>.ToString(upperBound));
                Assert.AreEqual(lowerString, CSVValueConverter.Numeric<T>.ToString(lowerBound));

                // from string
                Assert.AreEqual(5, CSVValueConverter.Numeric<T>.FromString("5"));
                Assert.AreEqual(-2, CSVValueConverter.Numeric<T>.FromString(" -2 "));
                Assert.AreEqual(0, CSVValueConverter.Numeric<T>.FromString(" "));
                Assert.AreEqual(0, CSVValueConverter.Numeric<T>.FromString(""));
                Assert.AreEqual(0, CSVValueConverter.Numeric<T>.FromString(null));
                Assert.AreEqual(upperBound, CSVValueConverter.Numeric<T>.FromString(upperString));
                Assert.AreEqual(lowerBound, CSVValueConverter.Numeric<T>.FromString(lowerString));
                Assert.Catch(() => CSVValueConverter.Numeric<T>.FromString("abc"));
                Assert.Catch(() => CSVValueConverter.Numeric<T>.FromString("10000000000000000000"));
            }

            Test<short>(5, -2, 0, 32767, "32767", -32768, "-32768");
            Test<int>(5, -2, 0, 2147483647, "2147483647", -2147483648, "-2147483648");
            Test<long>(5, -2, 0, 9223372036854775807, "9223372036854775807", -9223372036854775808, "-9223372036854775808");

            Assert.Catch(() => CSVValueConverter.Numeric<string>.FromString("abc"));
        }

        [Test]
        [TestCase(0f, "0")]
        [TestCase(1f, "1")]
        [TestCase(-5f, "-5")]
        [TestCase(10.5f, "10.5")]
        public void NumericFloatToString(float value, string expected)
        {
            Assert.AreEqual(expected, CSVValueConverter.Numeric<float>.ToString(value));
        }

        [Test]
        [TestCase(true, "", 0f)]
        [TestCase(true, "0", 0f)]
        [TestCase(true, " 1 ", 1f)]
        [TestCase(true, " -5", -5f)]
        [TestCase(true, " 10.5 ", 10.5f)]
        [TestCase(false, "abc")]
        public void NumericFloatFromString(bool success, string value, float expected = 0)
        {
            if (success)
                Assert.AreEqual(expected, CSVValueConverter.Numeric<float>.FromString(value));
            else
                Assert.Catch(() => CSVValueConverter.Numeric<float>.FromString(value));
        }

        [Test]
        [TestCase("blah")]
        [TestCase("")]
        [TestCase(null)]
        public void LocalizedStringToString(string value)
        {
            Assert.AreEqual(value, CSVValueConverter.LocalizedString.ToString(value));
        }

        [Test]
        [TestCase("blah")]
        [TestCase("")]
        [TestCase(null)]
        public void LocalizedStringFromString(string value)
        {
            Assert.AreEqual(value, CSVValueConverter.LocalizedString.FromString(value));
        }

        [Test]
        public void BoolArrayToString()
        {
            void Test(string expected, bool[] value)
            {
                Assert.AreEqual(expected, CSVValueConverter.BoolArray.ToString(value));
            }

            Test("", null);
            Test("", new bool[] { });
            Test("0", new[] { false });
            Test("1|0|1", new[] { true, false, true });
        }

        [Test]
        public void BoolArrayFromString()
        {
            void Test(bool success, string value, bool[] expected = null)
            {
                if (success)
                    Assert.AreEqual(expected, CSVValueConverter.BoolArray.FromString(value));
                else
                    Assert.Catch(() => CSVValueConverter.BoolArray.FromString(value));
            }

            Test(true, null, Array.Empty<bool>());
            Test(true, "", Array.Empty<bool>());
            Test(true, " ", Array.Empty<bool>());
            Test(true, " True ", new[] { true });
            Test(true, "1| False", new[] { true, false });
            Test(true, "||", new[] { false, false, false });
            Test(false, "0|a|1");
        }

        [Test]
        public void NumericArrayT()
        {
            void Test<T>(T five, T negTwo, T zero)
            {
                // to string
                Assert.AreEqual("", CSVValueConverter.NumericArray<T>.ToString(null));
                Assert.AreEqual("", CSVValueConverter.NumericArray<T>.ToString(Array.Empty<T>()));
                Assert.AreEqual("5", CSVValueConverter.NumericArray<T>.ToString(new T[] { five }));
                Assert.AreEqual("5|-2", CSVValueConverter.NumericArray<T>.ToString(new T[] { five, negTwo }));
                Assert.AreEqual("5|-2|0", CSVValueConverter.NumericArray<T>.ToString(new T[] { five, negTwo, zero }));

                // from string
                Assert.AreEqual(Array.Empty<T>(), CSVValueConverter.NumericArray<T>.FromString(null));
                Assert.AreEqual(Array.Empty<T>(), CSVValueConverter.NumericArray<T>.FromString(""));
                Assert.AreEqual(Array.Empty<T>(), CSVValueConverter.NumericArray<T>.FromString(" "));
                Assert.AreEqual(new T[] { five }, CSVValueConverter.NumericArray<T>.FromString(" 5 "));
                Assert.AreEqual(new T[] { five, negTwo }, CSVValueConverter.NumericArray<T>.FromString("5 |-2  "));
                Assert.AreEqual(new T[] { five, negTwo, zero }, CSVValueConverter.NumericArray<T>.FromString("5|-2|   0"));
                Assert.AreEqual(new T[] { zero, zero, zero }, CSVValueConverter.NumericArray<T>.FromString(" | |"));
                Assert.Catch(() => CSVValueConverter.NumericArray<T>.FromString("0|a|2"));
                Assert.Catch(() => CSVValueConverter.NumericArray<T>.FromString("0|10000000000000000000"));
            }

            Test<short>(5, -2, 0);
            Test<int>(5, -2, 0);
            Test<long>(5, -2, 0);

            Assert.Catch(() => CSVValueConverter.NumericArray<string>.FromString("0|1"));
        }

        [Test]
        public void NumericArrayFloat()
        {
            Assert.AreEqual("0.5|-1.5", CSVValueConverter.NumericArray<float>.ToString(new[] { 0.5f, -1.5f }));
            Assert.AreEqual(new[] { 0.5f, -1.5f }, CSVValueConverter.NumericArray<float>.FromString("0.5 | -1.5 "));
        }

        [Test]
        public void EnumArray()
        {
            Assert.AreEqual("", CSVValueConverter.NumericArray<TestEnum>.ToString(null));
            Assert.AreEqual("", CSVValueConverter.NumericArray<TestEnum>.ToString(Array.Empty<TestEnum>()));
            Assert.AreEqual("Enum1", CSVValueConverter.NumericArray<TestEnum>.ToString(new[] { TestEnum.Enum1 }));
            Assert.AreEqual("Enum1|Enum2", CSVValueConverter.NumericArray<TestEnum>.ToString(new[] {
                TestEnum.Enum1,
                TestEnum.Enum2
            }));
            Assert.AreEqual("Enum1|Enum2|Enum3", CSVValueConverter.NumericArray<TestEnum>.ToString(new[] {
                TestEnum.Enum1,
                TestEnum.Enum2,
                TestEnum.Enum3
            }));

            Assert.AreEqual(Array.Empty<TestEnum>(), CSVValueConverter.NumericArray<TestEnum>.FromString(null));
            Assert.AreEqual(Array.Empty<TestEnum>(), CSVValueConverter.NumericArray<TestEnum>.FromString(""));
            Assert.AreEqual(Array.Empty<TestEnum>(), CSVValueConverter.NumericArray<TestEnum>.FromString(" "));
            Assert.AreEqual(new TestEnum[] { TestEnum.Enum1 }, CSVValueConverter.NumericArray<TestEnum>.FromString(" Enum1 "));
            Assert.AreEqual(new TestEnum[] { TestEnum.Enum1, TestEnum.Enum2 }, CSVValueConverter.NumericArray<TestEnum>.FromString("Enum1|Enum2 "));
            Assert.AreEqual(new TestEnum[] { TestEnum.Enum1, TestEnum.Enum2, TestEnum.Enum3 }, CSVValueConverter.NumericArray<TestEnum>.FromString("Enum1|Enum2 |  Enum3 "));
            Assert.Catch(() => CSVValueConverter.NumericArray<TestEnum>.FromString("||"));
            Assert.Catch(() => CSVValueConverter.NumericArray<TestEnum>.FromString("Enum1|blah"));
        }

        [Test]
        public void StringArrayToString()
        {
            void Test(string expected, string[] value)
            {
                Assert.AreEqual(expected, CSVValueConverter.StringArray.ToString(value));
            }

            Test("", null);
            Test("", new string[] { });
            Test("", new[] { "" });
            Test(" ", new[] { " " });
            Test(" a ", new[] { " a " });
            Test("a| b | ", new[] { "a", " b ", " " });
        }

        [Test]
        public void StringArrayFromString()
        {
            void Test(string value, string[] expected = null)
            {
                Assert.AreEqual(expected, CSVValueConverter.StringArray.FromString(value));
            }

            Test(null, Array.Empty<string>());
            Test("", Array.Empty<string>());
            Test(" ", new[] { " " });
            Test(" a ", new[] { " a " });
            Test(" a | b", new[] { " a ", " b" });
        }
    }
}
