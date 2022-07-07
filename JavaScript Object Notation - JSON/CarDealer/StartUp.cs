using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO;
using CarDealer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;

namespace CarDealer
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new CarDealerContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var inputJson = File.ReadAllText("../../../Datasets/suppliers.json");
            //var inputJson = File.ReadAllText("../../../Datasets/parts.json");
            //var inputJson = File.ReadAllText("../../../Datasets/cars.json");
            //var inputJson = File.ReadAllText("../../../Datasets/customers.json");
            //var inputJson = File.ReadAllText("../../../Datasets/sales.json");

            Console.WriteLine(GetSalesWithAppliedDiscount(context));
        }

        private static IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            return config.CreateMapper();
        }
        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            var mapper = InitializeMapper();

            var jsonSuppliers = JsonConvert.DeserializeObject(inputJson);
            var suppliersDTO = mapper.Map<IEnumerable<SupplierDTO>>(jsonSuppliers);
            var mappedSuppliers = mapper.Map<IEnumerable<Supplier>>(suppliersDTO);

            context.Suppliers.AddRange(mappedSuppliers);
            context.SaveChanges();

            return $"Successfully imported {mappedSuppliers.Count()}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var mapper = InitializeMapper();

            var partsDTO = JsonConvert.DeserializeObject<IEnumerable<PartDTO>>(inputJson)
                .Where(x => context.Suppliers
                .Any(s => s.Id == x.SupplierId));


            var mappedParts = mapper.Map<IEnumerable<Part>>(partsDTO);
            
            context.Parts.AddRange(mappedParts);
            context.SaveChanges();

            return $"Successfully imported {mappedParts.Count()}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            var mapper = InitializeMapper();

            var carsDTO = JsonConvert.DeserializeObject<IEnumerable<CarDTO>>(inputJson).ToArray();
            var carsAdded = new List<Car>();

            foreach (var carDto in carsDTO)
            {
                var currentCar = mapper.Map<Car>(carDto);

                foreach (var part in carDto.PartsId.Distinct())
                {
                    var partCar = new PartCar
                    {
                        PartId = part
                    };

                    currentCar.PartCars.Add(partCar);
                }

                carsAdded.Add(currentCar);
            }

            context.Cars.AddRange(carsAdded);
            context.SaveChanges();
            
            return $"Successfully imported {carsAdded.Count()}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            var mapper = InitializeMapper();

            var customersJson = JsonConvert.DeserializeObject<IEnumerable<CustomerDTO>>(inputJson);
            var customers = mapper.Map<IEnumerable<Customer>>(customersJson);

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count()}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            var mapper = InitializeMapper();

            var salesJson = JsonConvert.DeserializeObject<IEnumerable<SaleDTO>>(inputJson);
            var sales = mapper.Map<IEnumerable<Sale>>(salesJson);

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count()}.";
        }

        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var customers = context.Customers
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver)
                .Select(x => new
                {
                    Name = x.Name,
                    BirthDate = x.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = x.IsYoungDriver
                })
                .ToArray();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            var json = JsonConvert.SerializeObject(customers, options);

            return json;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context.Cars
                .Where(x => x.Make == "Toyota")
                .Select(x => new
                {
                    x.Id,
                    x.Make,
                    x.Model,
                    x.TravelledDistance
                })
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .ToArray();

            var json = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return json;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context.Suppliers
                .Where(x => x.IsImporter == false)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(suppliers, Formatting.Indented);

            return json;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context.Cars
                .Select(x => new
                {
                    car = new
                    {
                        x.Make,
                        x.Model,
                        x.TravelledDistance
                    },
                    parts = x.PartCars.Select(p => new
                    {
                        Name = p.Part.Name,
                        Price = p.Part.Price.ToString("f2")
                    })
                })
                .ToArray();

            var json = JsonConvert.SerializeObject(cars, Formatting.Indented);

            return json;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context.Customers.Where(x => x.Sales.Any())
                .Select(x => new
                {
                    FullName = x.Name,
                    BoughtCars = x.Sales.Count,
                    SpentMoney = x.Sales.Sum(m => m.Car.PartCars.Sum(p => p.Part.Price))
                })
                .OrderByDescending(x => x.SpentMoney)
                .ToArray();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var json = JsonConvert.SerializeObject(customers, options);

            return json;
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var sales = context.Sales
                .Select(x => new
                {
                    car = new
                    {
                        Make = x.Car.Make,
                        Model = x.Car.Model,
                        TravelledDistance = x.Car.TravelledDistance
                    },
                    customerName = x.Customer.Name,
                    Discount = x.Discount,
                    price = x.Car.PartCars.Sum(p => p.Part.Price).ToString("f2"),
                    priceWithDiscount = (x.Car.PartCars.Sum(p => p.Part.Price) * ((100 - x.Discount) / 100)).ToString("f2")
                })
                .Take(10)
                .ToArray();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            var json = JsonConvert.SerializeObject(sales, options);

            return json;
        }
    }
}