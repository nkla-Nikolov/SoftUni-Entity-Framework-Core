using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace TeisterMask.DataProcessor.ExportDto
{
    [XmlType("Project")]
    public class ProjectOutputModel
    {
        public ProjectOutputModel()
        {
            this.Tasks = new List<TaskOutputModel>();
        }

        [XmlAttribute]
        public int TasksCount { get; set; }

        [XmlElement]
        public string ProjectName { get; set; }

        [XmlElement]
        public string HasEndDate { get; set; }

        [XmlArray]
        public List<TaskOutputModel> Tasks { get; set; }
    }
}
