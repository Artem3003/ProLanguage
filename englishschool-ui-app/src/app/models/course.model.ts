import { CourseLanguage } from './enums/course-language.enum';
import { CourseLevel } from './enums/course-level.enum';

export interface Course {
  id: string;
  title: string;
  description?: string;
  price: number;
  numberOfLessons: number;
  language: CourseLanguage;
  level: CourseLevel;
  durationMinutes: number;
  durationFormatted?: string;
  rating: number;
  isOnSale: boolean;
  discountPrice?: number;
  viewCount: number;
  createdAt: string;
  isNew?: boolean;
  imageUrl?: string;
}

export interface CourseFilter {
  isNew?: boolean;
  languages?: CourseLanguage[];
  minPrice?: number;
  maxPrice?: number;
  levels?: CourseLevel[];
  onSale?: boolean;
  minRating?: number;
  minDuration?: number;
  maxDuration?: number;
  title?: string;
  sortBy: string;
  pageSize: number;
  page: number;
}

export interface CourseFilterResult {
  courses: Course[];
  totalPages: number;
  currentPage: number;
  totalCount: number;
  pageSize: number;
}

export interface CourseFilterOptions {
  paginationOptions: number[];
  sortingOptions: { key: string; value: string }[];
  languages: { key: CourseLanguage; value: string }[];
  levels: { key: CourseLevel; value: string }[];
  ratingOptions: { key: number; value: string }[];
}
