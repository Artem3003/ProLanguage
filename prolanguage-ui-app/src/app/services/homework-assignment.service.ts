import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HomeworkAssignment } from '../models/homework-assignment.model';

@Injectable({
  providedIn: 'root'
})
export class HomeworkAssignmentService {
  private baseUrl = 'http://localhost:5201/assignments';

  constructor(private http: HttpClient) { }

  getHomeworkAssignments(): Observable<HomeworkAssignment[]> {
    return this.http.get<HomeworkAssignment[]>(this.baseUrl);
  }

  getHomeworkAssignmentById(id: string): Observable<HomeworkAssignment> {
    return this.http.get<HomeworkAssignment>(`${this.baseUrl}/${id}`);
  }

  createHomeworkAssignment(assignment: HomeworkAssignment): Observable<string> {
    return this.http.post<string>(this.baseUrl, assignment);
  }

  deleteHomeworkAssignment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  submitHomeworkAssignment(id: string, submission: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/submit`, submission);
  }

  gradeHomeworkAssignment(id: string, grade: any): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/grade`, grade);
  }
}
