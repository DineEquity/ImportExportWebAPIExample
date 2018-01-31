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
                throw new SystemException("Wrong object.  Number of columns in MajorGroup is 5.");
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


        public static FamilyGroup ToFamilyGroup(this string line)
        {
            char[] charsToTrim = { '"', ' ', '\'' };

            var cols = line.Split(',').ToList(); //split each column
            if (cols.Count != 6)
            {
                throw new SystemException("Wrong object.  Number of columns in FamilyGroup is 6.");
            }

            FamilyGroup f = new FamilyGroup
            {
                Id = Convert.ToDecimal(cols[0]),
                ObjectNumber = Convert.ToInt64(cols[1]),
                HierarchyId = Convert.ToInt64(cols[2]),
                ReportGroup = Convert.ToDecimal(cols[3]),
                Name = cols[4].Trim(charsToTrim),
                ParentMajorGroup = Convert.ToDecimal(cols[5])

                               
            };

            return f;
        }


        public static Hierarchy ToHierarchy(this string line)
        {
            char[] charsToTrim = { '"', ' ', '\'' };

            var cols = line.Split(',').ToList(); //split each column
            if (cols.Count != 7)
            {
                throw new SystemException("Wrong object.  Number of columns in Hierarchy is 7.");
            }

            Hierarchy h = new Hierarchy
            {
                Id = Convert.ToDecimal(cols[0]),
                ObjectNumber = Convert.ToInt64(cols[1]),
                HierarchyId = Convert.ToInt64(cols[2]),
                OrganizationId = Convert.ToInt64(cols[3]),
                UnitType = cols[4].Trim(charsToTrim),
                ParentHierarchyId = Convert.ToInt64(cols[5]),
                HierarchyName = cols[6].Trim(charsToTrim)

              
            };

            return h;
        }


    }
}
