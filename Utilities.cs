using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportExportWebAPIExample
{
    public static class Utilities
    {
        public static MajorGroup ToMajorGroup(this string line)
        {
            char[] charsToTrim = { '"', ' ', '\'' };

            var cols = line.Split(',').ToList(); //split each column
            if (cols.Count != 5)
            {
                throw new SystemException("Wrong object.  Number of columns in FamilyGroup should be 5.");
            }

            MajorGroup m = new MajorGroup
            {
                Id = Convert.ToInt64(cols[0]),
                ObjectNumber = Convert.ToInt64(cols[1]),
                HierarchyId = Convert.ToInt64(cols[2]),
                Name = cols[3].Trim(charsToTrim),
                ReportGroup = Convert.ToInt64(cols[4])
            };

            return m;
        }
    }
}
