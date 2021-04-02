namespace MusicHub.DataProcessor.ImportDtos
{

    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    [XmlType("Song")]
    public class ImportSongsDto
    {
        [XmlElement("Name")]
        [Required]
        [MinLength(3), MaxLength(20)]
        public string Name { get; set; }

        [XmlElement("Duration")]
        [Required]
        public string Duration { get; set; }

        [XmlElement("CreatedOn")]
        [Required]
        public string CreatedOn { get; set; }

        [XmlElement("Genre")]
        [Required]
        public string Genre { get; set; }

        [XmlElement("AlbumId")]
        public int? AlbumId { get; set; }

        [XmlElement("WriterId")]
        public int WriterId { get; set; }

        [XmlElement("Price")]
        [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
        public decimal Price { get; set; }

    }
}
