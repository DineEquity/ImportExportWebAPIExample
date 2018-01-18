namespace ImportExportWebAPIExample
{
    public enum AllowedOperation
    {
        Export,
        Import
    }

    public enum AllowedFormat
    {
        CSV,       
    }

    public enum RequestOrigin
    {
        WebApplication,
        WebApi,
        InternalApplication,
        Scheduler,
        Other
    }

    public enum RequestStatus
    {
        /// <summary>
        /// Request has some invalid parameters (This might be come into play with API to return status)
        /// </summary>
        Invalid,
        /// <summary>
        /// When a new request is created. This status is transient and should not be available once received successfully.
        /// </summary>
        New,
        /// <summary>
        /// Request received and successfully saved by the system for further process
        /// </summary>
        Received, 
        /// <summary>
        /// Ideally only one request should be in Processing state , to avoid bringing down the system
        /// </summary>
        Processing,
        /// <summary>
        /// If Import , all data should be committed successfully. if Export a new file should be available for download
        /// </summary>
        Complete,
        /// <summary>
        /// if Some no of rows below Threshold fail on Import(Failure rows should be available).
        /// </summary>
        CompletedWithErrors,
        /// <summary>
        /// Beyond threshold limit for failure.
        /// </summary>
        Error,
        /// <summary>
        /// if Request vanished in thin air :( sad state.
        /// </summary>
        Unknown,
        /// <summary>
        /// If user is unAuthroized to perform this operation.
        /// </summary>
        UnAuthorized,
        /// <summary>
        /// This request is scheduled and will be processed on scheduler.
        /// </summary>
        Scheduled
    }

    /// <summary>
    /// User should have ability to select the level of data they wish to see on export.
    /// by default will be the Choosen HierStruc.
    /// </summary>
    public enum DataLevel
    {
        SelectedHierarchy,
        SelectedHierarchyWithAncestors,
        SelectedHierarchyWithInherited,
        SelectedHierarchyWithAncestorsAndInherited
    }	
}
