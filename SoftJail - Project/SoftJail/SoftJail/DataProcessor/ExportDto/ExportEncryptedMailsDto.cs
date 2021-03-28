using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ExportDto
{
    [XmlType("Message")]
    public class ExportEncryptedMailsDto
    {

        [XmlElement("Description")]
        public string Description { get; set; }

    }
}
