using System;
using System.Collections.Generic;
using System.Linq;

namespace ImportExportWebAPIExample
{
    /// <summary>
    /// A Schedule instance which can be hooked up to a export request
    /// </summary>    
    public class RequestSchedule
    {
        /// <summary>
        /// Id value assigned by system
        /// </summary>
        
        public long Id { get; set; }

        /// <summary>
        /// Easily understandable Name for the schedule. Ex: Daily , Every 5th of the month etc.
        /// </summary>
        
        public string Name { get; set; }

        /// <summary>
        /// Last modified or created user for this schedule.
        /// </summary>
        
        public string UserName { get; set; }

        /// <summary>
        /// Organization short name
        /// </summary>
        
        public string OrgCode { get; set; }

        /// <summary>
        /// Date from which this schedule will be active.
        /// </summary>
        
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Time of the day for a given schedule
        /// </summary>
        
        public DateTime? TimeOfDay { get; set; }

        /// <summary>
        /// Scheduled for Sunday
        /// </summary>
        
        public bool Sunday { get; set; }

        /// <summary>
        /// Scheduled for Monday
        /// </summary>
        
        public bool Monday { get; set; }

        /// <summary>
        /// Scheduled for Tuesday
        /// </summary>
        
        public bool Tuesday { get; set; }

        /// <summary>
        /// Scheduled for Wednesday
        /// </summary>
        
        public bool Wednesday { get; set; }

        /// <summary>
        /// Scheduled for Thursday
        /// </summary>
        
        public bool Thursday { get; set; }

        /// <summary>
        /// Scheduled for Friday
        /// </summary>
        
        public bool Friday { get; set; }

        /// <summary>
        /// Scheduled for Satuday
        /// </summary>
        
        public bool Saturday { get; set; }

        /// <summary>
        /// Array of Days of month from 1 - 31
        /// </summary>
        
        public int[] DaysOfMonth { get; set; }

        /// <summary>
        /// Flag to turn on or turn off the schedule
        /// </summary>
        
        public bool Active { get; set; }


        /// <summary>
        /// All new schedules need to pass Validity before it gets into the system.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return (Validate().Count == 0);
        }

        /// <summary>
        /// Validates and returns a list of validation messages.
        /// Empty collection implies a valid request.
        /// </summary>
        /// <returns></returns>
        public List<string> Validate()
        {
            var messages = new List<string>();
            if(string.IsNullOrEmpty(Name))
            {
                messages.Add("Schedule Needs a Name.");
            }
            if(!string.IsNullOrEmpty(Name) && Name.Length > 40)
            {
                messages.Add("Schedule Name exceeds Max length of 40 chars.");
            }
            if(string.IsNullOrEmpty(UserName))
            {
                messages.Add("User missing on schedule addition/modification.");
            }
            bool weekSelected = false;
            if ((Sunday || Monday || Tuesday || Wednesday || Thursday || Friday || Saturday))
            {
                weekSelected = true;
            }
            bool DaysSelected = false;
            if (DaysOfMonth != null && DaysOfMonth.Length > 0)
            {
                DaysSelected = true;
                bool badDays = DaysOfMonth.Any(item => item <= 0 || item > 31);
                if (badDays)
                {
                    messages.Add("Invalid days found on request.");
                }               
            }
            if(!weekSelected && !DaysSelected)
            {
                messages.Add("Either Days of Week or Days of Month need to be on the schedule.");
            }
            return messages;
        }        
    }
}
