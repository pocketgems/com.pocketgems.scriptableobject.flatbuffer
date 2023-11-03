using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace PocketGems.Parameters.Processors
{
    public class FilePostprocessorTest
    {
        private string _testDirPath;

        private string _validFilePath;
        private string[] _emptyList;
        private string[] _oneValidPath;
        private string[] _twoValidPaths;

        // delegate callback
        private IsValidFile _checkDelegate;
        private OnFilesChanged _callback;
        private bool _delegateCalled;
        private List<string> _createdOrChanged;
        private List<string> _deleted;
        private List<string> _movedFrom;
        private List<string> _movedTo;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _testDirPath = Path.Combine(new[] { "Test", "Test" });

            // paths
            _validFilePath = Path.Combine(_testDirPath, "Blah.csv");
            var invalidFileType = Path.Combine(_testDirPath, "Blah.txt");
            var invalidFilePath = Path.Combine("Test2", "Blah.csv");
            _emptyList = new string[] { };
            _oneValidPath = new[] { _validFilePath };
            _twoValidPaths = new[] { _validFilePath, invalidFilePath, invalidFileType, _validFilePath };

            // delegate setup
            _checkDelegate = delegate (string filePath)
            {
                return filePath.ToLower().EndsWith(".csv") && filePath.StartsWith(_testDirPath);
            };

            _callback = delegate (List<string> createdOrChanged, List<string> deleted,
                List<string> movedFrom, List<string> movedTo)
            {
                _delegateCalled = true;
                _createdOrChanged = createdOrChanged;
                _deleted = deleted;
                _movedFrom = movedFrom;
                _movedTo = movedTo;
            };
            FilePostprocessor.AddObserver(_checkDelegate, _callback);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Assert.IsTrue(FilePostprocessor.RemoveObserver(_checkDelegate, _callback));
        }

        [SetUp]
        public void SetUp()
        {
            _delegateCalled = false;
        }

        [Test]
        public void AddAndRemoveObserver()
        {
            // delegate setup
            IsValidFile checkDelegate = filePath => false;
            OnFilesChanged callback = delegate(List<string> createdOrChanged, List<string> deleted,
                List<string> movedFrom, List<string> movedTo)
            { };
            FilePostprocessor.AddObserver(checkDelegate, callback);
            Assert.IsTrue(FilePostprocessor.RemoveObserver(checkDelegate, callback));
            Assert.IsFalse(FilePostprocessor.RemoveObserver(checkDelegate, callback));
        }

        [Test]
        public void NoValidAssets()
        {
            FilePostprocessor.OnPostprocessAllAssets(_emptyList, _emptyList, _emptyList, _emptyList);
            Assert.IsFalse(_delegateCalled);
        }

        [Test]
        public void ImportedAssets_OneValid()
        {
            FilePostprocessor.OnPostprocessAllAssets(_oneValidPath, _emptyList, _emptyList, _emptyList);
            Assert.IsTrue(_delegateCalled);
            Assert.AreEqual(1, _createdOrChanged.Count);
            Assert.AreEqual(_validFilePath, _createdOrChanged[0]);
            Assert.IsNull(_deleted);
            Assert.IsNull(_movedFrom);
            Assert.IsNull(_movedTo);
        }

        [Test]
        public void ImportedAssets_TwoValid()
        {
            FilePostprocessor.OnPostprocessAllAssets(_twoValidPaths, _emptyList, _emptyList, _emptyList);
            Assert.IsTrue(_delegateCalled);
            Assert.AreEqual(2, _createdOrChanged.Count);
            Assert.AreEqual(_validFilePath, _createdOrChanged[0]);
            Assert.AreEqual(_validFilePath, _createdOrChanged[1]);
            Assert.IsNull(_deleted);
            Assert.IsNull(_movedFrom);
            Assert.IsNull(_movedTo);
        }

        [Test]
        public void DeletedAssets_OneValid()
        {
            FilePostprocessor.OnPostprocessAllAssets(_emptyList, _oneValidPath, _emptyList, _emptyList);
            Assert.IsTrue(_delegateCalled);
            Assert.IsNull(_createdOrChanged);
            Assert.AreEqual(1, _deleted.Count);
            Assert.AreEqual(_validFilePath, _deleted[0]);
            Assert.IsNull(_movedFrom);
            Assert.IsNull(_movedTo);
        }

        [Test]
        public void DeletedAssets_TwoValid()
        {
            FilePostprocessor.OnPostprocessAllAssets(_emptyList, _twoValidPaths, _emptyList, _emptyList);
            Assert.IsTrue(_delegateCalled);
            Assert.IsNull(_createdOrChanged);
            Assert.AreEqual(2, _deleted.Count);
            Assert.AreEqual(_validFilePath, _deleted[0]);
            Assert.AreEqual(_validFilePath, _deleted[1]);
            Assert.IsNull(_movedFrom);
            Assert.IsNull(_movedTo);
        }

        [Test]
        public void MovedFrom()
        {
            var movedTo = new[] { "1", "2", "3", "4" };
            FilePostprocessor.OnPostprocessAllAssets(_emptyList, _emptyList, _twoValidPaths, movedTo);
            Assert.IsTrue(_delegateCalled);
            Assert.IsNull(_createdOrChanged);
            Assert.IsNull(_deleted);
            Assert.AreEqual(2, _movedFrom.Count);
            Assert.AreEqual(_validFilePath, _movedFrom[0]);
            Assert.AreEqual(_validFilePath, _movedFrom[1]);
            Assert.AreEqual(2, _movedTo.Count);
            Assert.AreEqual("1", _movedTo[0]);
            Assert.AreEqual("4", _movedTo[1]);
        }

        [Test]
        public void MovedTo()
        {
            var movedFrom = new[] { "1", "2", "3", "4" };
            FilePostprocessor.OnPostprocessAllAssets(_emptyList, _emptyList, movedFrom, _twoValidPaths);
            Assert.IsTrue(_delegateCalled);
            Assert.IsNull(_createdOrChanged);
            Assert.IsNull(_deleted);
            Assert.AreEqual(2, _movedFrom.Count);
            Assert.AreEqual("1", _movedFrom[0]);
            Assert.AreEqual("4", _movedFrom[1]);
            Assert.AreEqual(2, _movedTo.Count);
            Assert.AreEqual(_validFilePath, _movedTo[0]);
            Assert.AreEqual(_validFilePath, _movedTo[1]);
        }
    }
}
