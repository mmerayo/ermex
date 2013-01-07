// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
using System;



namespace ermeX.ConfigurationManagement.Settings
{
    internal interface IComponentSettings
    {
        //TODO:should be provided by the administrator 
        //TODO: FUTURE: from configuration component with special key
        Guid ComponentId { get; set; }
        int CacheExpirationSeconds { get; }
        Type ConfigurationManagerType { get; }
        bool DevLoggingActive { get; } // logs to console
    }
}