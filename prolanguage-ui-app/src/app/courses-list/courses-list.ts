import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Course, CourseFilter, CourseFilterResult, CourseFilterOptions } from '../models/course.model';
import { CourseLanguage } from '../models/enums/course-language.enum';
import { CourseLevel } from '../models/enums/course-level.enum';
import { CourseService } from '../services/course.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-courses-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './courses-list.html',
  styleUrl: './courses-list.scss'
})
export class CoursesList implements OnInit {
  // State
  courses: Course[] = [];
  filteredResult: CourseFilterResult = {
    courses: [],
    totalCount: 0,
    pageSize: 9,
    currentPage: 1,
    totalPages: 1
  };
  loading = false;
  error = '';

  // Filter state
  filter: CourseFilter = {
    page: 1,
    pageSize: 9,
    sortBy: 'newest'
  };

  searchTerm = '';

  // Dropdown states
  openDropdown: string | null = null;

  // Filter options (loaded from backend)
  languageOptions: { key: CourseLanguage; value: string }[] = [];
  levelOptions: { key: CourseLevel; value: string }[] = [];
  sortingOptions: { key: string; value: string }[] = [];
  ratingOptions: { key: number; value: string }[] = [];

  constructor(
    private courseService: CourseService,
    private router: Router
  ) {}

  // Selected filters
  selectedLanguages: CourseLanguage[] = [];
  selectedLevels: CourseLevel[] = [];
  selectedSortBy = '';
  isNewFilter = false;
  onSaleFilter = false;
  minPrice: number | null = null;
  maxPrice: number | null = null;
  minDuration: number | null = null;
  maxDuration: number | null = null;
  minRating: number | null = null;

  // Computed properties for template
  get totalCount(): number {
    return this.filteredResult.totalCount;
  }

  get totalPages(): number {
    return this.filteredResult.totalPages;
  }

  ngOnInit(): void {
    this.loadFilterOptions();
    this.loadCourses();
  }

  loadFilterOptions(): void {
    this.courseService.getFilterOptions().subscribe({
      next: (options: CourseFilterOptions) => {
        this.languageOptions = options.languages;
        this.levelOptions = options.levels;
        this.sortingOptions = options.sortingOptions;
        this.ratingOptions = options.ratingOptions;
      },
      error: (err) => {
        console.error('Error loading filter options', err);
        // Fallback to hardcoded options
        this.languageOptions = Object.values(CourseLanguage).map(lang => ({ key: lang, value: lang }));
        this.levelOptions = Object.values(CourseLevel).map(level => ({ key: level, value: level }));
        this.sortingOptions = [
          { key: 'newest', value: 'Newest First' },
          { key: 'price_asc', value: 'Price: Low to High' },
          { key: 'price_desc', value: 'Price: High to Low' },
          { key: 'rating', value: 'Highest Rated' },
          { key: 'popularity', value: 'Most Popular' }
        ];
        this.ratingOptions = [
          { key: 4.5, value: '4.5+ Stars' },
          { key: 4.0, value: '4.0+ Stars' },
          { key: 3.5, value: '3.5+ Stars' },
          { key: 3.0, value: '3.0+ Stars' }
        ];
      }
    });
  }

  loadCourses(): void {
    this.loading = true;
    this.error = '';

    this.applyFilters();
  }

  applyFilters(): void {
    this.loading = true;
    this.error = '';

    // Build filter object for API
    const apiFilter: CourseFilter = {
      page: this.filter.page,
      pageSize: this.filter.pageSize,
      sortBy: this.selectedSortBy || 'newest'
    };

    if (this.searchTerm.trim()) {
      apiFilter.title = this.searchTerm.trim();
    }
    if (this.isNewFilter) {
      apiFilter.isNew = true;
    }
    if (this.selectedLanguages.length > 0) {
      apiFilter.languages = this.selectedLanguages;
    }
    if (this.selectedLevels.length > 0) {
      apiFilter.levels = this.selectedLevels;
    }
    if (this.minPrice !== null) {
      apiFilter.minPrice = this.minPrice;
    }
    if (this.maxPrice !== null) {
      apiFilter.maxPrice = this.maxPrice;
    }
    if (this.onSaleFilter) {
      apiFilter.onSale = true;
    }
    if (this.minRating !== null) {
      apiFilter.minRating = this.minRating;
    }
    if (this.minDuration !== null) {
      apiFilter.minDuration = this.minDuration;
    }
    if (this.maxDuration !== null) {
      apiFilter.maxDuration = this.maxDuration;
    }

    this.courseService.getFilteredCourses(apiFilter).subscribe({
      next: (result: CourseFilterResult) => {
        this.filteredResult = result;
        this.courses = result.courses;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading courses', err);
        this.error = 'Failed to load courses. Please try again.';
        this.loading = false;
      }
    });
  }

