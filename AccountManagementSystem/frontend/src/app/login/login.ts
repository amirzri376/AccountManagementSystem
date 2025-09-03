import { Component, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login implements AfterViewInit {
  @Output() navigateToRegister = new EventEmitter<void>();
  @Output() navigateToForgotPassword = new EventEmitter<void>();
  @Output() navigateToUserDashboard = new EventEmitter<void>();
  @Output() navigateToAdminDashboard = new EventEmitter<void>();

  username: string = '';
  password: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  recaptchaResponse: string = '';

  constructor(private http: HttpClient) { }

  ngAfterViewInit(): void {
    // Ensure the widget is rendered when the component is shown (e.g., after navigating back)
    const render = () => {
      const grecaptcha = (window as any).grecaptcha;
      const container = document.querySelector('.g-recaptcha') as HTMLElement | null;
      if (!container) return;

      if (!grecaptcha) {
        // Script not loaded yet, retry shortly
        setTimeout(render, 200);
        return;
      }

      // If not already auto-rendered (no iframe inside), render explicitly
      const alreadyRendered = !!container.querySelector('iframe');
      if (!alreadyRendered) {
        const sitekey = container.getAttribute('data-sitekey');
        try {
          grecaptcha.render(container, { sitekey });
        } catch {
          // no-op, will rely on auto-render if it happens
        }
      }
    };

    // Defer to allow DOM to settle
    setTimeout(render, 0);
  }

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const grecaptcha = (window as any).grecaptcha;

    // Get reCAPTCHA response (first widget on the page)
    const recaptchaResponse = grecaptcha ? grecaptcha.getResponse() : '';

    if (!recaptchaResponse) {
      this.errorMessage = 'Please complete the reCAPTCHA verification.';
      this.isLoading = false;
      return;
    }

    const loginData = {
      username: this.username.trim(),
      password: this.password,
      recaptchaResponse: recaptchaResponse
    };

    // Call our backend API
    this.http.post<any>('/api/User/login', loginData)
      .subscribe({
        next: (response) => {
          this.successMessage = 'Login successful!';
          // Store the token
          localStorage.setItem('token', response.token);
          this.isLoading = false;

          // Reset the widget so the token can't be reused
          try { if (grecaptcha) { grecaptcha.reset(); } } catch {}

          // Navigate to dashboard after successful login
          setTimeout(() => {
            if (response.user.role === 'Admin') {
              this.navigateToAdminDashboard.emit();
            } else {
              this.navigateToUserDashboard.emit();
            }
          }, 1000);
        },
        error: (error) => {
          // Backend now returns simple string error messages
          this.errorMessage = error.error || 'Login failed. Please try again.';
          this.isLoading = false;

          // Reset the widget to allow a new token
          try { if (grecaptcha) { grecaptcha.reset(); } } catch {}
        }
      });
  }

  goToRegister() {
    this.navigateToRegister.emit();
  }

  goToForgotPassword() {
    this.navigateToForgotPassword.emit();
  }
}
