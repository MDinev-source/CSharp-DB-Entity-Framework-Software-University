namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using System;
    using System.Linq;

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
                      .OrderBy(o=>o.OfficerName)
                      .ToList(),
                      TotalOfficerSalary = decimal.Parse(p.PrisonerOfficers
                      .Sum(o => o.Officer.Salary)
                      .ToString("f2"))
                  })
                  .OrderBy(p=>p.Name)
                  .ThenBy(p=>p.Id)
                  .ToList();

            var json = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            throw new NotImplementedException();
        }
    }
}