﻿namespace BookShop.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using BookShop.Data.Models;
    using BookShop.Data.Models.Enums;
    using BookShop.DataProcessor.ImportDto;
    using Data;
    using Newtonsoft.Json;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedBook
            = "Successfully imported book {0} for {1:F2}.";

        private const string SuccessfullyImportedAuthor
            = "Successfully imported author - {0} with {1} books.";

        public static string ImportBooks(BookShopContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(BookInputModel[]), new XmlRootAttribute("Books"));
            var bookDtos = (BookInputModel[])serializer.Deserialize(new StringReader(xmlString));
            var sb = new StringBuilder();
            var books = new HashSet<Book>();
            
            foreach (var bookDto in bookDtos)
            {
                if (!IsValid(bookDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!Enum.IsDefined(typeof(Genre), bookDto.Genre))
                {
                    sb.AppendLine(ErrorMessage);
                    continue; 
                }

                var isValidDate = DateTime.TryParseExact(bookDto.PublishedOn, "MM/dd/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);

                if (!isValidDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var book = new Book
                {
                    Name = bookDto.Name,
                    Genre = (Genre)bookDto.Genre,
                    Price = bookDto.Price,
                    Pages = bookDto.Pages,
                    PublishedOn = date
                };

                books.Add(book);

                sb.AppendFormat(SuccessfullyImportedBook, book.Name, book.Price);
                sb.AppendLine();
            }

            context.Books.AddRange(books);
            context.SaveChanges();

            return sb.ToString();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var authorDtos = JsonConvert.DeserializeObject<AuthorInputModel[]>(jsonString);
            var sb = new StringBuilder();

            var emails = context.Authors.Select(x => x.Email).ToList();
            HashSet<int> existingBooks = context.Books.Select(x => x.Id).ToHashSet();
            var authors = new HashSet<Author>();
            
            foreach (var authorDto in authorDtos)
            {
                if (!IsValid(authorDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (emails.Contains(authorDto.Email))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                emails.Add(authorDto.Email);

                var author = new Author
                {
                    FirstName = authorDto.FirstName,
                    LastName = authorDto.LastName,
                    Phone = authorDto.Phone,
                    Email = authorDto.Email
                };

                foreach (var book in authorDto.Books)
                {
                    if(book.Id.HasValue && existingBooks.Contains((int)book.Id))
                    {
                        author.AuthorsBooks.Add(new AuthorBook
                        {
                            BookId = (int)book.Id,
                            Author = author
                        });
                    }
                    else
                    {
                        continue;
                    }
                }

                if (author.AuthorsBooks.Count == 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                authors.Add(author);

                sb.AppendFormat(SuccessfullyImportedAuthor, author.FirstName + " " + author.LastName, author.AuthorsBooks.Count);
                sb.AppendLine();
            }

            context.Authors.AddRange(authors);
            context.SaveChanges();

            return sb.ToString();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}