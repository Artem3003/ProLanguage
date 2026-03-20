import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Comment, CreateCommentRequest } from '../models/comment.model';
import { Course } from '../models/course.model';
import { CommentService } from '../services/comment.service';
import { CourseService } from '../services/course.service';

@Component({
  selector: 'app-comments-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './comments-list.html',
  styleUrl: './comments-list.scss'
})
export class CommentsList implements OnInit {
  // State
  courses: Course[] = [];
  selectedCourseId: string = '';
  comments: Comment[] = [];
  loading = false;
  error = '';
  successMessage = '';

  // Comment form
  commentName = '';
  commentBody = '';

  // Reply/Quote state
  replyingTo: Comment | null = null;
  quotingComment: Comment | null = null;
  replyName = '';
  replyBody = '';

  // Ban state
  banDurations: string[] = [];
  banUserName = '';
  banDuration = '';
  showBanForm = false;

  constructor(
    private commentService: CommentService,
    private courseService: CourseService
  ) { }

  ngOnInit(): void {
    this.loadCourses();
    this.loadBanDurations();
  }

  loadCourses(): void {
    this.courseService.getCourses().subscribe({
      next: (data: Course[]) => {
        this.courses = data;
        if (this.courses.length > 0) {
          this.selectedCourseId = this.courses[0].id;
          this.loadComments();
        }
      },
      error: (err) => {
        console.error('Error loading courses', err);
        this.error = 'Failed to load courses.';
      }
    });
  }

  loadComments(): void {
    if (!this.selectedCourseId) return;

    this.loading = true;
    this.error = '';
    this.commentService.getCommentsByCourseId(this.selectedCourseId).subscribe({
      next: (data: Comment[]) => {
        this.comments = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading comments', err);
        this.error = 'Failed to load comments.';
        this.loading = false;
      }
    });
  }

  loadBanDurations(): void {
    this.commentService.getBanDurations().subscribe({
      next: (data: string[]) => {
        this.banDurations = data;
        if (this.banDurations.length > 0) {
          this.banDuration = this.banDurations[0];
        }
      },
      error: (err) => {
        console.error('Error loading ban durations', err);
      }
    });
  }

  onCourseChange(): void {
    this.loadComments();
    this.cancelReply();
  }

  addComment(): void {
    if (!this.commentName.trim() || !this.commentBody.trim()) return;

    const request: CreateCommentRequest = {
      comment: {
        name: this.commentName.trim(),
        body: this.commentBody.trim()
      },
      parentId: null,
      action: null
    };

    this.commentService.addComment(this.selectedCourseId, request).subscribe({
      next: () => {
        this.commentName = '';
        this.commentBody = '';
        this.showSuccess('Comment added successfully!');
        this.loadComments();
      },
      error: (err) => {
        console.error('Error adding comment', err);
        this.error = err.error?.message || err.error || 'Failed to add comment. You may be banned.';
      }
    });
  }

  startReply(comment: Comment): void {
    this.replyingTo = comment;
    this.quotingComment = null;
    this.replyName = '';
    this.replyBody = '';
  }

  startQuote(comment: Comment): void {
    this.quotingComment = comment;
    this.replyingTo = null;
    this.replyName = '';
    this.replyBody = '';
  }

  cancelReply(): void {
    this.replyingTo = null;
    this.quotingComment = null;
    this.replyName = '';
    this.replyBody = '';
  }

  submitReply(): void {
    if (!this.replyName.trim() || !this.replyBody.trim() || !this.replyingTo) return;

    const formattedBody = `[${this.replyingTo.name}], ${this.replyBody.trim()}`;

    const request: CreateCommentRequest = {
      comment: {
        name: this.replyName.trim(),
        body: formattedBody
      },
      parentId: this.replyingTo.id,
      action: 'reply'
    };

    this.commentService.addComment(this.selectedCourseId, request).subscribe({
      next: () => {
        this.cancelReply();
        this.showSuccess('Reply added successfully!');
        this.loadComments();
      },
      error: (err) => {
        console.error('Error adding reply', err);
        this.error = err.error?.message || err.error || 'Failed to add reply.';
      }
    });
  }

  submitQuote(): void {
    if (!this.replyName.trim() || !this.replyBody.trim() || !this.quotingComment) return;

    const formattedBody = `[${this.quotingComment.body}], ${this.replyBody.trim()}`;

    const request: CreateCommentRequest = {
      comment: {
        name: this.replyName.trim(),
        body: formattedBody
      },
      parentId: this.quotingComment.id,
      action: 'quote'
    };

    this.commentService.addComment(this.selectedCourseId, request).subscribe({
      next: () => {
        this.cancelReply();
        this.showSuccess('Quote added successfully!');
        this.loadComments();
      },
      error: (err) => {
        console.error('Error adding quote', err);
        this.error = err.error?.message || err.error || 'Failed to add quote.';
      }
    });
  }

  deleteComment(comment: Comment): void {
    if (!confirm('Are you sure you want to delete this comment?')) return;

    this.commentService.deleteComment(this.selectedCourseId, comment.id).subscribe({
      next: () => {
        this.showSuccess('Comment deleted.');
        this.loadComments();
      },
      error: (err) => {
        console.error('Error deleting comment', err);
        this.error = 'Failed to delete comment.';
      }
    });
  }

  toggleBanForm(): void {
    this.showBanForm = !this.showBanForm;
  }

  banUser(): void {
    if (!this.banUserName.trim() || !this.banDuration) return;

    this.commentService.banUser({
      user: this.banUserName.trim(),
      duration: this.banDuration
    }).subscribe({
      next: () => {
        this.banUserName = '';
        this.showBanForm = false;
        this.showSuccess(`User banned for ${this.banDuration}.`);
      },
      error: (err) => {
        console.error('Error banning user', err);
        this.error = 'Failed to ban user.';
      }
    });
  }

  private showSuccess(message: string): void {
    this.successMessage = message;
    this.error = '';
    setTimeout(() => this.successMessage = '', 3000);
  }
}
