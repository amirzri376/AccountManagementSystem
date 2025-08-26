import { Component, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-forgot-password',
  imports: [FormsModule, CommonModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {
  @Output() navigateToLogin = new EventEmitter<void>();

  email: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  constructor(private http: HttpClient) { }

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const fpData = {
      email: this.email
    };

    // Call our backend API
    this.http.post<any>('/api/User/forgot-password', fpData)
      .subscribe({
        next: (response) => {
          this.successMessage = response.message;
          console.log('Forgot password response:', response);
          this.isLoading = false;
        },
        error: (error) => {
          // Backend now returns simple string error messages
          this.errorMessage = error.error || 'Please try again.';
          console.error('Forgot password error:', error);
          this.isLoading = false;
        }
      });
  }

  goToLogin() {
    this.navigateToLogin.emit();
  }
}
