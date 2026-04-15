import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Lessons } from '../models/lessons.model';

@Injectable({
  providedIn: 'root'
})
export class LessonService {
  private baseUrl = 'http://localhost:5201/lessons';

  constructor(private http: HttpClient) { }

  getLessons(): Observable<Lessons[]> {
    return this.http.get<Lessons[]>(this.baseUrl);
  }

  getLessonById(id: string): Observable<Lessons> {
    return this.http.get<Lessons>(`${this.baseUrl}/${id}`);
  }

  createLesson(lesson: Lessons): Observable<string> {
    return this.http.post<string>(this.baseUrl, lesson);
  }

  updateLesson(lesson: Lessons): Observable<void> {
    return this.http.put<void>(this.baseUrl, lesson);
  }

  deleteLesson(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
