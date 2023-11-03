namespace PocketGems.Parameters.Types
{
    public class LocalizedString
    {
        public string Key { get; }
        public string Text => ParameterLocalizationHandler.GetTranslation(Key);

        public LocalizedString(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Return the translated string in case developer accidentally forget to access the Text property
        /// </summary>
        /// <returns>The translated string.</returns>
        public override string ToString() => Text;
    }
}
