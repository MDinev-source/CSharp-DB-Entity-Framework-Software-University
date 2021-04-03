namespace Cinema.DataProcessor.ImportDto
{
    using System.ComponentModel.DataAnnotations;
    using System.Xml.Serialization;
    [XmlType("Customer")]
    public class ImportCustomersTicketsDto
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
        [Range(12, 110)]
        public int Age { get; set; }

        [XmlElement("Balance")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Balance { get; set; }

        [XmlArray("Tickets")]
        public ImportTicketsDto[] Tickets { get; set; }
    }

    [XmlType("Ticket")]
    public class ImportTicketsDto
    {
        [XmlElement("ProjectionId")]
        public int Projectionid { get; set; }

        [XmlElement("Price")]

        [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
        public decimal Price { get; set; }
    }
}
