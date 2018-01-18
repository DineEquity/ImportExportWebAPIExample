using System;
using System.Collections.Generic;

namespace ImportExportWebAPIExample
{
    /// <summary>
    /// This class handles all parameters that need to be collected from user (via WebInterface or commandline or any other mode)
    /// to complete import or export operation.
    /// </summary>    
    public class ImpExpRequest
    {
        /// <summary>
        /// Org Id for Multi-tenant database
        /// </summary>
        
        public string OrgCode { get; set; }
        
        /// <summary>
        /// This gives us at which level/scope (Property/Enterprise/Zone or RVC) we are trying to import or export data
        /// </summary>
        
        public long HierStrucId { get; set; }
        
        /// <summary>
        /// Selected Language for Import/Export
        /// </summary>
        
        public int LanguageId { get; set; }

        /// <summary>
        /// Import or Export
        /// </summary>
        
        public AllowedOperation SelectedOperation { get; set; }

        /// <summary>
        /// User will select from a drop down of types.
        /// Drop down list varies (Exportable list , Importable list) for the SelectedOperation 
        /// These lists will be build using reflection on interface types  IExportable  and IImportable
        /// </summary>
        
        public string ObjectType { get; set; }

        /// <summary>
        /// Based on the selected ObjectType above , we will present a
        /// list of optional column names(aka attribute names) that user can select.
        /// Mandatory attributes will be added by default and user will not have control on them.
        /// </summary>
        
        public List<string> SelectedObjectMembers { get; set; }

        /// <summary>
        /// User might request changes since last 'N' days for a given type.
        /// This date will be that date in the past. If 'N' is more than 7 or if this date is null
        /// we might have to return all the rows for that type at the given level(scope)
        /// </summary>
        
        public DateTime? DataSince { get; set; }


        /// <summary>
        /// Key is the attribute (from the list of SelectedObjectMemebers above)
        /// value is either asc,desc
        /// Refer to CollectionExportUtils.cs of how this will be used.
        /// </summary>
        
        public List<Tuple<string, string>> SortExpressions { get; set; }


        /// <summary>
        /// Format to export or import data
        /// </summary>
        
        public AllowedFormat SelectedFormat { get; set; }

        /// <summary>
        /// byte array of consumed file ( CSV, Excel or XML) in selectedFormat above for Import. 
        /// Check for max size we can consume (Configuration)
        /// </summary>
        
        public byte[] DataForImport { get; set; }

        /// <summary>
        /// Origin , WebApp , API , Scheduler etc.
        /// </summary>
        
        public RequestOrigin Origin { get; set; }

        /// <summary>
        /// Logged in User
        /// </summary>
        
        public string UserName { get; set; }

        /// <summary>
        /// Request Origin date
        /// </summary>
        
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Request status (on Response)
        /// </summary>
        
        public RequestStatus Status { get; set; }

        /// <summary>
        /// Request Id (on Response)
        /// </summary>
        
        public long RequestId { get; set; }

        /// <summary>
        /// Schedule Id (if this needs to be recurring. Allowed only on Exports.)
        /// </summary>
        
        public long? ScheduleId { get; set; }

        /// <summary>
        /// DataLevel ( on Export only ) - From which levels Data need to be Exported from.
        /// </summary>
        
        public DataLevel Level { get; set; }

        /// <summary>
        /// Name of request
        /// </summary>
        
        public string RequestName { get; set; }

        /// <summary>
        /// Id of Original scheduled request ( on Response )
        /// </summary>
        
        public long OriginalRequestId { get; set; }
        

        /// <summary>
        /// Validate the incoming request for valid parameters. This step is needed for sure
        /// if we are accepting a input paramter file command line more than a guided web UI.
        /// </summary>
        /// <returns></returns>
		public bool ValidateRequest()
		{			
			bool isValid = false;

			if (HierStrucId > 0 && LanguageId > 0 && !string.IsNullOrEmpty(ObjectType) 
				&& SelectedObjectMembers != null && SelectedObjectMembers.Count > 0)
			{
				if (SelectedOperation == AllowedOperation.Export)
				{
					isValid = true;
				}
				else if((SelectedOperation == AllowedOperation.Import && DataForImport != null && DataForImport.Length > 0))
				{
					isValid = true;
				}
			}
			return isValid;
		}
    }
}
