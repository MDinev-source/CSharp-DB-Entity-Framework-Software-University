namespace MusicHub.DataProcessor.ImportDtos
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;

    [XmlType("Performer")]
    public class ImportSongsPerformersDto
    {
        [XmlElement("FirstName")]
        [Required]
        [MinLength(3), MaxLength(20)]
        public string FirstName { get; set; }

        [XmlElement("LastName")]
        [Required]
        [MinLength(3), MaxLength(20)]
        public string LastName { get; set; }

        [XmlElement("Age")]
        [Range(18,70)]
        public int Age { get; set; }

        [XmlElement("NetWorth")]
        [Range(typeof(decimal),"0.00", "79228162514264337593543950335")]
        public decimal NetWorth { get; set; }

        [XmlArray("PerformersSongs")]
        public performerSongDto[] PerformersSongs { get; set; }
    }

    [XmlType("Song")]
    public class performerSongDto
    {
        [XmlAttribute("id")]
        public int SongId { get; set; }
    }
}
