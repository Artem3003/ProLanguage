# Epic 9 – Authorization (ProLanguage)

## General requirements

The **ProLanguage** platform is a web-based application for learning foreign languages.  
This epic describes the authorization and access control functionality for the platform.

The system uses:
- Angular Front-end for ProLanguage
- Existing Authorization microservice (Auth)

The system must support the following features:
- User management
- Login and authentication
- Role management
- Access control based on permissions

---

## Additional Requirements

### Roles

The system should have the following default roles:
- **Administrator**
- **Content Manager**
- **Teacher**
- **Student**
- **Guest** (not authorized)

Custom roles can be created if needed.  
Each user can have one or multiple roles.

---

### Role Descriptions

#### Administrator
- Manages users and roles
- Has full access to all system settings
- Can view system statistics and reports
- Can manage courses, payments, and platform configuration

#### Content Manager
- Manages language courses and lessons
- Publishes, edits, and archives learning content
- Cannot manage users or system settings

#### Teacher
- Creates and manages own courses and lessons
- Reviews assignments and provides feedback
- Communicates with students
- Cannot edit other teachers’ courses

#### Student
- Can access purchased or assigned courses
- Can complete lessons, tests, and assignments
- Can communicate with teachers and AI Tutor
- Cannot manage content or users

#### Guest
- Has read-only access
- Can view public information and course previews
- Cannot access learning materials or payments

---

### Default Roles Hierarchy

Higher roles inherit permissions from lower roles unless explicitly restricted.

```{xml}
Administrator
↓
Content Manager
↓
Teacher
↓
Student
↓
Guest
```

## Task Description

### E06 US1 – User Story 1  
Login endpoint.

```{xml}
Url: /users/login  
Type: POST  
Request Example:
{
  "model": {
    "login": "user@email.com",
    "password": "SecurePassword",
    "internalAuth": true
  }
```

Response Example:
```{xml}
{
  "token": "jwt_token_here"
}
```

E06 US2 – User Story 2

Check access to a platform resource.

```{xml}
Url: /users/access  
Type: POST  
Request Example:
{
  "targetPage": "Course",
  "targetId": "a1b2c3d4-1234-5678-9999-abcdef"
}
```

### E06 US3 – User Story 3

Get all users.

```{xml}
Url: /users  
Type: GET
```

Response Example:

```{xml}
[
  {
    "name": "Artem",
    "id": "454d4d01-406b-4a9b-9f8c-3fec63fc9266"
  }
]
```

## E06 US4 – User Story 4

Get user by ID.

```{xml}
Url: /users/{id}  
Type: GET
```

### E06 US5 – User Story 5

Delete user by ID.

```{xml}
Url: /users/{id}  
Type: DELETE
```

### E06 US6 – User Story 6

Get all roles.

```{xml}
Url: /roles  
Type: GET
```

### E06 US7 – User Story 7

Get role by ID.

```{xml}
Url: /roles/{id}  
Type: GET
```

### E06 US8 – User Story 8

Delete role by ID.

```{xml}
Url: /roles/{id}  
Type: DELETE
```

### E06 US9 – User Story 9

Add new user.

```{xml}
Url: /users  
Type: POST  
Request Example:
{
  "user": {
    "name": "New Student"
  },
  "roles": [
    "student-role-id"
  ],
  "password": "password123"
}
```

### E06 US10 – User Story 10

Update user.

```{xml}
Url: /users  
Type: PUT
```

### E06 US11 – User Story 11

Get user roles.

```{xml}
Url: /users/{id}/roles  
Type: GET
```

### E06 US12 – User Story 12

Get all permissions.

```{xml}
Url: /roles/permissions  
Type: GET
```

### E06 US13 – User Story 13

Get role permissions.

```{xml}
Url: /roles/{id}/permissions  
Type: GET
```

### E06 US14 – User Story 14

Add role.

```{xml}
Url: /roles  
Type: POST
```

### E06 US15 – User Story 15

Update role.

```{xml}
Url: /roles  
Type: PUT
```

### Non-functional Requirements

### E09 NFR1
The system must support claim-based authorization.

### E09 NFR2 (Optional)
Support authentication through an external authorization microservice.
