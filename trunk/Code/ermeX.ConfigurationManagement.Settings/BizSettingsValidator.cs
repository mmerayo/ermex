// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System.Collections.Generic;

namespace ermeX.ConfigurationManagement.Settings
{
    /// <summary>
    ///   Validator for ISettingsValidator
    /// </summary>
    internal class BizSettingsValidator : ISettingsValidator
    {
        private readonly IBizSettings _settings;

        public BizSettingsValidator(IBizSettings settings)
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