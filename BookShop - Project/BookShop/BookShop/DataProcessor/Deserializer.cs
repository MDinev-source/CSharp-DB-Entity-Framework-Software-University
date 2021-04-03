namespace BookShop.DataProcessor
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
            var xmlSerializer = new XmlSerializer(typeof(ImportBooksDto[]), new XmlRootAttribute("Books"));

            var importBookDto = (ImportBooksDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var books = new List<Book>();

            var sb = new StringBuilder();

            foreach (var importDto in importBookDto)
            {
                bool isValidGenre = int.Parse(importDto.Genre) >= 1 && int.Parse(importDto.Genre) <= 3;

                if (!IsValid(importDto) ||
                    !isValidGenre)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var book = new Book
                {
                    Name = importDto.Name,
                    Genre = Enum.Parse<Genre>(importDto.Genre),
                    Price = importDto.Price,
                    Pages = importDto.Pages,
                    PublishedOn = DateTime.ParseExact(importDto.PublishedOn, "MM/dd/yyyy", CultureInfo.InvariantCulture)
                };

                books.Add(book);

                sb.AppendLine(string.Format(SuccessfullyImportedBook, book.Name, book.Price));
            }

            context.Books.AddRange(books);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAuthors(BookShopContext context, string jsonString)
        {
            var importAuthorDto = JsonConvert.DeserializeObject<ImportAuthorsDto[]>(jsonString);

            var authors = new List<Author>();

            var sb = new StringBuilder();

            foreach (var authorDto in importAuthorDto)
            {
                if (!IsValid(authorDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var email = authors.FirstOrDefault(x => x.Email == authorDto.Email);

                if (email != null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var author = new Author
                {
                    FirstName = authorDto.FirstName,
                    LastName = authorDto.LastName,
                    Phone = authorDto.Phone,
                    Email = authorDto.Email,
                };


                foreach (var bookId in authorDto.Books)
                {
                    var book = context.Books.FirstOrDefault(x => x.Id == bookId.Id);

                    if (book == null)
                    {
                        continue;
                    }

                    author.AuthorsBooks.Add(new AuthorBook
                    {
                        Author=author,
                        Book = book
                    }); 
                }

                if (!author.AuthorsBooks.Any())
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                authors.Add(author);

                var name = author.FirstName + " " + author.LastName;
                sb.AppendLine(string.Format(SuccessfullyImportedAuthor, name, author.AuthorsBooks.Count));
            }

            context.Authors.AddRange(authors);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}