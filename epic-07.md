# Epic 7 - Course Catalog Filters

Extend the functionality of the ProLanguage website by adding course filtering, pagination, and sorting features.

## General requirements

Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)

System should support the following features:
* Course catalog filtration
* Course catalog pagination
* Course catalog sorting
* Course search functionality

---

## Domain Model Updates

### New Enums

#### CourseLanguage
- Deutsch
- English
- Polski
- Italiano

#### CourseLevel
- Beginner
- Elementary
- Intermediate
- UpperIntermediate
- Advanced
- Proficiency

---

### Updated Course Entity

The Course entity should be extended with the following properties:

| Property | Type | Description |
|----------|------|-------------|
| Language | CourseLanguage | Course teaching language |
| Level | CourseLevel | Difficulty level |
| DurationMinutes | int | Total course duration in minutes |
| Rating | double? | Average course rating (0-5) |
| IsOnSale | bool | Whether the course is currently on sale |
| DiscountPrice | double? | Discounted price (when IsOnSale is true) |
| IsNew | bool | Whether the course is marked as new |
| ViewCount | int | Number of times the course detail page was viewed |
| CreatedAt | DateTime | Course creation timestamp |

---

## Additional Requirements

### Filtration

Filtration by the following options is possible:

- **New**: toggle filter (show only courses marked as new)
- **Language**: multiple select (Deutsch, English, Polski, Italiano)
- **Price Range**: min and max inputs, if one of the values is not populated, should be treated as unlimited
- **Level**: multiple select (Beginner, Elementary, Intermediate, UpperIntermediate, Advanced, Proficiency)
- **On Sale**: toggle filter (show only courses currently on sale)
- **Rating**: single select (minimum rating threshold)
- **Duration Range**: min and max inputs (in minutes), if one of the values is not populated, should be treated as unlimited
- **Title**: text input (search catalog)

---

### Title Filtration

Filtering by any part of a title but at least 3 characters are required to start filtration by Course Title.

---

### Filter Combinations

- If multiple values of the same filter are selected (for example two languages are selected) these should be treated as **OR** statements (return courses which belong to language 1 OR language 2).
- If multiple filters are selected (for example some languages and some levels are selected) these should be treated as **AND** statements (return courses that belong to at least one from selected languages AND at least one from selected levels).

---

### Sorting

Sorting by the following options is possible:

- **Most popular**: most viewed courses are on the top
- **Newest**: most recently created courses are on the top
- **Price ASC**: cheapest courses are on the top
- **Price DESC**: most expensive courses are on the top
- **Duration ASC**: shortest courses are on the top
- **Duration DESC**: longest courses are on the top
- **Rating DESC**: highest rated courses are on the top
- **Title A-Z**: courses sorted alphabetically by title
- **Title Z-A**: courses sorted reverse alphabetically by title

---

### Filter Bar UI Layout

The filter bar should contain the following elements in order:
1. **Search catalog** - text input for title search
2. **New** - toggle button (checkmark when active)
3. **Language** - dropdown with multiple select
4. **Price** - dropdown with min/max range inputs
5. **Level** - dropdown with multiple select (checkmark when active)
6. **On sale** - toggle button
7. **Rating** - dropdown with single select options
8. **Duration** - dropdown with min/max range inputs
9. **Sorting by** - dropdown to select sorting option

Results count should be displayed (e.g., "46 results").

---

### Course Card Display

Each course card should display:
- **Title** - course name
- **Price** - displayed with currency symbol (₴), "Free" for zero-price courses
- **Duration** - formatted in user-friendly format (e.g., "2 hour 15 minutes")
- **Navigation arrow** - button to view course details

Course cards should be displayed in a 3-column grid layout with 3 rows per page by default (9 courses).

---

### Pagination Controls

Pagination should display:
- Current page range (e.g., "1 - 9 of 46")
- Previous/Next navigation arrows
- Page size selector

---

### Pagination

The following numbers of courses per page are available:
- 9
- 18
- 36
- 72
- all

---

## Task Description

### E07 US1 - User Story 1

Get pagination options endpoint.

