namespace BookShop.DataProcessor.ImportDto
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class ImportAuthorsDto
    {
        [Required]
        [MinLength(3), MaxLength(30)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(3), MaxLength(30)]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3}-[0-9]{3}-[0-9]{4}$")]
        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public importAuthorBook[] Books { get; set; }
    }

    public class importAuthorBook
    {
        public int? Id { get; set; }
    }
}
