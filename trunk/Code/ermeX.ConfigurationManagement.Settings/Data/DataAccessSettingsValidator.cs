// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;

namespace ermeX.ConfigurationManagement.Settings.Data
{
    internal class DataAccessSettingsValidator : ISettingsValidator
    {
        private readonly IDalSettings _settings;

        public DataAccessSettingsValidator(IDalSettings settings)
        {
            _settings = settings;
        }

        #region ISettingsValidator Members

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            bool result = true;
            if (_settings.SchemasApplied.Count == 0)
            {
                errors.Add("Configuration:SchemasApplied: Need to define one schema");
                result = false;
            }

            if (string.IsNullOrEmpty(_settings.ConfigurationConnectionString))
            {
                errors.Add("Configuration:ConfigurationConnectionString: Need to define one connection string");
                result = false;
            }

            return result;
        }

        #endregion
    }
}