namespace SoftJail.DataProcessor
{
    using Data;
    using System;
    using System.IO;
    using AutoMapper;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using System.Xml.Serialization;
    using SoftJail.Data.Models.Enums;
    using System.Collections.Generic;
    using SoftJail.DataProcessor.ImportDto;
    using System.ComponentModel.DataAnnotations;
    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
    using System.Globalization;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";
        private const string ImportedDepartmentMessage = "Imported {0} with {1} cells";
        private const string ImportedPrisonerMessage = "Imported {0} {1} years old";
        private const string ImportedOfficersPrisonerMessage = "Imported {0} ({1} prisoners)";
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var departmentDto = JsonConvert.DeserializeObject<Department[]>(jsonString);

            var sb = new StringBuilder();
            var validDepartments = new List<Department>();

            foreach (var depDto in departmentDto)
            {
                if (!IsValid(depDto) || !depDto.Cells.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                validDepartments.Add(depDto);

                sb.AppendLine($"Imported {depDto.Name} with {depDto.Cells.Count} cells");
            }

            context.Departments.AddRange(validDepartments);
            context.SaveChanges();

            var result = sb.ToString().TrimEnd();

            return result;
        }


        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonerDto = JsonConvert.DeserializeObject<ImportPrisonerDto[]>(jsonString);

            var sb = new StringBuilder();
            var validPrisoners = new List<Prisoner>();

            foreach (var prisoner in prisonerDto)
            {
                if (!IsValid(prisoner) || !prisoner.Mails.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
                var p = new Prisoner
                {
                    FullName = prisoner.FullName,
                    Nickname = prisoner.Nickname,
                    Age = prisoner.Age,
                    IncarcerationDate = DateTime.ParseExact(prisoner.IncarcerationDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    ReleaseDate = prisoner.ReleaseDate == null ? new DateTime?() : DateTime.ParseExact(prisoner.ReleaseDate.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Bail = prisoner.Bail,
                    CellId = prisoner.CellId,
                    Mails = prisoner.Mails.Select(m => new Mail
                    {
                        Description = m.Description,
                        Sender = m.Sender,
                        Address = m.Address
                    }).ToArray()
                };

                validPrisoners.Add(p);

                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(validPrisoners);
            context.SaveChanges();

            var result = sb.ToString().TrimEnd();

            return result;
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportOfficerDto[]), new XmlRootAttribute("Officers"));

            var officersDTO = (ImportOfficerDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            var validOfficers = new List<Officer>();

            var sb = new StringBuilder();

            foreach (var officer in officersDTO)
            {
                var isValidPosition = Enum.TryParse(officer.Position, out Position resultPosition);
                var isValidWeapon = Enum.TryParse(officer.Weapon, out Weapon resultWeapon);

                if (!IsValid(officer) || !isValidPosition || !isValidWeapon)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var o = new Officer
                {
                    FullName = officer.FullName,
                    Salary = officer.Salary,
                    Position = resultPosition,
                    Weapon = resultWeapon,
                    DepartmentId = officer.DepartmentId,
                    OfficerPrisoners = officer.Prisoners.Select(s => new OfficerPrisoner
                    {
                        PrisonerId = s.Id
                    }).ToArray()
                };
                validOfficers.Add(o);
                sb.AppendLine($"Imported {officer.FullName} ({officer.Prisoners.Count()} prisoners)");

            }

            context.Officers.AddRange(validOfficers);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}