# Epic 4 - Courses and UI Integration

## General Requirements

**Front-end Integration:**  
Create the Angular front-end: `prolanguage-ui-app`.

The system should support the following features:

- UI integration with API
- CRUD operations for Courses
- Get lessons by course name
- Appropriate HTTP status codes on endpoint results

**Technical Specifications:**

- Use the latest stable version of ASP.NET Core (MVC template)
- Implement N-layer architecture
- Use built-in dependency injection/service provider
- Follow SOLID principles
- Use JSON for request and response formats
- Use MS SQL Server for database

---

## Entities

### Course
- **Id**: Guid, required, unique
- **Title**: String, required
- **Description**: String, optional
- **Price**: Double, required
- **NumberOfLessons**: int

### Lesson
- **Id**: Guid, required, unique
- **CourseId**: Guid, required
- **Title**: String, required
- **Description**: string, required  
- **ScheduledDateTime**: DateTime, required
- **DurationMinutes**: int, required
- **Type**: LessonType, required  
- **Status**: Enum (Scheduled, InProgress, Completed, Cancelled)
- **MeetingLink**: string, optional 
- **Materials**: string, optional 
- **CreatedAt**: DateTime, required

---

  ## Additional Requirements

- All endpoints must validate input models.
- If validation passes, the system processes the request and returns a success status code.
- If validation fails, the system returns an appropriate error status code with validation errors.

---


### E04 US1 - User story 1
- Create and update endpoints must contain server-side validation.
- All fields are mandatory and should be populated with correct data.

```{xml}
"course": {
    "title": "English Basics",
    "description": "Introduction to English language",
    "teacherId": "abc123",
    "price": 100,
    "durationMinutes": 60,
}
```

### E04 US2 - User story 2 

Return courses with all relevant fields.

Get courses by course name endpoint.

Response Example:
```{xml}
{
  "id": "77c06fc6-f8a1-46f9-af03-2793e500112c",
  "title": "English Basics",
  "description": "Introduction to English language",
  "teacherId": "abc123",
  "price": 100,
  "durationMinutes": 60,
}
```

### E04 US3 - CRUD Lessons

CRUD endpoints for lessons linked to courses.

Validate CourseId and TeacherId references.

### E04 US4 - Update Course

All fields must be validated.

Title must be unique.

Request Example:

```{xml}
{
  "course": {
    "id": "77c06fc6-f8a1-46f9-af03-2793e500112c",
    "title": "English Basics - Updated",
    "description": "Updated description",
    "teacherId": "abc123",
    "price": 120,
    "durationMinutes": 75,
    "startDate": "2025-10-02T09:00:00"
  }
}
```

### E04 US5 - Delete Course

Endpoint: /courses/{id}

Type: DELETE

Returns appropriate success or error status code.

### E04 US6 - UI Integration

Configure prolanguage-ui-app to use the developed backend API.

Ensure all UI functionalities work without errors.

### Non-Functional Requirements

- Follow proper HTTP status code conventions for success and errors.
