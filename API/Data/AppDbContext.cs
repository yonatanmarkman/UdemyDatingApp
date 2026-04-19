using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<MemberLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MemberLike>()
            .HasKey(x => new
            {
                x.SourceMemberId, x.TargetMemberId
            });

        // Each memberLike has one source member,
        // and each member CAN LIKE many other members.
        // If the source member is deleted, then all their likes are also deleted.
        // This is set-up by the DeleteBehavior.Cascade property.
        modelBuilder.Entity<MemberLike>()
            .HasOne(s => s.SourceMember)
            .WithMany(t => t.LikedMembers)
            .HasForeignKey(s => s.SourceMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Each memberLike has one target member,
        // and each member CAN BE LIKED BY many other members.
        // If the target member is deleted, their likes are unaffected.
        // This is meant to prevent circular cascade deletions, but this also means
        // that we must handle the 'orphaned like records' by ourselves, manually.
        modelBuilder.Entity<MemberLike>()
            .HasOne(s => s.TargetMember)
            .WithMany(t => t.LikedByMembers)
            .HasForeignKey(s => s.TargetMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        // Create the value converter from universal DateTime to UTC DateTime
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            value => value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        );

        // For each entity type, go over each property,
        // and utilize the converter when the property is a DateTime.
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }
    }
}
