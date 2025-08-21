import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Login } from './login/login';
import { Register } from './register/register';
import { Dashboard } from './dashboard/dashboard';
import { AdminDashboard } from './admin-dashboard/admin-dashboard';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Login, Register, Dashboard, AdminDashboard, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
  currentPage: 'login' | 'register' | 'dashboard' | 'admin-dashboard' = 'login';

  showLogin() {
    this.currentPage = 'login';
  }

  showRegister() {
    this.currentPage = 'register';
  }

  showDashboard() {
    this.currentPage = 'dashboard';
  }

  showAdminDashboard() {
    this.currentPage = 'admin-dashboard';
  }
}
