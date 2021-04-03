namespace Cinema.DataProcessor
{
    using Data;
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Cinema.Data.Models;
    using System.Collections.Generic;
    using Cinema.DataProcessor.ImportDto;
    using System.ComponentModel.DataAnnotations;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
    using System.Linq;
    using Cinema.Data.Models.Enums;
    using System.Globalization;
    using System.Xml.Serialization;
    using System.IO;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";
        private const string SuccessfulImportMovie
            = "Successfully imported {0} with genre {1} and rating {2}!";
        private const string SuccessfulImportHallSeat
            = "Successfully imported {0}({1}) with {2} seats!";
        private const string SuccessfulImportProjection
            = "Successfully imported projection {0} on {1}!";
        private const string SuccessfulImportCustomerTicket
            = "Successfully imported customer {0} {1} with bought tickets: {2}!";

        public static string ImportMovies(CinemaContext context, string jsonString)
        {
            var moviesImport = JsonConvert.DeserializeObject<ImportMoviesDto[]>(jsonString);

            var movies = new List<Movie>();

            var sb = new StringBuilder();

            foreach (var importMovieDto in moviesImport)
            {
                var validGenre = Enum.TryParse(importMovieDto.Genre, out Genre genre);

                if (!IsValid(importMovieDto) ||
                    !validGenre ||
                    movies.Any(x => x.Title == importMovieDto.Title))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var movie = new Movie
                {
                    Title = importMovieDto.Title,
                    Genre = genre,
                    Duration = TimeSpan.ParseExact(importMovieDto.Duration, @"hh\:mm\:ss", CultureInfo.InvariantCulture),
                    Rating = importMovieDto.Rating,
                    Director = importMovieDto.Director
                };

                movies.Add(movie);

                sb.AppendLine(string.Format(SuccessfulImportMovie, movie.Title, movie.Genre.ToString(), movie.Rating.ToString("F2")));
            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var hallsImport = JsonConvert.DeserializeObject<ImportHallsSeastsDto[]>(jsonString);

            var halls = new List<Hall>();

            var sb = new StringBuilder();

            foreach (var importHallSeatsDto in hallsImport)
            {
                if (!IsValid(importHallSeatsDto) ||
                    importHallSeatsDto.Seats <= 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var hall = new Hall
                {
                    Name = importHallSeatsDto.Name,
                    Is4Dx = importHallSeatsDto.Is4Dx,
                    Is3D = importHallSeatsDto.Is3D
                };

                for (int i = 1; i <= importHallSeatsDto.Seats; i++)
                {
                    hall.Seats.Add(new Seat());
                }

                halls.Add(hall);

                string projectionType = string.Empty;

                if (hall.Is4Dx && hall.Is3D)
                {
                    projectionType = "4Dx/3D";
                }
                else if (hall.Is4Dx && !hall.Is3D)
                {
                    projectionType = "4Dx";
                }
                else if (!hall.Is4Dx && hall.Is3D)
                {
                    projectionType = "3D";
                }
                else
                {
                    projectionType = "Normal";
                }

                sb.AppendLine(string.Format(SuccessfulImportHallSeat, hall.Name, projectionType, hall.Seats.Count));
            }

            context.Halls.AddRange(halls);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportProjectionsDto[]), new XmlRootAttribute("Projections"));

            var projectionsImport = (ImportProjectionsDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var projections = new List<Projection>();

            var sb = new StringBuilder();

            foreach (var importProjectionDto in projectionsImport)
            {

                var validMovie = context.Movies.FirstOrDefault(x => x.Id == importProjectionDto.MovieId);
                var validHall = context.Halls.FirstOrDefault(x => x.Id == importProjectionDto.HallId);

                if (validHall == null
                    || validMovie == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var projection = new Projection
                {
                    MovieId = importProjectionDto.MovieId,
                    HallId = importProjectionDto.HallId,
                    DateTime = DateTime.ParseExact(importProjectionDto.DateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                };

                projections.Add(projection);

                sb.AppendLine(string.Format(SuccessfulImportProjection, validMovie.Title, projection.DateTime.ToString("MM/dd/yyyy")));
            }

            context.Projections.AddRange(projections);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportCustomersTicketsDto[]), new XmlRootAttribute("Customers"));
            var customersImport = (ImportCustomersTicketsDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var customers = new List<Customer>();

            var sb = new StringBuilder();

            foreach (var importCustomerDto in customersImport)
            {

                if (!IsValid(importCustomerDto)||
                    !importCustomerDto.Tickets.All(IsValid))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var customer = new Customer
                {
                    FirstName = importCustomerDto.FirstName,
                    LastName = importCustomerDto.LastName,
                    Age = importCustomerDto.Age,
                    Balance = importCustomerDto.Balance,
                    Tickets=importCustomerDto.Tickets.Select(x=>new Ticket
                    {
                        ProjectionId=x.Projectionid,
                        Price=x.Price
                    }).ToArray()
                };

                customers.Add(customer);

                sb.AppendLine(string.Format(SuccessfulImportCustomerTicket, customer.FirstName, customer.LastName, customer.Tickets.Count));
            }

            context.Customers.AddRange(customers);
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