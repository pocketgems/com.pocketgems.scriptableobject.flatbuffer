using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;

namespace PocketGems.Parameters.Types
{
    public class ReadOnlyListContainerTest
    {
        private int[] _array;
        private Func<int> _countFunc;
        private ReadOnlyListContainer<int> _container;
        [SetUp]
        public void SetUp()
        {
            _array = new[] { 1, 2, 3, 4 };
            _countFunc = Substitute.For<Func<int>>();
            _countFunc().Returns(_array.Length);

            _container = new ReadOnlyListContainer<int>(_countFunc, x => _array[x]);
        }

        [Test]
        public void ReadingValues()
        {
            Assert.AreEqual(_array.Length, _container.Count);
            Assert.AreEqual(_array[0], _container[0]);
            Assert.AreEqual(_array[1], _container[1]);
            Assert.AreEqual(_array[2], _container[2]);
            Assert.AreEqual(_array[3], _container[3]);

            int index = 0;
            foreach (var val in _container)
            {
                Assert.AreEqual(_array[index], val);
                index++;
            }
            Assert.AreEqual(_array.Length, index);

            // count should be cached
            _countFunc.Received(1);
        }

        [Test]
        public void Enumerator()
        {
            int index = 0;
            var enumerator = _container.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(_array[index], enumerator.Current);
                index++;
            }

            enumerator.Reset();

            index = 0;
            while (enumerator.MoveNext())
            {
                Assert.AreEqual(_array[index], enumerator.Current);
                index++;
            }
        }

        [Test]
        public void Enumerable()
        {
            if (_container is IEnumerable e)
            {
                int index = 0;
                foreach (var val in e)
                {
                    Assert.AreEqual(_array[index], val);
                    index++;
                }

                Assert.AreEqual(_array.Length, index);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
