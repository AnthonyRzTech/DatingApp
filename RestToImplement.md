# WebMatcha - Remaining Implementation Tasks

## 🚨 CRITICAL: Security Requirements (0% if any missing)
- [ ] **BCrypt Password Hashing** - Package installed, needs implementation
- [ ] **SQL Injection Prevention** - Use parameterized queries
- [ ] **XSS Protection** - Validate and sanitize all user inputs
- [ ] **CSRF Protection** - Implement anti-forgery tokens
- [ ] **Form Validation** - Server-side validation on all forms
- [ ] **File Upload Validation** - Check file types, sizes for photos
- [ ] **.env File** - Store all secrets (DB connection, JWT secret, SMTP)

## 📊 Database Setup
- [ ] **PostgreSQL Configuration**
  - [ ] Create database schema
  - [ ] Setup connection string in .env
  - [ ] Create tables with snake_case naming
- [ ] **Required Tables**
  - [ ] users (id, email, username, first_name, last_name, password_hash, verified, created_at)
  - [ ] user_profiles (gender, sexual_preference, biography, birth_date, fame_rating)
  - [ ] user_photos (user_id, photo_url, is_primary, uploaded_at)
  - [ ] tags (id, name)
  - [ ] user_tags (user_id, tag_id)
  - [ ] likes (liker_id, liked_id, created_at)
  - [ ] matches (user1_id, user2_id, matched_at)
  - [ ] messages (sender_id, receiver_id, content, sent_at, read_at)
  - [ ] notifications (user_id, type, content, created_at, read)
  - [ ] profile_views (viewer_id, viewed_id, viewed_at)
  - [ ] blocks (blocker_id, blocked_id, created_at)
  - [ ] reports (reporter_id, reported_id, reason, created_at)
  - [ ] user_locations (user_id, latitude, longitude, updated_at)
- [ ] **Seed Data** - Minimum 500 user profiles for evaluation
- [ ] **Manual SQL Queries** - No full ORM allowed, write raw queries

## 🔐 1. Authentication System

### Registration
- [ ] Create registration endpoint with validation
- [ ] Validate password strength (reject common words)
- [ ] Hash password with BCrypt before storage
- [ ] Generate unique email verification token
- [ ] Send verification email with MailKit
- [ ] Create email verification endpoint
- [ ] Activate account on verification

### Login/Logout
- [ ] Create login endpoint (username + password)
- [ ] Generate JWT token on successful login
- [ ] Store last activity timestamp
- [ ] Create logout endpoint to invalidate token
- [ ] Add JWT authentication middleware

### Password Recovery
- [ ] Create "forgot password" endpoint
- [ ] Generate unique reset token
- [ ] Send reset email with link
- [ ] Create password reset page
- [ ] Validate and update new password

## 👤 2. User Profile System

### Profile Completion
- [ ] Create profile completion page after registration
- [ ] Gender selection (required)
- [ ] Sexual preference selection (required)
- [ ] Biography text field
- [ ] Birth date for age calculation
- [ ] Interest tags system
  - [ ] Create tag input with autocomplete
  - [ ] Store and retrieve user tags
  - [ ] Reusable tags across users

### Photo Management
- [ ] Photo upload endpoint (max 5 photos)
- [ ] File type validation (images only)
- [ ] File size limits
- [ ] Store photos in wwwroot/uploads
- [ ] Set primary profile photo
- [ ] Delete photo functionality

### Profile Editing
- [ ] Edit all profile fields
- [ ] Update email with re-verification
- [ ] Change password with old password confirmation

### Geolocation
- [ ] Request GPS permission on profile
- [ ] Get location from browser Geolocation API
- [ ] Fallback to IP-based location if GPS denied
- [ ] Store latitude/longitude in database
- [ ] Allow manual location adjustment
- [ ] Calculate distances between users

### Fame Rating
- [ ] Algorithm implementation:
  - [ ] Points for profile completeness
  - [ ] Points for number of likes received
  - [ ] Points for profile views
  - [ ] Points for matches made
- [ ] Display fame rating (0-5 stars)
- [ ] Update rating on relevant actions

## 🔍 3. Browsing & Matching

### Suggestion Algorithm
- [ ] Query users based on:
  - [ ] Compatible sexual orientation
  - [ ] Within location radius (prioritize proximity)
  - [ ] Common interest tags
  - [ ] Similar fame rating
- [ ] Handle bisexuality correctly
- [ ] Default to bisexual if preference not set
- [ ] Exclude blocked users
- [ ] Exclude already matched users

### Browse Page Features
- [ ] Display user cards with photo, name, age, distance
- [ ] Sorting options:
  - [ ] By age
  - [ ] By distance
  - [ ] By fame rating
  - [ ] By common tags
