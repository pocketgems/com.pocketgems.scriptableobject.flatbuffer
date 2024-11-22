using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.DataGeneration.Operation.Editor;
using PocketGems.Parameters.Editor.Validation.TreeDataModel.Editor;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Validation;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Modified from Unity's provided example:
/// https://docs.unity3d.com/2021.3/Documentation/Manual/TreeViewAPI.html
/// http://files.unity3d.com/mads/TreeViewExamples.zip
/// </summary>
namespace PocketGems.Parameters.Editor.Validation.Editor
{
    [ExcludeFromCoverage]
    class ValidationWindow : EditorWindow
    {
        [Serializable]
        private enum ValidationState
        {
            NoData,
            NoErrors,
            Errors
        }

        // models
        [NonSerialized] private IReadOnlyList<ValidationError> _errors;
        [NonSerialized] private int _errorCount;
        [NonSerialized] private int _warningCount;
        [NonSerialized] private ValidationState _validationState = ValidationState.NoData;
        [NonSerialized] private long _executionMillis;

        // view states
        [NonSerialized] private bool _isInit;
        [NonSerialized] private bool _isDirty = true;
        [SerializeField] TreeViewState _treeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState _multiColumnHeaderState;
        SearchField _searchField;
        ValidationTreeView _treeView;
        private bool _showErrors = true;
        private bool _showWarnings = true;

        // red flickering notifier
        private bool _disableNextFlicker;
        private bool _enableFlicker;
        private bool _isFlickerOn;
        private Stopwatch _flickerStopwatch = new Stopwatch();
        private const float s_flickerDuration = 0.25f;

        // view
        private Texture2D _errorIcon;
        private Texture2D _warningIcon;
        private Texture2D _infoIcon;

        [MenuItem(MenuItemConstants.ValidationWindowPath, false, MenuItemConstants.ValidationWindowPriority)]
        public static void OpenWindow()
        {
            GetWindow(true);
        }

        public static ValidationWindow GetWindow(bool focus)
        {
            var window = GetWindow<ValidationWindow>(false, "Parameter Validation", focus);
            // ensure developers are aware of all of the columns
            window.minSize = new Vector2(900, 400);
            window.Repaint();
            return window;
        }

        public static bool HasOpenInstance() => HasOpenInstances<ValidationWindow>();

        internal void UpdateValidationResult(IReadOnlyList<ValidationError> errors, long executionMillis)
        {
            _isDirty = true;
            _errors = errors;
            _errorCount = 0;
            _warningCount = 0;
            if (_errors?.Count > 0)
            {
                foreach (var error in _errors)
                {
                    if (error.ErrorSeverity == ValidationError.Severity.Error)
                        _errorCount++;
                    else
                        _warningCount++;
                }
            }
            _executionMillis = executionMillis;
            _validationState = errors?.Count > 0
                ? ValidationState.NoErrors
                : ValidationState.Errors;

            _flickerStopwatch.Restart();
            _enableFlicker = !_disableNextFlicker && errors?.Count > 0;
            _disableNextFlicker = false;

            Repaint();
        }

        /// <summary>
        /// Function to serialize and save the latest errors so they can be displayed if the window is re-opened or reloaded
        /// </summary>
        /// <param name="errors">errors to save, null to clear all errors</param>
        internal static void SerializeToStorage(IReadOnlyList<ValidationError> errors)
        {
            ValidationWindowSerializer.SerializeToStorage(errors);
        }

        private Rect MultiColumnTreeViewRect => new Rect(20, 30, position.width - 40, position.height - 60);
        private Rect ToolbarRect => new Rect(20f, 10f, position.width - 40f, 20f);
        private Rect BottomToolbarRect => new Rect(20f, position.height - 18f, position.width - 40f, 16f);

