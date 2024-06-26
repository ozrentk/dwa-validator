using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwaValidatorApp.DbModels
{
    public class GetTablesByDependenciesResult
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public int DependencyLevel { get; set; }
    }
}
