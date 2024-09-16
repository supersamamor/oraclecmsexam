using Microsoft.AspNetCore.Mvc.Rendering;
using OracleCMS.CarStocks.Infrastructure.Data;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.Common.Data;
using OracleCMS.CarStocks.Web.Areas.Admin.Queries.Users;
using MediatR;
using OracleCMS.CarStocks.Web.Areas.Admin.Queries.Roles;
using OracleCMS.CarStocks.Application.Features.CarStocks.Report.Queries;
using System.Globalization;

namespace OracleCMS.CarStocks.Web.Service
{
    public class DropdownServices
    {
		private readonly ApplicationContext _context;
		private readonly IMediator _mediaTr;

		public DropdownServices(ApplicationContext context, IMediator mediaTr)
		{            
			_context = context;
			_mediaTr = mediaTr;
		} 
		public async Task<IEnumerable<SelectListItem>> GetRoleList()
		{
			var query = new GetRolesQuery()
			{
				PageSize = -1
			};
			return (await _mediaTr.Send(query)).Data.Select(l => new SelectListItem { Value = l.Name, Text = l.Name });
		}		
        public IEnumerable<SelectListItem> QueryTypeList()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                //new SelectListItem { Text = Core.Constants.QueryType.QueryBuilder, Value = Core.Constants.QueryType.QueryBuilder, },
                new SelectListItem { Text = Core.Constants.QueryType.TSql, Value = Core.Constants.QueryType.TSql, }
            };
            return items;
        }
        public IEnumerable<SelectListItem> ReportChartTypeList()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem { Text = Core.Constants.ReportChartType.HorizontalBar, Value = Core.Constants.ReportChartType.HorizontalBar, },
                new SelectListItem { Text = Core.Constants.ReportChartType.Pie, Value = Core.Constants.ReportChartType.Pie, },
                new SelectListItem { Text = Core.Constants.ReportChartType.Table, Value = Core.Constants.ReportChartType.Table, },
            };
            return items;
        }
        public IEnumerable<SelectListItem> DataTypeList()
        {
            IList<SelectListItem> items = new List<SelectListItem>
            {
                new SelectListItem { Text = Core.Constants.DataTypes.CustomDropdown, Value = Core.Constants.DataTypes.CustomDropdown, },
                new SelectListItem { Text = Core.Constants.DataTypes.Date, Value = Core.Constants.DataTypes.Date, },
                new SelectListItem { Text = Core.Constants.DataTypes.DropdownFromTable, Value = Core.Constants.DataTypes.DropdownFromTable, },
                new SelectListItem { Text = Core.Constants.DataTypes.Months, Value = Core.Constants.DataTypes.Months, },
                new SelectListItem { Text = Core.Constants.DataTypes.Years, Value = Core.Constants.DataTypes.Years, },
            };
            return items;
        }
        public IEnumerable<SelectListItem> GetDropdownFromCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Enumerable.Empty<SelectListItem>();
            }
            return value.Split(',')
                         .Select(option => new SelectListItem { Text = option.Trim(), Value = option.Trim() })
                         .ToList();
        }
        public IEnumerable<SelectListItem> GetYearsList(int yearsPrevious, int yearsAdvance)
        {
            List<SelectListItem> yearsList = new();
            int currentYear = DateTime.Now.Year;
            int startYear = currentYear - yearsPrevious;
            int endYear = currentYear + yearsAdvance;
            for (int year = startYear; year <= endYear; year++)
            {
                SelectListItem listItem = new()
                {
                    Text = year.ToString(),
                    Value = year.ToString(),
                };
                yearsList.Add(listItem);
            }
            return yearsList;
        }
        public IEnumerable<SelectListItem> GetMonthsList()
        {
            List<SelectListItem> monthsList = new();
            // Loop through the months and create SelectListItem objects for each month
            for (int month = 1; month <= 12; month++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                SelectListItem listItem = new()
                {
                    Text = monthName,
                    Value = month.ToString() // Month number as the 'Value'
                };
                monthsList.Add(listItem);
            }
            return monthsList;
        }
        public async Task<IEnumerable<SelectListItem>> GetDropdownFromTableKeyValue(string tableKeyValue, string? filter)
        {
            var dropdownValues = await _mediaTr.Send(new GetDropdownValuesQuery(tableKeyValue, filter));
            List<SelectListItem> selectListItems = new();
            foreach (var item in dropdownValues)
            {
                string? key = item.ContainsKey("Key") ? item["Key"] : "";
                string? value = item.ContainsKey("Value") ? item["Value"] : "";
                selectListItems.Add(new SelectListItem
                {
                    Text = value,
                    Value = key
                });
            }
            return selectListItems;
        }
        public async Task<IList<Dictionary<string, string>>> GetReportList()
        {
            return await _mediaTr.Send(new GetReportListQuery());
        }		
		public SelectList GetCarsList(string? id)
		{
			return _context.GetSingle<CarsState>(e => e.Id == id, new()).Result.Match(
				Some: e => new SelectList(new List<SelectListItem> { new() { Value = e.Id, Text = e.Id } }, "Value", "Text", e.Id),
				None: () => new SelectList(new List<SelectListItem>(), "Value", "Text")
			);
		}
		public SelectList GetDealersList(string? id)
		{
			return _context.GetSingle<DealersState>(e => e.Id == id, new()).Result.Match(
				Some: e => new SelectList(new List<SelectListItem> { new() { Value = e.Id, Text = e.Id } }, "Value", "Text", e.Id),
				None: () => new SelectList(new List<SelectListItem>(), "Value", "Text")
			);
		}
		
		public async Task<IEnumerable<SelectListItem>> GetUserList(string currentSelectedApprover, IList<string> allSelectedApprovers)
		{
			return (await _mediaTr.Send(new GetApproversQuery(currentSelectedApprover, allSelectedApprovers) { PageSize = -1 })).Data.Select(l => new SelectListItem { Value = l.Id, Text = l.Name });
		}
public async Task<IEnumerable<SelectListItem>> GetRoleApproverList(string currentSelectedApprover, IList<string> allSelectedApprovers)
		{
			return (await _mediaTr.Send(new GetApproverRolesQuery(currentSelectedApprover, allSelectedApprovers) { PageSize = -1 })).Data.Select(l => new SelectListItem { Value = l.Id, Text = l.Name });
		}
    }
}
