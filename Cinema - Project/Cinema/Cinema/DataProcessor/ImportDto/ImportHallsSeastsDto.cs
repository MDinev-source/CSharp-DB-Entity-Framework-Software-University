namespace Cinema.DataProcessor.ImportDto
{
    using System.ComponentModel.DataAnnotations;
    public class ImportHallsSeastsDto
    {
        [Required]
        [MinLength(3), MaxLength(20)]
        public string Name { get; set; }
        public bool Is4Dx { get; set; }
        public bool Is3D { get; set; }
        public int Seats { get; set; }

    }
}
