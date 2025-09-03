import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {
  @Output() navigateToLogin = new EventEmitter<void>();

  userInfo: any = null;
  dashboardData: any = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.errorMessage = '';

    // Get token from localStorage
    const token = localStorage.getItem('token');
    
    // Debug: Log token information
    console.log('DEBUG: Token from localStorage:', token ? 'Token exists' : 'No token found');
    if (token) {
      console.log('DEBUG: Token length:', token.length);
      console.log('DEBUG: Token starts with:', token.substring(0, 20) + '...');
    }
    
    if (!token) {
      this.errorMessage = 'No authentication token found. Please login again.';
      this.isLoading = false;
      return;
    }

    // Set up headers with JWT token
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    // Debug: Log request details
    console.log('DEBUG: Making request to /api/User/dashboard');
    console.log('DEBUG: Headers:', headers);

    // Call dashboard API
    this.http.get<any>('/api/User/dashboard', { headers })
      .subscribe({
        next: (response) => {
          this.userInfo = response.user;
          this.dashboardData = response.dashboardData;
          this.isLoading = false;
          console.log('Dashboard data:', response);
        },
        error: (error) => {
          this.errorMessage = error.error || 'Failed to load dashboard data.';
          console.error('Dashboard error:', error);
          console.error('DEBUG: Error status:', error.status);
          console.error('DEBUG: Error message:', error.message);
          console.error('DEBUG: Error details:', error);
          this.isLoading = false;
          
          // If unauthorized, redirect to login
          if (error.status === 401) {
            this.logout();
          }
        }
      });
  }

  logout() {
    // Clear token and user data
    localStorage.removeItem('token');
    this.userInfo = null;
    this.dashboardData = null;
    
    // Navigate back to login
    this.navigateToLogin.emit();
  }
}
