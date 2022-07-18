namespace BookShop.DataProcessor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ExportDto;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportMostCraziestAuthors(BookShopContext context)
        {
            var authors = context.Authors
               .ToArray()
               .Select(x => new
               {
                   AuthorName = x.FirstName + " " + x.LastName,
                   Books = x.AuthorsBooks
                   .OrderByDescending(x => x.Book.Price)
                   .Select(b => new
                   {
                       BookName = b.Book.Name,
                       BookPrice = b.Book.Price.ToString("F2")
                   })
                   .ToArray()
               })
               .OrderByDescending(x => x.Books.Count())
               .ThenBy(x => x.AuthorName);

            return JsonConvert.SerializeObject(authors, Formatting.Indented);
        }

        public static string ExportOldestBooks(BookShopContext context, DateTime date)
        {
            var serializer = new XmlSerializer(typeof(OldestBookDto[]), new XmlRootAttribute("Books"));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            var books = context.Books
                .Where(x => x.PublishedOn < date && x.Genre == Genre.Science)
                .ToArray()
                .Select(x => new OldestBookDto
                {
                    Pages = x.Pages,
                    Name = x.Name,
                    Date = x.PublishedOn.ToString("d", CultureInfo.InvariantCulture)
                })
                .OrderByDescending(x => x.Pages)
                .ThenByDescending(x => x.Date)
                .Take(10)
                .ToArray();

            serializer.Serialize(sw, books, namespaces);

            return sb.ToString();
        }
    }
}