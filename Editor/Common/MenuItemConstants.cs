namespace PocketGems.Parameters.Common.Editor
{
    public static class MenuItemConstants
    {
        // Priorities are spaced so Unity inserts a separator between groups (a gap of >= 11 draws a break):
        //   Regenerate All Data / Validation Window
        //   -----
        //   Generate CSVs / Open CSV Folder
        //   -----
        //   Regenerate Code / Config Panel
        public const string RegenerateDataPath = "Pocket Gems/Parameters/Regenerate All Data";
        public const int RegenerateDataPriority = 1;

        public const string ValidationWindowPath = "Pocket Gems/Parameters/Validation Window";
        public const int ValidationWindowPriority = 2;

        public const string GenerateCSVsPath = "Pocket Gems/Parameters/Generate CSVs";
        public const int GenerateCSVsPriority = 20;

        public const string OpenCSVFolderPath = "Pocket Gems/Parameters/Open CSV Folder";
        public const int OpenCSVFolderPriority = 21;

        public const string RegenerateCodePath = "Pocket Gems/Parameters/Regenerate Code";
        public const int RegenerateCodePriority = 40;

        public const string ConfigPanelPath = "Pocket Gems/Parameters/Config Panel";
        public const int ConfigPanelPriority = 41;
    }
}
