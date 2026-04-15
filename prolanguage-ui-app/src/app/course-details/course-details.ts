import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { CartService } from '../services/cart.service';
import { CommentService } from '../services/comment.service';
import { CourseService } from '../services/course.service';
import { LessonService } from '../services/lesson.service';
import { Comment, CreateCommentRequest } from '../models/comment.model';
import { CartItem } from '../models/cart.model';
import { Course } from '../models/course.model';
import { Lessons } from '../models/lessons.model';
import { AuthService } from '../services/auth.service';

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
  addedToCart = false;
  error = '';
  actionError = '';
  successMessage = '';

  showCommentForm = false;
  commentBody = '';
  commentRating = 0;

  activeParentComment: Comment | null = null;
  activeAction: 'reply' | null = null;
  actionBody = '';
  actionRating = 0;

  private likesMap: Record<string, string[]> = {};
  private currentUserId = '';

  readonly stars = [1, 2, 3, 4, 5];

  constructor(
    private route: ActivatedRoute,
    private courseService: CourseService,
    private lessonService: LessonService,
    private commentService: CommentService,
    private cartService: CartService,
    private authService: AuthService
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
      this.currentUserId = this.authService.getCurrentUser()?.userId ?? '';
      this.loadLikes();
      this.loadCoursePage();
    });
  }

  loadCoursePage(): void {
    this.loading = true;
    this.error = '';

    forkJoin({
      course: this.courseService.getCourseDetail(this.courseId),
      lessons: this.lessonService.getLessons().pipe(
        catchError(() => of([] as Lessons[]))
      ),
      cartItems: this.cartService.getCart().pipe(
        catchError(() => of([] as CartItem[]))
      ),
      comments: this.commentService.getCommentsByCourseId(this.courseId).pipe(
        catchError(() => of([] as Comment[]))
      )
    }).subscribe({
      next: ({ course, lessons, cartItems, comments }) => {
        this.course = course;
        this.lessons = lessons.filter(l => l.courseId === this.courseId);
        this.addedToCart = cartItems.some(item => item.courseId === this.courseId);
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
        this.addedToCart = true;
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
    if (!this.commentBody.trim() || this.commentRating < 1) {
      return;
    }

    const request: CreateCommentRequest = {
      comment: {
        body: this.commentBody.trim(),
        rating: this.commentRating
      },
      parentId: null,
      action: null
    };

    this.loadingComment = true;
    this.clearActionMessages();

    this.commentService.addComment(this.courseId, request).subscribe({
      next: () => {
        this.commentBody = '';
        this.commentRating = 0;
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
    if (!this.canManageComment(comment)) {
      return;
    }

    this.activeParentComment = comment;
    this.activeAction = 'reply';
    this.actionBody = '';
    this.actionRating = 0;
    this.clearActionMessages();
  }

  cancelActiveAction(): void {
    this.activeParentComment = null;
    this.activeAction = null;
    this.actionBody = '';
    this.actionRating = 0;
  }

  submitAction(): void {
    if (!this.activeParentComment || !this.activeAction || !this.actionBody.trim() || this.actionRating < 1) {
      return;
    }

    const prefix = this.activeAction === 'reply'
      ? this.activeParentComment.name
      : this.activeParentComment.body;

    const request: CreateCommentRequest = {
      comment: {
        body: `[${prefix}], ${this.actionBody.trim()}`,
        rating: this.actionRating
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
    if (!this.canManageComment(comment)) {
      return;
    }

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

  canManageComment(comment: Comment): boolean {
    return comment.isOwnComment;
  }

  formatCommentDate(value: string): string {
    if (!value) {
      return '';
    }

    return new Date(value).toLocaleDateString('uk-UA');
  }

  toggleLike(comment: Comment): void {
    if (!this.currentUserId) {
      return;
    }

    const likedUsers = this.likesMap[comment.id] ?? [];
    const existingIndex = likedUsers.indexOf(this.currentUserId);

    if (existingIndex >= 0)
    {
      likedUsers.splice(existingIndex, 1);
    }
    else
    {
      likedUsers.push(this.currentUserId);
    }

    this.likesMap[comment.id] = likedUsers;
    this.saveLikes();
  }

  isLiked(comment: Comment): boolean {
    return (this.likesMap[comment.id] ?? []).includes(this.currentUserId);
  }

  getLikesCount(comment: Comment): number {
    return (this.likesMap[comment.id] ?? []).length;
  }

  getAverageCommentRating(): number {
    const ratings = this.collectCommentRatings(this.comments);
    if (ratings.length === 0) {
      return 0;
    }

    const total = ratings.reduce((sum, rating) => sum + rating, 0);
    return total / ratings.length;
  }

  setCommentRating(rating: number): void {
    this.commentRating = rating;
  }

  setActionRating(rating: number): void {
    this.actionRating = rating;
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

  private likesStorageKey(): string {
    return `course-likes:${this.courseId}`;
  }

  private loadLikes(): void {
    const raw = localStorage.getItem(this.likesStorageKey());
    this.likesMap = raw ? JSON.parse(raw) : {};
  }

  private saveLikes(): void {
    localStorage.setItem(this.likesStorageKey(), JSON.stringify(this.likesMap));
  }

  private collectCommentRatings(items: Comment[]): number[] {
    return items.flatMap(item => [item.rating, ...this.collectCommentRatings(item.childComments || [])]);
  }
}
