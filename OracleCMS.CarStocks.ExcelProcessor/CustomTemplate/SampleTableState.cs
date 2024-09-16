namespace OracleCMS.CarStocks.ExcelProcessor.CustomTemplate
{
    /// <summary>
    /// This is a sample only for implementing batch upload with custom template
    /// </summary>
    public record SampleTableState : Common.Core.Base.Models.BaseEntity
    {
        public string SampleField1 { get; set; } = "";
        public DateTime SampleField2 { get; set; } 
        public decimal SampleField3 { get; set; } 
        public int SampleField4 { get; set; } 
    }
}
