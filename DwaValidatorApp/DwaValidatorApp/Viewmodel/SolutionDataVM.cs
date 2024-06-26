using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.Viewmodel
{
    public class SolutionDataVM : IComparable<SolutionDataVM>
    {
        public string SolutionPath { get; set; }
        public string StudentName { get; set; }

        public override string ToString()
        {
            return SolutionPath;
        }

        public override bool Equals(object? obj)
        {
            if (obj is SolutionDataVM other)
            {
                return string.Equals(
                    this.SolutionPath, 
                    other.SolutionPath, 
                    StringComparison.Ordinal);
            }

            return false;

        }

        public override int GetHashCode()
        {
            return this.SolutionPath?.GetHashCode() ?? 0;
        }

        public int CompareTo(SolutionDataVM other)
        {
            // If other is not a valid object reference, this instance is greater.
            if (other == null) 
                return 1;

            // The comparison of SolutionPath is case sensitive
            return String.Compare(
                this.SolutionPath, other.SolutionPath, StringComparison.Ordinal);
        }
    }
}
