using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Interface
{
    public class ParameterOverridesTest
    {
        private ParameterOverrides _parameterOverrides;
        private const int MaxSize = 10;

        [SetUp]
        public void SetUp()
        {
            _parameterOverrides = new();
        }

        [Test]
        public void TryGetOverride()
        {
            const int setIndexA = 2;
            const string setObjectA = "MyString";
            const int setIndexB = 3;
            const long setObjectB = 1000;

            // set
            Assert.That(_parameterOverrides.SetEditedProperty(setIndexA, setObjectA, MaxSize, out _), Is.True);
            Assert.That(_parameterOverrides.SetEditedProperty(setIndexB, setObjectB, MaxSize, out _), Is.True);

            // get all override indexes
            for (int i = 0; i < MaxSize; i++)
            {
                var result = _parameterOverrides.TryGetOverride(i, out object value);
                if (i == setIndexA)
                {
                    Assert.That(result, Is.True);
                    Assert.That(value, Is.EqualTo(setObjectA));
                }
                else if (i == setIndexB)
                {
                    Assert.That(result, Is.True);
                    Assert.That(value, Is.EqualTo(setObjectB));
                }
                else
                {
                    Assert.That(result, Is.False);
                }
            }
        }

        [Test]
        public void TryGetOverride_Empty()
        {
            Assert.That(_parameterOverrides.TryGetOverride(1000, out _), Is.False);

            LogAssert.Expect(LogType.Error, "Attempting to fetch index outside of bounds.");
            Assert.That(_parameterOverrides.TryGetOverride(-1000, out _), Is.False);
        }

        [Test]
        public void TryGetOverride_Errors()
        {
            LogAssert.Expect(LogType.Error, "Attempting to fetch index outside of bounds.");
            Assert.That(_parameterOverrides.TryGetOverride(-1000, out _), Is.False);

            // set to set size
            Assert.That(_parameterOverrides.SetEditedProperty(1, "string", MaxSize, out _), Is.True);

            // max size checks
            LogAssert.Expect(LogType.Error, "Attempting to fetch index outside of bounds.");
            Assert.That(_parameterOverrides.TryGetOverride(MaxSize, out _), Is.False);
            LogAssert.Expect(LogType.Error, "Attempting to fetch index outside of bounds.");
            Assert.That(_parameterOverrides.TryGetOverride(MaxSize + 1, out _), Is.False);
            Assert.That(_parameterOverrides.TryGetOverride(MaxSize - 1, out _), Is.False);
        }

        [Test]
        public void SetEditedProperty()
        {
            Assert.That(_parameterOverrides.SetEditedProperty(1, "s", 2, out _), Is.True);
            Assert.That(_parameterOverrides.TryGetOverride(1, out var value), Is.True);
            Assert.That(value, Is.EqualTo("s"));
        }

        [Test]
        public void SetEditedProperty_Errors()
        {
            Assert.That(_parameterOverrides.SetEditedProperty(1, "string", -1, out var error), Is.False);
            Assert.That(error, Is.EqualTo("Max size must be a positive number - package bug."));

            Assert.That(_parameterOverrides.SetEditedProperty(100, "string", 10, out error), Is.False);
            Assert.That(error, Is.EqualTo("Max size must be greater than index - package bug."));

            Assert.That(_parameterOverrides.SetEditedProperty(1, "string", 5, out error), Is.True);

            Assert.That(_parameterOverrides.SetEditedProperty(1, "string", 6, out error), Is.False);
            Assert.That(error, Is.EqualTo("Max size changed - package bug."));

            Assert.That(_parameterOverrides.SetEditedProperty(1, "string 2", 5, out error), Is.False);
            Assert.That(error, Is.EqualTo("Overrides is already set."));
        }
    }
}
