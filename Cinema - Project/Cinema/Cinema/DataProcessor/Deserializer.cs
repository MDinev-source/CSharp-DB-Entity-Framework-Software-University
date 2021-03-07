namespace Cinema.DataProcessor
{
    using Data;
    using System;
    using AutoMapper;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Cinema.Data.Models;
    using Cinema.Data.Models.Enums;
    using System.Collections.Generic;
    using Cinema.DataProcessor.ImportDto;
    using System.ComponentModel.DataAnnotations;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

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
            var moviesDto = JsonConvert.DeserializeObject<ImportMovieDto[]>(jsonString);

            List<Movie> movies = new List<Movie>();

            StringBuilder sb = new StringBuilder();

            foreach (var movieDto in moviesDto)
            {
                bool IsValidGenre = Enum.IsDefined(typeof(Genre), movieDto.Genre);

                bool isMovieExist = movies.Any(m => m.Title == movieDto.Title);

                if (IsValidGenre == false || isMovieExist == true)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Movie movie = Mapper.Map<Movie>(movieDto);

                bool isValidMovie = IsValid(movie);

                if (isValidMovie == false)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                movies.Add(movie);

                sb.AppendLine(string.Format(SuccessfulImportMovie,
                    movie.Title,
                    movie.Genre.ToString(),
                    movie.Rating.ToString("F2")));

            }

            context.Movies.AddRange(movies);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {

            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }

        public static string ImportHallSeats(CinemaContext context, string jsonString)
        {
            var hallsDto = JsonConvert.DeserializeObject<ImportHallWithSeatsDto[]>(jsonString);

            List<Hall> halls = new List<Hall>();

            StringBuilder sb = new StringBuilder();

            foreach (var hallDto in hallsDto)
            {
                bool isValidDto = IsValid(hallsDto);

                Hall hall = Mapper.Map<Hall>(hallDto);

                bool isValidHall = IsValid(hall);

                if (isValidDto == false || isValidHall == false)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                for (int i = 0; i < hallDto.SeatsCount; i++)
                {

                    hall.Seats.Add(new Seat());
                }

                string projectionType = string.Empty;

                if (hall.Is3D == true && hall.Is4Dx == true)
                {
                    projectionType = "4Dx/3D";
                }

                else if (hall.Is4Dx == true)
                {
                    projectionType = "4Dx";
                }
                else if (hall.Is3D == true)
                {
                    projectionType = "3D";
                }
                else
                {
                    projectionType = "Normal";
                }

                halls.Add(hall);

                sb.AppendLine(string.Format(SuccessfulImportHallSeat,
                        hall.Name,
                        projectionType,
                        hall.Seats.Count()));
            }

            context.Halls.AddRange(halls);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportProjections(CinemaContext context, string xmlString)
        {
            throw new NotImplementedException();
        }

        public static string ImportCustomerTickets(CinemaContext context, string xmlString)
        {
            throw new NotImplementedException();
        }
    }
}