namespace SoftJail.DataProcessor
{
    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Formatting = Newtonsoft.Json.Formatting;
    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
                  .Where(p => ids.Contains(p.Id))
                  .Select(p => new
                  {
                      Id = p.Id,
                      Name = p.FullName,
                      CellNumber = p.Cell.CellNumber,
                      Officers = p.PrisonerOfficers.Select(o => new
                      {
                          OfficerName = o.Officer.FullName,
                          Department = o.Officer.Department.Name
                      })
                      .OrderBy(o => o.OfficerName)
                      .ToList(),
                      TotalOfficerSalary = decimal.Parse(p.PrisonerOfficers
                      .Sum(o => o.Officer.Salary)
                      .ToString("f2"))
                  })
                  .OrderBy(p => p.Name)
                  .ThenBy(p => p.Id)
                  .ToList();

            var json = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var names = prisonersNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            var prisoners = context.Prisoners
                .Where(p => names.Contains(p.FullName))
                .Select(p => new ExportPrisonerDto
                {
                    Id = p.Id,
                    Name = p.FullName,
                    IncarcerationDate = p.IncarcerationDate.ToString("yyyy-MM-dd"),
                    EncryptedMessages=p.Mails.Select(m => new ExportEncryptedMailsDto
                    {
                        Description = string.Join("", m.Description.Reverse())
                    })
                    .ToArray()
                })
                .OrderBy(p=>p.Name)
                .ThenBy(p=>p.Id)
                .ToArray();


            var xmlSerializer = new XmlSerializer(typeof(ExportPrisonerDto[]), new XmlRootAttribute("Prisoners"));

            var sb = new StringBuilder();
            var namespaces = new XmlSerializerNamespaces(new [] { XmlQualifiedName.Empty });
            xmlSerializer.Serialize(new StringWriter(sb), prisoners, namespaces);

            return sb.ToString().TrimEnd();
        }
    }
}