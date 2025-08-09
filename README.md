# Login System

A full-stack web authentication system built with ASP.NET Core 8.0 backend and Angular 17 frontend. Features user registration, login, JWT authentication, and a protected dashboard.

## Features

- **User Registration** - Create new accounts with validation
- **User Login** - Secure authentication with JWT tokens
- **Protected Dashboard** - User-specific dashboard with account statistics
- **JWT Authentication** - Stateless token-based authentication
- **Responsive UI** - Modern, mobile-friendly interface
- **Real-time Validation** - Client and server-side validation
- **Secure Password Hashing** - BCrypt password encryption

## Technology Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server LocalDB** - Local database
- **JWT Bearer Authentication** - Token-based security
- **BCrypt.Net-Next** - Password hashing
- **Swagger/OpenAPI** - API documentation

### Frontend
- **Angular 17** - Frontend framework
- **TypeScript** - Programming language
- **HTTP Client** - API communication
- **CSS3** - Styling and responsive design

## Prerequisites

- **Visual Studio 2022** (or VS Code)
- **.NET 8.0 SDK**
- **Node.js** (v18 or higher)
- **Angular CLI** (`npm install -g @angular/cli`)
- **SQL Server LocalDB** (usually included with Visual Studio)

## Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/amirzri376/LoginSystem.git
   cd LoginSystem
   ```

2. **Install .NET dependencies**
   ```bash
   cd LoginSystem
   dotnet restore
   ```

3. **Install Angular dependencies**
   ```bash
   cd frontend
   npm install
   ```

4. **Set up the database**
   ```bash
   # In Package Manager Console (Visual Studio)
   Add-Migration InitialCreate
   Update-Database
   ```

## How to Run

1. **Build Angular frontend**
   ```bash
   cd frontend
   .\build.ps1
   ```

2. **Run the application**
   ```bash
   # In Visual Studio: Press F5 or click "Start"
   # Or from command line:
   dotnet run
   ```

3. **Access the application**
   - **Main App**: `https://localhost:7032`
   - **API Documentation**: `https://localhost:7032/swagger`

## API Endpoints

### Authentication
- `POST /api/User/register` - Register new user
- `POST /api/User/login` - User login
- `GET /api/User/dashboard` - Get user dashboard (requires JWT)

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

## Project Structure

```
LoginSystem/
├── Controllers/          # API controllers
│   └── UserController.cs
├── Models/              # Data models
│   └── User.cs
├── Data/                # Database context
│   └── ApplicationDbContext.cs
├── frontend/            # Angular application
│   ├── src/app/
│   │   ├── login/       # Login component
│   │   ├── register/    # Register component
│   │   └── dashboard/   # Dashboard component
│   └── build.ps1        # Build script
├── wwwroot/             # Compiled Angular files
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
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
   - Check user information display
   - Verify logout functionality

## Key Features Explained

- **JWT Authentication**: Secure, stateless authentication using JSON Web Tokens
- **Password Security**: Passwords are hashed using BCrypt with salt
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **Real-time Validation**: Immediate feedback on form inputs
- **Error Handling**: Comprehensive error messages and user feedback

## Development Notes

- **Frontend**: Angular components communicate via HTTP to ASP.NET Core API
- **Backend**: RESTful API with JWT authentication middleware
- **Database**: Entity Framework Core with SQL Server LocalDB
- **Build Process**: Angular builds to `wwwroot` folder, served by .NET

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is for educational purposes.
