
using Microsoft.EntityFrameworkCore;

namespace FanApplicationApp.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AerodynamicsData> AerodynamicsData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AerodynamicsData>(entity =>
        {
            entity.ToTable("aerodynamics_data");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasColumnName("gas_type")
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("aerodynamic_scheme")
                .IsRequired(false);

            entity.Property(e => e.StaticPressure1)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("a1");

            entity.Property(e => e.StaticPressure2)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("a2");

            entity.Property(e => e.StaticPressure3)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("a3");

            entity.Property(e => e.Efficiency1)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("b1");

            entity.Property(e => e.Efficiency2)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("b2");

            entity.Property(e => e.Efficiency3)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("b3");

            entity.Property(e => e.Efficiency4)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("b4");

            entity.Property(e => e.MinSpeed)
                .HasColumnType("numeric(18,2)")
                .HasColumnName("min_speed");

            entity.Property(e => e.MaxSpeed)
                .HasColumnType("numeric(18,2)")
                .HasColumnName("max_speed");

            entity.Property(e => e.OutletLength)
                .HasColumnType("numeric(18,3)")
                .HasColumnName("a");

            entity.Property(e => e.OutletWidth)
                .HasColumnType("numeric(18,3)")
                .HasColumnName("b");

            entity.Property(e => e.MinDeltaEfficiency)
                .HasColumnType("numeric(18,4)")
                .HasColumnName("min_delta");

            entity.Property(e => e.MaxDeltaEfficiency)
                .HasColumnType("numeric(18,4)")
                .HasColumnName("max_delta");

            entity.Property(e => e.NewMarkOfFan)
                .HasMaxLength(100)
                .HasColumnName("new_mark_of_fan");

            entity.Property(e => e.BladeLength)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("blade_length");

            entity.Property(e => e.BladeWidth)
                .HasColumnType("numeric(18,6)")
                .HasColumnName("average_blade_width");

            entity.Property(e => e.ImpellerWidth)
                .HasColumnType("numeric(18,3)")
                .HasColumnName("impeller_width");

            entity.Property(e => e.ImpellerInletDiameter)
                .HasColumnType("numeric(18,3)")
                .HasColumnName("impeller_inlet_diameter");

            entity.Property(e => e.NumberOfBlades)
                .HasColumnName("number_of_blades");

            entity.Property(e => e.TypeOfBlades)
                .HasMaxLength(100)
                .HasColumnName("type_of_blades");

            entity.Property(e => e.NewMarkOfFanD)
                .HasMaxLength(100)
                .HasColumnName("new_mark_of_fan_d");
        });
    }
}