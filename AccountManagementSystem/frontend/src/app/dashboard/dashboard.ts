import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class Dashboard implements OnInit {
  @Output() navigateToLogin = new EventEmitter<void>();

  userInfo: any = null;
  dashboardData: any = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  // Edit mode state
  isEditMode: boolean = false;
  editEmail: string = '';
  editFirstName: string = '';
  editLastName: string = '';
  updateMessage: string = '';
  isUpdating: boolean = false;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading = true;
    this.errorMessage = '';

    const token = localStorage.getItem('token');

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

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    console.log('DEBUG: Making request to /api/User/dashboard');

    this.http.get<any>('/api/User/dashboard', { headers })
      .subscribe({
        next: (response) => {
          this.userInfo = response.user;
          this.dashboardData = response.dashboardData;
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.error || 'Failed to load dashboard data.';
          this.isLoading = false;

          if (error.status === 401) {
            this.logout();
          }
        }
      });
  }

  enterEditMode() {
    if (this.userInfo) {
      this.isEditMode = true;
      this.editEmail = this.userInfo.email || '';
      this.editFirstName = this.userInfo.firstName || '';
      this.editLastName = this.userInfo.lastName || '';
      this.updateMessage = '';
      this.errorMessage = '';
    }
  }

  cancelEdit() {
    this.isEditMode = false;
    this.updateMessage = '';
    this.errorMessage = '';
  }

  updateProfile() {
    const token = localStorage.getItem('token');
    if (!token) {
      this.errorMessage = 'No authentication token found. Please login again.';
      return;
    }

    const updateData = {
      email: this.editEmail.trim(),
      firstName: this.editFirstName.trim(),
      lastName: this.editLastName.trim()
    };

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });

    this.isUpdating = true;
    this.updateMessage = '';
    this.errorMessage = '';

    this.http.post<any>('/api/User/update-profile', updateData, { headers })
      .subscribe({
        next: (response) => {
          this.userInfo = response.user;
          this.dashboardData = response.dashboardData;

          this.isEditMode = false;
          this.isUpdating = false;
          this.updateMessage = 'Profile updated successfully!';
        },
        error: (error) => {
          this.errorMessage = error.error || 'Failed to update profile. Please try again.';
          this.isUpdating = false;

          if (error.status === 401) {
            this.logout();
          }
        }
      });
  }

  logout() {
    localStorage.removeItem('token');
    this.userInfo = null;
    this.dashboardData = null;
    this.navigateToLogin.emit();
  }
}
