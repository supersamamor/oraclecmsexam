using AutoMapper;
using OracleCMS.CarStocks.Core.CarStocks;
using OracleCMS.CarStocks.Web.Areas.CarStocks.Models;
using OracleCMS.CarStocks.Application.Features.CarStocks.Report.Commands;
using OracleCMS.CarStocks.Application.DTOs;
using OracleCMS.CarStocks.Application.Features.CarStocks.Approval.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Commands;


namespace OracleCMS.CarStocks.Web.Areas.CarStocks.Mapping;

public class CarStocksProfile : Profile
{
    public CarStocksProfile()
    {
		CreateMap<ReportViewModel, AddReportCommand>()
            .ForMember(dest => dest.ReportRoleAssignmentList, opt => opt.MapFrom(src => src.ReportRoleAssignmentList!.Select(x => new ReportRoleAssignmentState { RoleName = x })));
        CreateMap<ReportViewModel, EditReportCommand>()
           .ForMember(dest => dest.ReportRoleAssignmentList, opt => opt.MapFrom(src => src.ReportRoleAssignmentList!.Select(x => new ReportRoleAssignmentState { RoleName = x })));
        CreateMap<ReportState, ReportViewModel>()
            .ForMember(dest => dest.ReportRoleAssignmentList, opt => opt.MapFrom(src => src.ReportRoleAssignmentList!.Select(x => x.RoleName)));
        CreateMap<ReportViewModel, ReportState>();      
        CreateMap<ReportQueryFilterState, ReportQueryFilterViewModel>().ForPath(e => e.ForeignKeyReport, o => o.MapFrom(s => s.Report!.ReportName));
        CreateMap<ReportQueryFilterViewModel, ReportQueryFilterState>();
        CreateMap<ReportResultModel, ReportResultViewModel>().ReverseMap();
        CreateMap<ReportQueryFilterModel, ReportQueryFilterViewModel>().ReverseMap();
		CreateMap<ReportViewModel, AddReportWithSQLQueryFromAICommand>();
		
        CreateMap<DealersViewModel, AddDealersCommand>();
		CreateMap<DealersViewModel, EditDealersCommand>();
		CreateMap<DealersState, DealersViewModel>().ReverseMap();
		CreateMap<CarsViewModel, AddCarsCommand>();
		CreateMap<CarsViewModel, EditCarsCommand>();
		CreateMap<CarsState, CarsViewModel>().ReverseMap();
		CreateMap<StocksViewModel, AddStocksCommand>();
		CreateMap<StocksViewModel, EditStocksCommand>();
		CreateMap<StocksState, StocksViewModel>().ForPath(e => e.ReferenceFieldCarID, o => o.MapFrom(s => s.Cars!.Id)).ForPath(e => e.ReferenceFieldDealerID, o => o.MapFrom(s => s.Dealers!.Id));
		CreateMap<StocksViewModel, StocksState>();
		
		CreateMap<ApproverAssignmentState, ApproverAssignmentViewModel>().ReverseMap();
		CreateMap<ApproverSetupViewModel, EditApproverSetupCommand>();
		CreateMap<ApproverSetupViewModel, AddApproverSetupCommand>();
		CreateMap<ApproverSetupState, ApproverSetupViewModel>().ReverseMap();
    }
}
