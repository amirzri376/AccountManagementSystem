import { Component, signal, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Login } from './login/login';
import { Register } from './register/register';
import { Dashboard } from './dashboard/dashboard';
import { AdminDashboard } from './admin-dashboard/admin-dashboard';
import { CommonModule } from '@angular/common';
import { ForgotPassword } from './forgot-password/forgot-password';
import { ResetPassword } from './reset-password/reset-password';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Login, Register, Dashboard, AdminDashboard, CommonModule, ForgotPassword, ResetPassword],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('Account Management System');
  currentPage: 'login' | 'register' | 'forgot-password' | 'reset-password' | 'dashboard' | 'admin-dashboard' = 'login';

  ngOnInit() {
    // Check if we're on the reset-password page with a token
    const urlParams = new URLSearchParams(window.location.search);
    const token = urlParams.get('token');
    
    // Check if the current path is /reset-password
    if (window.location.pathname === '/reset-password' && token) {
      this.currentPage = 'reset-password';
    }
  }

  showLogin() {
    this.currentPage = 'login';
  }

  showRegister() {
    this.currentPage = 'register';
  }

  showForgotPassword() {
    this.currentPage = 'forgot-password';
  }

  showResetPassword() {
    this.currentPage = 'reset-password';
  }

  showDashboard() {
    this.currentPage = 'dashboard';
  }

  showAdminDashboard() {
    this.currentPage = 'admin-dashboard';
  }
}
