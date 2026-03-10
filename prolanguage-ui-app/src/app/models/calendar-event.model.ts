import { EventType } from './enums/event-type.enum';

export interface CalendarEvent {
  id?: string;
  title: string;
  description?: string;
  startDateTime: Date;
  endDateTime: Date;
  type: EventType;
  color?: string;
  lessonId?: string;
}
