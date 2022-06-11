using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var context = new SoftUniContext();
            Console.WriteLine(RemoveTown(context));
        }


        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    JobTitle = e.JobTitle,
                    Salary = e.Salary
                }).ToArray();


            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} " +
                    $"{employee.JobTitle} {employee.Salary:F2}");
            }


            return sb.ToString().Trim();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(s => s.Salary > 50000)
                .Select(e => new { e.FirstName, e.Salary})
                .OrderBy(x => x.FirstName)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:F2}");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    DepartmentName = x.Department.Name,
                    x.Salary
                })
                .OrderBy(x => x.Salary)
                .ThenByDescending(x => x.FirstName)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.DepartmentName}" +
                    $" - ${employee.Salary:f2}");
            }

            return sb.ToString().Trim();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var addressToAdd = new Address();
            addressToAdd.AddressText = "Vitoshka 15";
            addressToAdd.TownId = 4;

            context.Addresses.Add(addressToAdd);

            var employee = context.Employees
                .FirstOrDefault(e => e.LastName == "Nakov");

            employee.Address = addressToAdd;
            context.SaveChanges();

            var addresses = context.Employees
                .OrderByDescending(x => x.AddressId)
                .Select(a => a.Address.AddressText)
                .Take(10)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine(address);
            }

            return sb.ToString().Trim();
        }


        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context.Employees
                .Select(e => new 
                {
                    e.FirstName, 
                    e.LastName, 
                    ManagerFirstName = e.Manager.FirstName, 
                    ManagerLastName = e.Manager.LastName,
                    Projects = e.EmployeesProjects.Select(p => new
                    {
                        Name = p.Project.Name,
                        p.Project.StartDate,
                        p.Project.EndDate
                    })
                })
                .Where(x => x.Projects.Any(p => p.StartDate.Year >= 2001 && p.StartDate.Year <= 2003))
                .Take(10)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - Manager: " +
                    $"{employee.ManagerFirstName} {employee.ManagerLastName}");

                foreach (var project in employee.Projects)
                {
                    if(project.EndDate == null)
                    {
                        sb.AppendLine($"--{project.Name} - {project.StartDate.ToString("M/d/yyyy h:mm:ss tt")} - not finished");
                    }
                    else
                    {
                        sb.AppendLine($"--{project.Name} - {project.StartDate.ToString("M/d/yyyy h:mm:ss tt")} - " +
                            $"{project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt")}");
                    }
                }
            }
             
            return sb.ToString();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Select(a => new
                {
                    a.AddressText,
                    TownName = a.Town.Name,
                    Count = a.Employees.Count
                })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.TownName)
                .ThenBy(x => x.AddressText)
                .Take(10)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine($"{address.AddressText}, {address.TownName} - {address.Count} employees");
            }

            return sb.ToString();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var employee = context.Employees
                .Where(x => x.EmployeeId == 147)
                .Select(x => new 
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    Projects = x.EmployeesProjects.Select(p => new
                    {
                        ProjectName = p.Project.Name
                    })
                })
                .ToArray()
                .FirstOrDefault();
            
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");

            foreach (var project in employee.Projects.OrderBy(x => x.ProjectName))
            {
                sb.AppendLine($"{project.ProjectName}");
            }

            return sb.ToString();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context.Departments
            .Where(x => x.Employees.Count > 5)
            .Select(d => new
            {
                DepartmentName = d.Name,
                ManagerFirstName = d.Manager.FirstName,
                ManagerLastName = d.Manager.LastName,
                Employees = d.Employees.Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle
                })
            })
            .OrderBy(x => x.Employees.Count())
            .ThenBy(x => x.DepartmentName)
            .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var department in departments)
            {
                sb.AppendLine($"{department.DepartmentName} - {department.ManagerFirstName} {department.ManagerLastName}");

                foreach (var employee in department.Employees.OrderBy(x => x.FirstName).ThenBy(x => x.LastName))
                {
                    sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle}");
                }
            }

            return sb.ToString();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context.Projects
                .OrderByDescending(x => x.StartDate)
                .Take(10)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var project in projects.OrderBy(x => x.Name))
            {
                sb.AppendLine($"{project.Name}");
                sb.AppendLine($"{project.Description}");
                sb.AppendLine($"{project.StartDate.ToString("M/d/yyyy h:mm:ss tt")}");
            }

            return sb.ToString().Trim();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.Department.Name == "Engineering" ||
                x.Department.Name == "Tool Design" ||
                x.Department.Name == "Marketing" ||
                x.Department.Name == "Information Services")
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    Salary = x.Salary * 1.12m
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToArray();

            
            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} (${employee.Salary:f2})");
            }

            return sb.ToString().Trim();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context.Employees
                .Where(x => x.FirstName.StartsWith("Sa"))
                .Select(x => new
                {
                    x.FirstName,
                    x.LastName,
                    x.JobTitle,
                    x.Salary
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToArray();

            StringBuilder sb = new StringBuilder();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} - {employee.JobTitle} - (${employee.Salary:f2})");
            }

            return sb.ToString().Trim();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var projectToDelete = context.EmployeesProjects.FirstOrDefault(x => x.ProjectId == 2);
            StringBuilder sb = new StringBuilder();

            var delete = context.EmployeesProjects.Remove(projectToDelete);
            context.SaveChanges();

            var projects = context
                .Projects
                .Take(10)
                .ToArray();

            foreach (var project in projects)
            {
                sb.AppendLine($"{project.Name}");
            }

            return sb.ToString().Trim();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var addresses = context.Addresses
                .Where(x => x.Town.Name == "Seattle")
                .ToArray();

            foreach (var address in addresses)
            {
                context.Addresses.Remove(address);
            }

            var town = context.Towns
                .Where(x => x.Name == "Seattle")
                .Single();

            context.Towns.Remove(town);
            context.SaveChanges();

            return $"{addresses.Length} addresses in Seattle were deleted";
        }
    }
}
