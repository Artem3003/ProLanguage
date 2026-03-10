import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Lessons } from '../models/lessons.model';
import { LessonService } from '../services/lesson.service';
import { CourseService } from '../services/course.service';
import { Course } from '../models/course.model';
import { LessonType } from '../models/enums/lesson-type.enum';
import { LessonStatus } from '../models/enums/lesson-status.enum';

@Component({
  selector: 'app-lessons-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './lessons-list.html',
  styleUrl: './lessons-list.scss'
})
export class LessonsList implements OnInit {
  title: string = 'Lessons';
  lessons: Lessons[] = [];
  courses: Course[] = [];
  loading: boolean = false;
  error: string = '';
  showForm: boolean = false;
  editingLesson: Lessons | null = null;

  // Form model
  lessonForm: Lessons = this.getEmptyLesson();

  // Enums for dropdowns - create arrays of {name, value} pairs
  lessonTypes = Object.keys(LessonType)
    .filter(key => !isNaN(Number(key)))
    .map(key => ({ value: Number(key), name: LessonType[Number(key)] }));

  lessonStatuses = Object.keys(LessonStatus)
    .filter(key => !isNaN(Number(key)))
    .map(key => ({ value: Number(key), name: LessonStatus[Number(key)] }));

  constructor(
    private lessonService: LessonService,
    private courseService: CourseService
  ) { }

  ngOnInit(): void {
    this.loadLessons();
    this.loadCourses();
  }

  loadCourses(): void {
    this.courseService.getAvailableCourses().subscribe({
      next: (data: Course[]) => {
        this.courses = data;
      },
      error: (err) => {
        console.error('Error fetching available courses', err);
      }
    });
  }

  loadLessons(): void {
    this.loading = true;
    this.error = '';
    this.lessonService.getLessons().subscribe({
      next: (data: Lessons[]) => {
        this.lessons = data || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching lessons', err);
        // If it's a 404 or the response is empty, treat it as no lessons found
        if (err.status === 404 || err.status === 204) {
          this.lessons = [];
        } else {
          this.error = 'Failed to load lessons';
        }
        this.loading = false;
      }
    });
  }

  getEmptyLesson(): Lessons {
    return {
      id: '',
      title: '',
      description: '',
      durationMinutes: 60,
      type: LessonType.Individual,
      status: LessonStatus.Scheduled,
      meetingLink: '',
      materialsLink: '',
      createdAt: new Date()
    };
  }

  openCreateForm(): void {
    this.editingLesson = null;
    this.lessonForm = this.getEmptyLesson();
    this.loadCourses(); // Reload available courses for creating
    this.showForm = true;
  }

  openEditForm(lesson: Lessons): void {
    this.editingLesson = lesson;
    this.lessonForm = { ...lesson };
    this.loadCoursesForEdit(lesson.id); // Load courses excluding current lesson
    this.showForm = true;
  }

  loadCoursesForEdit(lessonId: string): void {
    // When editing, exclude the current lesson from the capacity check
    this.courseService.getAvailableCourses(lessonId).subscribe({
      next: (data: Course[]) => {
        this.courses = data;
      },
      error: (err) => {
        console.error('Error fetching available courses for edit', err);
      }
    });
  }  closeForm(): void {
    this.showForm = false;
    this.editingLesson = null;
    this.lessonForm = this.getEmptyLesson();
  }

  saveLesson(): void {
    if (this.editingLesson) {
      // Update existing lesson
      this.lessonService.updateLesson(this.lessonForm).subscribe({
        next: () => {
          this.loadLessons();
          this.closeForm();
        },
        error: (err) => {
          console.error('Error updating lesson', err);
          this.error = 'Failed to update lesson';
        }
      });
    } else {
      // Create new lesson
      this.lessonService.createLesson(this.lessonForm).subscribe({
        next: () => {
          this.loadLessons();
          this.closeForm();
        },
        error: (err) => {
          console.error('Error creating lesson', err);
          this.error = 'Failed to create lesson';
        }
      });
    }
  }

  deleteLesson(id: string): void {
    if (confirm('Are you sure you want to delete this lesson?')) {
      this.lessonService.deleteLesson(id).subscribe({
        next: () => {
          this.loadLessons();
        },
        error: (err: any) => {
          console.error('Error deleting lesson', err);
          this.error = 'Failed to delete lesson';
        }
      });
    }
  }

  // Helper methods to get enum text
  getLessonTypeText(type: LessonType): string {
    return LessonType[type];
  }

  getLessonStatusText(status: LessonStatus): string {
    return LessonStatus[status];
  }
}
