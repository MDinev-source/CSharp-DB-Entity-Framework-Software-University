﻿namespace SoftJail.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Prisoner
    {
        public Prisoner()
        {
            this.Mails = new HashSet<Mail>();
            this.PrisonerOfficers = new HashSet<OfficerPrisoner>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string FullName { get; set; }

        [Required]
        [RegularExpression(@"^The [A - Z][a - z] +$")]
        public string Nickname { get; set; }

        [Required]
        [Range(18, 65)]
        public int Age { get; set; }

        [Required]
        public DateTime IncarcerationDate { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? Bail { get; set; }

        [Required]
        [ForeignKey(nameof(Cell))]
        public int? CellId { get; set; }

        public Cell Cell { get; set; }

        public ICollection<Mail> Mails { get; set; }
        public ICollection<OfficerPrisoner> PrisonerOfficers { get; set; }
    }
}