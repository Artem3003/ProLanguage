import { HomeworkAssignment } from './homework-assignment.model';

export interface Homework {
  id?: string;
  title: string;
  description: string;
  instructions: string;
  dueDate: Date;
  createdAt: Date;
  homeworkAssignments?: HomeworkAssignment[];
}
