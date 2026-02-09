
# English School Platform

## Description

The **EnglishSchoolPlatform** is a web-based system designed to facilitate the management and operation of an online English language school. It supports features for students, teachers, administrators, and general users. The platform allows for efficient scheduling, course management, communication, and monitoring of student progress. It also includes a secure authentication system, multi-role authorization, and UI designed for both learners and educators.

## Application Features

As a result of completing this task, you will receive an application that includes the following functionality:

- **User Roles and Management**: Admins, teachers, and students each have role-specific access and functionality.
- **User Authentication and Authorization**: Users can register, log in, and manage their personal accounts with role-based permissions.
- **Course Management**: Admins and teachers can create, edit, and remove courses; students can enroll and view course materials.
- **Schedule and Timetable**: Teachers and students can manage and view lesson schedules in real-time.
- **Student Progress Tracking**: Teachers and admins can monitor student attendance, homework completion, and test performance.
- **Unit Testing**: The platform includes unit tests to ensure backend reliability and correctness.
- **API Documentation**: Swagger is integrated to document all backend endpoints clearly.
- **Logging and Error Handling**: Comprehensive logging and error management systems are implemented.
- **Multi-language Support**: The platform supports multiple languages via localization features.
- **Notifications**: Students and teachers receive timely notifications regarding lessons, assignments, and messages.
- **UI Design Implementation**: A user-friendly UI aligned with provided Figma/mock-up designs.
- **Chat and Communication**: Real-time or asynchronous messaging between students and teachers.
- **Image and File Uploads**: Teachers can upload teaching materials, and students can submit assignments.
- **NoSQL Integration**: The application utilizes both relational and NoSQL databases where appropriate.
- **Cloud Readiness**: The platform is compatible with cloud services such as Microsoft Azure and AWS.

## .NET Platform

This project is built with the .NET 8 SDK and supports development using Visual Studio 2022 or Visual Studio Code with the .NET Core CLI.

## Branching

The application is divided into multiple epics. Each epic corresponds to a feature or major module and is implemented in a separate branch with its own `epic-##.md` description file.

| Epic | Epic Title | Epic Description | Feature Branch Name              |
|------|-----------|------------------|----------------------------------|
| 1.   | Admin Panel with Services | Implement an admin panel for managing the platform. | [english-school-epic-01](epic-01.md) |
| 2.   | Headers and Unit Testing | Implement proper headers and unit tests for API endpoints. | [english-school-epic-02](epic-02.md) |
| 3.   | Logging and Swagger | Add structured logging and API documentation with Swagger. | [english-school-epic-03](epic-03.md) |
| 4.   | Enhancements and UI Integration | Improve UI elements and overall user experience. | [english-school-epic-04](epic-04.md) |
| 5.   | Payment Methods | Extend functionality by adding payment gateway support. | [english-school-epic-05](epic-05.md) |
| 6.   | Authorization | Implement role-based access control for different user types. | [english-school-epic-06](epic-06.md) |
| 7.   | Filters | Implement course filtering and pagination features. | [english-school-epic-07](epic-07.md) |
| 8.   | NoSQL Database | Integrate a NoSQL database alongside the existing relational database. | [english-school-epic-08](epic-08.md) |
| 9.   | Comments and Moderation | Add user comments and content moderation features. | [english-school-epic-09](epic-09.md) |
| 10.  | Course Picture [Microsoft Azure] | Enable course image uploads and display them on course details pages. | [english-school-epic-10](epic-10.md) |
| 11.  | Big Data [Microsoft Azure] | Optimize the system for handling large-scale course data. | [english-school-epic-11](epic-11.md) |
| 12.  | Notifications [Microsoft Azure] | Implement user notifications for important events. | [english-school-epic-12](epic-12.md) |
| 13.  | UI Design [Angular] | Apply UI designs based on given mock-ups. | [english-school-epic-13](epic-13.md) |
| 14.  | Localization [Angular]| Support multiple languages for the application. | [english-school-epic-14](epic-14.md) |
