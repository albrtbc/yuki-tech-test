using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.ValueObjects;

namespace Yuki.Blog.Domain.UnitTests.ValueObjects;

public class AuthorNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var firstName = "Albert";
        var lastName = "Blanco";

        // Act
        var result = AuthorName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
    }

    [Theory]
    [InlineData(null, "Blanco")]
    [InlineData("", "Blanco")]
    [InlineData("   ", "Blanco")]
    public void Create_WithNullOrWhitespaceFirstName_ShouldReturnFailure(string firstName, string lastName)
    {
        // Act
        var result = AuthorName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("First name cannot be empty or whitespace.");
    }

    [Theory]
    [InlineData("Albert", null)]
    [InlineData("Albert", "")]
    [InlineData("Albert", "   ")]
    public void Create_WithNullOrWhitespaceLastName_ShouldReturnFailure(string firstName, string lastName)
    {
        // Act
        var result = AuthorName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be("Last name cannot be empty or whitespace.");
    }

    [Fact]
    public void Create_WithFirstNameExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var firstName = new string('a', AuthorName.MaxLength + 1);
        var lastName = "Blanco";

        // Act
        var result = AuthorName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be($"First name cannot exceed {AuthorName.MaxLength} characters.");
    }

    [Fact]
    public void Create_WithLastNameExceedingMaxLength_ShouldReturnFailure()
    {
        // Arrange
        var firstName = "Albert";
        var lastName = new string('a', AuthorName.MaxLength + 1);

        // Act
        var result = AuthorName.Create(firstName, lastName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be($"Last name cannot exceed {AuthorName.MaxLength} characters.");
    }

    [Fact]
    public void Create_WithNamesAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var firstName = new string('a', AuthorName.MaxLength);
        var lastName = new string('b', AuthorName.MaxLength);

        // Act
        var result = AuthorName.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
    }

    [Fact]
    public void FullName_ShouldReturnConcatenatedName()
    {
        // Arrange
        var firstName = "Albert";
        var lastName = "Blanco";
        var authorName = AuthorName.Create(firstName, lastName).Value;

        // Act
        var fullName = authorName.FullName;

        // Assert
        fullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void ToString_ShouldReturnFullName()
    {
        // Arrange
        var firstName = "Albert";
        var lastName = "Blanco";
        var authorName = AuthorName.Create(firstName, lastName).Value;

        // Act
        var result = authorName.ToString();

        // Assert
        result.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var name1 = AuthorName.Create("Albert", "Blanco").Value;
        var name2 = AuthorName.Create("Albert", "Blanco").Value;

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var name1 = AuthorName.Create("Albert", "Blanco").Value;
        var name2 = AuthorName.Create("Jane", "Smith").Value;

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 != name2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var name1 = AuthorName.Create("Albert", "Blanco").Value;
        var name2 = AuthorName.Create("Albert", "Blanco").Value;

        // Act & Assert
        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Fact]
    public void Properties_ShouldBeAccessible()
    {
        // Arrange & Act
        var authorName = AuthorName.Create("Albert", "Blanco").Value;

        // Assert
        authorName.FirstName.Should().Be("Albert");
        authorName.LastName.Should().Be("Blanco");
        authorName.FullName.Should().Be("Albert Blanco");
    }
}
