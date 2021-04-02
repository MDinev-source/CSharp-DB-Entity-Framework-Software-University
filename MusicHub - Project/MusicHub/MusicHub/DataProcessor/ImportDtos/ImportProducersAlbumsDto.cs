namespace MusicHub.DataProcessor.ImportDtos
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class ImportProducersAlbumsDto
    {
        [Required]
        [MinLength(3), MaxLength(30)]
        public string Name { get; set; }

        [RegularExpression(@"^[A-Z][a-z]+\s[A-Z][a-z]+$")]
        public string Pseudonym { get; set; }

        [RegularExpression(@"^\+359 [0-9]{3} [0-9]{3} [0-9]{3}$")]
        public string PhoneNumber { get; set; }

        public ICollection<AlbumDto> Albums { get; set; }
    }
    public class AlbumDto
    {
        [Required]
        [MinLength(3), MaxLength(40)]
        public string Name { get; set; }

        [Required]
        public string ReleaseDate { get; set; }

    }
}
