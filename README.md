# WebMatcha 💕

A modern dating web application built with ASP.NET Core Blazor Server, featuring real-time chat, smart matching algorithms, and comprehensive user profiles.

## 🌟 Features

### Core Functionality
- **User Authentication**: Secure registration and login system with BCrypt password hashing
- **User Profiles**: Complete profiles with photos, bio, interests, and location
- **Smart Matching**: Algorithm based on interests, location, and preferences
- **Real-time Chat**: Live messaging between matched users using SignalR
- **Notifications**: Instant notifications for likes, matches, and messages
- **Browse & Discover**: Search and filter users by various criteria
- **Fame Rating**: Reputation system based on user interactions

### Security Features
- BCrypt password hashing (complexity 12)
- SQL injection prevention
- XSS protection
- CSRF protection
- Form validation
- Environment variables for sensitive data

## 🚀 Quick Start

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL 14+
- Node.js (for frontend assets)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/WebMatcha.git
cd WebMatcha
```

2. **Set up environment variables**
```bash
cp .env.example .env
# Edit .env with your database credentials
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

5. **Run the application**
```bash
dotnet run
```

The application will be available at:
- HTTP: http://localhost:5192
- HTTPS: https://localhost:7036

## 🛠️ Development

### Development Scripts

```bash
# Quick development setup with local PostgreSQL
./dev-local.sh

# Check system status and dependencies
./run-status.sh

# Run authentication tests
./test-auth-simple.sh

# Seed database with test data (500 users)
curl http://localhost:5192/api/seed
```

### Project Structure

```
WebMatcha/
├── Components/           # Blazor components
│   ├── Layout/          # Layout components (MainLayout, NavMenu)
│   └── Pages/           # Page components (Login, Register, Browse, etc.)
├── Data/                # Entity Framework context and configurations
├── Models/              # Data models and DTOs
├── Services/            # Business logic services
├── Hubs/                # SignalR hubs for real-time features
├── Migrations/          # Database migrations
├── wwwroot/             # Static files (CSS, JS, images)
└── WebMatcha.Tests/     # Unit and integration tests
```

### Key Technologies

- **Framework**: ASP.NET Core 9.0 with Blazor Server
- **Database**: PostgreSQL with Entity Framework Core
- **Real-time**: SignalR for chat and notifications
- **Authentication**: Custom JWT-based authentication (coming soon)
- **Password Hashing**: BCrypt.Net-Next
- **Email**: MailKit for email verification
- **Testing**: xUnit, bUnit, Moq, FluentAssertions

## 📊 Database Schema

### Main Tables
- `users` - User profiles and information
- `user_passwords` - Hashed passwords (separate for security)
- `likes` - User likes/interests
- `matches` - Mutual matches between users
- `messages` - Chat messages
- `notifications` - User notifications
- `profile_views` - Profile view tracking
- `blocks` - Blocked users
- `reports` - User reports
- `tags` - Interest tags
- `user_tags` - User-tag relationships
- `user_photos` - User photo gallery

## 🔐 Authentication System

### Registration
- Username and email validation
- Password strength requirements
- Age verification (18+)
- Profile completion steps

### Login
- Session-based authentication (JWT coming soon)
- Remember me functionality
- Password reset via email

### Security
- Passwords hashed with BCrypt (12 rounds)
- SQL injection prevention via parameterized queries
- XSS protection through Blazor's built-in sanitization
- CSRF tokens on forms

## 🧪 Testing

### Run Tests
```bash
# Run unit tests
cd WebMatcha.Tests
dotnet test

# Run functional tests
./test-auth-simple.sh
```

### Test Coverage
- ✅ Authentication service tests
- ✅ Registration validation tests
- ✅ Login flow tests
- ✅ Password hashing tests
- ✅ Integration tests
- ✅ Component tests (Blazor)

## 📝 API Endpoints

### Public Endpoints
- `GET /api/health` - Health check
- `POST /auth/logout` - Logout user

### Development Endpoints
- `GET /api/seed` - Seed database with test data

### SignalR Hubs
- `/hubs/chat` - Real-time chat hub

## 🎨 UI Features

- **Responsive Design**: Mobile-first approach with Bootstrap 5
- **Dark Mode**: Coming soon
- **Interactive Components**: Real-time updates without page refresh
- **Smooth Animations**: CSS transitions and animations
- **Accessibility**: ARIA labels and keyboard navigation

## 🚧 Roadmap

### Phase 1 (Complete) ✅
- [x] Basic authentication system
- [x] User registration and login
- [x] Database setup with PostgreSQL
- [x] Basic UI with Bootstrap

### Phase 2 (In Progress) 🔄
- [ ] JWT authentication
- [ ] Email verification
- [ ] Password reset
- [ ] Complete user profiles

### Phase 3 (Planned) 📋
- [ ] Matching algorithm
- [ ] Real-time chat
- [ ] Notifications system
- [ ] Photo upload with validation

### Phase 4 (Future) 🔮
- [ ] Advanced search filters
- [ ] Fame rating system
- [ ] Report and block features
- [ ] Admin dashboard

## 🐛 Known Issues

- Navigation redirect shows both error and success messages (fix in progress)
- Email verification not working (SMTP server configuration needed)
- Some build warnings related to nullable references

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is part of the 42 School curriculum and follows the academic guidelines.

## 👨‍💻 Authors

- WebMatcha Team

## 🙏 Acknowledgments

- 42 School for the project subject
- ASP.NET Core team for the excellent framework
- Bootstrap team for the UI components
- All contributors and testers

## 📞 Support

For issues and questions:
- Create an issue on GitHub
- Check the [documentation](./docs/)
- Review the [subject requirements](./subject.md)

---

**Note**: This is an educational project. Please ensure you understand and implement proper security measures before deploying any dating application to production.