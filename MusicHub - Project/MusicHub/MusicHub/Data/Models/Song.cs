namespace MusicHub.Data.Models
{
    using System;
    using MusicHub.Data.Models.Enums;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Song
    {
        public Song()
        {
            this.SongPerformer = new HashSet<SongPerformer>();
        }
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime CreatedOn { get; set; }

        public Genre Genre { get; set; }

        public int? AlbumId { get; set; }

        public Album Album { get; set; }

        public int WriterId { get; set; }
        public Writer Writer { get; set; }

        public decimal Price { get; set; }

        public ICollection<SongPerformer> SongPerformer { get; set; }
    }
}
