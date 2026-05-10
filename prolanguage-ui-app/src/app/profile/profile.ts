import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../services/notification.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class ProfileComponent implements OnInit {
  private notificationService = inject(NotificationService);

  public availableMethods = signal<string[]>([]);
  public selectedMethods = signal<Set<string>>(new Set());
  public isLoading = signal<boolean>(true);
  public isSaving = signal<boolean>(false);
  public successMessage = signal<string>('');

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.notificationService.getAvailableMethods().subscribe({
      next: (methods) => this.availableMethods.set(methods),
      error: (err) => console.error('Failed to load available methods', err)
    });

    this.notificationService.getMyNotifications()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (methods) => {
          this.selectedMethods.set(new Set(methods));
        },
        error: (err) => console.error('Failed to load my notifications', err)
      });
  }

  toggleMethod(method: string, event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    const currentSelected = new Set(this.selectedMethods());

    if (isChecked) {
      currentSelected.add(method);
    } else {
      currentSelected.delete(method);
    }

    this.selectedMethods.set(currentSelected);
  }

  isSelected(method: string): boolean {
    return this.selectedMethods().has(method);
  }

  savePreferences(): void {
    this.isSaving.set(true);
    this.successMessage.set('');

    const methodsArray = Array.from(this.selectedMethods());

    this.notificationService.updateMyNotifications(methodsArray)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Notification preferences updated successfully!');
          setTimeout(() => this.successMessage.set(''), 3000);
        },
        error: (err) => console.error('Failed to save preferences', err)
      });
  }
}
