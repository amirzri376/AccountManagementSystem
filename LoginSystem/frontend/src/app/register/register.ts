import { Component, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  imports: [FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  @Output() navigateToLogin = new EventEmitter<void>();

  username: string = '';
  email: string = '';
  password: string = '';
  firstName: string = '';
  lastName: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private http: HttpClient) {}

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const registerData = {
      username: this.username,
      email: this.email,
      password: this.password,
      firstName: this.firstName,
      lastName: this.lastName
    };

    // Call our backend API
    this.http.post<any>('/api/User/register', registerData)
      .subscribe({
        next: (response) => {
          this.successMessage = 'Registration successful! You can now login.';
          console.log('Registration response:', response);
          this.isLoading = false;
          // Clear form after successful registration
          this.clearForm();
        },
        error: (error) => {
          this.errorMessage = error.error || 'Registration failed. Please try again.';
          console.error('Registration error:', error);
          this.isLoading = false;
        }
      });
  }

  clearForm() {
    this.username = '';
    this.email = '';
    this.password = '';
    this.firstName = '';
    this.lastName = '';
  }

  goToLogin() {
    this.navigateToLogin.emit();
  }
}
