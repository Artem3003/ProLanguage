import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Comment, CreateCommentRequest, BanRequest } from '../models/comment.model';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) { }

  getCommentsByCourseId(courseId: string): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.baseUrl}/courses/${courseId}/comments`);
  }

  addComment(courseId: string, request: CreateCommentRequest): Observable<string> {
    return this.http.post<string>(`${this.baseUrl}/courses/${courseId}/comments`, request);
  }

  deleteComment(courseId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/courses/${courseId}/comments/${commentId}`);
  }

  getBanDurations(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/comments/ban/durations`);
  }

  banUser(request: BanRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/comments/ban`, request);
  }
}
