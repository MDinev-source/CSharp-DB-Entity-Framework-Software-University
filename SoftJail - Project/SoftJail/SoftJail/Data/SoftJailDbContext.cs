namespace SoftJail.Data
{
    using SoftJail.Data.Models;
	using Microsoft.EntityFrameworkCore;

    public class SoftJailDbContext : DbContext
	{
		public SoftJailDbContext()
		{
		}

		public SoftJailDbContext(DbContextOptions options)
			: base(options)
		{
		}

        public DbSet<Cell> Cells { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Mail> Mails { get; set; }
        public DbSet<Officer> Officers { get; set; }
        public DbSet<OfficerPrisoner> OfficersPrisoners { get; set; }
        public DbSet<Prisoner> Prisoners { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder
					.UseSqlServer(Configuration.ConnectionString);
			}
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<OfficerPrisoner>()
			   .HasKey(k => new { k.OfficerId, k.PrisonerId });

			builder.Entity<OfficerPrisoner>()
			  .HasOne(p => p.Prisoner)
			  .WithMany(po => po.PrisonerOfficers)
			  .HasForeignKey(p => p.PrisonerId)
			  .OnDelete(DeleteBehavior.Restrict);

			builder.Entity<OfficerPrisoner>()
				.HasOne(o => o.Officer)
				.WithMany(op => op.OfficerPrisoners)
				.HasForeignKey(o => o.OfficerId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}