using AutoMapper;
using OracleCMS.Common.Core.Mapping;
using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Commands;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Application.Features.CarStocks.Report.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Commands;



namespace OracleCMS.CarStocks.Application.Features.CarStocks;

public class CarStocksProfile : Profile
{
    public CarStocksProfile()
    {
		CreateMap<AddReportCommand, ReportState>();
        CreateMap<EditReportCommand, ReportState>().IgnoreBaseEntityProperties();
		CreateMap<AddReportAnalyticsCommand, ReportAIIntegrationState>();
		CreateMap<AddReportWithSQLQueryFromAICommand, ReportState>();
		
        CreateMap<AddDealersCommand, DealersState>();
		CreateMap <EditDealersCommand, DealersState>().IgnoreBaseEntityProperties();
		CreateMap<AddCarsCommand, CarsState>();
		CreateMap <EditCarsCommand, CarsState>().IgnoreBaseEntityProperties();
		CreateMap<AddStocksCommand, StocksState>();
		CreateMap <EditStocksCommand, StocksState>().IgnoreBaseEntityProperties();
		
		CreateMap<EditApproverSetupCommand, ApproverSetupState>().IgnoreBaseEntityProperties();
		CreateMap<AddApproverSetupCommand, ApproverSetupState>().IgnoreBaseEntityProperties();
		CreateMap<ApproverAssignmentState, ApproverAssignmentState>().IgnoreBaseEntityProperties();
    }
}
