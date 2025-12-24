using System;
using System.Collections.Generic;
using PocketGems.Parameters.Editor.Validation.TreeDataModel.Editor;
using PocketGems.Parameters.Validation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

/// <summary>
/// Modified from Unity's provided example:
/// https://docs.unity3d.com/2021.3/Documentation/Manual/TreeViewAPI.html
/// http://files.unity3d.com/mads/TreeViewExamples.zip
/// </summary>
namespace PocketGems.Parameters.Editor.Validation.Editor
{
    [ExcludeFromCoverage]
    internal class ValidationTreeView : TreeViewWithTreeModel<ValidationTreeElement>
    {
        public delegate void ClickedTreeElement(ValidationTreeElement validationError);
        public event ClickedTreeElement OnClickedTreeElement;

        const float kRowHeights = 20f;

        enum Columns
        {
            Name,
            PropertyName,
            Icon,
            ErrorMessage
        }

        // view
        private readonly Texture2D _errorIcon;
        private readonly Texture2D _warningIcon;
        private readonly Texture2D _infoIcon;

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        public ValidationTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<ValidationTreeElement> model) : base(state, multicolumnHeader, model)
        {
            // Custom setup
            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI

            // found texture examples in AssetSettingsAnalyzeTreeView
            _errorIcon = EditorGUIUtility.FindTexture("console.errorIcon");
            _warningIcon = EditorGUIUtility.FindTexture("console.warnicon");
            _infoIcon = EditorGUIUtility.FindTexture("console.infoIcon");

            Reload();
        }

        // Note we We only build the visible rows, only the backend has the full tree information.
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            return rows;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            if (selectedIds.Count > 0)
                SingleClickedItem(selectedIds[0]);
        }

        protected override void SingleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);

            if (item == null)
                return;
            OnClickedTreeElement?.Invoke(((TreeViewItem<ValidationTreeElement>)item).data);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<ValidationTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<ValidationTreeElement> item, Columns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case Columns.Name:
                    {
                        // Default icon and label
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                    }
                    break;
                case Columns.Icon:
                    {
                        if (!string.IsNullOrEmpty(item.data.ValidationError?.Message))
                        {
                            var icon = _errorIcon;
                            if (item.data.ValidationError.ErrorSeverity == ValidationError.Severity.Warning)
                                icon = _warningIcon;
                            GUI.DrawTexture(cellRect, icon, ScaleMode.ScaleToFit);
                        }
                    }
                    break;
                case Columns.PropertyName:
                case Columns.ErrorMessage:
                    {
                        string value = "";
                        if (column == Columns.PropertyName)
                        {
                            var validationError = item.data.ValidationError;
                            value = validationError?.InfoProperty;
                            if (validationError?.StructKeyPath != null)
                                value = validationError?.StructKeyPath;
                            if (validationError?.StructProperty != null)
                                value = $"{value}.{validationError?.StructProperty}";
                        }
                        if (column == Columns.ErrorMessage)
                            value = item.data.ValidationError?.Message;
                        DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                    }
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 300,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Property Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 250,
                    minWidth = 150,
                    autoResize = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel")),
                    contextMenuText = "Asset",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 40,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Error Message"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 300,
                    minWidth = 150,
                    autoResize = true,
                    allowToggleVisibility = true
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(Columns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }
}
