import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Course, CourseFilter, CourseFilterResult, CourseFilterOptions } from '../models/course.model';

interface CreateCourseWithImageRequest {
  course: Partial<Course>;
  image?: string;
}

interface UpdateCourseWithImageRequest {
  course: Partial<Course>;
  image?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) { }

  getCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.baseUrl}/courses`);
  }

  getFilteredCourses(filter: CourseFilter): Observable<CourseFilterResult> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString())
      .set('sortBy', filter.sortBy || 'newest');

    if (filter.isNew !== undefined && filter.isNew !== null) {
      params = params.set('isNew', filter.isNew.toString());
    }
    if (filter.languages && filter.languages.length > 0) {
      filter.languages.forEach(lang => {
        params = params.append('languages', lang);
      });
    }
    if (filter.minPrice !== undefined && filter.minPrice !== null) {
      params = params.set('minPrice', filter.minPrice.toString());
    }
    if (filter.maxPrice !== undefined && filter.maxPrice !== null) {
      params = params.set('maxPrice', filter.maxPrice.toString());
    }
    if (filter.levels && filter.levels.length > 0) {
      filter.levels.forEach(level => {
        params = params.append('levels', level);
      });
    }
    if (filter.onSale !== undefined && filter.onSale !== null) {
      params = params.set('onSale', filter.onSale.toString());
    }
    if (filter.minRating !== undefined && filter.minRating !== null) {
      params = params.set('minRating', filter.minRating.toString());
    }
    if (filter.minDuration !== undefined && filter.minDuration !== null) {
      params = params.set('minDuration', filter.minDuration.toString());
    }
    if (filter.maxDuration !== undefined && filter.maxDuration !== null) {
      params = params.set('maxDuration', filter.maxDuration.toString());
    }
    if (filter.title) {
      params = params.set('title', filter.title);
    }

    return this.http.get<CourseFilterResult>(`${this.baseUrl}/courses/filter`, { params });
  }

  getFilterOptions(): Observable<CourseFilterOptions> {
    return this.http.get<CourseFilterOptions>(`${this.baseUrl}/courses/filter/options`);
  }

  getAvailableCourses(excludeLessonId?: string): Observable<Course[]> {
    let url = `${this.baseUrl}/courses/available`;
    if (excludeLessonId) {
      url += `?excludeLessonId=${excludeLessonId}`;
    }
    return this.http.get<Course[]>(url);
  }

  getCourseById(id: string): Observable<Course> {
    return this.http.get<Course>(`${this.baseUrl}/courses/${id}`);
  }

  getCourseDetail(id: string): Observable<Course> {
    return this.http.get<Course>(`${this.baseUrl}/courses/${id}/detail`);
  }

  getCourseImageUrl(id: string): string {
    return `${this.baseUrl}/courses/${id}/image`;
  }

  addCourse(course: Partial<Course>, image?: string): Observable<string> {
    const { imageUrl, ...coursePayload } = course;
    const payload: CreateCourseWithImageRequest = { course: coursePayload };
    if (image) {
      payload.image = image;
    }

    return this.http.post<string>(`${this.baseUrl}/courses`, payload);
  }

  updateCourse(course: Partial<Course>, image?: string): Observable<void> {
    const { imageUrl, ...coursePayload } = course;
    const payload: UpdateCourseWithImageRequest = { course: coursePayload };
    if (image) {
      payload.image = image;
    }

    return this.http.put<void>(`${this.baseUrl}/courses`, payload);
  }

  deleteCourse(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/courses/${id}`);
  }
}

