import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CalendarEvent } from '../models/calendar-event.model';

@Injectable({
  providedIn: 'root'
})
export class CalendarEventService {
  private baseUrl = 'http://localhost:5201/calendar';

  constructor(private http: HttpClient) { }

  getCalendarEvents(): Observable<CalendarEvent[]> {
    return this.http.get<CalendarEvent[]>(this.baseUrl);
  }

  getCalendarEventById(id: string): Observable<CalendarEvent> {
    return this.http.get<CalendarEvent>(`${this.baseUrl}/${id}`);
  }

  createCalendarEvent(event: CalendarEvent): Observable<string> {
    return this.http.post<string>(this.baseUrl, event);
  }

  updateCalendarEvent(event: CalendarEvent): Observable<void> {
    return this.http.put<void>(this.baseUrl, event);
  }

  deleteCalendarEvent(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