        private void OnEnable()
        {
            // support keeping error state across editor window sessions - relying on Editor serialization doesn't seem to work.
            var errors = ValidationWindowSerializer.DeserializeFromStorage();
            if (errors != null && errors.Count > 0)
            {
                _disableNextFlicker = true;
                UpdateValidationResult(errors, 0);
            }
        }

        private void ClearValidationResults()
        {
            ValidationWindowSerializer.SerializeToStorage(null);
            _isDirty = true;
            _errors = null;
            _errorCount = 0;
            _warningCount = 0;
            _validationState = ValidationState.NoData;
        }

        private void InitIfNeeded()
        {
            if (!_isInit)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (_treeViewState == null)
                    _treeViewState = new TreeViewState();

                bool firstInit = _multiColumnHeaderState == null;
                var headerState = ValidationTreeView.CreateDefaultMultiColumnHeaderState(MultiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
                _multiColumnHeaderState = headerState;

                var multiColumnHeader = new MultiColumnHeader(headerState);
                multiColumnHeader.canSort = false;
                multiColumnHeader.height = MultiColumnHeader.DefaultGUI.minimumHeight;
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<ValidationTreeElement>(CreateTreeElements());
                _treeView = new ValidationTreeView(_treeViewState, multiColumnHeader, treeModel);
                _treeView.OnClickedTreeElement += OnClickedTreeElement;

                _searchField = new SearchField();
                _searchField.downOrUpArrowKeyPressed += _treeView.SetFocusAndEnsureSelectedItem;

                // found texture examples in AssetSettingsAnalyzeTreeView
                _errorIcon = EditorGUIUtility.FindTexture("console.errorIcon");
                _warningIcon = EditorGUIUtility.FindTexture("console.warnicon");
                _infoIcon = EditorGUIUtility.FindTexture("console.infoIcon");

                _isInit = true;
            }
        }

        private void OnClickedTreeElement(ValidationTreeElement element)
        {
            if (element.ElementType == ValidationTreeElement.TreeElementType.General)
                return;

            Type interfaceType = null;
            if (element.ElementType == ValidationTreeElement.TreeElementType.Error)
            {
                var validationError = element.ValidationError;
                if (validationError.InfoType == null)
                    return;
                interfaceType = validationError.InfoType;
                var identifier = validationError.InfoIdentifier;
                if (!string.IsNullOrEmpty(identifier))
                {
                    var searchText = $"{identifier} t:{nameof(ParameterScriptableObject)}";
                    var guids = AssetDatabase.FindAssets(searchText);
                    for (int i = 0; i < guids.Length; i++)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        var asset = AssetDatabase.LoadAssetAtPath<ParameterScriptableObject>(path);
                        if (asset == null)
                            continue;
                        if (asset.name != identifier)
                            continue;
                        if (!validationError.InfoType.IsInstanceOfType(asset))
                            continue;
                        EditorGUIUtility.PingObject(asset);
                        AssetDatabase.OpenAsset(asset);
                        return;
                    }

                    ParameterDebug.LogError($"Unable to find Parameter Scriptable Object with identifier {identifier}");
                }
            }
            else if (element.ElementType == ValidationTreeElement.TreeElementType.Interface)
            {
                interfaceType = element.InterfaceType;
            }
            else
            {
                ParameterDebug.LogError($"Unhandled type {element.ElementType}");
            }

            var baseName = NamingUtil.BaseNameFromInfoInterfaceName(interfaceType.Name);
            var csvFileName = NamingUtil.CSVFileNameFromBaseName(baseName, true);
            var csvPath = Path.Combine(EditorParameterConstants.CSV.Dir, csvFileName);
            if (File.Exists(csvPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    return;
                }
            }

            ParameterDebug.LogError($"File doesn't exist: {csvPath}");
        }

