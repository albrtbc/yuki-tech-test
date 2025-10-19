Feature: Blog Posts Management
  As a blog user
  I want to create and retrieve blog posts
  So that I can share and access my content

  Background:
    Given the API version is "1.0"

  # Create Post Scenarios
  @smoke @critical
  Scenario: Create a valid blog post
    Given I am an author
    And I have a post with the following details:
      | Field       | Value                                                   |
      | Title       | Test Blog Post                                          |
      | Description | This is a test description for the blog post            |
      | Content     | This is the full content of the test blog post.         |
    When I send a POST request to "/api/posts"
    Then the response status code should be 201
    And the response should contain a post ID
    And the response should contain the post details
    And the response should have a Location header with the post URL

  @error-handling
  Scenario: Create a post with non-existent author
    Given I have a non-existent author ID
    And I have a post with the following details:
      | Field       | Value                        |
      | Title       | Test Blog Post               |
      | Description | This is a test description   |
      | Content     | This is the test content     |
    When I send a POST request to "/api/posts"
    Then the response status code should be 404

  @validation
  Scenario: Create a post with empty title
    Given I am an author
    And I have a post with the following details:
      | Field       | Value                        |
      | Title       |                              |
      | Description | This is a test description   |
      | Content     | This is the test content     |
    When I send a POST request to "/api/posts"
    Then the response status code should be 400

  @boundary
  Scenario Outline: Create a post with invalid field length
    Given I am an author
    And I have a post with a <field> of <length> characters
    When I send a POST request to "/api/posts"
    Then the response status code should be 400

    Examples:
      | field       | length |
      | title       | 201    |
      | description | 501    |
      | content     | 50001  |

  Scenario: Create a post with empty content
    Given I am an author
    And I have a post with the following details:
      | Field       | Value                        |
      | Title       | Test Blog Post               |
      | Description | This is a test description   |
      | Content     |                              |
    When I send a POST request to "/api/posts"
    Then the response status code should be 400

  @boundary @positive
  Scenario: Create a post with maximum allowed title length
    Given I am an author
    And I have a post with a title of 200 characters
    And the post has valid description and content
    When I send a POST request to "/api/posts"
    Then the response status code should be 201
    And the post title should have 200 characters

  @encoding @unicode
  Scenario: Create a post with special characters
    Given I am an author
    And I have a post with the following details:
      | Field       | Value                                                   |
      | Title       | Test with special chars: émojis 🚀 & symbols <>&"'      |
      | Description | Description with UTF-8: café, résumé, naïve, Zürich     |
      | Content     | Content with newlines\nand\ttabs\rand special symbols: €£¥©®™ |
    When I send a POST request to "/api/posts"
    Then the response status code should be 201
    And the response should contain special characters

  Scenario: Create a post using XML format
    Given I am an author
    And I have a post request in XML format with title "XML Test Post"
    When I send the XML request to "/api/posts"
    Then the response status code should be 201

  @performance @rate-limiting
  Scenario: Rate limiting on post creation
    Given I am an author
    And I have a valid post request
    When I send 12 rapid POST requests to "/api/posts"
    Then at least some requests should succeed
    And all requests should complete

  # Retrieve Post Scenarios
  @smoke @critical
  Scenario: Retrieve an existing post
    Given I am an author
    And I have created a post with title "Integration Test Post"
    When I send a GET request to "/api/posts/{postId}"
    Then the response status code should be 200
    And the response should contain the post details
    And the author information should not be included by default

  Scenario: Retrieve a post with author information
    Given I am an author
    And I have created a post with title "Integration Test Post"
    When I send a GET request to "/api/posts/{postId}?include=author"
    Then the response status code should be 200
    And the response should contain the post details
    And the author information should be included
    And the author should have a name and surname

  Scenario: Retrieve a non-existent post
    Given I have a non-existent post ID
    When I send a GET request to "/api/posts/{postId}"
    Then the response status code should be 404

  Scenario: Retrieve a post with invalid GUID
    When I send a GET request to "/api/posts/invalid-guid"
    Then the response status code should be 404

  Scenario: Retrieve a post with empty GUID
    When I send a GET request to "/api/posts/00000000-0000-0000-0000-000000000000"
    Then the response status code should be 404

  Scenario: Retrieve a post with XML response format
    Given I am an author
    And I have created a post with title "Integration Test Post"
    When I send a GET request with Accept header "application/xml" to "/api/posts/{postId}"
    Then the response status code should be 200
    And the response content type should be "application/xml"

  Scenario: Response caching for post retrieval
    Given I am an author
    And I have created a post with title "Integration Test Post"
    When I send two consecutive GET requests to "/api/posts/{postId}"
    Then both responses should have status code 200
    And both responses should have cache control headers

  Scenario: Retrieve post with multiple include parameters
    Given I am an author
    And I have created a post with title "Integration Test Post"
    When I send a GET request to "/api/posts/{postId}?include=author,tags,comments"
    Then the response status code should be 200
    And the author information should be included

  Scenario: Retrieve post with case insensitive include parameter
    Given I am an author
    And I have created a post with title "Integration Test Post"
    When I send a GET request to "/api/posts/{postId}?include=AUTHOR"
    Then the response status code should be 200
    And the author information should be included
