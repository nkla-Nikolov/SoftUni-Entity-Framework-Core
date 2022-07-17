using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    public class DepartmentDto
    {
        public DepartmentDto()
        {
            this.Cells = new List<CellDto>();
        }

        [Required]
        [MinLength(3)]
        [MaxLength(25)]
        public string Name { get; set; }

        public List<CellDto> Cells { get; set; }
    }
}