        private IList<ValidationTreeElement> CreateTreeElements()
        {
            List<ValidationTreeElement> elements = new List<ValidationTreeElement>();
            int idCount = 1;
            var rootElement = new ValidationTreeElement(ValidationTreeElement.TreeElementType.Root, "root", idCount++);
            elements.Add(rootElement);

            List<ValidationError> filteredErrors = null;
            if (_errors != null && _errors.Count > 0)
            {
                foreach (var error in _errors)
                {
                    if ((_showErrors && error.ErrorSeverity == ValidationError.Severity.Error) ||
                        (_showWarnings && error.ErrorSeverity == ValidationError.Severity.Warning))
                    {
                        if (filteredErrors == null)
                            filteredErrors = new();
                        filteredErrors.Add(error);
                    }
                }
            }

            if (!_showWarnings && _warningCount > 0)
            {
                string status = _warningCount == 1 ? "1 warning is hidden." : $"{_warningCount} warnings are hidden.";
                var parentElement = new ValidationTreeElement(ValidationTreeElement.TreeElementType.General, status, idCount++);
                elements.Add(parentElement);
            }

            if (!_showErrors && _errorCount > 0)
            {
                string status = _errorCount == 1 ? "1 error is hidden." : $"{_errorCount} errors are hidden.";
                var parentElement = new ValidationTreeElement(ValidationTreeElement.TreeElementType.General, status, idCount++);
                elements.Add(parentElement);
            }

            if (filteredErrors != null && filteredErrors.Count > 0)
            {
                List<ValidationError> sortedErrors = new(filteredErrors);
                int ErrorCompare(ValidationError a, ValidationError b)
                {
                    if (a.InfoType != b.InfoType)
                        return string.Compare(a.InfoType?.Name, b.InfoType?.Name, StringComparison.Ordinal);
                    if (a.InfoIdentifier != b.InfoIdentifier)
                        return string.Compare(a.InfoIdentifier, b.InfoIdentifier, StringComparison.Ordinal);
                    if (a.InfoProperty != b.InfoProperty)
                        return string.Compare(a.InfoProperty, b.InfoProperty, StringComparison.Ordinal);
                    return string.Compare(a.Message, b.Message, StringComparison.Ordinal);
                }

                sortedErrors.Sort(ErrorCompare);

                Type currentType = null;
                for (int i = 0; i < sortedErrors.Count; i++)
                {
                    var error = sortedErrors[i];
                    if (i == 0 || currentType != error.InfoType)
                    {
                        currentType = error.InfoType;
                        if (currentType == null)
                        {
                            var parentElement = new ValidationTreeElement(ValidationTreeElement.TreeElementType.General, "General", idCount++);
                            elements.Add(parentElement);
                        }
                        else
                        {
                            string baseName = null;
                            try
                            {
                                baseName = NamingUtil.BaseNameFromInfoInterfaceName(currentType.Name);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                            try
                            {
                                // in some cases, there are general errors about the struct type (e.g. missing validator)
                                if (baseName == null)
                                    baseName = NamingUtil.BaseNameFromStructInterfaceName(currentType.Name);
                            }
                            catch (Exception)
                            {
                                baseName = currentType.Name;
                            }
                            var parentElement = new ValidationTreeElement(ValidationTreeElement.TreeElementType.Interface, baseName, idCount++);
                            parentElement.InterfaceType = currentType;
                            elements.Add(parentElement);
                        }
                    }
                    var element = new ValidationTreeElement(ValidationTreeElement.TreeElementType.Error, error.InfoIdentifier ?? "", idCount++);
                    element.ValidationError = error;
                    elements.Add(element);
                }
            }

            if (_errors == null || _errors.Count == 0)
            {
                string status = _validationState == ValidationState.NoData ? "No Validation Results" : "No Errors";
                var parentElement = new ValidationTreeElement(ValidationTreeElement.TreeElementType.General, status, idCount++);
                elements.Add(parentElement);
            }

            return elements;
        }

        public void OnInspectorUpdate()
        {
            // turn on color
            if (_enableFlicker && !_isFlickerOn && _flickerStopwatch.Elapsed.TotalSeconds <= s_flickerDuration)
                Repaint();
            // turn off
            if (_enableFlicker && _isFlickerOn && _flickerStopwatch.Elapsed.TotalSeconds > s_flickerDuration)
                Repaint();
            // turn off
            if (!_enableFlicker && _isFlickerOn)
                Repaint();
        }

        private void OnGUI()
        {
            InitIfNeeded();

            ToolBar(ToolbarRect);
            TreeView(MultiColumnTreeViewRect);
            BottomToolBar(BottomToolbarRect);
        }

        private void ToolBar(Rect rect)
        {
            var buttonRect = new Rect(rect.x, rect.y, 180, EditorStyles.miniButton.fixedHeight);
            if (GUI.Button(buttonRect, "Validate Parameter Data"))
            {
                _disableNextFlicker = true;

                // temp enable validation if it's currently disabled
                var prevConfig = ParameterPrefs.AutoValidateDataOnAssetChange;
                ParameterPrefs.AutoValidateDataOnAssetChange = true;
                EditorParameterDataManager.GenerateData(GenerateDataType.All, out _);
                ParameterPrefs.AutoValidateDataOnAssetChange = prevConfig;
            }

            var clearRect = new Rect(rect.x + 185, rect.y, 100, EditorStyles.miniButton.fixedHeight);
            if (GUI.Button(clearRect, "Clear"))
            {
                ClearValidationResults();
            }

            // show error & warning toggle on the very right
            var errorButton = new GUIContent(_errorCount.ToString(), _errorIcon);
            Vector2 errorButtonSize = EditorStyles.toolbarButton.CalcSize(errorButton);
            var errorButtonRect = new Rect(
                rect.x + rect.width - errorButtonSize.x,
                rect.y,
                errorButtonSize.x,
                errorButtonSize.y);
            var newShowErrors = GUI.Toggle(errorButtonRect, _showErrors, errorButton, EditorStyles.toolbarButton);
            _isDirty |= newShowErrors != _showErrors;
            _showErrors = newShowErrors;

            var warningButton = new GUIContent(_warningCount.ToString(), _warningIcon);
            Vector2 warningButtonSize = EditorStyles.toolbarButton.CalcSize(warningButton);
            var warningButtonRect = new Rect(
                rect.x + rect.width - warningButtonSize.x - errorButtonSize.x,
                rect.y,
                warningButtonSize.x,
                warningButtonSize.y);
            var newShowWarnings = GUI.Toggle(warningButtonRect, _showWarnings, warningButton, EditorStyles.toolbarButton);
            _isDirty |= newShowWarnings != _showWarnings;
            _showWarnings = newShowWarnings;
            var searchRect = new Rect(
                rect.x + 290,
                rect.y,
                rect.width - 290 - errorButtonSize.x - warningButtonSize.x - 5,
                rect.height);
            _treeView.searchString = _searchField.OnGUI(searchRect, _treeView.searchString);
        }

        private void TreeView(Rect rect)
        {
            var prevColor = GUI.color;
            _isFlickerOn = _enableFlicker &&
                           _flickerStopwatch.Elapsed.TotalSeconds <= s_flickerDuration;
            if (_isFlickerOn)
                GUI.color = new Color(255f / 255f, 170f / 255f, 170f / 255f);

            if (_isDirty)
            {
                _treeView.treeModel.SetData(CreateTreeElements());
                _treeView.ExpandAll();
                _treeView.Reload();
                _isDirty = false;
            }
            _treeView.OnGUI(rect);

            GUI.color = prevColor;
        }

        private void BottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);
            using (new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("Expand All", style))
                {
                    _treeView.ExpandAll();
                }
                if (GUILayout.Button("Collapse All", style))
                {
                    _treeView.CollapseAll();
                }
                GUILayout.FlexibleSpace();
                if (_validationState != ValidationState.NoData && _executionMillis != 0)
                    GUILayout.Label($"Validation Execution Duration: {_executionMillis}ms");
            }
            GUILayout.EndArea();
        }
    }
}
