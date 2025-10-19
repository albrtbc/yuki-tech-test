using AutoMapper;
using FluentAssertions;
using Xunit;
using Yuki.Blog.Application.Common.Mappings;
using Yuki.Blog.Application.Features.Posts.Commands.CreatePost;
using Yuki.Blog.Application.Features.Posts.Queries.GetPostById;
using Yuki.Blog.Domain.Entities;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Application.UnitTests.Common.Mappings;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void MappingProfile_ShouldHaveValidConfiguration()
    {
        // Act & Assert
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_PostToGetPostByIdResponse_ShouldMapCorrectly()
    {
        // Arrange
        var authorId = AuthorId.CreateUnique();
        var createdAt = DateTime.UtcNow;
        var post = Post.Create(
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            createdAt).Value;

        // Act
        var response = _mapper.Map<GetPostByIdResponse>(post);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(post.Id.Value);
        response.AuthorId.Should().Be(authorId.Value);
        response.Title.Should().Be("Test Title");
        response.Description.Should().Be("Test Description");
        response.Content.Should().Be("Test Content");
        response.CreatedAt.Should().Be(createdAt);
        response.UpdatedAt.Should().BeNull();
        response.Author.Should().BeNull(); // Ignored in mapping
    }

    [Fact]
    public void Map_PostToCreatePostResponse_ShouldMapCorrectly()
    {
        // Arrange
        var authorId = AuthorId.CreateUnique();
        var createdAt = DateTime.UtcNow;
        var post = Post.Create(
            authorId,
            "Test Title",
            "Test Description",
            "Test Content",
            createdAt).Value;

        // Act
        var response = _mapper.Map<CreatePostResponse>(post);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(post.Id.Value);
        response.AuthorId.Should().Be(authorId.Value);
        response.Title.Should().Be("Test Title");
        response.Description.Should().Be("Test Description");
        response.Content.Should().Be("Test Content");
        response.CreatedAt.Should().Be(createdAt);
        response.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Map_UpdatedPost_ShouldIncludeUpdatedAt()
    {
        // Arrange
        var authorId = AuthorId.CreateUnique();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        var post = Post.Create(
            authorId,
            "Original Title",
            "Original Description",
            "Original Content",
            createdAt).Value;

        post.UpdateContent("Updated Content", updatedAt);

        // Act
        var response = _mapper.Map<CreatePostResponse>(post);

        // Assert
        response.UpdatedAt.Should().Be(updatedAt);
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Map_AuthorToGetPostByIdAuthorResponse_ShouldMapCorrectly()
    {
        // Arrange
        var firstName = "Albert";
        var lastName = "Blanco";
        var author = Author.Create(firstName, lastName, DateTime.UtcNow).Value;

        // Act
        var response = _mapper.Map<GetPostByIdAuthorResponse>(author);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(author.Id.Value);
        response.Name.Should().Be(firstName);
        response.Surname.Should().Be(lastName);
    }

    [Fact]
    public void Map_Post_ShouldUnwrapValueObjects()
    {
        // Arrange
        var authorId = AuthorId.CreateUnique();
        var post = Post.Create(
            authorId,
            "Title",
            "Description",
            "Content",
            DateTime.UtcNow).Value;

        // Act
        var response = _mapper.Map<CreatePostResponse>(post);

        // Assert
        response.Id.Should().Be(post.Id.Value);
        response.AuthorId.Should().Be(post.AuthorId.Value);
        response.Title.Should().Be(post.Title.Value);
        response.Description.Should().Be(post.Description.Value);
        response.Content.Should().Be(post.Content.Value);
    }

    [Fact]
    public void Map_MultiplePostsToResponses_ShouldWorkCorrectly()
    {
        // Arrange
        var posts = new List<Post>
        {
            Post.Create(AuthorId.CreateUnique(), "Title1", "Desc1", "Content1", DateTime.UtcNow).Value,
            Post.Create(AuthorId.CreateUnique(), "Title2", "Desc2", "Content2", DateTime.UtcNow).Value,
            Post.Create(AuthorId.CreateUnique(), "Title3", "Desc3", "Content3", DateTime.UtcNow).Value
        };

        // Act
        var responses = _mapper.Map<List<CreatePostResponse>>(posts);

        // Assert
        responses.Should().HaveCount(3);
        responses[0].Title.Should().Be("Title1");
        responses[1].Title.Should().Be("Title2");
        responses[2].Title.Should().Be("Title3");
    }
}
