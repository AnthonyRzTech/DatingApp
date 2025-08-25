# WebMatcha 💕

A modern dating web application built with ASP.NET Core Blazor Server, featuring real-time chat, smart matching algorithms, and comprehensive user profiles.

## 🌟 Features

### Core Functionality
- **User Authentication**: Secure registration and login system with BCrypt password hashing and email verification
- **User Profiles**: Complete profiles with photos, bio, interests, and geolocation
- **Smart Matching**: Algorithm based on interests, location, sexual orientation, and preferences
- **Real-time Chat**: Live messaging between matched users using SignalR
- **Notifications**: Instant notifications for likes, matches, messages, and profile views
- **Browse & Discover**: Search and filter users by age, location, interests, and fame rating
- **Fame Rating**: Dynamic reputation system based on user interactions and profile completeness
- **Profile Views**: Track who viewed your profile with notification system
- **Block & Report**: Safety features to block and report inappropriate users

### Security Features
- BCrypt password hashing (complexity 12)
- SQL injection prevention via Entity Framework parameterized queries
- XSS protection through Blazor's built-in sanitization
- CSRF protection with antiforgery tokens
- Form validation with FluentValidation
- Environment variables for sensitive data
- Authentication required for protected pages
- File upload validation for photos

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 14+
- SMTP server for email (or use MailDev for development)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/WebMatcha.git
cd WebMatcha
```

2. **Set up environment variables**
```bash
cp .env.example .env
# Edit .env with your configuration:
# - Database credentials
# - SMTP settings
# - JWT secret key
```

3. **Install dependencies**
```bash
dotnet restore
```

4. **Set up the database**
```bash
# Create database
createdb -U postgres webmatcha

# Run migrations
dotnet ef database update
```

5. **Seed the database (optional)**
```bash
# Start the application first
dotnet run

# In another terminal, seed with 500 users
curl http://localhost:5192/api/seed
```

6. **Access the application**
- HTTP: http://localhost:5192
- HTTPS: https://localhost:7036

## 🛠️ Development

### Development Scripts

```bash
# Quick development setup with local PostgreSQL
./dev-local.sh

# Check system status and dependencies
./run-status.sh

# Run with containerized PostgreSQL
./dev.sh

# Deploy to production
./deploy.sh
```

### Project Structure

```
WebMatcha/
├── Components/           # Blazor components
│   ├── Layout/          # Layout components (MainLayout, NavMenu)
│   ├── Pages/           # Page components
│   └── Shared/          # Shared components (AuthRequired)
├── Data/                # Entity Framework context and configurations
├── Models/              # Data models and DTOs
├── Services/            # Business logic services
│   ├── CompleteAuthService.cs    # Authentication with email verification
│   ├── EmailService.cs           # Email sending functionality
│   ├── PhotoService.cs           # Photo upload and management
│   ├── BlockReportService.cs     # User safety features
│   ├── ProfileViewService.cs     # Profile view tracking
│   └── ...
├── Hubs/                # SignalR hubs for real-time features
│   └── ChatHub.cs       # Real-time messaging
├── Migrations/          # Database migrations
├── wwwroot/            # Static files
│   └── uploads/        # User uploaded photos
└── WebMatcha.Tests/    # Unit and integration tests
```

### Key Technologies

- **Framework**: ASP.NET Core 9.0 with Blazor Server
- **Database**: PostgreSQL with Entity Framework Core (Snake case naming)
- **Real-time**: SignalR for chat and notifications
- **Authentication**: Session-based (JWT ready)
- **Password Hashing**: BCrypt.Net-Next
- **Email**: MailKit for SMTP
- **Validation**: FluentValidation
- **UI**: Bootstrap 5 with custom CSS

## 📊 Database Schema

### Main Tables
- `users` - User profiles with all information
- `user_passwords` - Hashed passwords (separate for security)
- `user_photos` - Photo gallery (up to 5 photos per user)
- `likes` - Like relationships between users
- `matches` - Mutual matches
- `messages` - Chat messages with read status
- `notifications` - All types of notifications
- `profile_views` - Who viewed whose profile
- `blocks` - Blocked users
- `reports` - User reports with resolution status
- `tags` - Available interest tags
- `user_tags` - User interests

## 🔐 Security Implementation

### Authentication Flow
1. **Registration**
   - Username, email, and password validation
   - Age verification (18+)
   - Email verification token sent
   - Account inactive until email verified

2. **Login**
   - Email verified check
   - BCrypt password verification
   - Session creation
   - Last seen timestamp update

3. **Protected Routes**
   - AuthRequired component wraps protected pages
   - Automatic redirect to login if not authenticated
   - Session validation on each request

### Password Security
- Minimum 8 characters with complexity requirements
- BCrypt hashing with 12 rounds
- Password reset via email token
- Token expiration (24 hours)

## 🎨 Features Implementation

### 1. Profile Management
- **Edit Profile** (`/profile/edit`)
  - Basic info, photos, interests, location
  - Photo upload with validation (max 5MB, jpg/png/gif)
  - Geolocation with browser API
  - Interest tags selection

### 2. Matching System
- **Browse** (`/browse`)
  - Smart suggestions based on preferences
  - Filter by age, distance, fame, interests
  - Real-time updates
  - Like/unlike functionality

### 3. Real-time Chat
- **Chat** (`/chat`)
  - SignalR for instant messaging
  - Typing indicators
  - Read receipts
  - Online status
  - Message history

### 4. Search & Discovery
- **Advanced Search** (`/search`)
  - Multiple filter criteria
  - Sort by age, fame, distance, common tags
  - Real-time results
  - Save search preferences

### 5. Notifications
- Profile views
- New likes
- Matches (mutual likes)
- Messages
- Unlike notifications

## 🧪 Testing

### Run Tests
```bash
# Unit tests
cd WebMatcha.Tests
dotnet test

