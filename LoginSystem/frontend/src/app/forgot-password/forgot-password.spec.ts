import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FormsModule } from '@angular/forms';
import { ForgotPassword } from './forgot-password';

describe('ForgotPasswordComponent', () => {
  let component: ForgotPassword;
  let fixture: ComponentFixture<ForgotPassword>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ForgotPassword,
        HttpClientTestingModule,
        FormsModule
      ]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ForgotPassword);
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
    expect(component.email).toBe('');
    expect(component.errorMessage).toBe('');
    expect(component.successMessage).toBe('');
    expect(component.isLoading).toBe(false);
  });

  it('should set loading state when submitting', () => {
    component.email = 'test@example.com';
    component.onSubmit();

    expect(component.isLoading).toBe(true);
    expect(component.errorMessage).toBe('');
    expect(component.successMessage).toBe('');
  });

  it('should make HTTP POST request to forgot-password endpoint', () => {
    const testEmail = 'test@example.com';
    component.email = testEmail;
    
    component.onSubmit();

    const req = httpMock.expectOne('/api/User/forget-password');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: testEmail });
    
    req.flush({ message: 'If an account with this email exists, a reset link has been sent.' });
  });

  it('should handle successful response', () => {
    const testEmail = 'test@example.com';
    const successMessage = 'If an account with this email exists, a reset link has been sent.';
    
    component.email = testEmail;
    component.onSubmit();

    const req = httpMock.expectOne('/api/User/forget-password');
    req.flush({ message: successMessage });

    expect(component.successMessage).toBe(successMessage);
    expect(component.isLoading).toBe(false);
    expect(component.errorMessage).toBe('');
  });

  it('should handle error response', () => {
    const testEmail = 'test@example.com';
    const errorMessage = 'Invalid email format';
    
    component.email = testEmail;
    component.onSubmit();

    const req = httpMock.expectOne('/api/User/forget-password');
    req.flush({ error: errorMessage }, { status: 400, statusText: 'Bad Request' });

    expect(component.errorMessage).toBe(errorMessage);
    expect(component.isLoading).toBe(false);
    expect(component.successMessage).toBe('');
  });

  it('should handle network error', () => {
    const testEmail = 'test@example.com';
    
    component.email = testEmail;
    component.onSubmit();

    const req = httpMock.expectOne('/api/User/forget-password');
    req.error(new ErrorEvent('Network error'));

    expect(component.errorMessage).toBe('Please try again.');
    expect(component.isLoading).toBe(false);
    expect(component.successMessage).toBe('');
  });

  it('should emit navigateToLogin event when goToLogin is called', () => {
    spyOn(component.navigateToLogin, 'emit');
    
    component.goToLogin();
    
    expect(component.navigateToLogin.emit).toHaveBeenCalled();
  });

  it('should display email input field', () => {
    const compiled = fixture.nativeElement;
    const emailInput = compiled.querySelector('input[type="email"]');
    
    expect(emailInput).toBeTruthy();
    expect(emailInput.getAttribute('placeholder')).toBe('Enter your email');
  });

  it('should display submit button', () => {
    const compiled = fixture.nativeElement;
    const submitButton = compiled.querySelector('button[type="submit"]');
    
    expect(submitButton).toBeTruthy();
    expect(submitButton.textContent).toContain('Send Reset Link');
  });

  it('should disable submit button when loading', () => {
    component.isLoading = true;
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
    
    expect(submitButton.textContent).toContain('Sending...');
  });

  it('should display success message when available', () => {
    const successMessage = 'Reset link sent successfully!';
    component.successMessage = successMessage;
    fixture.detectChanges();
    
    const compiled = fixture.nativeElement;
    const successElement = compiled.querySelector('.success-message');
    
    expect(successElement).toBeTruthy();
    expect(successElement.textContent).toContain(successMessage);
  });

  it('should display error message when available', () => {
    const errorMessage = 'Email not found';
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
});
