using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using System.Configuration;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ImportExportWebAPIExample
{
    class WebAPIExample
    {
        #region Constants

        //const string DataServiceURL = "http://localhost:8282/ImportExport/DataService.svc/web/";
        //const string SecurityServiceURL = "http://localhost:8282/ImportExport/SecurityService.svc/web/GetUserAutheticationToken";

        #endregion

        static string DataServiceURL = ConfigurationManager.AppSettings["DataServiceURL"];
        static string SecurityServiceURL = ConfigurationManager.AppSettings["SecurityServiceURL"];
        static string UserName = ConfigurationManager.AppSettings["Username"];
        static string Password = ConfigurationManager.AppSettings["Password"];
        static string OrgCode = ConfigurationManager.AppSettings["Organization"];

        static string EXP_LocationName = ConfigurationManager.AppSettings["EXP_LocationName"];
        static string EXP_LanguageId = ConfigurationManager.AppSettings["EXP_LanguageId"];
        static string EXP_Level = ConfigurationManager.AppSettings["EXP_Level"];
        static string EXP_ObjectType = ConfigurationManager.AppSettings["EXP_ObjectType"];
        static string EXP_OrgCode = ConfigurationManager.AppSettings["EXP_OrgCode"];
        static string EXP_Origin = ConfigurationManager.AppSettings["EXP_Origin"];
        static string EXP_RequestName = ConfigurationManager.AppSettings["EXP_RequestName"];
        static string EXP_UserName = ConfigurationManager.AppSettings["EXP_UserName"];



        static void Main(string[] args)
        {
            args[0] = "SUBMITEXPORTREQUEST"; // hardcoded by Leon P.
            args[1] = "true"; // hardcoded by Leon P.
            if (args.Length < 1)
            {
                Console.WriteLine("Example program showing the ImportExport WebAPI methods");
                Console.WriteLine("\nWEBAPIExample <Command>");
                Console.WriteLine("\nCommands:");

                Console.WriteLine("AddSchedule\t\t\t\t\tAdds a new schedule. ");
                Console.WriteLine("GetBuildVersion\t\t\t\t\tBuild version of API. ");
                Console.WriteLine("GetExportableTypes/{userName}/{*orgCode}\ttype of objects exportable\n\t\t\t\t\t\t from this service. ");
                Console.WriteLine("GetFileData/{requestId}/{userName}/{*orgCode}\tfile bytes for a given export\n\t\t\t\t\t\t request. ");
                Console.WriteLine("GetHierarchyStructure/{userName}/{*orgCode}\tHierarchy objects(Enterprise,\n\t\t\t\t\t\t Properties, RVC's and Zones)\n\t\t\t\t\t\t available for the current\n\t\t\t\t\t\t user in a given organization. ");
                Console.WriteLine("GetImportableTypes/{userName}/{*orgCode}\ttype of objects importable from\n\t\t\t\t\t\t this service. ");
                Console.WriteLine("GetObjectInfo/{userName}/{*orgCode}\t\tHelp Info : Returns Object\n\t\t\t\t\t\t Information. ");
                Console.WriteLine("GetPropertiesForType/{fullyQualifiedTypeName}\n/{userName}/{*orgCode}\t\t\t\tattributes (Columns) of objects\n\t\t\t\t\t\t available on a given object. ");
                Console.WriteLine("GetRequests/{days}/{userName}/{*orgCode}\trequests made by this users org\n\t\t\t\t\t\t for specified upto 30 days\n\t\t\t\t\t\t with latest status. Includes\n\t\t\t\t\t\t originally scheduled active\n\t\t\t\t\t\t requests. ");
                Console.WriteLine("GetRequestStatus/{requestId}/{userName}\n/{*orgCode}\t\t\t\t\trequest object with latest\n\t\t\t\t\t\t status. ");
                Console.WriteLine("GetSchedules/{userName}/{*orgCode}\t\tschedules available in this\n\t\t\t\t\t\t organization ");
                Console.WriteLine("GetServerTimeWithZone\t\t\t\tserver current time and time\n\t\t\t\t\t\t zone. ");
                Console.WriteLine("GetSupportedLanguages/{userName}/{*orgCode}\tSimphony configured lanugages\n\t\t\t\t\t\t for a given organization. ");
                Console.WriteLine("SubmitExportRequest\t\t\t\tSubmit a request for Export. ");
                Console.WriteLine("SubmitImportRequest\t\t\t\tSubmit a request for Import. ");
                Console.WriteLine("UpdateScheduleStatus\t\t\t\tEnables or disables a schedule. ");
                Console.WriteLine("AddSchedule\t\t\t\t\tCreates Demo Schedule. ");
                Console.WriteLine("setScheduleActive <true/false>\t\t\tChange Active Status of schedule. ");

            }
            else
            {
                switch (args[0].ToUpper())
                {
                    //POST Method Calls
                    case "SUBMITEXPORTREQUEST":
                        if (args.Length > 1)
                            SubmitExportRequest(bool.Parse(args[1]));
                        else
                            SubmitExportRequest();
                        break;
                    case "SUBMITIMPORTREQUEST":
                        if (args.Length > 1)
                            SubmitImportRequest(args[1]);
                        else
                            SubmitImportRequest(@"C:\temp\FGTest.csv");
                        break;
                    case "ADDSCHEDULE":
                        addSchedule();
                        break;
                    case "SETSCHEDULEACTIVE":
                        setScheduleActive(args[1], bool.Parse(args[2]));
                        break;
                    //GET Method calls
                    case "GETREQUESTS":
                        GetRequests(args[1]);
                        break;
                    default:
                        if (args.Length > 1)
                            GetGeneric(args[0], args[1]);  //Optional argument for filePath. filePath is only useful for GetFileData.
                        else
                            GetGeneric(args[0]); //This method deals with all simple commands
                        break;

                }

            }

#if DEBUG
            Console.ReadKey();
#endif
        }

        #region GET Methods

        /// <summary>
        /// This method just sends the first argument of the console app to the webservice as is and returns the returned data to the console
        /// list of strings. This is sufficient for most of the GET requests as the required parameters are all described in one string
        /// e.g GetRequestStatus/{requestId}/{*orgCode}.
        /// 
        /// Only GETFILEDATA needs to be handled differently, as it returns an array of bytes for export results.
        /// </summary>
        private static void GetGeneric(string methodName, string filePath = "c:\\temp\\GETFILEDATARESULTS.csv")
        {
            // Pass server Url, Operation Name and Parameter (Optional)
            int i;
            byte[] byteArray;
            String[] arr;
            string responseStr;
            var context = new APBMenuCatalogEntities();
            //MajorGroup mgtable = new MajorGroup();
            Regex regexObj = new Regex(@"[^\d]");
            WebClient client = new WebClient();
            try
            {
                if (GetAuthToken(client)) //Attempt to get authorization from the security service.
                {
                    if (methodName.ToUpper().StartsWith("GETFILEDATA"))  //returns a file as a byte array
                    {

                        byte[] json = client.DownloadData(DataServiceURL + "/" + methodName);   //Attempt to call webservice method and retrieve a byte array.

                        responseStr = System.Text.Encoding.UTF8.GetString(json);
                        arr = responseStr.Substring(1, responseStr.Length - 1).Split(',');
                        byteArray = new byte[arr.Length];
                        for (i = 0; i <= arr.Length - 1; i++)
                        {
                            arr[i] = regexObj.Replace(arr[i], ""); // format all values in arr[i] to be numeric or else GPF
                            byteArray[i] = Convert.ToByte(arr[i]);
                        }
                        var str = System.Text.Encoding.Default.GetString(byteArray);
                        str = str.TrimEnd('\r', '\n'); //Remove last CRLF

                        var csvRecords = str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToList(); //Skip column names, create a list of MajorGroup records
                                                                                                                                    //var dbSetProperties = typeof(APBMenuCatalogEntities).GetProperties().Where(p => p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                        if (string.Compare(EXP_ObjectType, "MajorGroup") == 0)
                        {
                            context.MajorGroups.RemoveRange(context.MajorGroups.Where(c => c.Id > 0)); // delete all records prior to insertion of new records
                            context.SaveChanges();
                            csvRecords.ForEach(m =>
                            {
                                MajorGroup mg = m.ToMajorGroup();
                                context.MajorGroups.Add(mg);
                            }

                            );

                        }
                        else if (string.Compare(EXP_ObjectType, "FamilyGroup") == 0)
                        {


                            context.FamilyGroups.RemoveRange(context.FamilyGroups.Where(c => c.Id > 0)); // delete all records prior to insertion of new records
                            context.SaveChanges();
                            csvRecords.ForEach(m =>
                            {
                                FamilyGroup mg = m.ToFamilyGroup();
                                context.FamilyGroups.Add(mg);
                            }

                            );
                            
                        }
                        else if (string.Compare(EXP_ObjectType, "Hierarchy") == 0)
                        {


                            context.Hierarchies.RemoveRange(context.Hierarchies.Where(c => c.Id > 0)); // delete all records prior to insertion of new records
                            context.SaveChanges();
                            csvRecords.ForEach(m =>
                            {
                                Hierarchy mg = m.ToHierarchy();
                                context.Hierarchies.Add(mg);
                            }

                            );

                        }

                        //var MajorGroupItems = from line in str
                        //    .Split(new string[] { Environment.NewLine }, StringSplitOptions.None) //Split each row
                        //    .Skip(1) //Skip column names
                        //            let col = line.Split(',') //split each column
                        //            select new MajorGroup()
                        //            {
                        //                Id = Convert.ToInt64(col[0]),
                        //                ObjectNumber = Convert.ToInt64(col[1]),
                        //                HierarchyId = Convert.ToInt64(col[2]),
                        //                Name = col[3].Trim(charsToTrim),
                        //                ReportGroup = Convert.ToInt64(col[4])


                        //            };




                        //File.WriteAllBytes(filePath, str);  //Save the byte array as a file to the path specified in filePath.
                        // File.WriteAllText(filePath, str);  //Save the string as a file to the path specified in filePath.
                        context.SaveChanges();
                    }
                    else        //retur ns simple string results
                    {
                        // Operations:
                        //methodName = "GetImportableTypes";
                        //methodName = "GetExportableTypes";
                        // methodName = "GetBuildVersion";
                        // methodName = "GetHierarchyStructure";
                        // methodName = "GetObjectInfo";
                        // methodName = "GetServerTimeWithZone"; 400 bad request
                        // methodName = "GetSupportedLanguages";
                        //methodName = "UpdateScheduleStatus"; method not allowed http 405

                        var json = client.DownloadString(DataServiceURL + "/" + methodName + "/api spec/");   //Attempt to call webservice method and retrieve a string.
                        //var json = client.DownloadString(DataServiceURL + "/" + methodName );   //Attempt to call webservice method and retrieve a string.
                        ConsoleWriteJsonList(json); //Generates a List from the returned data
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }





        /// <summary>
        /// This call is never directly called and only exists as an example.
        /// GetPropertiesForType requires a fully qualified name for the type you wish to get information on. This full name is one of those returned
        /// by the GetExportableTypes method.
        /// </summary>
        static void GetPropertiesForType(string TypeName)
        {
            WebClient client = new WebClient();
            GetAuthToken(client);

            //TypeName is a fully qualified name such as com.oracle.pos.fb.simphony.ObjectModel.ImpExpObjects.FamilyGroup as returned by GetExportableTypes
            var json = client.DownloadString(DataServiceURL + "GetPropertiesForType/" + TypeName);  //i.e GetPropertiesForType/com.oracle.pos.fb.simphony.ObjectModel.ImpExpObjects.FamilyGroup
            ConsoleWriteJsonList(json);
        }

        /// <summary>
        /// This call is never directly called and only exists as an example.
        /// Help returns a html page with a list of all the current webservice methods. 
        /// </summary>
        static void GetHelp()
        {
            WebClient client = new WebClient();
            GetAuthToken(client);

            //this returns a html page with a list of all the current webservice methods. It won't be very readable in the console.
            var json = client.DownloadString(DataServiceURL + "Help");
            ConsoleWriteJsonList(json);
        }

        /// <summary>
        /// This call is never directly called and only exists as an example.
        /// GetSchedules will return a json object containing all the schedule information. You can see an example of serializing this object with 
        /// the RquestSchedule class in the setScheduleActive method.
        /// </summary>
        /// <param name="OrgCode"></param>
        static void GetSchedules(string OrgCode)
        {
            WebClient client = new WebClient();
            GetAuthToken(client);

            var json = client.DownloadString(DataServiceURL + "GetSchedules//" + OrgCode);
            ConsoleWriteJsonList(json);
        }

        /// <summary>
        /// This is a short cut call to GetRequests that accepts username as a parameter. It is mostly used for quick testing and provides an 
        /// example of building a requesting from some variables in order to correctly populate the parameters. You can still call the full
        /// GetRequests/{days}/{userName}/{*orgCode} from the command line and it will be processed by GetGeneric normally.
        /// </summary>
        /// <param name="userName"></param>
        static void GetRequests(string userName)
        {
            string days = "30";
            WebClient client = new WebClient();
            GetAuthToken(client);

            var json = client.DownloadString(DataServiceURL + "GetRequests//" + days + "/" + userName + "/" + OrgCode);
            ConsoleWriteJsonList(json);
        }

        /// <summary>
        /// This is a short cut call to GetRequests that accepts username as a parameter. It is mostly used for quick testing and provides an 
        /// example of building a call that gets the requests that have been submitted today. You can still call the full
        /// GetRequests/{days}/{userName}/{*orgCode} from the command line and it will be processed by GetGeneric normally.
        /// </summary>
        /// <param name="userName"></param>
        static void GetTodaysRequests(string userName)
        {
            string days = "0";
            WebClient client = new WebClient();
            GetAuthToken(client);

            var json = client.DownloadString(DataServiceURL + "GetRequests//" + days + "/" + userName + "/" + OrgCode);
            ConsoleWriteJsonList(json);
        }

        #endregion

        #region POST Methods

        /// <summary>
        /// Set the schedule as Active or InActive. Example using .Net DataContractSerializer.
        /// </summary>
        /// <param name="scheduleID">Schedule identifier</param>
        /// <param name="setScheduleActive">Sets the schedule as active (true) or inactive (false)</param>
        private static void setScheduleActive(string scheduleID, bool setScheduleActive)
        {

            long schedID = 0;       //Set to zero for checking against later.
            long.TryParse(scheduleID, out schedID);  //Check parameter is of the correct type.

            WebClient client = new WebClient();

            if (GetAuthToken(client)) //Attempt to get authorization from the security service.
            {
                var json = client.DownloadString(DataServiceURL + "/getschedules/" + OrgCode);   // Get a list of current schedules

                // Create the Json serializer and parse the response
                List<RequestSchedule> ScheduleList = new List<RequestSchedule>(); //Prepare a list for the schedules

                //Deserialize using the RequestSchedule class. There is a useful article covering this at
                // http://blog.anthonybaker.me/2013/05/how-to-consume-json-rest-api-in-net.html 
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<RequestSchedule>));
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    ScheduleList = (List<RequestSchedule>)serializer.ReadObject(ms);
                }

                //search the list of RequestSchedule objects for one matching the scheduleID provided as a parameter
                RequestSchedule schedule = ScheduleList.Find(rs => rs.Id == schedID);

                if (schedule != null)  //If we found ScheduleID in the list...
                {
                    schedule.Active = setScheduleActive; //Set the schedule status per the isScheduleActive parameter.
                    if (schedule.StartDate < DateTime.Now)
                        schedule.StartDate = DateTime.Now.AddHours(5);  //Currently the startdate must be in the future to update values.

                    bool validated = schedule.IsValid(); //Check that the updated schedule is valid (which it should be as we grabbed an existing one)
                    if (validated)
                    {
                        client.Headers.Add("Content-Type", "application/json");  //Let the server know what type of content we are sending
                        //Serialize and send the updated schedule to the webservice.
                        json = client.UploadString(DataServiceURL + "UpdateScheduleStatus", Serialize(schedule));
                        Console.WriteLine(json);
                        //Tell the user we are successful, and remind them what we did.
                        Console.WriteLine(string.Format("Schedule ID {0} is set to {1}", schedID, setScheduleActive.ToString()));
                    }
                    else
                    {
                        // This will check for common validation errors (see RequestSchedule class) and return them into a list
                        List<string> ValidationErrors = schedule.Validate();

                        //Display the list of problems.
                        Console.WriteLine("Schedule Validation Error");
                        foreach (string item in ValidationErrors)
                            Console.WriteLine(item.ToString());
                    }

                }
                else
                {
                    Console.WriteLine("Provided ScheduleID not found!"); //We did not find ScheduleID in the list...
                }
            }
        }

        /// <summary>
        /// Example of sending a new schedule type from scratch. Parses and sends Json manually, avoiding the use of Json.NET or .NET serialization.
        /// </summary>
        private static void addSchedule()
        {
            RequestSchedule schedule = new RequestSchedule();

            // set default properties

            Dictionary<string, dynamic> IEFieldDict = new Dictionary<string, dynamic>();

            IEFieldDict.Add("Active", true);
            IEFieldDict.Add("Monday", true);
            IEFieldDict.Add("Tuesday", true);
            IEFieldDict.Add("Wednesday", true);
            IEFieldDict.Add("Thursday", true);
            IEFieldDict.Add("Friday", true);
            IEFieldDict.Add("Saturday", true);
            IEFieldDict.Add("Sunday", true);
            IEFieldDict.Add("TimeOfDay", string.Format(@"\/Date({0})\/", ToUnixEpoch(new DateTime(2000, 1, 1, 8, 0, 0), true)));
            IEFieldDict.Add("OrgCode", OrgCode);
            IEFieldDict.Add("Name", "NewManualSchedule2");
            IEFieldDict.Add("StartDate", string.Format(@"\/Date({0})\/", ToUnixEpoch(DateTime.Now.AddHours(5), true)));
            IEFieldDict.Add("UserName", UserName);

            StringBuilder sb = new StringBuilder();

            //Build the Json string from the items added above.
            foreach (KeyValuePair<string, dynamic> item in IEFieldDict)
            {
                if (item.Value is string && !item.Value.Substring(0, 1).Equals("["))
                    sb.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\",");
                else if (item.Value is Boolean)
                    sb.Append("\"" + item.Key + "\"" + ":" + (item.Value ? "true" : "false") + ", ");
                else
                    sb.Append("\"" + item.Key + "\"" + ":" + item.Value + ", ");
            }

            string FinalJson = "{" + sb.ToString().Substring(0, sb.Length - 1) + "}";  //Surround the string with {} to meet Json specifications

            WebClient client = new WebClient();
            GetAuthToken(client);

            client.Headers.Add("Content-Type", "application/json");  //Let the server know what type of content we are sending
            var json = client.UploadString(DataServiceURL + "AddSchedule", FinalJson);
            Console.WriteLine(json);
        }


        /// <summary>
        /// Exmaple showing prepared import and export. 
        /// </summary>
        /// <param name="filepath">Example Only. If filepath is populated with a valid file this will do an import of the file, otherwise it will do an export
        /// using default values.</param>
        static void SubmitExportRequest(bool WaitForResults = false)
        {
            // Life cycle of a Export
            //  1.  Send an export request via SubmitImportExportRequest, possibly linking it to a schedule (See GetSchedules and UpdateScheduleStatus)
            //  2.  Check Export status with GetRequestStatus.
            //  3.  Once the request has been completed, download it with GetFileData

            String FinalJson;
            if (!BuildImportExportString(AllowedOperation.Export, out FinalJson))
            {
                Console.WriteLine("Unable to build export Json file.");
                return; //Probable building string so cannot continue
            }

            WebClient client = new WebClient();
            GetAuthToken(client);
            client.Headers.Add("Content-Type", "application/json");  //Let the server know what type of content we are sending
            var json = client.UploadString(DataServiceURL + "SubmitImportExportRequest", FinalJson);

            if (WaitForResults == true)
            {
                // Get Request ID from reply  -- Expected reply format is {"RequestId":104,"Status":2}
                string[] JsonResponses = json.Substring(1, json.Length - 2).Split(',');
                int requestID;
                int.TryParse(Regex.Match(JsonResponses[0], @"\d+").Value, out requestID);  //strip non numeric values from string to get request ID
                RequestStatus statusCode = (RequestStatus)Enum.Parse(typeof(RequestStatus), Regex.Match(JsonResponses[1], @"\d+").Value);
                if (requestID != 0)
                {
                    MonitorRequestFor(requestID); //Checks request status every 5 minutes then attempts to download once status becomes Completed.
                    WaitForResults = false;
                    Console.WriteLine(json); //lp temp
                }
                else
                    Console.WriteLine("Invalid request. Status: {0}", statusCode.ToString());
            }
            else
                Console.WriteLine(json);
        }

        private static void MonitorRequestFor(int requestID, int timeoutMinutes = 1)
        {
            // Check status of Request
            WebClient client = new WebClient();
            if (!GetAuthToken(client))
                return;
            bool keepTrying = true;
            DateTime startTime = DateTime.UtcNow;
            RequestStatus statusCode = RequestStatus.Unknown;

            Console.WriteLine("Checking status of request {0}", requestID);
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable && keepTrying)
                {
                    try
                    {
                        //string jsonResponse = client.DownloadString(DataServiceURL + "GetRequestStatus/" + requestID + "/" + OrgCode);
                        string jsonResponse = client.DownloadString(DataServiceURL + "GetRequestStatus/" + requestID + "/" + "api spec");
                        statusCode = (RequestStatus)Enum.Parse(typeof(RequestStatus), Regex.Match(jsonResponse, "\"Status\":(\\d*)").Groups[1].Value);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex.Message);
                        continue;
                    }

                    switch (statusCode)
                    {
                        case RequestStatus.Received:
                        case RequestStatus.Scheduled:
                        case RequestStatus.Processing:
                            Console.WriteLine("{0}: Waiting...", statusCode.ToString());
                            break;
                        case RequestStatus.Complete:
                        case RequestStatus.CompletedWithErrors:
                        case RequestStatus.Error:
                        case RequestStatus.Invalid:
                        case RequestStatus.UnAuthorized:
                            Console.WriteLine("Error waiting for completion: Returned status = {0}", statusCode.ToString());
                            keepTrying = false;
                            break;
                        default:
                            Console.WriteLine("Unknown Status. Aborting.");
                            keepTrying = false;
                            break;
                    }
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));  // For real life applications use Task Scheduler rather than this.
                }
            } while (keepTrying && DateTime.UtcNow - startTime < TimeSpan.FromMinutes(timeoutMinutes) && Console.ReadKey(true).Key != ConsoleKey.Escape);

            // Download Request
            if (statusCode == RequestStatus.Complete || statusCode == RequestStatus.CompletedWithErrors)
            {
                string filePath = string.Format("C:\\temp\\RequestNo{0}.csv", requestID);  //Create unique file name
                //GetGeneric("GETFILEDATA/" + requestID + "/" + OrgCode, filePath);  //No need to create a special method for this.
                GetGeneric("GETFILEDATA/" + requestID + "/" + "api spec", filePath);  //No need to create a special method for this.
                //GetGenericLeon("GETFILEDATA/" + requestID + "/" + "api spec", filePath);  //No need to create a special method for this.
            }
            else
            {
                Console.WriteLine("Unable to retrieve results.");
                if (keepTrying)
                    Console.WriteLine("Timeout exceeded before export completed. Last status reported is {0}", statusCode.ToString());
                else
                {   //If an error has occurred and a requestID has been returned then it will be pointing at an error log 
                    // that we can download. 
                    string filePath = string.Format("C:\\temp\\ErrorNo{0}.csv", requestID);  //Create unique file name
                    GetGeneric("GETFILEDATA/" + requestID + "/" + OrgCode, filePath);  //No need to create a special method for this.
                    if (File.Exists(filePath))
                        Console.WriteLine("Error log downloaded to {0}", filePath);
                }
            }
        }

        static void SubmitImportRequest(string filePath)
        {
            // Life cycle of a Import
            //  1.  Send an Import request via SubmitImportExportRequest.
            //  2.  Check Export status with GetRequestStatus.

            // Check the filepath is pointing at a valid file - If so then do an Import.

            String FinalJson;
            if (!BuildImportExportString(AllowedOperation.Import, out FinalJson, filePath))
                return; //Probable building string so cannot continue

            WebClient client = new WebClient();
            GetAuthToken(client);
            client.Headers.Add("Content-Type", "application/json");  //Let the server know what type of content we are sending
            var json = client.UploadString(DataServiceURL + "SubmitImportExportRequest", FinalJson);
            Console.WriteLine(json);
        }


        /// <summary>
        /// Build a Import or Export Json string manually.
        /// </summary>
        /// <param name="IEType">Specifies whether this is an Import or Export operation. filePath is required for Import.</param>
        /// <param name="FinalJson">Returned string containing the complete Json Import/Export request</param>
        /// <param name="filePath">required for Import, ignored for export.</param>
        /// <returns>false if unable to build the Json string</returns>
        private static bool BuildImportExportString(AllowedOperation IEType, out String FinalJson, string filePath = "")
        {
            FinalJson = "";
            if (IEType == AllowedOperation.Export)
            {
                Dictionary<string, dynamic> IEFieldDict = new Dictionary<string, dynamic>();
                //IEFieldDict.Add("DataSince", string.Format(@"\/Date({0})\/", ToUnixEpoch(DateTime.Now.AddDays(-30), true)));

                int HierStructID;
                int LanguageID;

                if (!TryGetHierStructID(string.IsNullOrEmpty(EXP_LocationName) ? "Bar" : EXP_LocationName, out HierStructID, out LanguageID))  //Gets HierStructID & language for given location name. Returns true if found.
                {
                    Console.WriteLine("Unable to find the property ({0}) in the Hierarchy.", string.IsNullOrEmpty(EXP_LocationName) ? "DE LAB ZONE 01" : EXP_LocationName);
                    return false; // Can't continue without HierStructID 
                }
                IEFieldDict.Add("HierStrucId", HierStructID);
                IEFieldDict.Add("LanguageId", LanguageID);
                IEFieldDict.Add("Level", string.IsNullOrEmpty(EXP_Level) ? (int)DataLevel.SelectedHierarchy : int.Parse(EXP_Level));

                string FullObjectName;
                if (!TryGetExportObject(string.IsNullOrEmpty(EXP_ObjectType) ? "MenuItemPrice" : EXP_ObjectType, out FullObjectName))  //Gets Full Object Name. Returns true if found.
                {
                    Console.WriteLine("Unable to find the Object(FamilyGroup) in available exportable objects.");
                    return false; // Can't continue without an Object 
                }
                IEFieldDict.Add("ObjectType", FullObjectName);

                string ObjectMembers = GetColumnsForObject(FullObjectName);

                IEFieldDict.Add("SelectedObjectMembers", ObjectMembers);
                //IEFieldDict.Add("OrgCode", string.IsNullOrEmpty(EXP_OrgCode) ? OrgCode : EXP_OrgCode );   //orgcode from constants
                IEFieldDict.Add("Origin", string.IsNullOrEmpty(EXP_Origin) ? (int)RequestOrigin.WebApi : int.Parse(EXP_Origin));   //see enums for options.
                IEFieldDict.Add("RequestDate", string.Format(@"\/Date({0})\/", ToUnixEpoch(DateTime.Now), true));
                IEFieldDict.Add("RequestName", string.IsNullOrEmpty(EXP_RequestName) ? "DemoImportRequest" : EXP_RequestName);
                IEFieldDict.Add("SelectedFormat", (int)AllowedFormat.CSV);   //see enums for options.
                IEFieldDict.Add("SelectedOperation", (int)AllowedOperation.Export);   //see enums for options.
                //IEFieldDict.Add("SelectedOperation", (int)999);   //deliberate error for error handling
                IEFieldDict.Add("UserName", string.IsNullOrEmpty(EXP_UserName) ? UserName : EXP_UserName);  //username from constants



                StringBuilder sb = new StringBuilder();
                try
                {
                    //Build the Json string from the items added above.
                    foreach (KeyValuePair<string, dynamic> item in IEFieldDict)
                    {
                        if (item.Value is string && item.Value.ToString().Length > 0 && !item.Value.Substring(0, 1).Equals("["))
                            sb.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\",");

                        else
                            sb.Append("\"" + item.Key + "\"" + ":" + item.Value + ", ");


                    }

                    FinalJson = "{" + sb.ToString().Substring(0, sb.Length - 1) + "}";  //Surround the string with {} to meet Json specifications
                    return true;
                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error creating string> {0}", sb.ToString());
                    Console.WriteLine("Error: {0}", ex.Message);
                    return false;
                }
            }
            else
            {  //Import
                Dictionary<string, dynamic> IEFieldDict = new Dictionary<string, dynamic>();

                try
                {
                    byte[] ImportFile = System.IO.File.ReadAllBytes(filePath);
                    StringBuilder ByteArrayString = new StringBuilder();
                    foreach (byte Byte in ImportFile)
                    {
                        ByteArrayString.Append(Byte.ToString() + ",");
                    }
                    IEFieldDict.Add("DataForImport", "[" + ByteArrayString.ToString().Substring(0, ByteArrayString.Length - 1) + "]");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading file for Import: " + ex.Message);
                    return false;   //Can't proceed without a file.
                }

                IEFieldDict.Add("DataSince", string.Format(@"\/Date({0})\/", ToUnixEpoch(DateTime.Now.AddDays(-30), true)));

                int HierStructID;
                int LanguageID;

                if (!TryGetHierStructID("Test Property 2", out HierStructID, out LanguageID))  //Gets HierStructID & language for given location name. Returns true if found.
                {
                    Console.WriteLine("Unable to find the property in the Hierarchy.");
                    return false; // Can't continue without HierStructID 
                }
                IEFieldDict.Add("HierStrucId", HierStructID);
                IEFieldDict.Add("LanguageId", LanguageID);
                IEFieldDict.Add("Level", (int)DataLevel.SelectedHierarchy);

                string FullObjectName;
                if (!TryGetExportObject("FamilyGroup", out FullObjectName))  //Gets Full Object Name. Returns true if found.
                {
                    Console.WriteLine("Unable to find the Object(FamilyGroup) in available exportable objects.");
                    return false; // Can't continue without an Object 
                }
                IEFieldDict.Add("ObjectType", FullObjectName);

                // string ObjectMembers = GetColumnsForObject(FullObjectName);

                //IEFieldDict.Add("SelectedObjectMembers", ObjectMembers);

                IEFieldDict.Add("OrgCode", OrgCode);   //orgcode from constants
                IEFieldDict.Add("Origin", (int)RequestOrigin.WebApi);   //see enums for options.
                IEFieldDict.Add("RequestDate", string.Format(@"\/Date({0})\/", ToUnixEpoch(DateTime.Now), true));
                IEFieldDict.Add("RequestName", "DemoImportRequest");
                IEFieldDict.Add("SelectedFormat", (int)AllowedFormat.CSV);   //see enums for options.
                IEFieldDict.Add("SelectedOperation", (int)AllowedOperation.Import);   //see enums for options.
                IEFieldDict.Add("UserName", UserName);  //username from constants

                StringBuilder sb = new StringBuilder();

                //Build the Json string from the items added above.
                foreach (KeyValuePair<string, dynamic> item in IEFieldDict)
                {
                    if (item.Value is string && !item.Value.Substring(0, 1).Equals("["))
                        sb.Append("\"" + item.Key + "\"" + ":" + "\"" + item.Value + "\",");
                    else
                        sb.Append("\"" + item.Key + "\"" + ":" + item.Value + ", ");
                }

                FinalJson = "{" + sb.ToString().Substring(0, sb.Length - 1) + "}";  //Surround the string with {} to meet Json specifications
                return true;
            }
        }

        /// <summary>
        /// Find a specific location or Hierarchy item
        /// </summary>
        /// <param name="LocationNameToFind">name of hierarchy item to find.</param>
        /// <param name="HierStructID">HierStructID of hierarchy item if found.</param>
        /// <param name="LanguageID">LanguageID for returned hierarchy item. </param>
        /// <returns>true if hierarchy item is found.</returns>
        private static bool TryGetHierStructID(string LocationNameToFind, out int HierStructID, out int LanguageID)
        {

            WebClient client = new WebClient();
            GetAuthToken(client);

            var json = client.DownloadString(DataServiceURL + "GetHierarchyStructure/" + UserName + "/" + OrgCode);
            //Converts the Json string to an array of the results. Each member will be a different Hierarchy level in this case.
            JArray HierarchyResponse = JArray.Parse(json);

            //Intialise the OUT results.
            HierStructID = 0;
            LanguageID = 0;

            // This will be used to respond on if we were successful.
            bool FoundHierStruct = false;

            foreach (JToken HierarchyItem in HierarchyResponse)
            {
                JToken HierarchyName = HierarchyItem["HierarchyName"][0]; //Provides an array of results based on Languages. We are just grabbing the first language for simplicity
                if ((string)HierarchyName["Text"] == LocationNameToFind)
                {   //Found the location!
                    HierStructID = (int)HierarchyItem["Id"];
                    LanguageID = (int)HierarchyName["LangId"];   //Use the same language as Location Name, though you could call the seperate GetSupportedLanguages method to see all that are available.
                    FoundHierStruct = true;
                    break;  //No need to search Further
                }
            }

            return FoundHierStruct;
        }

        /// <summary>
        /// Find a specific object type e.g (Major_group, Family_group, Menu_item etc) for import or 
        /// export using a partial name, and return the fully qualified name. Returns the 
        /// first hit.
        /// </summary>
        /// <param name="ObjectNameToFind">Name of Object to find</param>
        /// <param name="FullObjectName">returned fully qualified name of object</param>
        /// <returns>true if object is found.</returns>
        private static bool TryGetExportObject(string ObjectNameToFind, out string FullObjectName)
        {

            WebClient client = new WebClient();
            GetAuthToken(client);

            string json = client.DownloadString(DataServiceURL + "GetExportableTypes/api spec/");

            // This returns a simple list so we will not use a Json convertor or deserialiser
            string[] ListOfObjectTypes = json.Substring(1, json.Length - 2).Split(',');

            //Intialise the OUT results.
            FullObjectName = "";

            // This will be used to respond on if we were successful.
            bool FoundObject = false;

            foreach (string ObjectType in ListOfObjectTypes)
            {
                string cleanObjectType = ObjectType.Replace("\"", "");  //Remove surrounding quotation marks from string
                if (cleanObjectType.ToUpper().Contains(ObjectNameToFind.ToUpper()))
                {   //Found the location!
                    FullObjectName = cleanObjectType;
                    FoundObject = true;
                    break;  //No need to search Further
                }
            }

            return FoundObject;
        }

        /// <summary>
        /// Returns all available columns for a specific object type.
        /// </summary>
        /// <param name="FullObjectName">Fully qualified name of object (see TryGetExportObject)</param>
        /// <returns>true if object is found.</returns>
        private static string GetColumnsForObject(string FullObjectName)
        {

            WebClient client = new WebClient();
            GetAuthToken(client);

            var json = client.DownloadString(DataServiceURL + "GetPropertiesForType/" + FullObjectName + "/api spec/");

            // This returns a simple list so we will not use a Json convertor or deserialiser
            return json;

        }

        #endregion

        #region WebClient Methods

        /// <summary>
        /// Attempt to get authentication token from the security service.
        /// </summary>
        /// <param name="client">Webclient to attach the returned token to.</param>
        /// <returns>true if successful</returns>
        private static bool GetAuthToken(WebClient client)
        {
            //Build the request for the Authorization request. Url, Username, password and Orgcode defined in Constants

            //01Apr2016 | BKurundupotha | 122088 - Building JSON string to call Http POST method
            string loginString = BuildLoginString(UserName, Password, OrgCode);

            //Send the request and stored the result in the token string.
            //The string is returned with surrounding quotation marks, and must be adjusted before being added to the client header
            //  i.e "\"IExb6b4734c-9a53-4971-8ad0-6fa959f0a7218ba8ee6243ee073d6e99c5e08ea4e16eeb74cfc2\""

            try
            {
                string token = string.Empty;
                using (WebClient authClient = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    authClient.Headers.Add("Content-Type", "application/json");  //Let the server know what type of content we are sending
                    token = authClient.UploadString(SecurityServiceURL, loginString);
                }

                if (token.Length > 5 && !token.Contains("error")) //Check that we have a token
                {
                    string cleanToken = token.Substring(1, token.Length - 2);  //strip surrounding quotation marks "" by ignoring the first and last character
                    client.Headers.Add("AuthenticationToken", cleanToken);
                    client.Headers.Add("X-Forwarded-Scheme", "https");
                    return true;
                }
                else
                {
                    throw new Exception("No Valid Token");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error getting authorization from " + SecurityServiceURL);
                return false;
            }

        }

        /// <summary>
        /// Builds the JSON string with login parameters for the request
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="orgCode"></param>
        /// <returns></returns>
        private static string BuildLoginString(string userName, string password, string orgCode)
        {
            string finalJson = string.Empty;

            StringBuilder sb = new StringBuilder();
            //Build the Json string from above parameters.
            sb.Append("\"" + "userName" + "\"" + ":" + "\"" + userName + "\",");
            sb.Append("\"" + "password" + "\"" + ":" + "\"" + password + "\",");
            sb.Append("\"" + "orgCode" + "\"" + ":" + "\"" + orgCode + "\",");

            finalJson = "{" + sb.ToString().Substring(0, sb.Length - 1) + "}";  //Surround the string with {} to meet Json specifications

            return finalJson;
        }

        #endregion

        #region JSON Handlers

        /// <summary>
        /// Does basic parsing of a json object in order to return a list of its properties.
        /// </summary>
        /// <param name="json">string to be parsed</param>
        private static void ConsoleWriteJsonList(string json)
        {
            // Stripping square brackets and splitting Json list by comma
            string[] StringList = json.Replace("[", string.Empty).Replace("]", string.Empty).Split(',');
            foreach (string item in StringList)
            {
                Console.WriteLine(item.ToString());
            }
        }

        /// <summary>
        /// Takes a Json object and converts it to a string. Uses DataContractJsonSerializer
        /// </summary>
        /// <param name="obj">json class object</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Convert Datetime to Unix or POSIX time (Required by Json). This is defined as the number of seconds that have elapsed 
        /// since 00:00:00 Coordinated Universal Time (UTC), Thursday, 1 January 1970, not counting leap seconds. 
        /// C# DateTimes do not count leapseconds. 
        /// Optionally add timezone offset.
        /// </summary>
        /// <param name="dateTime">datetime to be converted</param>
        /// <param name="addTimeZoneOffset">if true then add timezone offset</param>
        /// <returns></returns>
        public static string ToUnixEpoch(DateTime dateTime, bool addTimeZoneOffset = false)
        {
            DateTime dateBase = new DateTime(1970, 1, 1);
            DateTime dateUTC = dateTime.ToUniversalTime();
            TimeSpan unixTime = new TimeSpan(dateUTC.Ticks - dateBase.Ticks);
            if (addTimeZoneOffset)
            {
                TimeSpan Offset = new TimeSpan();
                Offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
                return unixTime.TotalMilliseconds.ToString("#") + "+" + Offset.ToString("hhmm");
            }
            else
                return unixTime.TotalMilliseconds.ToString("#");
        }

        #endregion

    }
}
