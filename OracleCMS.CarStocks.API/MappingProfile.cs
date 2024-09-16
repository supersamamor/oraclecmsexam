using AutoMapper;
using OracleCMS.CarStocks.API.Controllers.v1;
using OracleCMS.CarStocks.Application.Features.CarStocks.Dealers.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Cars.Commands;
using OracleCMS.CarStocks.Application.Features.CarStocks.Stocks.Commands;


namespace OracleCMS.CarStocks.API;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<DealersViewModel, AddDealersCommand>();
		CreateMap <DealersViewModel, EditDealersCommand>();
		CreateMap<CarsViewModel, AddCarsCommand>();
		CreateMap <CarsViewModel, EditCarsCommand>();
		CreateMap<StocksViewModel, AddStocksCommand>();
		CreateMap <StocksViewModel, EditStocksCommand>();
		
    }
}
