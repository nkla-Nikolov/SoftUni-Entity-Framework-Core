using SoftJail.Data.Models;
using SoftJail.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class OfficerPrisonerDto
    {
        public OfficerPrisonerDto()
        {
            this.Prisoners = new List<PrisonerIdDto>();
        }

        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("Money")]
        [Range(0, double.MaxValue)]
        public decimal Money { get; set; }

        [Required]
        [XmlElement("Position")]
        public string Position { get; set; }

        [Required]
        [XmlElement("Weapon")]
        public string Weapon { get; set; }

        [XmlElement("DepartmentId")]
        public int DepartmentId { get; set; }

        [XmlArray("Prisoners")]
        public List<PrisonerIdDto> Prisoners { get; set; }
    }
}