- [ ] Filtering options:
  - [ ] Age range slider
  - [ ] Distance radius
  - [ ] Fame rating range
  - [ ] Specific tags

### Advanced Search
- [ ] Create search page with criteria:
  - [ ] Age range (min-max)
  - [ ] Fame rating range
  - [ ] Location/distance
  - [ ] Multiple interest tags
- [ ] Display results with same sorting options
- [ ] Pagination for results

## ❤️ 4. Interaction Features

### Profile Viewing
- [ ] Detailed profile page showing:
  - [ ] All photos
  - [ ] Biography
  - [ ] Age
  - [ ] Location/distance
  - [ ] Interest tags
  - [ ] Fame rating
  - [ ] Online/offline status
  - [ ] Last seen (if offline)
- [ ] Record profile view in database
- [ ] Show view notification to viewed user

### Like System
- [ ] Like button on profiles (only with photo)
- [ ] Store like in database
- [ ] Check for mutual like → create match
- [ ] Send like notification
- [ ] Unlike functionality
- [ ] Remove match on unlike
- [ ] Update fame ratings

### Blocking & Reporting
- [ ] Block user button
- [ ] Remove from all searches/suggestions
- [ ] Prevent all interactions
- [ ] Report fake account button
- [ ] Store report with reason

## 💬 5. Real-time Features

### Chat System
- [ ] Create SignalR ChatHub
- [ ] Only allow chat between matched users
- [ ] Real-time message delivery (<10 seconds)
- [ ] Store messages in database
- [ ] Message read status
- [ ] Typing indicators
- [ ] Chat history retrieval
- [ ] New message notifications

### Notification System
- [ ] Create SignalR NotificationHub
- [ ] Real-time notifications (<10 seconds) for:
  - [ ] Profile liked
  - [ ] Profile viewed
  - [ ] New message
  - [ ] New match
  - [ ] User unliked you
- [ ] Notification badge/counter
- [ ] Mark as read functionality
- [ ] Notification history page

## 📱 6. UI/UX Requirements

### Responsive Design
- [ ] Mobile-first approach
- [ ] Test on various screen sizes
- [ ] Touch-friendly interface
- [ ] Responsive image galleries

### Browser Compatibility
- [ ] Test on latest Firefox
- [ ] Test on latest Chrome
- [ ] Fix any console errors
- [ ] No JavaScript errors allowed

### Layout Structure
- [ ] Consistent header on all pages
- [ ] Navigation menu
- [ ] Footer with links
- [ ] User menu with logout

## 🚀 7. Deployment & Testing

### Environment Setup
- [ ] Create .env file with all secrets
- [ ] Add .env to .gitignore
- [ ] Document all environment variables

### Docker Configuration
- [ ] Create Dockerfile for app
- [ ] Docker Compose with PostgreSQL
- [ ] Volume mapping for uploads
- [ ] Environment variable injection

### Testing Requirements
- [ ] Create 500+ test user profiles
- [ ] Test all security measures
- [ ] Load testing for real-time features
- [ ] Mobile responsiveness testing
- [ ] Cross-browser testing

## 📋 Implementation Priority Order

### Phase 1: Foundation (Must complete first)
1. PostgreSQL database setup
2. User registration with email verification
3. Login/logout with JWT
4. Basic profile creation

### Phase 2: Core Features
1. Profile photo upload
2. Browse page with suggestions
3. Like/unlike system
4. Match detection

### Phase 3: Real-time
1. SignalR setup
2. Chat between matches
3. Real-time notifications

### Phase 4: Advanced Features
1. Geolocation
2. Fame rating
3. Advanced search
4. Filtering and sorting

### Phase 5: Security & Polish
1. Security audit
2. Block/report system
3. Password recovery
4. Mobile optimization

## ⚠️ Common Pitfalls to Avoid

1. **Security**: Any security flaw = 0% grade
2. **Real-time delay**: Must be <10 seconds
3. **Manual queries**: Can't use full ORM
4. **Mobile**: Must be responsive
5. **Console errors**: Zero tolerance
6. **500 profiles**: Required for evaluation

## 🎯 Success Criteria

- ✅ All mandatory features implemented
- ✅ No security vulnerabilities
- ✅ Real-time features work (<10 sec)
- ✅ Mobile responsive
- ✅ No console errors
- ✅ 500+ user profiles in database
- ✅ Manual SQL queries used
- ✅ Works on Firefox & Chrome latest

## 📝 Notes

- Focus on functionality over design
- Security is the top priority
- Test everything thoroughly
- Keep the code clean and documented
- Follow the checklist in subject.md exactly