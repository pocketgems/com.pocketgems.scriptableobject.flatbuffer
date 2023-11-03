using System;
using PocketGems.Parameters.Editor.Validation.TreeDataModel;
using PocketGems.Parameters.Validation;
using UnityEngine.TestTools;

namespace PocketGems.Parameters.Editor.Validation
{
    [ExcludeFromCoverage]
    internal class ValidationTreeElement : TreeElement
    {
        public enum TreeElementType
        {
            Root,
            General,
            Interface,
            Error
        }

        public TreeElementType ElementType { get; private set; }
        public Type InterfaceType { get; set; }
        public ValidationError ValidationError { get; set; }

        public ValidationTreeElement(TreeElementType elementType, string name, int id) : base(name, Depth(elementType), id)
        {
            ElementType = elementType;
        }

        private static int Depth(TreeElementType elementType)
        {
            switch(elementType)
            {
                case TreeElementType.Root:
                    return -1;
                case TreeElementType.General:
                    return 0;
                case TreeElementType.Interface:
                    return 0;
                case TreeElementType.Error:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(elementType), elementType, null);
            }
        }
    }
}
