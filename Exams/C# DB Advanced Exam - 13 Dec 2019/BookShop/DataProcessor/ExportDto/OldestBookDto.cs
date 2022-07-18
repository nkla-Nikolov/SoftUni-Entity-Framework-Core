using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace BookShop.DataProcessor.ExportDto
{
    [XmlType("Book")]
    public class OldestBookDto
    {
        [XmlAttribute]
        [Range(50, 5000)]
        public int Pages { get; set; }

        [XmlElement]
        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public string Name { get; set; }
        
        [XmlElement]
        [Required]
        public string Date { get; set; }
    }
}
