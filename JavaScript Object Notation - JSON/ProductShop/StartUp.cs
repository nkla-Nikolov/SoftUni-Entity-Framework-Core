using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;
using AutoMapper;
using ProductShop.DataTransferObjects;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProductShop
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();
            //context.Database.EnsureDeleted();
            //context.Database.EnsureCreated();

            //var inputJson = File.ReadAllText("../../../Datasets/users.json");
            //var inputJson = File.ReadAllText("../../../Datasets/products.json");
            //var inputJson = File.ReadAllText("../../../Datasets/categories.json");
            //var inputJson = File.ReadAllText("../../../Datasets/categories-products.json");
            Console.WriteLine(GetUsersWithProducts(context));
        }

        public static IMapper InitializeMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            return config.CreateMapper();
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            var mapper = InitializeMapper();
            var users = JsonConvert.DeserializeObject<ICollection<UserDTO>>(inputJson);

            var mappedUsers = mapper.Map<ICollection<User>>(users);
            context.Users.AddRange(mappedUsers);
            context.SaveChanges();

            return $"Successfully imported {mappedUsers.Count}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            var mapper = InitializeMapper();
            var products = JsonConvert.DeserializeObject<ICollection<ProductDTO>>(inputJson);

            var mappedProducts = mapper.Map<ICollection<Product>>(products);
            context.Products.AddRange(mappedProducts);
            context.SaveChanges();

            return $"Successfully imported {mappedProducts.Count}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            var mapper = InitializeMapper();

            //var options = new JsonSerializerSettings()
            //{
            //    NullValueHandling = NullValueHandling.Ignore
            //};

            var categories = JsonConvert.DeserializeObject<ICollection<CategoryDTO>>(inputJson)
                .Where(x => !string.IsNullOrEmpty(x.Name));

            var mappedCategories = mapper.Map<ICollection<Category>>(categories);
            context.Categories.AddRange(mappedCategories);
            context.SaveChanges();

            return $"Successfully imported {mappedCategories.Count}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            var mapper = InitializeMapper();
            var categoryProducts = JsonConvert.DeserializeObject<ICollection<CategoryProductDTO>>(inputJson);

            var mappedCategoryProducts = mapper.Map<ICollection<CategoryProduct>>(categoryProducts);
            context.CategoryProducts.AddRange(mappedCategoryProducts);
            context.SaveChanges();

            return $"Successfully imported {mappedCategoryProducts.Count}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new
                {
                    x.Name,
                    x.Price,
                    Seller = x.Seller.FirstName + " " + x.Seller.LastName
                })
                .OrderBy(x => x.Price)
                .ToList();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var jsonProducts = JsonConvert.SerializeObject(products, options);
            return jsonProducts;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Count >= 1 && x.ProductsSold.Any(b => b.Buyer != null))
                .Select(x => new
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    SoldProducts = x.ProductsSold.Where(b => b.Buyer != null).Select(p => new
                    {
                        p.Name,
                        p.Price,
                        BuyerFirstName = p.Buyer.FirstName,
                        BuyerLastName = p.Buyer.LastName
                    })
                })
                .OrderBy(x => x.LastName)
                .ThenBy(x => x.FirstName)
                .ToArray();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var jsonUsers = JsonConvert.SerializeObject(users, options);

            return jsonUsers;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var products = context.Categories
                .OrderByDescending(x => x.CategoryProducts.Count)
                .Select(x => new
                {
                    Category = x.Name,
                    ProductsCount = x.CategoryProducts.Count,
                    AveragePrice = x.CategoryProducts.Average(p => p.Product.Price).ToString("f2"),
                    TotalRevenue = x.CategoryProducts.Sum(p => p.Product.Price).ToString("f2")
                })
                .ToList();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var jsonCategories = JsonConvert.SerializeObject(products, options);

            return jsonCategories;
        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Count >= 1 && x.ProductsSold.Any(b => b.Buyer != null))
                .Include(x => x.ProductsSold)
                .ToList()
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.Age,
                    SoldProducts = new
                    {
                        Count = x.ProductsSold.Where(b => b.Buyer != null).Count(),
                        Products = x.ProductsSold.Where(p => p.Buyer != null).Select(p => new
                        {
                            p.Name,
                            p.Price
                        })
                    }
                })
                .OrderByDescending(x => x.SoldProducts.Count)
                .ToList();

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver()
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };

            var result = new
            {
                usersCount = users.Count,
                Users = users
            };

            var jsonUsers = JsonConvert.SerializeObject(result, options);

            return jsonUsers;
        }
    }
}