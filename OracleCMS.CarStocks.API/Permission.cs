namespace OracleCMS.CarStocks.API;

public static class Permission
{
    public static class Dealers
	{
		public const string View = "Permission.Dealers.View";
		public const string Create = "Permission.Dealers.Create";
		public const string Edit = "Permission.Dealers.Edit";
		public const string Delete = "Permission.Dealers.Delete";
		public const string Upload = "Permission.Dealers.Upload";
		public const string History = "Permission.Dealers.History";
	}
	public static class Cars
	{
		public const string View = "Permission.Cars.View";
		public const string Create = "Permission.Cars.Create";
		public const string Edit = "Permission.Cars.Edit";
		public const string Delete = "Permission.Cars.Delete";
		public const string Upload = "Permission.Cars.Upload";
		public const string History = "Permission.Cars.History";
		public const string Approve = "Permission.Cars.Approve";
	}
	public static class Stocks
	{
		public const string View = "Permission.Stocks.View";
		public const string Create = "Permission.Stocks.Create";
		public const string Edit = "Permission.Stocks.Edit";
		public const string Delete = "Permission.Stocks.Delete";
		public const string Upload = "Permission.Stocks.Upload";
		public const string History = "Permission.Stocks.History";
	}
	
}