using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.UnitTests.Common;

public class ValueObjectTests
{
    // Test implementation of ValueObject for testing purposes
    private class TestValueObject : ValueObject
    {
        public string Property1 { get; }
        public int Property2 { get; }

        public TestValueObject(string property1, int property2)
        {
            Property1 = property1;
            Property2 = property2;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Property1;
            yield return Property2;
        }
    }

    // Another test implementation with a single property
    private class SinglePropertyValueObject : ValueObject
    {
        public string Value { get; }

        public SinglePropertyValueObject(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        vo1.Equals(vo2).Should().BeTrue();
        vo1.Equals((object)vo2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 43);

        // Act & Assert
        vo1.Equals(vo2).Should().BeFalse();
        vo1.Equals((object)vo2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var vo = new TestValueObject("test", 42);

        // Act & Assert
        vo.Equals(null).Should().BeFalse();
        vo.Equals((object?)null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new SinglePropertyValueObject("test");

        // Act & Assert
        vo1.Equals(vo2).Should().BeFalse();
        vo1.Equals((object)vo2).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        (vo1 == vo2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 43);

        // Act & Assert
        (vo1 == vo2).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WithBothNull_ShouldReturnTrue()
    {
        // Arrange
        TestValueObject? vo1 = null;
        TestValueObject? vo2 = null;

        // Act & Assert
        (vo1 == vo2).Should().BeTrue();
    }

    [Fact]
    public void OperatorEquals_WithOneNull_ShouldReturnFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        TestValueObject? vo2 = null;

        // Act & Assert
        (vo1 == vo2).Should().BeFalse();
        (vo2 == vo1).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithSameValues_ShouldReturnFalse()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        (vo1 != vo2).Should().BeFalse();
    }

    [Fact]
    public void OperatorNotEquals_WithDifferentValues_ShouldReturnTrue()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 43);

        // Act & Assert
        (vo1 != vo2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 42);

        // Act & Assert
        vo1.GetHashCode().Should().Be(vo2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentValues_ShouldBeDifferent()
    {
        // Arrange
        var vo1 = new TestValueObject("test", 42);
        var vo2 = new TestValueObject("test", 43);

        // Act & Assert
        vo1.GetHashCode().Should().NotBe(vo2.GetHashCode());
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var vo = new TestValueObject("test", 42);

        // Act & Assert
        vo.Equals(vo).Should().BeTrue();
        (vo == vo).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithComplexEqualityComponents_ShouldWorkCorrectly()
    {
        // Arrange
        var vo1 = new TestValueObject("ABC", 123);
        var vo2 = new TestValueObject("ABC", 123);
        var vo3 = new TestValueObject("abc", 123);
        var vo4 = new TestValueObject("ABC", 124);

        // Act & Assert
        vo1.Should().Be(vo2);
        vo1.Should().NotBe(vo3); // Different string case
        vo1.Should().NotBe(vo4); // Different number
    }
}
