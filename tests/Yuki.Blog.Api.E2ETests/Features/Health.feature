Feature: Health Check API
  As a system administrator
  I want to check the health status of the API
  So that I can monitor the system's availability and performance

  Background:
    Given the API version is "1.0"

  @smoke @critical
  Scenario: Health check returns healthy status
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the health status should be "Healthy"
    And the total duration should be greater than or equal to 0
    And the response should contain health checks

  Scenario: Health check includes database connectivity
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the database health check should be present
    And the database status should be "Healthy"
    And the database check duration should be greater than or equal to 0

  Scenario: Health check returns valid structure
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the health status should be one of "Healthy, Degraded, Unhealthy"
    And all health checks should have valid status values
    And all health checks should have non-negative durations

  Scenario: Health check responds quickly
    When I time a GET request to "/api/health"
    Then the response status code should be 200
    And the response time should be less than 5 seconds

  Scenario: Multiple health check requests succeed
    When I send 5 concurrent GET requests to "/api/health"
    Then all responses should have status code 200
    And all responses should have "Healthy" status

  Scenario: Health check works without API version header
    Given I remove the API version header
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the health status should be "Healthy"

  Scenario: Health check returns valid JSON structure
    When I send a GET request to "/api/health"
    Then the response status code should be 200
    And the response content type should be "application/json"
    And all health checks should have non-empty names
    And all health checks should have non-empty status values
