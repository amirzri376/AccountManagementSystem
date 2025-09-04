# Account Management System

A comprehensive full-stack account management system built with ASP.NET Core 8.0 backend and Angular 20 frontend. Features user registration, login, JWT authentication, password reset, admin user management, and role-based dashboards.

## Features

- **User Registration** - Create new accounts with validation
- **User Login** - Secure authentication with JWT tokens
- **Password Reset** - Email-based password recovery with secure tokens
- **Admin User Management** - Admin dashboard with user status controls
- **Role-Based Access** - User and Admin-specific dashboards
- **Profile Update** - Edit email, first name, and last name from the user dashboard
- **JWT Authentication** - Stateless token-based authentication
- **Responsive UI** - Modern, mobile-friendly interface
- **Real-time Validation** - Client and server-side validation
- **Secure Password Hashing** - BCrypt password encryption
- **Email Integration** - SMTP email service for password reset
- **Google reCAPTCHA v2** - Bot protection on the login form

## Technology Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server LocalDB** - Local database
- **JWT Bearer Authentication** - Token-based security
- **BCrypt.Net-Next** - Password hashing
- **Swagger/OpenAPI** - API documentation

### Frontend
- **Angular 20** - Frontend framework
- **TypeScript** - Programming language
- **HTTP Client** - API communication
- **CSS3** - Styling and responsive design

## Prerequisites

- **Visual Studio 2022** (or VS Code)
- **.NET 8.0 SDK**
- **Node.js** (v18 or higher)
- **Angular CLI** (`npm install -g @angular/cli`)
- **SQL Server LocalDB** (usually included with Visual Studio)
- **Google reCAPTCHA v2 keys** (Site key and Secret key)

## Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/amirzri376/AccountManagementSystem.git
   cd AccountManagementSystem
   ```

2. **Install .NET dependencies**
   ```bash
   cd AccountManagementSystem
   dotnet restore
   ```

3. **Install Angular dependencies**
   ```bash
   cd frontend
   npm install
   ```

4. **Set up the database**
   ```bash
   # From the project directory (AccountManagementSystem/AccountManagementSystem)
   dotnet ef database update
   ```

5. **Configure the application**
   - Copy `appsettings.template.json` to `appsettings.Local.json`
   - Update `appsettings.Local.json` with your settings:
     - Database connection string
     - JWT secret key
     - Email settings (if using real email service)
   - **Note:** `appsettings.Local.json` is ignored by Git for security

## How to Run

1. **Build Angular frontend**
   ```bash
   cd frontend
   .\build.ps1
   ```

2. **Run the application**
   ```bash
   # Navigate to the project directory
   cd AccountManagementSystem
   
   # Run the application
   dotnet run
   ```

3. **Access the application**
   - **Main App**: `https://localhost:7032`
   - **API Documentation**: `https://localhost:7032/swagger`

## API Endpoints

### Authentication
- `POST /api/User/register` - Register new user
- `POST /api/User/login` - User login
- `POST /api/User/forget-password` - Request password reset
- `POST /api/User/reset-password` - Reset password with token
- `GET /api/User/dashboard` - Get user dashboard (requires JWT)
- `POST /api/User/update-profile` - Update user profile (requires JWT)

### Admin Management
- `GET /api/Admin/users` - Get all users (Admin only)
- `PUT /api/Admin/users/{id}/toggle-status` - Toggle user status (Admin only)

### Request/Response Examples

**Register User:**
```json
POST /api/User/register
{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "firstName": "Test",
  "lastName": "User"
}
```

**Login:**
```json
POST /api/User/login
{
  "username": "testuser",
  "password": "password123"
}
```

## Email Configuration

### Current Setup: Test Email Service
The application currently uses a **Test Email Service** for development purposes:
- **No real emails are sent** - emails are simulated and logged to console
- **Password reset tokens are generated and stored** in the database
- **Perfect for development and testing** without email service issues

### Switching to Real Email Service
To use real email sending (Gmail SMTP):

1. **Update `Program.cs`:**
   ```csharp
   // Change from:
   builder.Services.AddScoped<IEmailService, TestEmailService>();
   
   // To:
   builder.Services.AddScoped<IEmailService, EmailService>();
   ```

2. **Configure Gmail settings in `appsettings.Local.json`:**
   ```json
   {
     "EmailSettings": {
       "SmtpServer": "smtp.gmail.com",
       "SmtpPort": 587,
       "SmtpUsername": "your-email@gmail.com",
       "SmtpPassword": "your-app-password",
       "FromEmail": "your-email@gmail.com",
       "FromName": "Account Management System"
     }
   }
   ```

3. **Gmail Setup Requirements:**
   - Enable 2-Step Verification
   - Generate App Password (not regular password)
   - Use port 587 (TLS) or 465 (SSL)

## Project Structure

```
AccountManagementSystem/
├── Controllers/          # API controllers
│   ├── UserController.cs
│   └── AdminController.cs
├── Models/              # Data models
│   ├── User.cs
│   └── EmailSettings.cs
├── Services/            # Business services
│   ├── IEmailService.cs
│   ├── EmailService.cs      # Real email service (Gmail SMTP)
│   └── TestEmailService.cs  # Test email service (development)
├── Data/                # Database context
│   └── ApplicationDbContext.cs
├── frontend/            # Angular application
│   ├── src/app/
│   │   ├── login/       # Login component
│   │   ├── register/    # Register component
│   │   ├── forgot-password/ # Password reset component
│   │   ├── reset-password/  # Password reset form
│   │   ├── dashboard/   # User dashboard component (supports profile update)
│   │   └── admin-dashboard/ # Admin dashboard component
│   └── build.ps1        # Build script
├── wwwroot/             # Compiled Angular files
├── Program.cs           # Application entry point
├── appsettings.json     # Base configuration (template values)
├── appsettings.Local.json # Local configuration (ignored by Git)
└── appsettings.template.json # Template for local configuration
```

## Testing

1. **Start the application**
2. **Navigate to** `https://localhost:7032`
3. **Test Registration:**
   - Click "Register here"
   - Fill out the form
   - Verify success message
4. **Test Login:**
   - Use registered credentials
   - Verify redirect to dashboard
5. **Test Dashboard:**
   - Click "Edit Profile" on the dashboard
   - Update email, first name, or last name and save
   - Verify success message and updated fields
6. **Test Password Reset:**
   - Click "Forgot Password?"
   - Enter email address
   - Check console for test email output
   - Use the reset token to change password

## Troubleshooting

### Common Issues

1. **"Couldn't find a project to run"**
   - Make sure you're in the correct directory: `AccountManagementSystem/AccountManagementSystem`
   - Run `dotnet run` from the project directory

2. **Database connection errors**
   - Ensure SQL Server LocalDB is installed
   - Run `dotnet ef database update` to create/update database

3. **Email service errors**
   - Currently using test email service (no real emails sent)
   - Check console output for simulated email content
   - To use real email, follow the Email Configuration section above




## License

This project is for educational purposes.
