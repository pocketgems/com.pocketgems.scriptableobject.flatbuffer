using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Modified from Unity's provided example:
/// https://docs.unity3d.com/2021.3/Documentation/Manual/TreeViewAPI.html
/// http://files.unity3d.com/mads/TreeViewExamples.zip
/// </summary>
namespace PocketGems.Parameters.Editor.Validation.TreeDataModel.Editor
{
    public class TreeModelTest
    {
        [Test]
        public static void TestTreeModelCanAddElements()
        {
            var root = new TreeElement { name = "Root", depth = -1 };
            var listOfElements = new List<TreeElement>();
            listOfElements.Add(root);

            var model = new TreeModel<TreeElement>(listOfElements);
            model.AddElement(new TreeElement { name = "Element" }, root, 0);
            model.AddElement(new TreeElement { name = "Element " + root.children.Count }, root, 0);
            model.AddElement(new TreeElement { name = "Element " + root.children.Count }, root, 0);
            model.AddElement(new TreeElement { name = "Sub Element" }, root.children[1], 0);

            // Assert order is correct
            string[] namesInCorrectOrder = { "Root", "Element 2", "Element 1", "Sub Element", "Element" };
            Assert.AreEqual(namesInCorrectOrder.Length, listOfElements.Count, "Result count does not match");
            for (int i = 0; i < namesInCorrectOrder.Length; ++i)
                Assert.AreEqual(namesInCorrectOrder[i], listOfElements[i].name);

            // Assert depths are valid
            TreeElementUtility.ValidateDepthValues(listOfElements);
        }

        [Test]
        public static void TestTreeModelCanRemoveElements()
        {
            var root = new TreeElement { name = "Root", depth = -1 };
            var listOfElements = new List<TreeElement>();
            listOfElements.Add(root);

            var model = new TreeModel<TreeElement>(listOfElements);
            model.AddElement(new TreeElement { name = "Element" }, root, 0);
            model.AddElement(new TreeElement { name = "Element " + root.children.Count }, root, 0);
            model.AddElement(new TreeElement { name = "Element " + root.children.Count }, root, 0);
            model.AddElement(new TreeElement { name = "Sub Element" }, root.children[1], 0);

            model.RemoveElements(new[] { root.children[1].children[0], root.children[1] });

            // Assert order is correct
            string[] namesInCorrectOrder = { "Root", "Element 2", "Element" };
            Assert.AreEqual(namesInCorrectOrder.Length, listOfElements.Count, "Result count does not match");
            for (int i = 0; i < namesInCorrectOrder.Length; ++i)
                Assert.AreEqual(namesInCorrectOrder[i], listOfElements[i].name);

            // Assert depths are valid
            TreeElementUtility.ValidateDepthValues(listOfElements);
        }
    }
}
