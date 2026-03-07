import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CalendarEvent } from '../models/calendar-event.model';
import { EventType } from '../models/enums/event-type.enum';
import { CalendarEventService } from '../services/calendar-event.service';
import { Lessons } from '../models/lessons.model';
import { LessonService } from '../services/lesson.service';

@Component({
  selector: 'app-calendar-list',
  imports: [CommonModule, FormsModule],
  templateUrl: './calendar-list.html',
  styleUrl: './calendar-list.scss'
})
export class CalendarList implements OnInit {
  title: string = 'Calendar';
  events: CalendarEvent[] = [];
  lessons: Lessons[] = [];
  loading: boolean = false;
  error: string = '';
  showForm: boolean = false;
  editingEvent: CalendarEvent | null = null;

  // Calendar view state
  currentDate: Date = new Date();
  viewMode: 'month' | 'week' = 'week';
  weekDays: Date[] = [];
  hours: number[] = Array.from({ length: 14 }, (_, i) => i + 8); // 8 AM to 9 PM
  selectedTimeSlot: { date: Date; hour: number } | null = null;

  // Form model
  eventForm: CalendarEvent = this.getEmptyEvent();

  // Enum for template - create arrays of {name, value} pairs
  eventTypes = Object.keys(EventType)
    .filter(key => !isNaN(Number(key)))
    .map(key => ({ value: Number(key), name: EventType[Number(key)] }));

  constructor(
    private calendarService: CalendarEventService,
    private lessonService: LessonService
  ) { }

  ngOnInit(): void {
    this.loadEvents();
    this.loadLessons();
    this.generateWeekDays();
  }

  loadLessons(): void {
    this.lessonService.getLessons().subscribe({
      next: (data: Lessons[]) => {
        this.lessons = data;
      },
      error: (err) => {
        console.error('Error fetching lessons', err);
      }
    });
  }

  generateWeekDays(): void {
    const start = this.getWeekStart(this.currentDate);
    this.weekDays = Array.from({ length: 7 }, (_, i) => {
      const day = new Date(start);
      day.setDate(start.getDate() + i);
      return day;
    });
  }

  getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day + (day === 0 ? -6 : 1); // Adjust when day is sunday
    return new Date(d.setDate(diff));
  }

  previousWeek(): void {
    this.currentDate.setDate(this.currentDate.getDate() - 7);
    this.currentDate = new Date(this.currentDate);
    this.generateWeekDays();
  }

  nextWeek(): void {
    this.currentDate.setDate(this.currentDate.getDate() + 7);
    this.currentDate = new Date(this.currentDate);
    this.generateWeekDays();
  }

  today(): void {
    this.currentDate = new Date();
    this.generateWeekDays();
  }

  onTimeSlotClick(date: Date, hour: number): void {
    this.selectedTimeSlot = { date, hour };
    this.openCreateFormWithTime(date, hour);
  }

  openCreateFormWithTime(date: Date, hour: number): void {
    const startDateTime = new Date(date);
    startDateTime.setHours(hour, 0, 0, 0);

    const endDateTime = new Date(startDateTime);
    endDateTime.setHours(hour + 1, 0, 0, 0);

    this.editingEvent = null;
    this.eventForm = {
      ...this.getEmptyEvent(),
      startDateTime: startDateTime,
      endDateTime: endDateTime
    };
    this.showForm = true;
  }

  getEventsForTimeSlot(date: Date, hour: number): CalendarEvent[] {
    return this.events.filter(event => {
      const eventStart = new Date(event.startDateTime);
      const slotDate = new Date(date);
      slotDate.setHours(0, 0, 0, 0);
      eventStart.setHours(eventStart.getHours(), 0, 0, 0);

      return eventStart.getDate() === slotDate.getDate() &&
             eventStart.getMonth() === slotDate.getMonth() &&
             eventStart.getFullYear() === slotDate.getFullYear() &&
             eventStart.getHours() === hour;
    });
  }

  isToday(date: Date): boolean {
    const today = new Date();
    return date.getDate() === today.getDate() &&
           date.getMonth() === today.getMonth() &&
           date.getFullYear() === today.getFullYear();
  }

  formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
  }

  formatTime(hour: number): string {
    const period = hour >= 12 ? 'PM' : 'AM';
    const displayHour = hour > 12 ? hour - 12 : hour === 0 ? 12 : hour;
    return `${displayHour}:00 ${period}`;
  }

  // Helper method to get lesson title by ID
  getLessonTitle(lessonId: string | undefined): string {
    if (!lessonId) return 'N/A';
    const lesson = this.lessons.find(l => l.id === lessonId);
    return lesson ? lesson.title : lessonId;
  }

  // Handle lesson selection to auto-fill title and description
  onLessonSelected(lessonId: string | undefined): void {
    if (!lessonId) {
      // If no lesson selected, clear the auto-filled fields only if they were from a previous lesson
      return;
    }

    const lesson = this.lessons.find(l => l.id === lessonId);
    if (lesson) {
      this.eventForm.title = lesson.title;
      this.eventForm.description = lesson.description;
      this.eventForm.type = EventType.Lesson;

      // Calculate end time based on lesson duration from the current start time
      if (this.eventForm.startDateTime) {
        const scheduledEnd = new Date(this.eventForm.startDateTime);
        scheduledEnd.setMinutes(scheduledEnd.getMinutes() + lesson.durationMinutes);
        this.eventForm.endDateTime = scheduledEnd;
      }
    }
  }

  loadEvents(): void {
    this.loading = true;
    this.error = '';
    this.calendarService.getCalendarEvents().subscribe({
      next: (data: CalendarEvent[]) => {
        this.events = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching calendar events', err);
        this.error = 'Failed to load calendar events';
        this.loading = false;
      }
    });
  }

  getEmptyEvent(): CalendarEvent {
    return {
      id: '',
      title: '',
      description: '',
      startDateTime: new Date(),
      endDateTime: new Date(Date.now() + 3600000), // +1 hour
      type: EventType.Lesson,
      color: '#3f51b5'
    };
  }

  openCreateForm(): void {
    this.editingEvent = null;
    this.eventForm = this.getEmptyEvent();
    this.showForm = true;
  }

  openEditForm(event: CalendarEvent): void {
    this.editingEvent = event;
    this.eventForm = {
      ...event,
      startDateTime: new Date(event.startDateTime),
      endDateTime: new Date(event.endDateTime)
    };
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingEvent = null;
    this.eventForm = this.getEmptyEvent();
  }

  saveEvent(): void {
    if (this.editingEvent) {
      // Update existing event
      this.calendarService.updateCalendarEvent(this.eventForm).subscribe({
        next: () => {
          this.loadEvents();
          this.closeForm();
        },
        error: (err: any) => {
          console.error('Error updating event', err);
          this.error = 'Failed to update event';
        }
      });
    } else {
      // Create new event
      this.calendarService.createCalendarEvent(this.eventForm).subscribe({
        next: () => {
          this.loadEvents();
          this.closeForm();
        },
        error: (err: any) => {
          console.error('Error creating event', err);
          this.error = 'Failed to create event';
        }
      });
    }
  }

  deleteEvent(id: string): void {
    if (confirm('Are you sure you want to delete this event?')) {
      console.log('Deleting event with ID:', id);
      this.calendarService.deleteCalendarEvent(id).subscribe({
        next: () => {
          console.log('Event deleted successfully');
          this.loadEvents();
          this.closeForm();
        },
        error: (err: any) => {
          console.error('Error deleting event:', err);
          console.error('Error details:', JSON.stringify(err, null, 2));
          this.error = `Failed to delete event: ${err.error?.message || err.message || 'Unknown error'}`;
        }
      });
    }
  }

  formatDateTime(dateTime: string | Date): string {
    const date = new Date(dateTime);
    return date.toLocaleString();
  }

  getEventTypeClass(type: EventType): string {
    return String(type).toLowerCase();
  }

  // Helper method to convert Date to datetime-local format
  toDateTimeLocal(date: Date | string): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  // Helper method to handle datetime input change
  onStartDateTimeChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.eventForm.startDateTime = new Date(input.value);
  }

  onEndDateTimeChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.eventForm.endDateTime = new Date(input.value);
  }
}
