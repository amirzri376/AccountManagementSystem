import { Component, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  @Output() navigateToRegister = new EventEmitter<void>();
  @Output() navigateToUserDashboard = new EventEmitter<void>();
  @Output() navigateToAdminDashboard = new EventEmitter<void>();

  username: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private http: HttpClient) {}

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const loginData = {
      username: this.username.trim(),
      password: this.password
    };

    // Call our backend API
    this.http.post<any>('/api/User/login', loginData)
      .subscribe({
        next: (response) => {
          this.successMessage = 'Login successful!';
          console.log('Login response:', response);
          // Store the token
          localStorage.setItem('token', response.token);
          this.isLoading = false;
          
          // Navigate to dashboard after successful login
          setTimeout(() => {
            if (response.user.role === 'Admin') {
              this.navigateToAdminDashboard.emit();
            } else {
              this.navigateToUserDashboard.emit();
            }
          }, 1000); // Small delay to show success message
        },
        error: (error) => {
          // Backend now returns simple string error messages
          this.errorMessage = error.error || 'Login failed. Please try again.';
          console.error('Login error:', error);
          this.isLoading = false;
        }
      });
  }

  goToRegister() {
    this.navigateToRegister.emit();
  }
}
