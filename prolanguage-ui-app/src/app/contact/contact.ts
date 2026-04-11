import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ContactService } from '../services/contact.service';
import { ContactRequest } from '../models/contact.model';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.scss'
})
export class ContactComponent {
  model: ContactRequest = {
    name: '',
    surname: '',
    email: '',
    message: ''
  };

  loading = false;
  successMessage = '';
  errorMessage = '';
  submitted = false;

  constructor(
    private contactService: ContactService,
    private router: Router
  ) {}

  isFieldEmpty(value: string): boolean {
    return this.submitted && !value.trim();
  }

  isEmailInvalid(): boolean {
    if (!this.submitted) {
      return false;
    }

    const email = this.model.email.trim();
    if (!email) {
      return false;
    }

    return !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  isFormInvalid(): boolean {
    return !this.model.name.trim() ||
      !this.model.surname.trim() ||
      !this.model.email.trim() ||
      !this.model.message.trim() ||
      this.isEmailInvalid();
  }

  send(): void {
    this.submitted = true;
    if (this.isFormInvalid()) {
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.contactService.sendMessage(this.model).subscribe({
      next: () => {
        this.loading = false;
        sessionStorage.setItem('flashSuccessMessage', 'Your message was sent. We will contact you soon.');
        this.model = { name: '', surname: '', email: '', message: '' };
        this.submitted = false;
        this.router.navigateByUrl('/');
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.message || 'Failed to send message. Please try again.';
      }
    });
  }
}
