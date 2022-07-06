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
            var inputJson = File.ReadAllText("../../../Datasets/sales.json");

            Console.WriteLine(GetOrderedCustomers(context));
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
    }
}