namespace Artillery.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.DataProcessor.ImportDto;
    using System.Linq;
    using Newtonsoft.Json;
    using Artillery.Data.Models.Enums;

    public class Deserializer
    {
        private const string ErrorMessage =
                "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(CountryXmlInputModel[]), new XmlRootAttribute("Countries"));
            var sb = new StringBuilder();

            var countryDtos = (CountryXmlInputModel[])serializer.Deserialize(new StringReader(xmlString));
            var countries = new HashSet<Country>();

            foreach (var countryDto in countryDtos)
            {
                if (!IsValid(countryDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var country = new Country
                {
                    CountryName = countryDto.CountryName,
                    ArmySize = countryDto.ArmySize
                };

                countries.Add(country);

                sb.AppendFormat(SuccessfulImportCountry, country.CountryName, country.ArmySize);
                sb.AppendLine();
            }

            context.Countries.AddRange(countries);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ManufacturerXmlInputModel[]), new XmlRootAttribute("Manufacturers"));
            var sb = new StringBuilder();
            

            var manufacturerDtos = (ManufacturerXmlInputModel[])serializer.Deserialize(new StringReader(xmlString));
            var manufacturers = new HashSet<Manufacturer>();

            foreach (var manufacturerDto in manufacturerDtos)
            {
                if (!IsValid(manufacturerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if(manufacturers.Select(x => x.ManufacturerName).Contains(manufacturerDto.ManufacturerName))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var manufacturer = new Manufacturer
                {
                    ManufacturerName = manufacturerDto.ManufacturerName,
                    Founded = manufacturerDto.Founded
                };

                manufacturers.Add(manufacturer);

                var manufacturerInfo = manufacturer.Founded.Split(", ").TakeLast(2).ToArray();
                var town = manufacturerInfo[0];
                var country = manufacturerInfo[1];
                
                sb.AppendFormat(SuccessfulImportManufacturer, manufacturer.ManufacturerName, $"{town}, {country}");
                sb.AppendLine();
            }

            context.Manufacturers.AddRange(manufacturers);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ShellXmlInputModel[]), new XmlRootAttribute("Shells"));
            var sb = new StringBuilder();

            var shellDtos = (ShellXmlInputModel[])serializer.Deserialize(new StringReader(xmlString));
            var shells = new HashSet<Shell>();

            foreach (var shellDto in shellDtos)
            {
                if (!IsValid(shellDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var shell = new Shell
                {
                    ShellWeight = shellDto.ShellWeight,
                    Caliber = shellDto.Caliber
                };

                shells.Add(shell);

                sb.AppendFormat(SuccessfulImportShell, shell.Caliber, shell.ShellWeight);
                sb.AppendLine();
            }

            context.Shells.AddRange(shells);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var guns = new HashSet<Gun>();
            var gunDtos = JsonConvert.DeserializeObject<GunJsonInputModel[]>(jsonString);

            foreach (var gunDto in gunDtos)
            {
                if (!IsValid(gunDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(GunType), gunDto.GunType))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (gunDto.BarrelLength < 2)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var gun = new Gun
                {
                    ManufacturerId = gunDto.ManufacturerId,
                    GunWeight = gunDto.GunWeight,
                    BarrelLength = gunDto.BarrelLength,
                    NumberBuild = gunDto.NumberBuild,
                    Range = gunDto.Range,
                    GunType = Enum.Parse<GunType>(gunDto.GunType),
                    ShellId = gunDto.ShellId
                };

                foreach (var country in gunDto.Countries)
                {
                    gun.CountriesGuns.Add(new CountryGun
                    {
                        CountryId = country.Id,
                        Gun = gun
                    });
                }

                guns.Add(gun);

                sb.AppendFormat(SuccessfulImportGun, gun.GunType, gun.GunWeight, gun.BarrelLength);
                sb.AppendLine();
            }
            
            context.Guns.AddRange(guns);
            context.SaveChanges();

            return sb.ToString();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
