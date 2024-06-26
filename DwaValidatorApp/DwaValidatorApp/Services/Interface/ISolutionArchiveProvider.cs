using DwaValidatorApp.Viewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.Services.Interface
{
    public interface ISolutionArchiveProvider
    {
        public void AddArchives(IEnumerable<string> archives);
        public IEnumerable<SolutionDataVM> GetData();
    }

}
