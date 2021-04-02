namespace MusicHub.DataProcessor
{
    using Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using MusicHub.DataProcessor.ExportDtos;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums
                .Where(x => x.ProducerId == producerId)
                .Select(a => new
                {
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy",CultureInfo.InvariantCulture),
                    ProducerName = a.Producer.Name,
                    Songs = a.Songs.Select(s => new
                    {
                        SongName = s.Name,
                        Price = s.Price.ToString("F2"),
                        Writer = s.Writer.Name
                    })
                    .OrderByDescending(p=>p.SongName)
                    .ThenBy(w=>w.Writer)
                    .ToList(),
                    AlbumPrice=a.Price.ToString("F2")
                })
                .OrderByDescending(a=>decimal.Parse(a.AlbumPrice))
                .ToList();

            var json = JsonConvert.SerializeObject(albums, Formatting.Indented);
            return json;
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                  .Where(s => s.Duration.TotalSeconds > duration)
                  .Select(s => new ExportSongsDto
                  {
                      SongName = s.Name,
                      Writer = s.Writer.Name,
                      Performer = s.SongPerformer.Select(p => p.Performer.FirstName + " " + p.Performer.LastName).FirstOrDefault(),
                      AlbumProducer=s.Album.Producer.Name,
                      Duration=s.Duration.ToString("c", CultureInfo.InvariantCulture)
                  })
                  .OrderBy(s=>s.SongName)
                  .ThenBy(s=>s.Writer)
                  .ThenBy(s=>s.Performer)
                  .ToArray();

            var xmlSerializer = new XmlSerializer(typeof(ExportSongsDto[]), new XmlRootAttribute("Songs"));

            var sb = new StringBuilder();

            var namespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            xmlSerializer.Serialize(new StringWriter(sb), songs, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}