import { LessonStatus } from "./enums/lesson-status.enum";
import { LessonType } from "./enums/lesson-type.enum";

export interface Lessons {
  id: string;
  title: string;
  description: string;
  durationMinutes: number;
  type: LessonType;
  status: LessonStatus;
  meetingLink?: string;
  materialsLink?: string;
  courseId?: string;
  courseTitle?: string;
  createdAt: Date;
}
