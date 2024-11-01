using System.Collections.Generic;
using UnityEngine.TestTools;

/// <summary>
/// Modified from Unity's provided example:
/// https://docs.unity3d.com/2021.3/Documentation/Manual/TreeViewAPI.html
/// http://files.unity3d.com/mads/TreeViewExamples.zip
/// </summary>
namespace PocketGems.Parameters.Editor.Validation.TreeDataModel.Editor
{
    [ExcludeFromCoverage]
    internal class TreeElement
    {
        private int m_ID;
        private string m_Name;
        private int m_Depth;
        private TreeElement m_Parent;
        private List<TreeElement> m_Children;

        public int depth
        {
            get { return m_Depth; }
            set { m_Depth = value; }
        }

        public TreeElement parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public List<TreeElement> children
        {
            get { return m_Children; }
            set { m_Children = value; }
        }

        public bool hasChildren
        {
            get { return children != null && children.Count > 0; }
        }

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public int id
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        public TreeElement()
        {
        }

        public TreeElement(string name, int depth, int id)
        {
            m_Name = name;
            m_ID = id;
            m_Depth = depth;
        }
    }

}


