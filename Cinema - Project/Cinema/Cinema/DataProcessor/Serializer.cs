namespace Cinema.DataProcessor
{
    using Data;
    using System;
    using System.IO;
    using System.Xml;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using System.Xml.Serialization;
    using Cinema.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportTopMovies(CinemaContext context, int rating)
        {
            var movies = context.Movies
                  .Where(m => m.Rating >= rating
                  && m.Projections.Any(p => p.Tickets.Count >= 0))
                  .OrderByDescending(m => m.Rating)
                  .ThenByDescending(m => m.Projections.Sum(p => p.Tickets.Sum(t => t.Price)))
                  .Select(m => new
                  {
                      MovieName = m.Title,
                      Rating = m.Rating.ToString("F2"),
                      TotalIncomes = (m.Projections.Sum(p => p.Tickets.Sum(t => t.Price))).ToString("F2"),
                      Customers = m.Projections.SelectMany(x => x.Tickets).Select(c => new
                      {
                          FirstName = c.Customer.FirstName,
                          LastName = c.Customer.LastName,
                          Balance = c.Customer.Balance.ToString("F2")
                      })
                      .OrderByDescending(c => decimal.Parse(c.Balance))
                      .ThenBy(c => c.FirstName)
                      .ThenBy(c => c.LastName)
                      .ToList()
                  })
                  .Take(10)
                  .ToList();

            var json = JsonConvert.SerializeObject(movies, Formatting.Indented);

            return json;

        }

        public static string ExportTopCustomers(CinemaContext context, int age)
        {
            var customers = context.Customers
                .Where(c => c.Age >= age)
                .Select(c => new ExportCustomersDto
                {
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    SpentMoney = c.Tickets.Sum(t => t.Price).ToString("F2"),
                    SpentTime = TimeSpan.FromSeconds(c.Tickets.Sum(x => x.Projection.Movie.Duration.TotalSeconds)).ToString(@"hh\:mm\:ss")
                })
                .OrderByDescending(p => decimal.Parse(p.SpentMoney))
                .Take(10)
                .ToArray();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCustomersDto[]), new XmlRootAttribute("Customers"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") });

            xmlSerializer.Serialize(new StringWriter(sb), customers, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}