# Epic 8 – Comments and Moderation

Extend web applications functionality by adding comments, replies, quotations, and moderation features for courses and lessons on the ProLanguage language learning website.

## General requirements

Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)


System should support the following features:
* Add comments to courses or lessons.
* Get comments for a course or lesson.
* Delete comment.
* Ban user for selected duration.
* Reply to a comment.
* Quote a comment.

---

## Entities

### Comment
* **Id:** Guid, required, unique  
* **Name:** String, required  
* **Body:** String, required  
* **ParentCommentId:** Guid, optional  
* **CourseId:** Guid, required  

---

## Additional Requirements

### Comments hierarchy
Comments reply depth is unlimited.

---

### Comment reply
The reply should have the following format:

`[Author], Text of Reply`

Where **[Author]** is the name of the author of the parent comment.

---

### Comment quote
Quote works like reply, but automatically adds text from the parent comment.

Quote format:

`[Body], Text of Quote`

Where **[Body]** is the content of the parent comment.

---

### Deleted Comment
The message **"A comment/quote was deleted"** should be shown after comment deletion instead of the original body.

All comments that quote a deleted comment must also contain the text **"A comment/quote was deleted"** instead of the deleted comment text.

---

### Ban
Banned users cannot add comments to courses or lessons.

Ban durations:
* 1 hour
* 1 day
* 1 week
* 1 month
* permanent

Since a full user system may not yet exist, the ban can be implemented by **name**.


## Task Description

### E08 US1 – User story 1

Add comment by course id endpoint.

```{xml}
Url: /courses/{id}/comments
Type: POST

Validation: All comment fields are required

Request Example:
{
  "comment": {
    "name": "Student Name",
    "body": "This lesson was very helpful!"
  },
  "parentId": null,
  "action": null
}
```

### E08 US2 – User story 2

Get all comments by course id endpoint.

```{xml}
Url: /courses/{id}/comments
Type: GET
Response example:
{
  "id": "55d8e87f-3710-40d1-88ed-257594ebadaa",
  "name": "Student Name",
  "body": "Great explanation!",
  "childComments": [
    {
      "id": "f2e4b854-14b2-4075-b6ab-18fc29341a27",
      "name": "Teacher",
      "body": "Thank you!",
      "childComments": []
    }
  ]
},
{
  "id": "bfcbf906-0cc7-4321-8854-a0c88baa9e07",
  "name": "Another Student",
  "body": "Very useful course",
  "childComments": []
}
```

### E08 US3 – User story 3

Delete comment endpoint.

```{xml}
Url: /courses/{courseId}/comments/{id}
Type: DELETE
```

### E08 US4 – User story 4

Get ban duration options endpoint.

```{xml}
Url: /comments/ban/durations
Type: GET
Response Example:
[
  "1 hour",
  "1 day",
  "1 week",
  "1 month",
  "permanent"
]
```

### E08 US5 – User story 5

Ban user by name endpoint.

```{xml}
Url: /comments/ban
Type: POST
Request Example:
{
  "user": "StudentName",
  "duration": "1 month"
}
```

### E08 US6 – User story 6

Reply to a comment endpoint.

```{xml}
Url: /courses/{id}/comments
Type: POST
Request Example:
{
  "comment": {
    "name": "Teacher",
    "body": "[StudentName], Thank you for your feedback!"
  },
  "parentId": "comment-id",
  "action": "reply"
}
```

### E08 US7 – User story 7

Quote a comment endpoint.

```{xml}
Url: /courses/{id}/comments
Type: POST
Request Example:
{
  "comment": {
    "name": "Student",
    "body": "[Great explanation!], I agree with this comment."
  },
  "parentId": "comment-id",
  "action": "quote"
}
```

### Non-functional Requirements

### E08 NFR1
The system must support hierarchical comments with unlimited depth.

### E08 NFR2
The system must ensure that banned users cannot add comments during the ban period.

### E08 NFR3
Deleted comments must preserve comment structure and show the text "A comment/quote was deleted" instead of removed content.