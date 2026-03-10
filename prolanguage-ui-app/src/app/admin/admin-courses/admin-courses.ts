import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Course } from '../../models/course.model';
import { CourseLanguage } from '../../models/enums/course-language.enum';
import { CourseLevel } from '../../models/enums/course-level.enum';
import { CourseService } from '../../services/course.service';

@Component({
  selector: 'app-admin-courses',
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-courses.html',
  styleUrl: './admin-courses.scss'
})
export class AdminCourses implements OnInit {
  courses: Course[] = [];
  loading = false;
  error = '';
  successMessage = '';

  // Modal state
  showModal = false;
  isEditing = false;

  // Form data
  currentCourse: Partial<Course> = this.getEmptyCourse();

  // Enum options for dropdowns
  languageOptions = Object.values(CourseLanguage);
  levelOptions = Object.values(CourseLevel);

  // Image upload state
  imagePreview: string | null = null;
  isDragOver = false;
  selectedImageFile: File | null = null;

  constructor(private courseService: CourseService) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading = true;
    this.error = '';
    this.courseService.getCourses().subscribe({
      next: (courses) => {
        this.courses = courses;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load courses';
        this.loading = false;
        console.error(err);
      }
    });
  }

  getEmptyCourse(): Partial<Course> {
    return {
      id: '',
      title: '',
      description: '',
      price: 0,
      numberOfLessons: 0,
      language: CourseLanguage.English,
      level: CourseLevel.Beginner,
      durationMinutes: 0,
      rating: 0,
      isOnSale: false,
      discountPrice: undefined,
      viewCount: 0,
      imageUrl: ''
    };
  }

  openCreateModal(): void {
    this.currentCourse = this.getEmptyCourse();
    this.isEditing = false;
    this.showModal = true;
    this.clearMessages();
    this.resetImageState();
  }

  openEditModal(course: Course): void {
    this.currentCourse = { ...course };
    this.isEditing = true;
    this.showModal = true;
    this.clearMessages();
    this.imagePreview = course.imageUrl || null;
    this.selectedImageFile = null;
  }

  closeModal(): void {
    this.showModal = false;
    this.currentCourse = this.getEmptyCourse();
    this.resetImageState();
  }

  // Image upload methods
  resetImageState(): void {
    this.imagePreview = null;
    this.selectedImageFile = null;
    this.isDragOver = false;
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleImageFile(files[0]);
    }
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleImageFile(input.files[0]);
    }
  }

  handleImageFile(file: File): void {
    if (!file.type.startsWith('image/')) {
      this.error = 'Please select an image file';
      return;
    }

    this.selectedImageFile = file;
    const reader = new FileReader();
    reader.onload = (e) => {
      this.imagePreview = e.target?.result as string;
      this.currentCourse.imageUrl = this.imagePreview;
    };
    reader.readAsDataURL(file);
  }

  removeImage(event: Event): void {
    event.stopPropagation();
    this.imagePreview = null;
    this.selectedImageFile = null;
    this.currentCourse.imageUrl = '';
  }

  saveCourse(): void {
    if (!this.validateCourse()) {
      return;
    }

    this.loading = true;
    this.clearMessages();

    if (this.isEditing) {
      this.courseService.updateCourse(this.currentCourse as Course).subscribe({
        next: () => {
          this.successMessage = 'Course updated successfully';
          this.closeModal();
          this.loadCourses();
        },
        error: (err) => {
          this.error = 'Failed to update course';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      const newCourse = { ...this.currentCourse } as Course;
      delete (newCourse as any).id; // Remove id for new course
      this.courseService.addCourse(newCourse).subscribe({
        next: () => {
          this.successMessage = 'Course created successfully';
          this.closeModal();
          this.loadCourses();
        },
        error: (err) => {
          this.error = 'Failed to create course';
          this.loading = false;
          console.error(err);
        }
      });
    }
  }

  deleteCourse(course: Course): void {
    if (!confirm(`Are you sure you want to delete "${course.title}"?`)) {
      return;
    }

    this.loading = true;
    this.clearMessages();

    this.courseService.deleteCourse(course.id).subscribe({
      next: () => {
        this.successMessage = 'Course deleted successfully';
        this.loadCourses();
      },
      error: (err) => {
        this.error = 'Failed to delete course';
        this.loading = false;
        console.error(err);
      }
    });
  }

  validateCourse(): boolean {
    if (!this.currentCourse.title?.trim()) {
      this.error = 'Title is required';
      return false;
    }
    if (this.currentCourse.price === undefined || this.currentCourse.price < 0) {
      this.error = 'Price must be a positive number';
      return false;
    }
    return true;
  }

  clearMessages(): void {
    this.error = '';
    this.successMessage = '';
  }

  formatDuration(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours > 0) {
      return `${hours}h ${mins}m`;
    }
    return `${mins}m`;
  }
}
