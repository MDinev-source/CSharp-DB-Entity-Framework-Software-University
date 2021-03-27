namespace MusicHub.DataProcessor.ImportDtos
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;

    public class ImportWritersDto
    {
        [JsonProperty("Name")]
        [Required]
        [MinLength(3), MaxLength(30)]
        public string Name { get; set; }

        [JsonProperty("Pseudonym")]
        [RegularExpression(@"^[A - Z][a - z] +\s[A - Z][a - z] +$")]
        public string Pseudonym { get; set; }

    }
}