# Manual testing checklist
- [ ] Registration with email verification
- [ ] Login/logout flow
- [ ] Profile completion
- [ ] Photo upload
- [ ] Browse and like users
- [ ] Match creation
- [ ] Real-time chat
- [ ] Notifications
- [ ] Search functionality
- [ ] Block/report users
```

## 📝 API Endpoints

### Authentication
- `POST /auth/login` - Login with username/password
- `POST /auth/logout` - Logout current user
- `GET /api/verify-email/{token}` - Verify email address
- `POST /api/password-reset` - Request password reset
- `POST /api/reset-password` - Reset password with token

### Development
- `GET /api/health` - Health check
- `GET /api/users/count` - Get user count
- `GET /api/seed` - Seed database with 500 users
- `GET /api/debug/users` - Debug user data

### SignalR Hubs
- `/hubs/chat` - Real-time chat messaging

## 🚧 Remaining Implementation

### High Priority
1. **JWT Authentication**
   - Replace session with JWT tokens
   - Refresh token mechanism
   - Token storage strategy

2. **FastEndpoints Integration**
   - Convert controllers to endpoints
   - Add request/response DTOs
   - Implement validation

3. **Manual SQL Queries**
   - Replace LINQ with raw SQL
   - Optimize complex queries
   - Add database indexes

### Medium Priority
4. **Notification Hub**
   - Real-time push notifications
   - Notification preferences
   - Batch notifications

5. **Admin Features**
   - User management
   - Report resolution
   - Statistics dashboard

6. **Mobile Optimization**
   - Progressive Web App
   - Touch gestures
   - Offline support

### Low Priority
7. **Advanced Features**
   - Video chat
   - Events/activities
   - Premium features
   - Analytics

## 🐛 Known Issues

1. **Build Warnings**: Nullable reference warnings (non-critical)
2. **Session State**: Sometimes requires page refresh after login
3. **Photo Upload**: Large files may timeout
4. **Geolocation**: Requires HTTPS in production

## 🚀 Deployment

### Production Checklist
- [ ] Update connection strings
- [ ] Configure SMTP for production
- [ ] Set strong JWT secret
- [ ] Enable HTTPS
- [ ] Configure CORS
- [ ] Set up backup strategy
- [ ] Monitor performance
- [ ] Configure rate limiting

### Docker Deployment
```bash
# Build and run with Docker
./deploy.sh

# Or manually
docker build -t webmatcha .
docker run -p 80:80 -p 443:443 webmatcha
```

## 📄 License

This project is part of the 42 School curriculum.

## 👨‍💻 Contributors

- WebMatcha Development Team

---

**Important**: This is an educational project. Ensure proper security measures and legal compliance before deploying any dating application to production.