import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Homework } from '../models/homework.model';
import { HomeworkService } from '../services/homework.service';

@Component({
  selector: 'app-homeworks-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './homeworks-list.html',
  styleUrl: './homeworks-list.scss'
})
export class HomeworksList implements OnInit {
  title: string = 'Homeworks';
  homeworks: Homework[] = [];
  loading: boolean = false;
  error: string = '';
  showForm: boolean = false;
  editingHomework: Homework | null = null;

  // Form model
  homeworkForm: Homework = this.getEmptyHomework();

  constructor(private homeworkService: HomeworkService) { }

  ngOnInit(): void {
    this.loadHomeworks();
  }

  loadHomeworks(): void {
    this.loading = true;
    this.error = '';
    this.homeworkService.getHomeworks().subscribe({
      next: (data: Homework[]) => {
        this.homeworks = data || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching homeworks', err);
        // If it's a 404 or the response is empty, treat it as no homeworks found
        if (err.status === 404 || err.status === 204) {
          this.homeworks = [];
        } else {
          this.error = 'Failed to load homeworks';
        }
        this.loading = false;
      }
    });
  }

  getEmptyHomework(): Homework {
    return {
      id: '',
      title: '',
      description: '',
      instructions: '',
      dueDate: new Date(),
      createdAt: new Date(),
      homeworkAssignments: []
    };
  }

  openCreateForm(): void {
    this.editingHomework = null;
    this.homeworkForm = this.getEmptyHomework();
    this.showForm = true;
  }

  openEditForm(homework: Homework): void {
    this.editingHomework = homework;
    this.homeworkForm = { ...homework };
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingHomework = null;
    this.homeworkForm = this.getEmptyHomework();
  }

  saveHomework(): void {
    if (this.editingHomework) {
      // Update existing homework
      this.homeworkService.updateHomework(this.homeworkForm).subscribe({
        next: () => {
          this.loadHomeworks();
          this.closeForm();
        },
        error: (err) => {
          console.error('Error updating homework', err);
          this.error = 'Failed to update homework';
        }
      });
    } else {
      // Create new homework
      this.homeworkService.createHomework(this.homeworkForm).subscribe({
        next: () => {
          this.loadHomeworks();
          this.closeForm();
        },
        error: (err) => {
          console.error('Error creating homework', err);
          this.error = 'Failed to create homework';
        }
      });
    }
  }

  deleteHomework(id: string): void {
    if (confirm('Are you sure you want to delete this homework?')) {
      this.homeworkService.deleteHomework(id).subscribe({
        next: () => {
          this.loadHomeworks();
        },
        error: (err) => {
          console.error('Error deleting homework', err);
          this.error = 'Failed to delete homework';
        }
      });
    }
  }
}
