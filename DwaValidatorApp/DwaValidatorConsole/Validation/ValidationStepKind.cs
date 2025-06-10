using System.ComponentModel.DataAnnotations;

namespace DwaValidatorConsole.Validation
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

        [Display(Name = "Configuration Update")]
        ConfigurationUpdate = 7,
    }
}
