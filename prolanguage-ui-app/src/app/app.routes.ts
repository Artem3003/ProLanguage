import { Routes } from '@angular/router';
import { CoursesList } from './courses-list/courses-list';
import { LessonsList } from './lessons-list/lessons-list';
import { HomeworksList } from './homeworks-list/homeworks-list';
import { AssignmentsList } from './assignments-list/assignments-list';
import { CalendarList } from './calendar-list/calendar-list';
import { Cart } from './cart/cart';
import { Orders } from './orders/orders';
import { SigninComponent } from './signin/signin';
import { RegisterComponent } from './register/register';
import { ForgotPasswordComponent } from './forgot-password/forgot-password';
import { ResetPasswordComponent } from './reset-password/reset-password';
import { AdminCourses } from './admin/admin-courses/admin-courses';
import { CommentsList } from './comments-list/comments-list';
import { CourseDetails } from './course-details/course-details';
import { authGuard, guestGuard, roleGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/courses', pathMatch: 'full' },
  { path: 'courses', component: CoursesList, canActivate: [authGuard] },
  { path: 'courses/:id', component: CourseDetails, canActivate: [authGuard] },
  { path: 'lessons', component: LessonsList, canActivate: [authGuard] },
  { path: 'homeworks', component: HomeworksList, canActivate: [authGuard] },
  { path: 'assignments', component: AssignmentsList, canActivate: [authGuard] },
  { path: 'calendar', component: CalendarList, canActivate: [authGuard] },
  { path: 'cart', component: Cart, canActivate: [authGuard] },
  { path: 'orders', component: Orders, canActivate: [authGuard] },
  { path: 'comments', component: CommentsList, canActivate: [authGuard] },
  { path: 'signin', component: SigninComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'forgot-password', component: ForgotPasswordComponent, canActivate: [guestGuard] },
  { path: 'reset-password', component: ResetPasswordComponent, canActivate: [guestGuard] },
  // Admin routes
  { path: 'admin/courses', component: AdminCourses, canActivate: [roleGuard], data: { roles: ['Admin'] } }
];

