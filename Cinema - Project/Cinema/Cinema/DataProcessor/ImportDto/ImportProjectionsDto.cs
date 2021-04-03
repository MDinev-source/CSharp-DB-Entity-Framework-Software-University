namespace Cinema.DataProcessor.ImportDto
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;
    [XmlType("Projection")]
    public class ImportProjectionsDto
    {
        [XmlElement("MovieId")]
        public int MovieId { get; set; }

        [XmlElement("HallId")]
        public int HallId { get; set; }

        [XmlElement("DateTime")]
        [Required]
        public string DateTime { get; set; }
    }
}
