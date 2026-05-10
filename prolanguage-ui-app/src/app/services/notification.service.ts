import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UpdateNotificationsRequest } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getAvailableMethods(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/notifications`);
  }

  getMyNotifications(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/my/notifications`);
  }

  updateMyNotifications(notifications: string[]): Observable<void> {
    const request: UpdateNotificationsRequest = { notifications };
    return this.http.put<void>(`${this.apiUrl}/notifications`, request);
  }
}
