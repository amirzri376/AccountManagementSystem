import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reset-password',
  imports: [FormsModule, CommonModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPassword implements OnInit {
  @Output() navigateToLogin = new EventEmitter<void>();

  token: string = '';
  newPassword: string = '';
  confirmPassword: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    // Extract token from URL parameters directly
    const urlParams = new URLSearchParams(window.location.search);
    this.token = urlParams.get('token') || '';
    
    if (!this.token) {
      this.errorMessage = 'Invalid reset link. Please request a new password reset.';
    }
  }

  onSubmit() {
    if (!this.token) {
      this.errorMessage = 'Invalid reset link. Please request a new password reset.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    if (this.newPassword.length < 6) {
      this.errorMessage = 'Password must be at least 6 characters long.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const rpData = {
      token: this.token,
      newPassword: this.newPassword
    };

    // Call our backend API
    this.http.post<any>('/api/User/reset-password', rpData)
      .subscribe({
        next: (response) => {
          this.successMessage = response.message;
          console.log('Reset password response:', response);
          this.isLoading = false;
          // Clear form after successful reset
          this.newPassword = '';
          this.confirmPassword = '';
        },
        error: (error) => {
          this.errorMessage = error.error || 'An error occurred while resetting your password.';
          console.error('Reset password error:', error);
          this.isLoading = false;
        }
      });
  }

  goToLogin() {
    this.navigateToLogin.emit();
  }
}
