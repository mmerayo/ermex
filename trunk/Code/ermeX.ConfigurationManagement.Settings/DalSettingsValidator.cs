using System.Collections.Generic;

namespace ermeX.ConfigurationManagement.Settings
{
    internal class DalSettingsValidator : ISettingsValidator
    {
        private readonly IDalSettings _settings;

        public DalSettingsValidator(IDalSettings settings)
        {
            _settings = settings;
        }

        #region ISettingsValidator Members

        public bool Validate(out List<string> errors)
        {
            //TODO
            errors = new List<string>();
            return true;
        }

        #endregion
    }
}