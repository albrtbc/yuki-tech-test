using FluentAssertions;
using Xunit;
using Yuki.Blog.Domain.Common;

namespace Yuki.Blog.Domain.UnitTests.Common;

public class AggregateRootTests
{
    // Test implementation of domain event
    private class TestDomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; }

        public TestDomainEvent()
        {
            OccurredOn = DateTime.UtcNow;
        }
    }

    // Test implementation of AggregateRoot
    private class TestAggregateRoot : AggregateRoot<Guid>
    {
        public TestAggregateRoot(Guid id) : base(id) { }

        private TestAggregateRoot() : base() { }

        public void AddTestEvent(IDomainEvent domainEvent)
        {
            RaiseDomainEvent(domainEvent);
        }

        public static TestAggregateRoot CreateEmpty() => new TestAggregateRoot();
    }

    [Fact]
    public void Constructor_WithId_ShouldCreateAggregateRoot()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var aggregate = new TestAggregateRoot(id);

        // Assert
        aggregate.Id.Should().Be(id);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ParameterlessConstructor_ShouldCreateAggregateRoot()
    {
        // Act
        var aggregate = TestAggregateRoot.CreateEmpty();

        // Assert
        aggregate.Should().NotBeNull();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToDomainEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.AddTestEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void RaiseDomainEvent_MultipleEvents_ShouldAddAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        var event3 = new TestDomainEvent();

        // Act
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);
        aggregate.AddTestEvent(event3);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents.Should().Contain(event1);
        aggregate.DomainEvents.Should().Contain(event2);
        aggregate.DomainEvents.Should().Contain(event3);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.AddTestEvent(new TestDomainEvent());
        aggregate.AddTestEvent(new TestDomainEvent());
        aggregate.AddTestEvent(new TestDomainEvent());

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WhenNoEvents_ShouldNotThrow()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());

        // Act
        var act = () => aggregate.ClearDomainEvents();

        // Assert
        act.Should().NotThrow();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.AddTestEvent(new TestDomainEvent());

        // Act
        var domainEvents = aggregate.DomainEvents;

        // Assert
        domainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }

    [Fact]
    public void DomainEvents_AfterClear_NewEventsCanBeAdded()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(Guid.NewGuid());
        aggregate.AddTestEvent(new TestDomainEvent());
        aggregate.ClearDomainEvents();

        // Act
        var newEvent = new TestDomainEvent();
        aggregate.AddTestEvent(newEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(newEvent);
    }

    [Fact]
    public void InheritsFromEntity_ShouldHaveEntityBehavior()
    {
        // Arrange
        var id = Guid.NewGuid();
        var aggregate1 = new TestAggregateRoot(id);
        var aggregate2 = new TestAggregateRoot(id);

        // Act & Assert
        aggregate1.Should().Be(aggregate2);
        (aggregate1 == aggregate2).Should().BeTrue();
        aggregate1.GetHashCode().Should().Be(aggregate2.GetHashCode());
    }
}
