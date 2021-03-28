namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var departmentsCells = JsonConvert.DeserializeObject<ImportDepartmentsCellsDto[]>(jsonString);

            var departments = new List<Department>();

            var sb = new StringBuilder();

            foreach (var departCells in departmentsCells)
            {
                if (!IsValid(departCells) ||
                    !departCells.Cells.All(IsValid) ||
                    departCells.Cells.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = departCells.Name,
                    Cells = departCells.Cells
                    .Select(c => new Cell
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow
                    }).ToList()
                };

                departments.Add(department);

                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersMails = JsonConvert.DeserializeObject<ImportPrisonersMailsDto[]>(jsonString);

            var prisoners = new List<Prisoner>();

            var sb = new StringBuilder();

            foreach (var prisMails in prisonersMails)
            {
                if (!IsValid(prisMails)
                    || !prisMails.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var isValidReleaseDate = DateTime.TryParseExact(
                    prisMails.ReleaseDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime releaseDate);

                var incarcerationDate = DateTime.ParseExact(
                    prisMails.IncarcerationDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture);

                var prisoner = new Prisoner
                {
                    FullName = prisMails.FullName,
                    Nickname = prisMails.Nickname,
                    Age = prisMails.Age,
                    IncarcerationDate = incarcerationDate,
                    ReleaseDate = releaseDate,
                    Bail = prisMails.Bail,
                    CellId = prisMails.CellId,
                    Mails = prisMails.Mails.Select(m => new Mail
                    {
                        Description = m.Description,
                        Sender = m.Sender,
                        Address = m.Address
                    }).ToList()
                };

                prisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var xmlSerializar = new XmlSerializer(typeof(ImportOfficersPrisonersDto[]), new XmlRootAttribute("Officers"));

            var officersPrisoners = (ImportOfficersPrisonersDto[])xmlSerializar.Deserialize(new StringReader(xmlString));

            var officers = new List<Officer>();

            var sb = new StringBuilder();

            foreach (var officPrisoners in officersPrisoners)
            {
                var isValidPosition = Enum.TryParse(officPrisoners.Position, out Position resultPosition);
                var isValidWeapon = Enum.TryParse(officPrisoners.Weapon, out Weapon resultWeapon);

                if (!IsValid(officPrisoners) ||
                    !isValidPosition ||
                    !isValidWeapon)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officer = new Officer
                {
                    FullName = officPrisoners.Name,
                    Salary = officPrisoners.Money,
                    DepartmentId = officPrisoners.DepartmentId,
                    Position = resultPosition,
                    Weapon = resultWeapon,
                    OfficerPrisoners = officPrisoners.Prisoners.Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id
                    }).ToList()

                };

                officers.Add(officer);
                sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}