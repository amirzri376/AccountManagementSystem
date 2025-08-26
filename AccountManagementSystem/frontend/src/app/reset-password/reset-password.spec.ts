import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { ResetPassword } from './reset-password';

describe('ResetPasswordComponent', () => {
  let component: ResetPassword;
  let fixture: ComponentFixture<ResetPassword>;
  let httpMock: HttpTestingController;
  let mockActivatedRoute: any;

  beforeEach(async () => {
    // Mock ActivatedRoute
    mockActivatedRoute = {
      queryParams: of({ token: 'test-token-123' })
    };

    await TestBed.configureTestingModule({
      imports: [
        ResetPassword,
        HttpClientTestingModule,
        FormsModule
      ],
      providers: [
        { provide: ActivatedRoute, useValue: mockActivatedRoute }
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ResetPassword);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with empty values', () => {
    expect(component.newPassword).toBe('');
    expect(component.confirmPassword).toBe('');
    expect(component.errorMessage).toBe('');
    expect(component.successMessage).toBe('');
    expect(component.isLoading).toBe(false);
  });

  it('should extract token from URL parameters on init', () => {
    expect(component.token).toBe('test-token-123');
  });

  it('should show error when no token is provided', () => {
    // Reset component
    component.token = '';
    component.errorMessage = '';
    
    component.onSubmit();
    
    expect(component.errorMessage).toBe('Invalid reset link. Please request a new password reset.');
  });

  it('should show error when passwords do not match', () => {
    component.token = 'test-token';
    component.newPassword = 'password123';
    component.confirmPassword = 'password456';
    
    component.onSubmit();
    
    expect(component.errorMessage).toBe('Passwords do not match.');
  });

  it('should show error when password is too short', () => {
    component.token = 'test-token';
    component.newPassword = '123';
    component.confirmPassword = '123';
    
    component.onSubmit();
    
    expect(component.errorMessage).toBe('Password must be at least 6 characters long.');
  });

  it('should set loading state when submitting with valid data', () => {
    component.token = 'test-token';
    component.newPassword = 'password123';
    component.confirmPassword = 'password123';
    
    component.onSubmit();
    
    expect(component.isLoading).toBe(true);
    expect(component.errorMessage).toBe('');
    expect(component.successMessage).toBe('');
  });

  it('should make HTTP POST request to reset-password endpoint', () => {
    const testToken = 'test-token-123';
    const testPassword = 'newpassword123';
    
    component.token = testToken;
    component.newPassword = testPassword;
    component.confirmPassword = testPassword;
    
    component.onSubmit();
    
    const req = httpMock.expectOne('/api/User/reset-password');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ 
      token: testToken, 
      newPassword: testPassword 
    });
    
    req.flush({ message: 'Password has been reset successfully' });
  });

  it('should handle successful response', () => {
    const testToken = 'test-token-123';
    const testPassword = 'newpassword123';
    const successMessage = 'Password has been reset successfully';
    
    component.token = testToken;
    component.newPassword = testPassword;
    component.confirmPassword = testPassword;
    
    component.onSubmit();
    
    const req = httpMock.expectOne('/api/User/reset-password');
    req.flush({ message: successMessage });
    
    expect(component.successMessage).toBe(successMessage);
    expect(component.isLoading).toBe(false);
    expect(component.errorMessage).toBe('');
    expect(component.newPassword).toBe('');
    expect(component.confirmPassword).toBe('');
  });

  it('should handle error response', () => {
    const testToken = 'test-token-123';
    const testPassword = 'newpassword123';
    const errorMessage = 'Invalid or expired reset token';
    
    component.token = testToken;
    component.newPassword = testPassword;
    component.confirmPassword = testPassword;
    
    component.onSubmit();
    
    const req = httpMock.expectOne('/api/User/reset-password');
    req.flush({ error: errorMessage }, { status: 400, statusText: 'Bad Request' });
    
    expect(component.errorMessage).toBe(errorMessage);
    expect(component.isLoading).toBe(false);
    expect(component.successMessage).toBe('');
  });

  it('should handle network error', () => {
    const testToken = 'test-token-123';
    const testPassword = 'newpassword123';
    
    component.token = testToken;
    component.newPassword = testPassword;
    component.confirmPassword = testPassword;
    
    component.onSubmit();
    
    const req = httpMock.expectOne('/api/User/reset-password');
    req.error(new ErrorEvent('Network error'));
    
    expect(component.errorMessage).toBe('An error occurred while resetting your password.');
    expect(component.isLoading).toBe(false);
    expect(component.successMessage).toBe('');
  });

  it('should emit navigateToLogin event when goToLogin is called', () => {
    spyOn(component.navigateToLogin, 'emit');
    
    component.goToLogin();
    
    expect(component.navigateToLogin.emit).toHaveBeenCalled();
  });

  it('should display new password input field', () => {
    const compiled = fixture.nativeElement;
    const passwordInput = compiled.querySelector('input[name="newPassword"]');
    
    expect(passwordInput).toBeTruthy();
    expect(passwordInput.getAttribute('type')).toBe('password');
    expect(passwordInput.getAttribute('placeholder')).toBe('Enter your new password');
  });

  it('should display confirm password input field', () => {
    const compiled = fixture.nativeElement;
    const confirmInput = compiled.querySelector('input[name="confirmPassword"]');
    
    expect(confirmInput).toBeTruthy();
    expect(confirmInput.getAttribute('type')).toBe('password');
    expect(confirmInput.getAttribute('placeholder')).toBe('Confirm your new password');
  });

  it('should display submit button', () => {
    const compiled = fixture.nativeElement;
    const submitButton = compiled.querySelector('button[type="submit"]');
    
    expect(submitButton).toBeTruthy();
    expect(submitButton.textContent).toContain('Reset Password');
  });

  it('should disable submit button when loading', () => {
    component.isLoading = true;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement;
    const submitButton = compiled.querySelector('button[type="submit"]');
    
    expect(submitButton.disabled).toBe(true);
  });

  it('should disable submit button when no token', () => {
    component.token = '';
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement;
    const submitButton = compiled.querySelector('button[type="submit"]');
    
    expect(submitButton.disabled).toBe(true);
  });

  it('should show loading text when submitting', () => {
    component.isLoading = true;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement;
    const submitButton = compiled.querySelector('button[type="submit"]');
    
    expect(submitButton.textContent).toContain('Resetting...');
  });

  it('should display success message when available', () => {
    const successMessage = 'Password has been reset successfully!';
    component.successMessage = successMessage;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement;
    const successElement = compiled.querySelector('.success-message');
    
    expect(successElement).toBeTruthy();
    expect(successElement.textContent).toContain(successMessage);
  });

  it('should display error message when available', () => {
    const errorMessage = 'Invalid reset token';
    component.errorMessage = errorMessage;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement;
    const errorElement = compiled.querySelector('.error-message');
    
    expect(errorElement).toBeTruthy();
    expect(errorElement.textContent).toContain(errorMessage);
  });

  it('should display back to login link', () => {
    const compiled = fixture.nativeElement;
    const loginLink = compiled.querySelector('.login-link');
    
    expect(loginLink).toBeTruthy();
    expect(loginLink.textContent).toContain('Back to Login');
  });

  it('should handle missing token in URL', () => {
    // Create new component with no token
    const mockRouteNoToken = {
      queryParams: of({})
    };

    TestBed.overrideProvider(ActivatedRoute, { useValue: mockRouteNoToken });
    
    const newFixture = TestBed.createComponent(ResetPassword);
    const newComponent = newFixture.componentInstance;
    newFixture.detectChanges();
    
    expect(newComponent.token).toBe('');
    expect(newComponent.errorMessage).toBe('Invalid reset link. Please request a new password reset.');
  });

  it('should validate password length correctly', () => {
    component.token = 'test-token';
    
    // Test too short password
    component.newPassword = '12345';
    component.confirmPassword = '12345';
    component.onSubmit();
    expect(component.errorMessage).toBe('Password must be at least 6 characters long.');
    
    // Test valid password
    component.newPassword = '123456';
    component.confirmPassword = '123456';
    component.onSubmit();
    expect(component.errorMessage).toBe('');
  });
});
