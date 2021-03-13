﻿namespace SoftJail.DataProcessor
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
    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";
        private const string ImportedDepartmentMessage = "Imported {0} with {1} cells";
        private const string ImportedPrisonerMessage = "Imported {0} {1} years old";
        private const string ImportedOfficersPrisonerMessage = "Imported {0} ({1} prisoners)";
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var departmentsDto = JsonConvert.DeserializeObject<ImportDepatmentDto[]>(jsonString);

            List<Department> departments = new List<Department>();

            StringBuilder sb = new StringBuilder();

            foreach (var departmentDto in departmentsDto)
            {
                Department department = Mapper.Map<Department>(departmentDto);

                bool isValidDepartment = IsValid(department); ;

                bool hasInvalidCell = department.Cells.Any(c => IsValid(c) == false);

                if (isValidDepartment == false || hasInvalidCell == true)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                departments.Add(department);

                sb.AppendLine(string.Format(ImportedDepartmentMessage, department.Name, department.Cells.Count));

            }
            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersDto = JsonConvert.DeserializeObject<ImportDepatmentDto[]>(jsonString);
            List<Prisoner> prisoners = new List<Prisoner>();
            StringBuilder sb = new StringBuilder();

            foreach (var prisonerDto in prisonersDto)
            {
                Prisoner prisoner = Mapper.Map<Prisoner>(prisonerDto);

                bool isValidPrisoner = IsValid(prisoner);
                bool hasInvalidMail = prisoner.Mails.Any(m => IsValid(m) == false);

                if (isValidPrisoner == false || hasInvalidMail == true)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                prisoners.Add(prisoner);

                sb.AppendLine(String.Format(ImportedPrisonerMessage, prisoner.FullName, prisoner.Age));
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(ImportOfficerDto[]), new XmlRootAttribute("Officers"));
            var officersDto = (ImportOfficerDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            List<Officer> officers = new List<Officer>();

            StringBuilder sb = new StringBuilder();

            foreach (var officerDto in officersDto)
            {
                bool isValidPosition = Enum.IsDefined(typeof(Position), officerDto.Position);
                bool isValidWeapon = Enum.IsDefined(typeof(Weapon), officerDto.Weapon);

                if (isValidPosition == false || isValidWeapon == false)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Officer officer = Mapper.Map<Officer>(officerDto);

                bool isValidOfficer = IsValid(officer);

                if (isValidOfficer == false)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                officer.OfficerPrisoners = officerDto.Prisoners
                    .Select(p => new OfficerPrisoner
                    {
                        PrisonerId = p.Id
                    })
                    .ToList();


                officers.Add(officer);

                sb.AppendLine(string.Format(ImportedOfficersPrisonerMessage,
                 officer.FullName,
                 officer.OfficerPrisoners.Count));
            }
            context.Officers.AddRange(officers);
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