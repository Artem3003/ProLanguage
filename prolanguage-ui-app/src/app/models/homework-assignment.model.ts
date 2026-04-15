import { AssignmentStatus } from './enums/assignment-status.enum';
import { Homework } from './homework.model';

export interface HomeworkAssignment {
  id?: string;
  homeworkId: string;
  homework?: Homework;
  submissionText?: string;
  attachmentUrl?: string;
  submittedAt?: Date;
  status: AssignmentStatus;
  grade?: number;
  teacherFeedback?: string;
  gradedAt?: Date;
}
