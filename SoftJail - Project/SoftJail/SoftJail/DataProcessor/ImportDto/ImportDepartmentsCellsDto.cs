namespace SoftJail.DataProcessor.ImportDto
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ImportDepartmentsCellsDto
    {
        [Required]
        [MinLength(3), MaxLength(25)]
        public string Name { get; set; }

        public ICollection<InputCells> Cells { get; set; }
    }
    public class InputCells
    {
        [Range(1,1000)]
        public int CellNumber { get; set; }
        public bool HasWindow { get; set; }
    }
}


