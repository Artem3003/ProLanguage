import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HomeworkAssignment } from '../models/homework-assignment.model';
import { HomeworkAssignmentService } from '../services/homework-assignment.service';
import { AssignmentStatus } from '../models/enums/assignment-status.enum';
import { Homework } from '../models/homework.model';
import { HomeworkService } from '../services/homework.service';

@Component({
  selector: 'app-assignments-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './assignments-list.html',
  styleUrl: './assignments-list.scss'
})
export class AssignmentsList implements OnInit {
  title: string = 'Homework Assignments';
  assignments: HomeworkAssignment[] = [];
  homeworks: Homework[] = [];
  loading: boolean = false;
  error: string = '';
  showForm: boolean = false;
  showSubmitForm: boolean = false;
  showGradeForm: boolean = false;
  editingAssignment: HomeworkAssignment | null = null;

  // Form models
  assignmentForm: HomeworkAssignment = this.getEmptyAssignment();
  submitForm = { submissionText: '', attachmentUrl: '' };
  gradeForm = { grade: 0, teacherFeedback: '' };

  // Enums for dropdowns - create arrays of {name, value} pairs
  assignmentStatuses = Object.keys(AssignmentStatus)
    .filter(key => !isNaN(Number(key)))
    .map(key => ({ value: Number(key), name: AssignmentStatus[Number(key)] }));

  constructor(
    private assignmentService: HomeworkAssignmentService,
    private homeworkService: HomeworkService
  ) { }

  ngOnInit(): void {
    this.loadAssignments();
    this.loadHomeworks();
  }

  loadHomeworks(): void {
    this.homeworkService.getHomeworks().subscribe({
      next: (data: Homework[]) => {
        this.homeworks = data || [];
      },
      error: (err) => {
        console.error('Error fetching Assignments', err);
        // If it's a 404 or the response is empty, treat it as no assigns found
        if (err.status === 404 || err.status === 204) {
          this.assignments = [];
        } else {
          this.error = 'Failed to load assignments';
        }
        this.loading = false;
      }
    });
  }

  loadAssignments(): void {
    this.loading = true;
    this.error = '';
    this.assignmentService.getHomeworkAssignments().subscribe({
      next: (data: HomeworkAssignment[]) => {
        this.assignments = data || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching assignments', err);
        // If it's a 404 or the response is empty, treat it as no assignments found
        if (err.status === 404 || err.status === 204) {
          this.assignments = [];
        } else {
          this.error = 'Failed to load assignments';
        }
        this.loading = false;
      }
    });
  }

  getEmptyAssignment(): HomeworkAssignment {
    return {
      id: '',
      homeworkId: '',
      status: AssignmentStatus.Assigned,
      submissionText: '',
      attachmentUrl: '',
      submittedAt: undefined,
      grade: undefined,
      teacherFeedback: '',
      gradedAt: undefined
    };
  }

  openCreateForm(): void {
    this.editingAssignment = null;
    this.assignmentForm = this.getEmptyAssignment();
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.showSubmitForm = false;
    this.showGradeForm = false;
    this.editingAssignment = null;
    this.assignmentForm = this.getEmptyAssignment();
  }

  saveAssignment(): void {
    this.assignmentService.createHomeworkAssignment(this.assignmentForm).subscribe({
      next: () => {
        this.loadAssignments();
        this.closeForm();
      },
      error: (err) => {
        console.error('Error creating assignment', err);
        this.error = 'Failed to create assignment';
      }
    });
  }

  openSubmitForm(assignment: HomeworkAssignment): void {
    this.editingAssignment = assignment;
    this.submitForm = {
      submissionText: assignment.submissionText || '',
      attachmentUrl: assignment.attachmentUrl || ''
    };
    this.showSubmitForm = true;
  }

  submitAssignment(): void {
    if (this.editingAssignment) {
      this.assignmentService.submitHomeworkAssignment(this.editingAssignment.id!, this.submitForm).subscribe({
        next: () => {
          this.loadAssignments();
          this.closeForm();
        },
        error: (err) => {
          console.error('Error submitting assignment', err);
          this.error = 'Failed to submit assignment';
        }
      });
    }
  }

  openGradeForm(assignment: HomeworkAssignment): void {
    this.editingAssignment = assignment;
    this.gradeForm = {
      grade: assignment.grade || 0,
      teacherFeedback: assignment.teacherFeedback || ''
    };
    this.showGradeForm = true;
  }

  gradeAssignment(): void {
    if (this.editingAssignment) {
      this.assignmentService.gradeHomeworkAssignment(this.editingAssignment.id!, this.gradeForm).subscribe({
        next: () => {
          this.loadAssignments();
          this.closeForm();
        },
        error: (err) => {
          console.error('Error grading assignment', err);
          this.error = 'Failed to grade assignment';
        }
      });
    }
  }

  deleteAssignment(id: string): void {
    if (confirm('Are you sure you want to delete this assignment?')) {
      this.assignmentService.deleteHomeworkAssignment(id).subscribe({
        next: () => {
          this.loadAssignments();
        },
        error: (err) => {
          console.error('Error deleting assignment', err);
          this.error = 'Failed to delete assignment';
        }
      });
    }
  }

  // Helper method to get enum text
  getAssignmentStatusText(status: AssignmentStatus): string {
    return AssignmentStatus[status];
  }

  // Helper method to get homework title by ID
  getHomeworkTitle(homeworkId: string): string {
    const homework = this.homeworks.find(h => h.id === homeworkId);
    return homework ? homework.title : homeworkId;
  }
}
