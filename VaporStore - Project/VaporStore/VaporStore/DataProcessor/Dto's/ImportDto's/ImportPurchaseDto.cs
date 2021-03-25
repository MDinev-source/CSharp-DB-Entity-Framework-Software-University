using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace VaporStore.DataProcessor.Dto_s.ImportDto_s
{
    [XmlType("Purchases")]
   public class ImportPurchaseDto
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlElement("Type")]
        [Required]
        public string Type { get; set; }

        [XmlElement("Key")]
        [Required]
        [RegularExpression(@"^[\dA-Z]{4}-[\dA-Z]{4}-[\dA-Z]{4}$")]
        public string Key { get; set; }

        [XmlElement("Card")]
        [Required]
        public string Card { get; set; }

        [XmlElement("Date")]
        [Required]
        public string Date { get; set; }

    }
}
