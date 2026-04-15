# Epic 9 - Course Picture [Microsoft Azure]
Add a picture to the details of the course.

## General requirements
Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)

System should support the following features:
* Manage course with picture.
* Display course picture.

Technical specifications:
* Use the latest stable version of ASP.NET Core (Web API/MVC template).
* Use N-layer architecture.
* Use built-in service provider.
* Follow SOLID principles.
* Use JSON as request/response format.
* Use MS SQL Server.

---

## Task Description

### E09 US1 - User story 1
Add course endpoint should be updated to receive image in base64 format.

Request example:
```{xml}
{
  "course": {
    "title": "English Basics",
    "description": "Introduction to English language",
    "price": 100,
    "numberOfLessons": 12,
    "language": "English",
    "level": "Beginner",
    "durationMinutes": 720
  },
  "image": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAhYAAA....."
}
```

### E09 US2 - User story 2
Update course endpoint should be updated to receive image in base64 format.

Request example:
```{xml}
{
  "course": {
    "id": "77c06fc6-f8a1-46f9-af03-2793e500112c",
    "title": "English Basics - Updated",
    "description": "Updated description",
    "price": 120,
    "numberOfLessons": 14,
    "language": "English",
    "level": "Elementary",
    "durationMinutes": 840
  },
  "image": "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD....."
}
```

### E09 US3 - User story 3
Update course deletion in a way to remove course pictures in the scope of this operation.

```{xml}
Url: /courses/{id}
Type: DELETE
```

### E09 US4 - User story 4
Get course image endpoint.

```{xml}
Url: /courses/{id}/image
Type: GET
Response: image file
```

### E09 US5 - User story 5
Remove course image endpoint.

```{xml}
Url: /courses/{id}/image
Type: DELETE
```

---

## Non-functional requirements

**E09 NFR1**
Images should be stored in Azure Blob Storage.

**E09 NFR2**
Implement a caching mechanism for the get course image endpoint.

**E09 NFR3**
Allowed image formats: jpg, jpeg, png, webp. Maximum size: 5 MB.
