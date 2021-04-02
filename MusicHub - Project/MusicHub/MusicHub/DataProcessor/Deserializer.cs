namespace MusicHub.DataProcessor
{
    using Data;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using MusicHub.Data.Models;
    using System.Globalization;
    using System.Xml.Serialization;
    using System.Collections.Generic;
    using MusicHub.Data.Models.Enums;
    using MusicHub.DataProcessor.ImportDtos;
    using System.ComponentModel.DataAnnotations;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

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
            var writersImport = JsonConvert.DeserializeObject<ImportWritersDto[]>(jsonString);

            var sb = new StringBuilder();

            var writers = new List<Writer>();

            foreach (var writerDto in writersImport)
            {

                if (!IsValid(writerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var writer = new Writer
                {
                    Name = writerDto.Name,
                    Pseudonym = writerDto.Pseudonym
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
            var producersImport = JsonConvert.DeserializeObject<ImportProducersAlbumsDto[]>(jsonString);

            var producers = new List<Producer>();

            var sb = new StringBuilder();

            foreach (var producerDto in producersImport)
            {
                if(!IsValid(producerDto)||
                    !producerDto.Albums.All(IsValid))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var producer = new Producer
                {
                    Name = producerDto.Name,
                    Pseudonym = producerDto.Pseudonym,
                    PhoneNumber = producerDto.PhoneNumber
                };

                foreach (var albums in producerDto.Albums)
                {
                    var album = new Album
                    {
                        Name = albums.Name,
                        ReleaseDate = DateTime.ParseExact(albums.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    };

                    producer.Albums.Add(album);
                }

                producers.Add(producer);

                if (producer.PhoneNumber != null)
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
            var xmlSerializer = new XmlSerializer(typeof(ImportSongsDto[]),new XmlRootAttribute("Songs"));
            var songsImport = (ImportSongsDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var songs = new List<Song>();
            var sb = new StringBuilder();

            foreach (var importSongDto in songsImport)
            {
                var validAlbum = context.Albums.FirstOrDefault(x => x.Id == importSongDto.AlbumId);
                var validWriter = context.Writers.FirstOrDefault(x => x.Id == importSongDto.WriterId);
                var validGenreSong = Enum.TryParse<Genre>(importSongDto.Genre, out Genre genre);

              if (!IsValid(importSongDto)||
                    !validGenreSong||
                    validAlbum==null||
                    validWriter==null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

             
                var song = new Song
                {
                    Name = importSongDto.Name,
                    Duration = TimeSpan.ParseExact(importSongDto.Duration, "c", CultureInfo.InvariantCulture),
                    CreatedOn = DateTime.ParseExact(importSongDto.CreatedOn, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Genre = genre,
                    AlbumId = importSongDto.AlbumId,
                    WriterId = importSongDto.WriterId,
                    Price = importSongDto.Price
                };

                songs.Add(song);

                sb.AppendLine(string.Format(SuccessfullyImportedSong, song.Name, song.Genre, song.Duration));
            }

            context.Songs.AddRange(songs);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportSongsPerformersDto[]), new XmlRootAttribute("Performers"));
            var songsPerformersImport = (ImportSongsPerformersDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var songsPerformers = new List<Performer>();

            var sb = new StringBuilder();

            foreach (var songPerformerDto in songsPerformersImport)
            {
                if (!IsValid(songPerformerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isValidSongid = true;

                foreach (var songId in songPerformerDto.PerformersSongs)
                {
          
                    if (!(context.Songs.Any(x=>x.Id==songId.SongId)))
                    {
                        isValidSongid = false;
                        break;
                    }
                }

                if (!isValidSongid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var performer = new Performer
                {
                    FirstName = songPerformerDto.FirstName,
                    LastName = songPerformerDto.LastName,
                    Age = songPerformerDto.Age,
                    NetWorth = songPerformerDto.NetWorth
                   
                };

                performer.PerformerSongs = songPerformerDto.PerformersSongs
                    .Select(s => new SongPerformer
                    {
                        SongId = s.SongId
                    }).ToList();

                songsPerformers.Add(performer);

                sb.AppendLine(string.Format(SuccessfullyImportedPerformer,
                performer.FirstName,
                performer.PerformerSongs.Count));
            }

            context.Performers.AddRange(songsPerformers);
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