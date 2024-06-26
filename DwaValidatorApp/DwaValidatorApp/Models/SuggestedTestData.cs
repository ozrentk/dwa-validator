namespace DwaValidatorApp.Models
{
    public class SuggestedTestData : TestData
    {
        public int ColumnId { get; set; }
        public string TypeName { get; set; }
        public int? TypeLength { get; set; }
        public int? TypePrecision { get; set; }
        public int? TypeScale { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public string ColumnDescriptor {
            get 
            {
                string lenPrecScale = "";
                if (TypePrecision.HasValue && TypeScale.HasValue)
                {
                    lenPrecScale = $"len: {TypeLength} ({TypePrecision}, {TypeScale})";
                }
                else if (TypeLength.HasValue)
                {
                    lenPrecScale = $"len: {TypeLength}";
                }
                else
                {
                    lenPrecScale = $"len: max?";
                }
                return $"Type: {TypeName}, {lenPrecScale}";
            }
        }
    }
}
