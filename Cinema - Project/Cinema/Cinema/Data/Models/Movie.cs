namespace Cinema.Data.Models
{
    using Cinema.Data.Models.Enums;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class Movie
    {
        public Movie()
        {
            this.Projections = new HashSet<Projection>();
        }
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public Genre Genre { get; set; }

        public TimeSpan Duration { get; set; }

        public double Rating { get; set; }

        [Required]
        public string Director { get; set; }

        public ICollection<Projection> Projections { get; set; }
    }
}
