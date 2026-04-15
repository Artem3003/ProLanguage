# ProLanguage Platform - Frontend Testing Application

This is a simple Angular application for testing all backend API endpoints of the ProLanguage Platform.

## Features

- **Courses Management**: View all available courses
- **Lessons Management**: View scheduled lessons with details
- **Homeworks Management**: View homework assignments and instructions
- **Homework Assignments**: Track student homework submissions and grades
- **Calendar Events**: View all calendar events with types and dates

## Running the Application

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Start the development server**:
   ```bash
   npm start
   ```

3. **Open in browser**:
   Navigate to `http://localhost:4200`

## Backend API Configuration

The frontend is configured to connect to the backend API at:
- **Base URL**: `http://localhost:5000/api` (for Lessons, Homeworks, Assignments, Calendar)
- **Courses URL**: `http://localhost:5201/courses`

### API Endpoints Tested

#### Courses
- `GET /courses` - Get all courses
- `GET /courses/{id}` - Get course by ID
- `POST /courses` - Create new course
- `PUT /courses/{id}` - Update course
- `DELETE /courses/{id}` - Delete course

#### Lessons
- `GET /api/lessons` - Get all lessons
- `GET /api/lessons/{id}` - Get lesson by ID
- `POST /api/lessons` - Create new lesson
- `PUT /api/lessons/{id}` - Update lesson
- `DELETE /api/lessons/{id}` - Delete lesson

#### Homeworks
- `GET /api/homeworks` - Get all homeworks
- `GET /api/homeworks/{id}` - Get homework by ID
- `POST /api/homeworks` - Create new homework
- `PUT /api/homeworks/{id}` - Update homework
- `DELETE /api/homeworks/{id}` - Delete homework

#### Homework Assignments
- `GET /api/homework-assignments` - Get all homework assignments
- `GET /api/homework-assignments/{id}` - Get assignment by ID
- `POST /api/homework-assignments` - Create new assignment
- `PUT /api/homework-assignments/{id}` - Update assignment
- `DELETE /api/homework-assignments/{id}` - Delete assignment
- `POST /api/homework-assignments/{id}/submit` - Submit homework
- `POST /api/homework-assignments/{id}/grade` - Grade homework

#### Calendar Events
- `GET /api/calendar-events` - Get all calendar events
- `GET /api/calendar-events/{id}` - Get event by ID
- `POST /api/calendar-events` - Create new event
- `PUT /api/calendar-events/{id}` - Update event
- `DELETE /api/calendar-events/{id}` - Delete event

## Project Structure

```
src/app/
├── models/           # TypeScript interfaces for data models
│   ├── enums/       # Enum definitions (LessonStatus, LessonType, etc.)
│   ├── course.model.ts
│   ├── lessons.model.ts
│   ├── homework.model.ts
│   ├── homework-assignment.model.ts
│   └── calendar-event.model.ts
├── services/        # HTTP services for API communication
│   ├── course.service.ts
│   ├── lesson.service.ts
│   ├── homework.service.ts
│   ├── homework-assignment.service.ts
│   └── calendar-event.service.ts
├── courses-list/    # Course listing component
├── lessons-list/    # Lessons listing component
├── homeworks-list/  # Homeworks listing component
├── assignments-list/# Assignments listing component
└── calendar-list/   # Calendar events listing component
```

## Models & Enums

### Enums
- `AssignmentStatus`: Assigned, InProgress, Submitted, Graded, Late
- `AttendanceStatus`: Present, Absent, Late
- `EventType`: Lesson, Assignment, Exam, Holiday, Personal
- `LessonStatus`: Scheduled, InProgress, Completed, Cancelled
- `LessonType`: Individual, Group, Workshop
- `UserRole`: Student, Teacher, Admin

### Models
All models match the backend C# entities with proper TypeScript typing and nullable fields.

## Testing

To test the endpoints:

1. Ensure your backend is running on the configured ports
2. Navigate through the different sections using the sidebar menu
3. Check the browser console for any errors
4. Verify data is loading correctly from the API

## Note

This is a testing/development application. In a production environment, you should add:
- Proper error handling and user notifications
- Loading indicators
- Form validation
- Authentication and authorization
- CRUD operations UI (currently only GET operations are displayed)
