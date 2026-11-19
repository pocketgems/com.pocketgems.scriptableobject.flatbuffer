using NUnit.Framework;

namespace PocketGems.Parameters.Interface
{
    public class BaseMutableParameterTest
    {
        private class BaseMutableParameterSubclass : BaseMutableParameter
        {
            public BaseMutableParameterSubclass(IParameterManager parameterManager) : base(parameterManager)
            {
            }

            public override bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error)
            {
                error = null;
                return true;
            }

            public override bool RevertEditedProperty(string propertyName, out string error)
            {
                error = null;
                return true;
            }

            public bool TestTryGetOverride<T>(int index, out T parameterOverride) =>
                TryGetOverride(index, out parameterOverride);

            public bool TestTrySetOverride<T>(int index, T parameterOverride, int maxIndex, out string error) =>
                TrySetOverride(index, parameterOverride, maxIndex, out error);

            public bool TestTryRemoveOverride(int index, out string error) =>
                TryRemoveOverride(index, out error);

            public override IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager) => new BaseMutableParameterSubclass(
                parameterManager);
        }

        private BaseMutableParameterSubclass _parameter;

        [SetUp]
        public void SetUp()
        {
            _parameter = new BaseMutableParameterSubclass(null);
        }

        [Test]
        public void CoverageMethodCalls()
        {
            _parameter.EditProperty(null, null, null, out _);
            _ = _parameter.CreateLinkedMutableParameter(null);
        }

        [Test]
        public void Overrides()
        {
            const int max = 5;
            const int indexA = 4;
            const int valueA = 5;

            // set first
            Assert.That(_parameter.TestTryGetOverride<int>(indexA, out _), Is.False);
            Assert.That(_parameter.TestTrySetOverride<int>(indexA, valueA, max, out _), Is.True);
            Assert.That(_parameter.TestTryGetOverride<int>(indexA, out var intValue), Is.True);
            Assert.That(intValue, Is.EqualTo(valueA));

            // try to set again
            Assert.That(_parameter.TestTrySetOverride<int>(indexA, valueA, max, out var error), Is.False);
            Assert.That(error, Is.EqualTo("Overrides is already set."));

            // set 2nd
            const int indexB = 2;
            const string valueB = "my name";
            Assert.That(_parameter.TestTryGetOverride<int>(indexB, out _), Is.False);
            Assert.That(_parameter.TestTrySetOverride<string>(indexB, valueB, max, out _), Is.True);
            Assert.That(_parameter.TestTryGetOverride<string>(indexB, out var stringValue), Is.True);
            Assert.That(stringValue, Is.EqualTo(valueB));

            // clear
            _parameter.RemoveAllEdits();
            Assert.That(_parameter.TestTryGetOverride<int>(indexA, out _), Is.False);
            Assert.That(_parameter.TestTryGetOverride<int>(indexB, out _), Is.False);
        }

        [Test]
        public void RemoveOverride()
        {
            const int max = 5;
            const int index = 2;

            // remove with no overrides initialized
            Assert.That(_parameter.TestTryRemoveOverride(index, out var error), Is.False);
            Assert.That(error, Is.EqualTo("No overrides to remove."));

            // set then remove
            Assert.That(_parameter.TestTrySetOverride<int>(index, 99, max, out _), Is.True);
            Assert.That(_parameter.TestTryRemoveOverride(index, out error), Is.True);
            Assert.That(error, Is.Null);
            Assert.That(_parameter.TestTryGetOverride<int>(index, out _), Is.False);

            // removing last override nulls out _parameterOverrides — re-set should succeed (not hit "max size changed")
            Assert.That(_parameter.TestTrySetOverride<int>(index, 100, max, out error), Is.True);

            // double remove
            Assert.That(_parameter.TestTryRemoveOverride(index, out _), Is.True);
            Assert.That(_parameter.TestTryRemoveOverride(index, out error), Is.False);
            Assert.That(error, Is.EqualTo("No overrides to remove."));
        }
    }
}
