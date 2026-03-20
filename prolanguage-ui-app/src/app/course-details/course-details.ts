import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CartService } from '../services/cart.service';
import { CommentService } from '../services/comment.service';
import { CourseService } from '../services/course.service';
import { LessonService } from '../services/lesson.service';
import { Comment, CreateCommentRequest } from '../models/comment.model';
import { Course } from '../models/course.model';
import { Lessons } from '../models/lessons.model';

@Component({
  selector: 'app-course-details',
  imports: [CommonModule, FormsModule],
  templateUrl: './course-details.html',
  styleUrl: './course-details.scss'
})
export class CourseDetails implements OnInit {
  courseId = '';
  course: Course | null = null;
  lessons: Lessons[] = [];
  comments: Comment[] = [];

  loading = true;
  loadingComment = false;
  addingToCart = false;
  error = '';
  actionError = '';
  successMessage = '';

  showCommentForm = false;
  commentName = '';
  commentBody = '';

  activeParentComment: Comment | null = null;
  activeAction: 'reply' | 'quote' | null = null;
  actionName = '';
  actionBody = '';

  readonly stars = [1, 2, 3, 4, 5];

  constructor(
    private route: ActivatedRoute,
    private courseService: CourseService,
    private lessonService: LessonService,
    private commentService: CommentService,
    private cartService: CartService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (!id) {
        this.error = 'Course id is missing.';
        this.loading = false;
        return;
      }

      this.courseId = id;
      this.loadCoursePage();
    });
  }

  loadCoursePage(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      course: this.courseService.getCourseDetail(this.courseId),
      lessons: this.lessonService.getLessons(),
      comments: this.commentService.getCommentsByCourseId(this.courseId)
    }).subscribe({
      next: ({ course, lessons, comments }) => {
        this.course = course;
        this.lessons = lessons.filter(l => l.courseId === this.courseId);
        this.comments = comments;
        this.loading = false;
      },
      error: (err) => {
        this.error = err?.error?.message || 'Failed to load course page data.';
        this.loading = false;
      }
    });
  }

  addToCart(): void {
    if (!this.courseId) {
      return;
    }

    this.actionError = '';
    this.successMessage = '';
    this.addingToCart = true;

    this.cartService.addToCart(this.courseId).subscribe({
      next: () => {
        this.successMessage = 'Course added to cart.';
        this.addingToCart = false;
      },
      error: (err) => {
        this.actionError = err?.error?.message || 'Failed to add course to cart.';
        this.addingToCart = false;
      }
    });
  }

  toggleCommentForm(): void {
    this.showCommentForm = !this.showCommentForm;
    this.clearActionMessages();
  }

  submitComment(): void {
    if (!this.commentName.trim() || !this.commentBody.trim()) {
      return;
    }

    const request: CreateCommentRequest = {
      comment: {
        name: this.commentName.trim(),
        body: this.commentBody.trim()
      },
      parentId: null,
      action: null
    };

    this.loadingComment = true;
    this.clearActionMessages();

    this.commentService.addComment(this.courseId, request).subscribe({
      next: () => {
        this.commentName = '';
        this.commentBody = '';
        this.showCommentForm = false;
        this.successMessage = 'Comment posted successfully.';
        this.reloadComments();
      },
      error: (err) => {
        this.actionError = err?.error?.message || 'Failed to post comment.';
        this.loadingComment = false;
      }
    });
  }

  startReply(comment: Comment): void {
    this.activeParentComment = comment;
    this.activeAction = 'reply';
    this.actionName = '';
    this.actionBody = '';
    this.clearActionMessages();
  }

  startQuote(comment: Comment): void {
    this.activeParentComment = comment;
    this.activeAction = 'quote';
    this.actionName = '';
    this.actionBody = '';
    this.clearActionMessages();
  }

  cancelActiveAction(): void {
    this.activeParentComment = null;
    this.activeAction = null;
    this.actionName = '';
    this.actionBody = '';
  }

  submitAction(): void {
    if (!this.activeParentComment || !this.activeAction || !this.actionName.trim() || !this.actionBody.trim()) {
      return;
    }

    const prefix = this.activeAction === 'reply'
      ? this.activeParentComment.name
      : this.activeParentComment.body;

    const request: CreateCommentRequest = {
      comment: {
        name: this.actionName.trim(),
        body: `[${prefix}], ${this.actionBody.trim()}`
      },
      parentId: this.activeParentComment.id,
      action: this.activeAction
    };

    this.loadingComment = true;
    this.clearActionMessages();

    this.commentService.addComment(this.courseId, request).subscribe({
      next: () => {
        const completedAction = this.activeAction;
        this.cancelActiveAction();
        this.successMessage = completedAction === 'reply'
          ? 'Reply posted successfully.'
          : 'Quote posted successfully.';
        this.reloadComments();
      },
      error: (err) => {
        this.actionError = err?.error?.message || 'Failed to post comment action.';
        this.loadingComment = false;
      }
    });
  }

  deleteComment(comment: Comment): void {
    this.loadingComment = true;
    this.clearActionMessages();

    this.commentService.deleteComment(this.courseId, comment.id).subscribe({
      next: () => {
        this.successMessage = 'Comment deleted.';
        this.reloadComments();
      },
      error: (err) => {
        this.actionError = err?.error?.message || 'Failed to delete comment.';
        this.loadingComment = false;
      }
    });
  }

  private reloadComments(): void {
    this.commentService.getCommentsByCourseId(this.courseId).subscribe({
      next: comments => {
        this.comments = comments;
        this.loadingComment = false;
      },
      error: () => {
        this.loadingComment = false;
      }
    });
  }

  getCommentsCount(items: Comment[]): number {
    return items.reduce((acc, item) => acc + 1 + this.getCommentsCount(item.childComments || []), 0);
  }

  getFilledStars(value: number | undefined): number {
    const rating = value ?? 0;
    return Math.max(0, Math.min(5, Math.round(rating)));
  }

  formatDuration(minutes: number | undefined): string {
    if (!minutes || minutes <= 0) {
      return '0 minutes';
    }

    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;

    if (hours === 0) {
      return `${mins} minutes`;
    }

    if (mins === 0) {
      return `${hours} hour${hours > 1 ? 's' : ''}`;
    }

    return `${hours} hour${hours > 1 ? 's' : ''} ${mins} minutes`;
  }

  getCoursePriceText(): string {
    if (!this.course) {
      return '';
    }

    const price = this.course.isOnSale && this.course.discountPrice
      ? this.course.discountPrice
      : this.course.price;

    return `${price}₴`;
  }

  private clearActionMessages(): void {
    this.actionError = '';
    this.successMessage = '';
  }
}
