using AutoMapper;
using CarDealer.Models;
using CarDealer.Dtos.Import;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<SupplierDto, Supplier>();

            CreateMap<PartDto, Part>();

            CreateMap<CarDto, Car>();

            CreateMap<CustomerDto, Customer>();

            CreateMap<SaleDto, Sale>();
        }
    }
}