  // Dropdown management
  toggleDropdown(name: string): void {
    this.openDropdown = this.openDropdown === name ? null : name;
  }

  closeDropdowns(): void {
    this.openDropdown = null;
  }

  isDropdownOpen(name: string): boolean {
    return this.openDropdown === name;
  }

  // Filter toggles
  toggleNewFilter(): void {
    this.isNewFilter = !this.isNewFilter;
    this.filter.page = 1;
    this.applyFilters();
  }

  toggleOnSaleFilter(): void {
    this.onSaleFilter = !this.onSaleFilter;
    this.filter.page = 1;
    this.applyFilters();
  }

  toggleLanguage(lang: CourseLanguage): void {
    const index = this.selectedLanguages.indexOf(lang);
    if (index > -1) {
      this.selectedLanguages.splice(index, 1);
    } else {
      this.selectedLanguages.push(lang);
    }
    this.filter.page = 1;
    this.applyFilters();
  }

  isLanguageSelected(lang: CourseLanguage): boolean {
    return this.selectedLanguages.includes(lang);
  }

  hasLanguageFilter(): boolean {
    return this.selectedLanguages.length > 0;
  }

  toggleLevel(level: CourseLevel): void {
    const index = this.selectedLevels.indexOf(level);
    if (index > -1) {
      this.selectedLevels.splice(index, 1);
    } else {
      this.selectedLevels.push(level);
    }
    this.filter.page = 1;
    this.applyFilters();
  }

  isLevelSelected(level: CourseLevel): boolean {
    return this.selectedLevels.includes(level);
  }

  hasLevelFilter(): boolean {
    return this.selectedLevels.length > 0;
  }

  hasPriceFilter(): boolean {
    return this.minPrice !== null || this.maxPrice !== null;
  }

  hasRatingFilter(): boolean {
    return this.minRating !== null;
  }

  hasDurationFilter(): boolean {
    return this.minDuration !== null || this.maxDuration !== null;
  }

  setRating(rating: number | null): void {
    this.minRating = rating;
    this.filter.page = 1;
    this.applyFilters();
    this.closeDropdowns();
  }

  setSorting(sortBy: string): void {
    this.selectedSortBy = sortBy;
    this.applyFilters();
    this.closeDropdowns();
  }

  onPriceChange(): void {
    this.filter.page = 1;
    this.applyFilters();
  }

  onDurationChange(): void {
    this.filter.page = 1;
    this.applyFilters();
  }

  onSearch(): void {
    this.filter.page = 1;
    this.applyFilters();
  }

  // Pagination
  goToPage(page: number): void {
    if (page >= 1 && page <= this.filteredResult.totalPages) {
      this.filter.page = page;
      this.applyFilters();
    }
  }

  nextPage(): void {
    this.goToPage(this.filter.page + 1);
  }

  prevPage(): void {
    this.goToPage(this.filter.page - 1);
  }

  getEndIndex(): number {
    return Math.min(this.filter.page * this.filter.pageSize, this.totalCount);
  }

  // Utility methods
  formatDuration(minutes: number | undefined): string {
    if (!minutes) return '';
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    if (hours === 0) return `${mins}m`;
    if (mins === 0) return `${hours}h`;
    return `${hours}h ${mins}m`;
  }

  isNew(course: Course): boolean {
    // Use backend-computed isNew property if available
    if (course.isNew !== undefined) return course.isNew;
    if (!course.createdAt) return false;
    const sevenDaysAgo = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000);
    return new Date(course.createdAt) > sevenDaysAgo;
  }

  getEffectivePrice(course: Course): number {
    return course.isOnSale && course.discountPrice ? course.discountPrice : course.price;
  }

  getSortLabel(): string {
    const option = this.sortingOptions.find(o => o.key === this.selectedSortBy);
    return option ? option.value : 'Sorting by';
  }

  viewCourseDetail(courseId: string): void {
    this.router.navigate(['/courses', courseId]);
  }

  getActiveFiltersCount(): number {
    let count = 0;
    if (this.isNewFilter) count++;
    if (this.selectedLanguages.length > 0) count++;
    if (this.selectedLevels.length > 0) count++;
    if (this.minPrice !== null || this.maxPrice !== null) count++;
    if (this.onSaleFilter) count++;
    if (this.minRating !== null) count++;
    if (this.minDuration !== null || this.maxDuration !== null) count++;
    return count;
  }
}

