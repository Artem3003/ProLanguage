import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ContactRequest } from '../models/contact.model';

@Injectable({
  providedIn: 'root'
})
export class ContactService {
  private readonly apiUrl = '/api/contact';

  constructor(private http: HttpClient) {}

  sendMessage(request: ContactRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(this.apiUrl, request);
  }
}
