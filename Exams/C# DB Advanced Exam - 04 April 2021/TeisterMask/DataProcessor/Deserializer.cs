namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;

    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using TeisterMask.Data.Models;
    using TeisterMask.Data.Models.Enums;
    using TeisterMask.DataProcessor.ImportDto;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ProjectInputModel[]), new XmlRootAttribute("Projects"));
            var sb = new StringBuilder();
 
            var projectDtos = (ProjectInputModel[])serializer.Deserialize(new StringReader(xmlString));
            var projects = new HashSet<Project>();
 
            foreach (var projectDto in projectDtos) 
            {
                if (!IsValid(projectDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
 
                var isValidOpenDate = DateTime.TryParseExact(projectDto.OpenDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var projectOpenDate);
 
                if (!isValidOpenDate)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                DateTime? projectDueDate = null;

                if (!string.IsNullOrWhiteSpace(projectDto.DueDate))
                {
                    var isValidDueDate = DateTime.TryParseExact(projectDto.DueDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate);

                    if (!isValidDueDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    projectDueDate = dueDate;
                }
 
                var project = new Project
                {
                    Name = projectDto.Name,
                    OpenDate = projectOpenDate,
                    DueDate = projectDueDate
                };
 
                foreach (var taskDto in projectDto.Tasks)
                {
                    if (!IsValid(taskDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
 
                    var isValidTaskOpenDate = DateTime.TryParseExact(taskDto.OpenDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var taskOpenDate);
 
                    var isValidTaskDueDate = DateTime.TryParseExact(taskDto.DueDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var taskDueDate);
 
                    if (!isValidTaskDueDate || !isValidTaskOpenDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }
 
                    if (taskOpenDate < projectOpenDate || (projectDueDate.HasValue && taskDueDate > projectDueDate.Value))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }   
 
                    var task = new Task
                    {
                        Name = taskDto.Name,
                        OpenDate = taskOpenDate,
                        DueDate = taskDueDate,
                        ExecutionType = (ExecutionType)taskDto.ExecutionType,
                        LabelType = (LabelType)taskDto.LabelType
                    };
 
                    project.Tasks.Add(task);
                }
 
                projects.Add(project);
 
                sb.AppendFormat(SuccessfullyImportedProject, project.Name, project.Tasks.Count);
                sb.AppendLine();
            }
 
            context.Projects.AddRange(projects);
            context.SaveChanges();
 
            return sb.ToString();
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var employeeDtos = JsonConvert.DeserializeObject<EmployeeInputModel[]>(jsonString);
            var sb = new StringBuilder();

            var existingTasks = context.Tasks.Select(x => x.Id).ToHashSet();
            var employees = new HashSet<Employee>();
            
            foreach (var employeeDto in employeeDtos)
            {
                if (!IsValid(employeeDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var employee = new Employee
                {
                    Username = employeeDto.Username,
                    Email = employeeDto.Email,
                    Phone = employeeDto.Phone
                };

                foreach (var taskId in employeeDto.Tasks.Distinct())
                {
                    if (!existingTasks.Contains(taskId))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var task = new EmployeeTask
                    {
                        TaskId = taskId,
                        Employee = employee
                    };

                    employee.EmployeesTasks.Add(task);
                }

                employees.Add(employee);

                sb.AppendFormat(SuccessfullyImportedEmployee, employee.Username, employee.EmployeesTasks.Count);
                sb.AppendLine();
            }

            context.Employees.AddRange(employees);
            context.SaveChanges();

            return sb.ToString();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}