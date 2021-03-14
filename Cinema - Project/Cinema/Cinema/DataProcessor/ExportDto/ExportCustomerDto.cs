namespace Cinema.DataProcessor.ExportDto
{
    using System.Xml.Serialization;

    [XmlType("Customer")]
    public class ExportCustomerDto
    {
        [XmlElement(ElementName ="FirstName")]
        public string FirstName { get; set; }

        [XmlElement(ElementName ="LastName")]
        public string LastName { get; set; }

        [XmlElement(ElementName ="SpentMoney")]
        public string SpentMoney { get; set; }

        [XmlElement(ElementName ="SpentTime")]
        public string SpentTime { get; set; }

    }
}
