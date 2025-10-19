using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.Entities;

namespace Yuki.Blog.Domain.UnitTests.Entities;

public class AuthorTests
{
    private readonly DateTime _createdAt = DateTime.UtcNow;

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Albert";
        var surname = "Blanco";

        // Act
        var result = Author.Create(name, surname, _createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Surname.Should().Be(surname);
        result.Value.CreatedAt.Should().Be(_createdAt);
        result.Value.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(null, "Blanco")]
    [InlineData("", "Blanco")]
    [InlineData("   ", "Blanco")]
    public void Create_WithInvalidName_ShouldReturnFailure(string name, string surname)
    {
        // Act
        var result = Author.Create(name, surname, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("First name cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData("Albert", null)]
    [InlineData("Albert", "")]
    [InlineData("Albert", "   ")]
    public void Create_WithInvalidSurname_ShouldReturnFailure(string name, string surname)
    {
        // Act
        var result = Author.Create(name, surname, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Last name cannot be empty or whitespace.");
    }

    [Fact]
    public void CreateWithId_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Albert";
        var surname = "Blanco";

        // Act
        var result = Author.CreateWithId(id, name, surname, _createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Value.Should().Be(id);
        result.Value.Name.Should().Be(name);
        result.Value.Surname.Should().Be(surname);
        result.Value.CreatedAt.Should().Be(_createdAt);
    }

    [Fact]
    public void CreateWithId_WithInvalidName_ShouldReturnFailure()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "";
        var surname = "Blanco";

        // Act
        var result = Author.CreateWithId(id, name, surname, _createdAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("First name cannot be empty or whitespace.");
    }

    [Fact]
    public void UpdateName_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;
        var newName = "Jane";
        var newSurname = "Smith";

        // Act
        var result = author.UpdateName(newName, newSurname);

        // Assert
        result.IsSuccess.Should().BeTrue();
        author.Name.Should().Be(newName);
        author.Surname.Should().Be(newSurname);
    }

    [Theory]
    [InlineData(null, "Smith")]
    [InlineData("", "Smith")]
    [InlineData("   ", "Smith")]
    public void UpdateName_WithInvalidName_ShouldReturnFailure(string name, string surname)
    {
        // Arrange
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;

        // Act
        var result = author.UpdateName(name, surname);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("First name cannot be empty or whitespace.");
        author.Name.Should().Be("Albert");
        author.Surname.Should().Be("Blanco");
    }

    [Theory]
    [InlineData("Jane", null)]
    [InlineData("Jane", "")]
    [InlineData("Jane", "   ")]
    public void UpdateName_WithInvalidSurname_ShouldReturnFailure(string name, string surname)
    {
        // Arrange
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;

        // Act
        var result = author.UpdateName(name, surname);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Last name cannot be empty or whitespace.");
        author.Name.Should().Be("Albert");
        author.Surname.Should().Be("Blanco");
    }

    [Fact]
    public void FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var name = "Albert";
        var surname = "Blanco";
        var author = Author.Create(name, surname, _createdAt).Value;

        // Act
        var fullName = author.FullName;

        // Assert
        fullName.Should().Be($"{name} {surname}");
    }

    [Fact]
    public void FullName_ShouldUpdateAfterNameChange()
    {
        // Arrange
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;
        var newName = "Jane";
        var newSurname = "Smith";

        // Act
        author.UpdateName(newName, newSurname);

        // Assert
        author.FullName.Should().Be($"{newName} {newSurname}");
    }

    [Fact]
    public void Create_WithLongValidNames_ShouldReturnSuccess()
    {
        // Arrange
        var name = new string('A', 50);
        var surname = new string('B', 50);

        // Act
        var result = Author.Create(name, surname, _createdAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Surname.Should().Be(surname);
    }

    [Fact]
    public void UpdateName_ShouldNotChangeCreatedAt()
    {
        // Arrange
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;
        var originalCreatedAt = author.CreatedAt;

        // Act
        author.UpdateName("Jane", "Smith");

        // Assert
        author.CreatedAt.Should().Be(originalCreatedAt);
    }

    [Fact]
    public void UpdateName_ShouldNotChangeId()
    {
        // Arrange
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;
        var originalId = author.Id;

        // Act
        author.UpdateName("Jane", "Smith");

        // Assert
        author.Id.Should().Be(originalId);
    }

    [Fact]
    public void Properties_ShouldBeAccessible()
    {
        // Arrange & Act
        var author = Author.Create("Albert", "Blanco", _createdAt).Value;

        // Assert - Access all properties to ensure they're covered
        author.Id.Should().NotBeNull();
        author.Name.Should().Be("Albert");
        author.Surname.Should().Be("Blanco");
        author.CreatedAt.Should().Be(_createdAt);
        author.FullName.Should().Be("Albert Blanco");
    }

    [Fact]
    public void CreateWithId_ShouldSetSpecificId()
    {
        // Arrange
        var specificId = Guid.NewGuid();

        // Act
        var author = Author.CreateWithId(specificId, "Albert", "Blanco", _createdAt).Value;

        // Assert
        author.Id.Value.Should().Be(specificId);
    }
}
