// /*---------------------------------------------------------------------------------------*/
// If you viewing this code.....
// The current code is under construction.
// The reason you see this text is that lot of refactors/improvements have been identified and they will be implemented over the next iterations versions. 
// This is not a final product yet.
// /*---------------------------------------------------------------------------------------*/
namespace ermeX.ConfigurationManagement.Status
{
    internal delegate void StatusChangedHandler(object sender, ComponentStatus newStatus);

    /// <summary>
    /// Handles the component status
    /// </summary>
    internal interface IStatusManager
    {
        event StatusChangedHandler StatusChanged;

        ComponentStatus CurrentStatus { get; set; }
        StatusManager.GlobalSync SyncEvents { get; }

        /// <summary>
        /// Caller waits until is running
        /// </summary>
        /// <returns></returns>
        void WaitIsRunning();
    }
}
