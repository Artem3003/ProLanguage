import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Homework } from '../models/homework.model';

@Injectable({
  providedIn: 'root'
})
export class HomeworkService {
  private baseUrl = 'http://localhost:5201/homeworks';

  constructor(private http: HttpClient) { }

  getHomeworks(): Observable<Homework[]> {
    return this.http.get<Homework[]>(this.baseUrl);
  }

  getHomeworkById(id: string): Observable<Homework> {
    return this.http.get<Homework>(`${this.baseUrl}/${id}`);
  }

  createHomework(homework: Homework): Observable<string> {
    return this.http.post<string>(this.baseUrl, homework);
  }

  updateHomework(homework: Homework): Observable<void> {
    return this.http.put<void>(this.baseUrl, homework);
  }

  deleteHomework(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
