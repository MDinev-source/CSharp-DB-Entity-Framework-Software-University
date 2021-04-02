namespace MusicHub.DataProcessor.ImportDtos
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;

    public class ImportWritersDto
    {
        [Required]
        [MinLength(3), MaxLength(20)]
        public string Name { get; set; }

        [RegularExpression(@"^[A-Z][a-z]+\s[A-Z][a-z]+$")]
        public string Pseudonym { get; set; }

    }
}
