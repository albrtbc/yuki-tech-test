Feature: Correlation ID Distributed Tracing
  As a system administrator
  I want every request to have a correlation ID based on OpenTelemetry TraceId
  So that I can trace requests through the system using a single identifier across logs and traces

  Background:
    Given the API version is "1.0"

  @smoke @critical
  Scenario: Correlation ID is automatically generated from OpenTelemetry TraceId
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response should contain X-Correlation-ID header
    And the correlation ID should be a valid GUID

  @error-handling
  Scenario: Correlation ID is included in error responses
    When I send a GET request to "/api/posts/00000000-0000-0000-0000-000000000000"
    Then the response status code should be 404
    And the response should contain X-Correlation-ID header

  Scenario: Different requests generate different correlation IDs
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response should contain X-Correlation-ID header
    And I save the correlation ID as "first"
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response should contain X-Correlation-ID header
    And the correlation ID should be different from "first"

  Scenario: Auto-generated correlation ID matches OpenTelemetry TraceId format
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response should contain X-Correlation-ID header
    And the correlation ID should be a valid GUID
    And the correlation ID should match OpenTelemetry TraceId format

  @distributed-tracing
  Scenario: Correlation ID from caller is preserved across service boundaries
    Given I set X-Correlation-ID header to "abc123def456789012345678901234ab"
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response should contain X-Correlation-ID header
    And the response X-Correlation-ID header should be "abc123def456789012345678901234ab"

  @distributed-tracing @w3c
  Scenario: W3C traceparent header is extracted for correlation ID
    Given I set traceparent header to "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01"
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response should contain X-Correlation-ID header
    And the response X-Correlation-ID header should be "4bf92f3577b34da6a3ce929d0e0e4736"
