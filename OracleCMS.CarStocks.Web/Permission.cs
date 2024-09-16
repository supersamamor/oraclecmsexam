using System.Reflection;
namespace OracleCMS.CarStocks.Web;

public static class Permission
{
    public static IEnumerable<string> GenerateAllPermissions()
	{
		var permissions = new List<string>();
		// Get all nested classes in the Permissions class
		var nestedClasses = typeof(Permission).GetNestedTypes();
		foreach (var nestedClass in nestedClasses)
		{
			// Get all public static string fields in the nested class
			var permissionsInClass = nestedClass.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(string))
				.Select(f => f.GetValue(null)!.ToString());

			permissions.AddRange(permissionsInClass!);
		}
		return permissions.OrderBy(l=>l);
	}
	public static IEnumerable<string> GeneratePermissionsForModule(string module)
	{
		var permissions = new List<string>();
		// Get the nested class for the specified module
		var moduleType = typeof(Permission).GetNestedType(module);
		if (moduleType != null)
		{
			// Get all public static string fields in the module class
			var modulePermissions = moduleType.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(f => f.FieldType == typeof(string))
				.Select(f => f.GetValue(null)!.ToString());
			permissions.AddRange(modulePermissions!);
		}     
		return permissions.OrderBy(l => l);
	}

    public static class Admin
    {
        public const string View = "Permission.Admin.View";
        public const string Create = "Permission.Admin.Create";
        public const string Edit = "Permission.Admin.Edit";
        public const string Delete = "Permission.Admin.Delete";
    }

    public static class Entities
    {
        public const string View = "Permission.Entities.View";
        public const string Create = "Permission.Entities.Create";
        public const string Edit = "Permission.Entities.Edit";
        public const string Delete = "Permission.Entities.Delete";
    }

    public static class Roles
    {
        public const string View = "Permission.Roles.View";
        public const string Create = "Permission.Roles.Create";
        public const string Edit = "Permission.Roles.Edit";
        public const string Delete = "Permission.Roles.Delete";
    }

    public static class Users
    {
        public const string View = "Permission.Users.View";
        public const string Create = "Permission.Users.Create";
        public const string Edit = "Permission.Users.Edit";
        public const string Delete = "Permission.Users.Delete";
    }

    public static class Apis
    {
        public const string View = "Permission.Apis.View";
        public const string Create = "Permission.Apis.Create";
        public const string Edit = "Permission.Apis.Edit";
        public const string Delete = "Permission.Apis.Delete";
    }

    public static class Applications
    {
        public const string View = "Permission.Applications.View";
        public const string Create = "Permission.Applications.Create";
        public const string Edit = "Permission.Applications.Edit";
        public const string Delete = "Permission.Applications.Delete";
    }

    public static class AuditTrail
    {
        public const string View = "Permission.AuditTrail.View";
        public const string Create = "Permission.AuditTrail.Create";
        public const string Edit = "Permission.AuditTrail.Edit";
        public const string Delete = "Permission.AuditTrail.Delete";
    }
	public static class Report
    {
        public const string View = "Permission.Report.View";
		public const string AIDrivenDataAnalysisAndInsights = "Permission.Report.AIDrivenDataAnalysisAndInsights";
    }
    public static class ReportSetup
    {
        public const string View = "Permission.ReportSetup.View";
        public const string Create = "Permission.ReportSetup.Create";
        public const string Edit = "Permission.ReportSetup.Edit";
        public const string Delete = "Permission.ReportSetup.Delete";
        public const string Approve = "Permission.ReportSetup.Approve";
    }
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
	
	public static class ApproverSetup
	{
		public const string Create = "Permission.ApproverSetup.Create";
		public const string View = "Permission.ApproverSetup.View";
		public const string Edit = "Permission.ApproverSetup.Edit";
		public const string PendingApprovals = "Permission.ApproverSetup.PendingApprovals";
	}
}
