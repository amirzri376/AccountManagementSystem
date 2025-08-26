import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CommonModule],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.css'
})
export class AdminDashboard implements OnInit {
  @Output() navigateToLogin = new EventEmitter<void>();

  adminInfo: any = null;
  dashboardData: any = null;
  users: any[] = [];
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadAdminDashboardData();
  }

  loadAdminDashboardData() {
    this.isLoading = true;
    this.errorMessage = '';

    // Get token from localStorage
    const token = localStorage.getItem('token');
    
    if (!token) {
      this.errorMessage = 'No authentication token found. Please login again.';
      this.isLoading = false;
      return;
    }

    // Set up headers with JWT token
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    // Call admin dashboard API
    this.http.get<any>('/api/Admin/admin-dashboard', { headers })
      .subscribe({
        next: (response) => {
          this.adminInfo = response.admin;
          this.dashboardData = response.dashboardData;
          this.users = response.users;
          this.isLoading = false;
          console.log('Admin dashboard data:', response);
        },
        error: (error) => {
          this.errorMessage = error.error || 'Failed to load admin dashboard data.';
          console.error('Admin dashboard error:', error);
          this.isLoading = false;
          
          // If unauthorized, redirect to login
          if (error.status === 401) {
            this.logout();
          }
        }
      });
  }

  toggleUserStatus(userId: number) {
    // Get token from localStorage
    const token = localStorage.getItem('token');
    
    if (!token) {
      this.errorMessage = 'No authentication token found. Please login again.';
      return;
    }

    // Set up headers with JWT token
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    // Call toggle status API
    this.http.put<any>(`/api/Admin/users/${userId}/toggle-status`, {}, { headers })
      .subscribe({
        next: (response) => {
          console.log('Toggle status response:', response);
          
          // Update the user in the local array
          const userIndex = this.users.findIndex(u => u.id === userId);
          if (userIndex !== -1) {
            this.users[userIndex].isActive = response.isActive;
          }
          
          // Show success message (you could add a toast notification here)
          console.log(response.message);
        },
        error: (error) => {
          console.error('Toggle status error:', error);
          this.errorMessage = error.error || 'Failed to update user status.';
        }
      });
  }

  logout() {
    // Clear token and user data
    localStorage.removeItem('token');
    this.adminInfo = null;
    this.dashboardData = null;
    this.users = [];
    
    // Navigate back to login
    this.navigateToLogin.emit();
  }
}
