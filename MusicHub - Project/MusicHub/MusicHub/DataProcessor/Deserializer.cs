namespace MusicHub.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using MusicHub.Data.Models;
    using MusicHub.Data.Models.Enums;
    using MusicHub.DataProcessor.ImportDtos;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";

        private const string SuccessfullyImportedWriter
            = "Imported {0}";
        private const string SuccessfullyImportedProducerWithPhone
            = "Imported {0} with phone: {1} produces {2} albums";
        private const string SuccessfullyImportedProducerWithNoPhone
            = "Imported {0} with no phone number produces {1} albums";
        private const string SuccessfullyImportedSong
            = "Imported {0} ({1} genre) with duration {2}";
        private const string SuccessfullyImportedPerformer
            = "Imported {0} ({1} songs)";

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var writersDto = JsonConvert.DeserializeObject<ImportWritersDto[]>(jsonString);

            var writers = new List<Writer>();

            var sb = new StringBuilder();

            foreach (var writerDto in writersDto)
            {

                if (!IsValid(writerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var writer = new Writer
                {
                    Name = writerDto.Name,
                    Pseudonym = writerDto.Pseudonym,
                };

                writers.Add(writer);

                sb.AppendLine(string.Format(SuccessfullyImportedWriter, writer.Name));
            }

            context.Writers.AddRange(writers);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var producersAlbumsDto = JsonConvert.DeserializeObject<ImportProducersAlbumsDto[]>(jsonString);

            var producers = new List<Producer>();

            var sb = new StringBuilder();

            foreach (var producerAlbum in producersAlbumsDto)
            {
                if (!IsValid(producerAlbum) || !producerAlbum.Albums.All(IsValid))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var producer = new Producer
                {
                    Name = producerAlbum.Name,
                    Pseudonym = producerAlbum.Pseudonym,
                    PhoneNumber = producerAlbum.PhoneNumber
                };

                foreach (var album in producerAlbum.Albums)
                {
                    producer.Albums.Add(new Album
                    {
                        Name = album.Name,
                        ReleaseDate = DateTime.ParseExact(album.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    });
                }

                producers.Add(producer);

                if (producerAlbum.PhoneNumber != null)
                {

                    sb.AppendLine(string.Format(SuccessfullyImportedProducerWithPhone, producer.Name, producer.PhoneNumber, producer.Albums.Count));
                }
                else
                {
                    sb.AppendLine(string.Format(SuccessfullyImportedProducerWithNoPhone, producer.Name, producer.Albums.Count));
                }
            }

            context.Producers.AddRange(producers);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportSongsDto[]), new XmlRootAttribute("Songs"));
            var songsDto = (ImportSongsDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var songs = new List<Song>();

            var sb = new StringBuilder();

            foreach (var songDto in songsDto)
            {
                if (!IsValid(songDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var validGenre = Enum.TryParse<Genre>(songDto.Genre, out Genre genreResult);
                var writer = context.Writers.Find(songDto.WriterId);
                var album = context.Albums.Find(songDto.AlbumId);

                if (!validGenre || writer == null | album == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var song = new Song
                {
                    Name = songDto.Name,
                    Duration = TimeSpan.ParseExact(songDto.Duration, "c", CultureInfo.InvariantCulture),
                    CreatedOn = DateTime.ParseExact(songDto.CreatedOn, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Genre = Enum.Parse<Genre>(songDto.Genre),
                    AlbumId = songDto.AlbumId,
                    WriterId = songDto.WriterId,
                    Price = songDto.Price
                };

                songs.Add(song);

                sb.AppendLine(string.Format(SuccessfullyImportedSong, song.Name, song.Genre, song.Duration));

            }

            context.AddRange(songs);
            context.SaveChanges();

            return sb.ToString().TrimEnd();

        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportSongPerfomersDto[]), new XmlRootAttribute("Performers"));
            var songPerformersDto = (ImportSongPerfomersDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var performers = new List<Performer>();
            var sb = new StringBuilder();

            var songsIds = context.Songs.Select(s => s.Id).ToList();

            foreach (var songPerformer in songPerformersDto)
            {
                if (!IsValid(songPerformer))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                var validSongsCount = context.Songs.Count(s => songPerformer.PerformersSongs.Any(i => i.SongId == s.Id));

                if (validSongsCount != songPerformer.PerformersSongs.Length)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var performer = new Performer
                {
                    FirstName = songPerformer.FirstName,
                    LastName = songPerformer.LastName,
                    Age = songPerformer.Age,
                    NetWorth = songPerformer.NetWorth
                };

                foreach (var song in songPerformer.PerformersSongs)
                {
                    performer.PerformerSongs.Add(new SongPerformer
                    {
                        SongId = song.SongId
                    });

                }

                performers.Add(performer);

                sb.AppendLine(string.Format(SuccessfullyImportedPerformer, performer.FirstName, performer.PerformerSongs.Count));
            }

            context.Performers.AddRange(performers);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object entity)
        {
            var validationContext = new ValidationContext(entity);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(entity, validationContext, validationResult, true);
            return isValid;
        }
    }
}