
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;
using Route = RaceDay.Api.Models.Route;

namespace RaceDay.Api.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<WeatherSnapshot> WeatherSnapshots => Set<WeatherSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasIndex(p => p.UserId)
            .IsUnique();

        modelBuilder.Entity<Route>()
            .HasIndex(r => r.EventId)
            .IsUnique();

        modelBuilder.Entity<EventCategory>()
            .HasIndex(c => new { c.EventId, c.Name })
            .IsUnique();

        modelBuilder.Entity<Enrolment>()
            .HasIndex(e => new { e.CategoryId, e.ParticipantId })
            .IsUnique();

        modelBuilder.Entity<Result>()
            .HasIndex(r => r.EnrolmentId)
            .IsUnique();

        modelBuilder.Entity<WeatherSnapshot>()
            .HasIndex(w => new { w.EventId, w.ObservedAtUtc })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.OrganisedEvents)
            .WithOne(e => e.Organiser)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Participant)
            .WithMany(u => u.Enrolments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<UserProfile>()
            .HasOne(p => p.User)
            .WithOne(u => u.UserProfile)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Route)
            .WithOne(r => r.Event)
            .HasForeignKey<Route>(r => r.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventCategory>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Categories)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WeatherSnapshot>()
            .HasOne(w => w.Event)
            .WithMany(e => e.WeatherSnapshots)
            .HasForeignKey(w => w.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Enrolment>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Result>()
            .HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<UserProfile>().ToTable("UserProfiles");
        modelBuilder.Entity<Event>().ToTable("Events");
        modelBuilder.Entity<Route>().ToTable("Routes");
        modelBuilder.Entity<EventCategory>().ToTable("EventCategories");
        modelBuilder.Entity<Enrolment>().ToTable("Enrolments");
        modelBuilder.Entity<Result>().ToTable("Results");
        modelBuilder.Entity<WeatherSnapshot>().ToTable("WeatherSnapshots");
    }
}
