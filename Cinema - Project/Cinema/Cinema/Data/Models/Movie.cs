namespace Cinema.Data.Models
{
    using System;
    using Cinema.Data.Models.Enums;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Movie
    {
        public Movie()
        {
            this.Projections = new HashSet<Projection>();
        }
        [Key]
        public int Id { get; set; }

        [MinLength(3), MaxLength(20), Required]
        public string Title { get; set; }

        [Required]
        public Genre Genre { get; set; }

        [Required]
        public TimeSpan Duration { get; set; }

        [Range(1, 10), Required]
        public double Rating { get; set; }

        [MinLength(3), MaxLength(20), Required]
        public string Director { get; set; }

        public virtual ICollection<Projection> Projections { get; set; }
    }
}
