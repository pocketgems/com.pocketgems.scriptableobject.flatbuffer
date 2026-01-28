using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.DataGeneration.Operations.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using UnityEngine;

namespace PocketGems.Parameters.DataGeneration.Operation.Editor
{
    public class StringLocalizationOperationTest
    {
        private const string Key1 = "key1";
        private const string Key2 = "key2";
        private const string Key3 = "key3";
        private const string Script1 = "script1";
        private const string Script2 = "script2";

        private class ParameterScriptableObjectClass : ParameterScriptableObject
        {
            public List<string> KeysToAdd = new();
            public List<string> ScriptsToAdd = new();

            public override ValidationError[] ValidationErrors() => throw new System.NotImplementedException();

            public override void CollectLocalizationStrings(HashSet<string> localizationKeys, HashSet<string> localizedScript)
            {
                foreach (var key in KeysToAdd)
                    localizationKeys.Add(key);

                foreach (var script in ScriptsToAdd)
                    localizedScript.Add(script);
            }
        }

        [Test]
        public void Execute()
        {
            // context setup
            var context = new DataOperationContext();

            // scriptable object 1
            var metaDataMock1 = Substitute.For<IScriptableObjectMetadata>();
            var scriptableObject1 = (ParameterScriptableObjectClass)ScriptableObject.CreateInstance(typeof(ParameterScriptableObjectClass));
            scriptableObject1.KeysToAdd.Add(Key1);
            scriptableObject1.KeysToAdd.Add(Key2);
            scriptableObject1.ScriptsToAdd.Add(Script1);
            metaDataMock1.ScriptableObject.ReturnsForAnyArgs(scriptableObject1);

            // scriptable object 2
            var metaDataMock2 = Substitute.For<IScriptableObjectMetadata>();
            var scriptableObject2 =
                (ParameterScriptableObjectClass)ScriptableObject.CreateInstance(typeof(ParameterScriptableObjectClass));
            scriptableObject2.KeysToAdd.Add(Key2);
            scriptableObject2.KeysToAdd.Add(Key3);
            scriptableObject2.ScriptsToAdd.Add(Script2);
            metaDataMock2.ScriptableObject.ReturnsForAnyArgs(scriptableObject2);

            var metaDatas = new List<IScriptableObjectMetadata>();
            metaDatas.Add(metaDataMock1);
            metaDatas.Add(metaDataMock2);

            context.ScriptableObjectMetadatas[Substitute.For<IParameterInfo>()] = metaDatas;

            var operation = new StringLocalizationOperation(out var localizationKeys, out var localizedScript);
            operation.Execute(context);

            Assert.That(localizationKeys.Count, Is.EqualTo(3));
            Assert.That(localizationKeys, Does.Contain(Key1));
            Assert.That(localizationKeys, Does.Contain(Key2));
            Assert.That(localizationKeys, Does.Contain(Key3));
            Assert.That(localizedScript.Count, Is.EqualTo(2));
            Assert.That(localizedScript, Does.Contain(Script1));
            Assert.That(localizedScript, Does.Contain(Script2));
        }
    }
}
