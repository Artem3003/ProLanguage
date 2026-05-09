# Epic 11 - Big Data [Microsoft Azure]

## General Requirements
Please use the following Angular Front-end: [prolanguage-ui-app](prolanguage-ui-app)

System should support the following features:
* A large course and lesson dataset.
* High-performance filtering and search capabilities.
* Scalable infrastructure for handling concurrent user requests.

### Technical Specifications
#### Data Mock
The external library can be used for generating fake course and lesson data.

## Task Description

### E11 US1 - User Story 1

Add 100,000 courses and 500,000 lessons with all populated fields to the database.

### E11 US2 - User Story 2
Refactor application to achieve a maximum execution time under 10 seconds for getting courses/lessons by filters endpoint (measure only API call time without UI).

## Non-functional Requirements

**E11 NFR1**
Use asynchronous and multithreading approaches during refactoring to improve performance.

**E11 NFR2**
Implement caching strategies (in-memory and distributed caching) to reduce database load.

**E11 NFR3**
Use pagination and lazy loading techniques to optimize data retrieval.

**E11 NFR4**
Deploy the application on Microsoft Azure with auto-scaling capabilities to handle peak loads.
