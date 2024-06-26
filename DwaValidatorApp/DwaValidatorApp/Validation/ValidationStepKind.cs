using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DwaValidatorApp.Validation
{
    public enum ValidationStepKind
    {
        None = 0,

        [Display(Name = "Zip Archive")]
        ZipArchiveValidation = 1,

        [Display(Name = "Folder Structure")]
        FolderStructureValidation = 2,

        [Display(Name = "Archive Unpacking")]
        ArchiveUnpacking = 3,

        [Display(Name = "Solution File")]
        SolutionFileValidation = 4,

        [Display(Name = "SQL Script")]
        SqlScriptValidation = 5,

        [Display(Name = "Creation of DB")]
        DbCreation = 6,

        [Display(Name = "Web API Project Structure")]
        ApiProjectStructureValidation = 7,

        [Display(Name = "MVC Project Structure")]
        MvcProjectStructureValidation = 8,

        [Display(Name = "Web API Project Build")]
        ApiProjectBuildValidation = 9,

        [Display(Name = "MVC Project Build")]
        MvcProjectBuildValidation = 10,

        [Display(Name = "Start Projects")]
        ProjectsStart = 11,

        //[Display(Name = "Collect Database Metadata")]
        //CollectDatabaseMetadata = 12,

        //[Display(Name = "Collect OpenAPI Metadata")]
        //CollectOpenApiMetadata = 13,

        //[Display(Name = "Provide Dashboard Data")]
        //ProvideDashboardData = 14,
    }
}
