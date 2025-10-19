using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for Post entity.
/// </summary>
public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        // Primary key
        builder.HasKey(p => p.Id);

        // Strongly-typed ID conversion
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => ConvertToPostId(value))
            .IsRequired();

        builder.Property(p => p.AuthorId)
            .HasConversion(
                id => id.Value,
                value => ConvertToAuthorId(value))
            .IsRequired();

        // Value objects as owned entities (EF Core Owned Types)
        builder.OwnsOne(p => p.Title, title =>
        {
            title.Property(t => t.Value)
                .HasColumnName("title")
                .IsRequired()
                .HasMaxLength(PostTitle.MaxLength);
        });

        builder.OwnsOne(p => p.Description, description =>
        {
            description.Property(d => d.Value)
                .HasColumnName("description")
                .IsRequired()
                .HasMaxLength(PostDescription.MaxLength);
        });

        builder.OwnsOne(p => p.Content, content =>
        {
            content.Property(c => c.Value)
                .HasColumnName("content")
                .IsRequired()
                .HasMaxLength(PostContent.MaxLength);
        });

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false);

        // Indexes for performance
        builder.HasIndex(p => p.AuthorId)
            .HasDatabaseName("idx_posts_author_id");

        builder.HasIndex(p => p.CreatedAt)
            .HasDatabaseName("idx_posts_created_at");

        // Ignore domain events (not persisted to database)
        builder.Ignore(p => p.DomainEvents);
    }

    private static PostId ConvertToPostId(Guid value)
    {
        var result = PostId.Create(value);
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Invalid Post ID in database: {result.ErrorMessage}");
        }
        return result.Value;
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
