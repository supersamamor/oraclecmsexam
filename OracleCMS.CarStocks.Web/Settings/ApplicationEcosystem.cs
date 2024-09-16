namespace OracleCMS.CarStocks.Web.Settings
{
    public class ApplicationEcosystem
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
        public int Sequence { get; set; }
        public bool IsCurrentApplication { get; set; }
		public bool IsDisabled { get; set; }
    }
}
