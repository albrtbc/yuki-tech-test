using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Author entity.
/// </summary>
public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("authors");

        // Primary key
        builder.HasKey(a => a.Id);

        // Strongly-typed ID conversion
        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => ConvertToAuthorId(value))
            .IsRequired();

        // Simple properties
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(AuthorName.MaxLength);

        builder.Property(a => a.Surname)
            .IsRequired()
            .HasMaxLength(AuthorName.MaxLength);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Ignore computed property (not persisted)
        builder.Ignore(a => a.FullName);

        // Ignore domain events (not persisted to database)
        builder.Ignore(a => a.DomainEvents);

        // Seed data for development
        builder.HasData(
            new
            {
                Id = AuthorId.Create(Guid.Parse("00000000-0000-0000-0000-000000000001")).Value,
                Name = "Albert",
                Surname = "Blanco",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = AuthorId.Create(Guid.Parse("00000000-0000-0000-0000-000000000002")).Value,
                Name = "Test",
                Surname = "Two",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = AuthorId.Create(Guid.Parse("00000000-0000-0000-0000-000000000003")).Value,
                Name = "TestData",
                Surname = "Three",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }

    private static AuthorId ConvertToAuthorId(Guid value)
    {
        var result = AuthorId.Create(value);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Invalid Author ID in database: {result.ErrorMessage}");
        }
        return result.Value;
    }
}