```
Url: /courses/pagination-options
Type: GET
Response Example:
[
  "9",
  "18",
  "36",
  "72",
  "all"
]
```

---

### E07 US2 - User Story 2

Get sorting options endpoint.

```
Url: /courses/sorting-options
Type: GET
Response Example:
[
  "Most popular",
  "Newest",
  "Price ASC",
  "Price DESC",
  "Duration ASC",
  "Duration DESC",
  "Rating DESC",
  "Title A-Z",
  "Title Z-A"
]
```

---

### E07 US3 - User Story 3

Get course languages endpoint.

```
Url: /courses/languages
Type: GET
Response Example:
[
  "Deutsch",
  "English",
  "Polski",
  "Italiano"
]
```

---

### E07 US4 - User Story 4

Get course levels endpoint.

```
Url: /courses/levels
Type: GET
Response Example:
[
  "Beginner",
  "Elementary",
  "Intermediate",
  "UpperIntermediate",
  "Advanced",
  "Proficiency"
]
```

---

### E07 US5 - User Story 5

Get rating filter options endpoint.

```
Url: /courses/rating-options
Type: GET
Response Example:
[
  "4.5+",
  "4.0+",
  "3.5+",
  "3.0+",
  "Any"
]
```

---

### E07 US6 - User Story 6

Get courses endpoint should be updated to support filters in the query.

```
Url: /courses
Type: GET
Query Parameters:
  - isNew: bool (optional)
  - languages: array of CourseLanguage (optional)
  - levels: array of CourseLevel (optional)
  - minPrice: double (optional)
  - maxPrice: double (optional)
  - onSale: bool (optional)
  - minRating: double (optional)
  - minDuration: int (optional)
  - maxDuration: int (optional)
  - title: string (optional, min 3 characters)
  - sortBy: string (optional, default "Newest")
  - pageSize: string (optional, default "9")
  - page: int (optional, default 1)

Response Example:
{
  "courses": [
    {
      "id": "8865d113-830c-49c7-bfad-9f9d2d47f103",
      "title": "Beginner English Course",
      "description": "Introduction to English for beginners",
      "price": 0,
      "discountPrice": null,
      "isOnSale": false,
      "isNew": true,
      "durationMinutes": 135,
      "language": "English",
      "level": "Beginner",
      "rating": 4.5,
      "numberOfLessons": 10,
      "createdAt": "2026-01-15T10:00:00"
    },
    {
      "id": "0d86dd4f-265d-407b-848b-76cb82a4ce38",
      "title": "Business English Course",
      "description": "Professional English for business",
      "price": 300,
      "discountPrice": 250,
      "isOnSale": true,
      "isNew": false,
      "durationMinutes": 90,
      "language": "English",
      "level": "Intermediate",
      "rating": 4.8,
      "numberOfLessons": 15,
      "createdAt": "2026-01-10T14:00:00"
    }
  ],
  "totalPages": 5,
  "currentPage": 1,
  "totalCount": 46
}
```

---

## Non-functional Requirements

**E07 NFR1**  
Add functionality to count the number of views of the Course detail page. Increment `ViewCount` each time `/courses/{id}` is accessed.

**E07 NFR2**  
Use pipeline pattern to implement filtration/sorting/pagination logic. Create the following interfaces:
- `ICourseFilter` - base interface for all filters
- `ICourseSorter` - interface for sorting strategies
- `ICoursePaginator` - interface for pagination

**E07 NFR3**  
Ensure filter parameters are validated:
- Title filter requires minimum 3 characters
- Price and duration values must be non-negative
- Page number must be positive
- Rating must be between 0 and 5

**E07 NFR4**  
Implement caching for static filter options (pagination options, sorting options, languages, levels, rating options) with appropriate cache duration (e.g., 24 hours).

**E07 NFR5**  
Display course duration in user-friendly format (e.g., "2 hour 15 minutes" instead of "135 minutes") in the response.

**E07 NFR6**  
Compute the `IsNew` flag automatically based on `CreatedAt` - courses created within the last 30 days should be marked as new.
