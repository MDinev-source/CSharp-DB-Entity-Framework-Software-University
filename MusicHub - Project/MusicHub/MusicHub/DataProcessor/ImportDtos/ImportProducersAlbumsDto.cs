namespace MusicHub.DataProcessor.ImportDtos
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;

    public class ImportProducersAlbumsDto
    {
        [JsonProperty("Name")]
        [Required]
        [MinLength(3), MaxLength(30)]
        public string Name { get; set; }

        [JsonProperty("Pseudonym")]
        [RegularExpression(@"^[A-Z][a-z]+\s[A-Z][a-z]+$")]
        public string Pseudonym { get; set; }

        [JsonProperty("PhoneNumber")]
        [RegularExpression(@"^\+359\s\d{3}\s\d{3}\s\d{3}$")]
        public string PhoneNumber { get; set; }

        [JsonProperty("Albums")]
        public AlbumsDto[] Albums { get; set; }

    }

    public class AlbumsDto
    {
        [JsonProperty("Name")]
        [Required]
        [MinLength(3), MaxLength(40)]
        public string Name { get; set; }

        [JsonProperty("ReleaseDate")]
        [Required]
        public string ReleaseDate { get; set; }
    }
}
