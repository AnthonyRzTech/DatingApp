# WebMatcha - Comprehensive Testing Checklist

**Date:** 2025-11-14
**Project:** WebMatcha Dating Application
**Purpose:** Final testing before project submission

---

## 🚀 QUICK START

### Prerequisites
```bash
# 1. Ensure PostgreSQL is running
sudo service postgresql status
# If not running:
sudo service postgresql start

# 2. Create database (if not exists)
createdb -U postgres webmatcha

# 3. Start the application
dotnet run

# 4. Access at: https://localhost:7036
```

### Test Credentials
```
Username: demo
Password: Demo123!
```
✅ Email already verified, ready to use

---

## ✅ MANDATORY FEATURES TESTING (Subject Requirements)

### 📝 1. AUTHENTICATION & REGISTRATION

#### 1.1 Registration
- [ ] Go to `/register`
- [ ] Fill all required fields:
  - [ ] First name
  - [ ] Last name
  - [ ] Username (alphanumeric + underscore only)
  - [ ] Email (valid format)
  - [ ] Password (min 6 chars, uppercase, lowercase, number)
  - [ ] Confirm password
  - [ ] Birth date (must be 18+)
  - [ ] Gender (select from dropdown)
  - [ ] Sexual preference (select from dropdown)
- [ ] Submit form
- [ ] **Expected:** Success message, email verification prompt
- [ ] **Security Test:** Try password "password123" → Should be rejected (common password)
- [ ] **Security Test:** Try age < 18 → Should be rejected
- [ ] **Security Test:** Try duplicate username → Should show error
- [ ] **Security Test:** Try duplicate email → Should show error

#### 1.2 Email Verification
- [ ] Check server logs for verification token
- [ ] Copy the verification link from logs
- [ ] Go to `/verify-email?token={TOKEN}`
- [ ] **Expected:** "Email verified successfully" message
- [ ] Try logging in → Should work now

#### 1.3 Login
- [ ] Go to `/login`
- [ ] Enter username: `demo`
- [ ] Enter password: `Demo123!`
- [ ] Click "Login"
- [ ] **Expected:** Redirect to home/browse page
- [ ] **Security Test:** Try wrong password → Should show error
- [ ] **Security Test:** Try non-existent username → Should show error
- [ ] Check session cookie is set (browser dev tools)

#### 1.4 Password Reset
- [ ] Go to `/forgot-password`
- [ ] Enter email address
- [ ] Check server logs for reset token
- [ ] Go to `/reset-password?token={TOKEN}`
- [ ] Enter new password
- [ ] **Expected:** Password reset successful
- [ ] Try logging in with new password → Should work

#### 1.5 Logout
- [ ] Click "Logout" button (in navigation)
- [ ] **Expected:** Redirect to home page
- [ ] Try accessing protected pages → Should redirect to login

---

### 👤 2. USER PROFILE

#### 2.1 View Own Profile
- [ ] Login as demo user
- [ ] Go to `/profile/edit` or click "Edit Profile"
- [ ] Verify all fields are displayed:
  - [ ] First name, Last name
  - [ ] Email
  - [ ] Gender, Sexual preference
  - [ ] Biography
  - [ ] Interest tags (comma-separated)
  - [ ] Photos (up to 5)
  - [ ] Geolocation (latitude/longitude)

#### 2.2 Edit Profile
- [ ] Modify biography
- [ ] Add interest tags (e.g., "vegan,geek,music")
- [ ] Click "Save Changes"
- [ ] **Expected:** Profile updated successfully
- [ ] Reload page → Changes should persist
- [ ] **Security Test:** Try XSS in biography: `<script>alert('xss')</script>`
  - [ ] **Expected:** Script should be sanitized/encoded

#### 2.3 Upload Photos
- [ ] Click "Upload Photo"
- [ ] Select a valid image (JPG/PNG/GIF/WEBP, max 5MB)
- [ ] **Expected:** Photo uploaded successfully
- [ ] Try uploading 6th photo → Should show error (max 5)
- [ ] **Security Test:** Try uploading non-image file → Should be rejected
- [ ] **Security Test:** Try uploading file > 5MB → Should be rejected
- [ ] Set one photo as profile photo
- [ ] **Expected:** Profile photo updated

#### 2.4 Geolocation
- [ ] Check if geolocation is set (should default to Paris area)
- [ ] If browser supports GPS, allow location access
- [ ] **Expected:** Latitude/longitude updated
- [ ] **Fallback:** If GPS denied, manual location or IP-based should work

#### 2.5 Fame Rating
- [ ] Check fame rating is visible on profile
- [ ] **Expected:** Rating between 0-100 (displayed as stars)
- [ ] Fame should increase with:
  - [ ] Profile views
  - [ ] Likes received
  - [ ] Messages received

#### 2.6 Profile Visibility
- [ ] Go to another user's profile (e.g., `/profile/2`)
- [ ] **Expected:** Can see:
  - [ ] Name, age, photos
  - [ ] Biography, interest tags
  - [ ] Fame rating (as stars)
  - [ ] Online status (green dot if online)
  - [ ] Last seen (if offline)
- [ ] **Expected:** CANNOT see:
  - [ ] Email address
  - [ ] Password

---

### 🔍 3. BROWSING & SEARCH

#### 3.1 Browse Suggestions
- [ ] Go to `/browse`
- [ ] **Expected:** List of users displayed
- [ ] Verify suggestions are based on:
  - [ ] Sexual orientation compatibility
  - [ ] Geographic proximity (distance shown in km)
  - [ ] Common interest tags
  - [ ] Fame rating
- [ ] **Expected:** At least 10-20 profiles visible (from 500 seeded)

#### 3.2 Filters
- [ ] Use "Age Range" filter
  - [ ] Drag slider (18-100)
  - [ ] Click "Apply Filters"
  - [ ] **Expected:** Only users in age range shown
- [ ] Use "Max Distance" filter
  - [ ] Set to 10km
  - [ ] **Expected:** Only nearby users shown
- [ ] Use "Min Fame Rating" filter
  - [ ] Set to 3 stars
  - [ ] **Expected:** Only users with fame ≥ 60 shown

#### 3.3 Sorting
- [ ] Sort by "Distance"
  - [ ] **Expected:** Closest users first
- [ ] Sort by "Age"
  - [ ] **Expected:** Youngest users first
- [ ] Sort by "Fame Rating"
  - [ ] **Expected:** Highest rated users first

#### 3.4 Advanced Search
- [ ] Go to `/search`
- [ ] Enter search criteria:
  - [ ] Age range: 25-35
  - [ ] Fame rating: 50-100
  - [ ] Location: Within 20km
  - [ ] Interest tags: "music" or "geek"
- [ ] Click "Search"
- [ ] **Expected:** Filtered results matching ALL criteria
- [ ] Try different combinations of filters

---

### 👁️ 4. PROFILE VIEWING & INTERACTIONS

#### 4.1 View Profile
- [ ] Click on a user card in browse
- [ ] **Expected:** Redirect to `/profile/{userId}`
- [ ] Verify all profile info is visible
- [ ] **Expected:** View is recorded in database
- [ ] Check profile_views table:
  ```sql
  SELECT * FROM profile_views WHERE viewer_id = {your_id} ORDER BY viewed_at DESC LIMIT 5;
  ```

#### 4.2 View History
- [ ] Check "Who viewed my profile" section
- [ ] **Expected:** List of users who viewed your profile
- [ ] Verify timestamps are correct

#### 4.3 Like User
- [ ] Go to another user's profile
- [ ] Click "Like" button (heart icon)
- [ ] **Expected:** Heart icon fills in
- [ ] **Expected:** Notification sent to liked user
- [ ] Check likes table:
  ```sql
  SELECT * FROM likes WHERE liker_id = {your_id};
  ```

#### 4.4 Match (Mutual Like)
- [ ] Like a user (User A)
- [ ] Login as another user (or use API to simulate)
- [ ] Have User A like you back
- [ ] **Expected:** Match is created automatically
- [ ] **Expected:** Both users receive "New Match" notification
- [ ] **Expected:** Match appears in `/matches` page
- [ ] Check matches table:
  ```sql
  SELECT * FROM matches WHERE user1id = {userId} OR user2id = {userId};
  ```

#### 4.5 Unlike User
- [ ] Go to a profile you liked
- [ ] Click "Unlike" button
- [ ] **Expected:** Like removed
- [ ] **Expected:** If was a match, match is deleted
- [ ] **Expected:** "Unlike" notification sent
- [ ] Verify in database

#### 4.6 Block User
- [ ] Go to a user's profile
- [ ] Click "Block" button
- [ ] **Expected:** User is blocked
- [ ] Verify blocked user:
  - [ ] Does NOT appear in browse
  - [ ] Does NOT appear in search
  - [ ] Cannot send you messages
  - [ ] Cannot see your profile
- [ ] Check blocks table:
  ```sql
  SELECT * FROM blocks WHERE blocker_id = {your_id};
  ```

#### 4.7 Report User
- [ ] Go to a user's profile
- [ ] Click "Report as Fake Account"
- [ ] **Expected:** Report is recorded
- [ ] Check reports table:
  ```sql
  SELECT * FROM reports WHERE reporter_id = {your_id};
  ```

---

### 💬 5. REAL-TIME CHAT

#### 5.1 Access Chat
- [ ] Go to `/chat`
- [ ] **Expected:** List of conversations with matches
- [ ] **Expected:** Can ONLY chat with matched users (mutual likes)

#### 5.2 Send Message
- [ ] Click on a conversation
- [ ] Type a message
- [ ] Click "Send"
- [ ] **Expected:** Message appears immediately (< 1 second)
- [ ] **Expected:** Message saved to database
- [ ] Check messages table:
  ```sql
  SELECT * FROM messages WHERE sender_id = {your_id} ORDER BY sent_at DESC LIMIT 10;
  ```

#### 5.3 Receive Message (Real-time)
- [ ] **Test:** Open chat in 2 browser windows (2 different users who are matched)
- [ ] Send message from User A
- [ ] **Expected:** User B receives message in < 10 seconds (subject requirement)
- [ ] **Expected:** Unread message indicator appears
- [ ] **Expected:** Notification sent

#### 5.4 Message History
- [ ] Open a conversation
- [ ] **Expected:** Previous messages are loaded
- [ ] **Expected:** Messages sorted by time (oldest first)
- [ ] Scroll up
- [ ] **Expected:** Can see full conversation history

#### 5.5 Online Status
- [ ] Check if user status shows "Online" or "Last seen X minutes ago"
- [ ] **Test:** Open app in new browser
- [ ] **Expected:** Status updates to "Online" within seconds
- [ ] Close browser
- [ ] **Expected:** Status updates to "Offline" within seconds

---

### 🔔 6. REAL-TIME NOTIFICATIONS

#### 6.1 Notification Types
Test that notifications are sent (< 10 seconds) for:

- [ ] **Like Received**
  - [ ] Have another user like your profile
  - [ ] **Expected:** Notification appears in notification center
  - [ ] **Expected:** Badge count increases

- [ ] **Profile View**
  - [ ] Have another user view your profile
  - [ ] **Expected:** Notification appears

- [ ] **New Match**
  - [ ] Complete a mutual like (match)
  - [ ] **Expected:** Both users get match notification

- [ ] **New Message**
  - [ ] Receive a message from matched user
  - [ ] **Expected:** Notification appears
  - [ ] **Expected:** Unread count updates

- [ ] **Unlike**
  - [ ] Have a user unlike you
  - [ ] **Expected:** Notification sent
  - [ ] **Expected:** Match removed if applicable

#### 6.2 Notification Center
- [ ] Go to `/notifications`
- [ ] **Expected:** List of all notifications
- [ ] **Expected:** Unread notifications highlighted
- [ ] Click "Mark as Read"
- [ ] **Expected:** Notification marked as read
- [ ] **Expected:** Badge count decreases

#### 6.3 Real-time Delivery
- [ ] **Test:** Open app in 2 browsers (2 users)
- [ ] User A likes User B
- [ ] **Expected:** User B receives notification in < 10 seconds
- [ ] Verify notification appears without page refresh

---

### 🔒 7. SECURITY TESTING (CRITICAL - 0% IF FAIL)

#### 7.1 Password Security
- [ ] Check passwords are hashed in database:
  ```sql
  SELECT password_hash FROM user_passwords LIMIT 5;
  ```
  - [ ] **Expected:** All passwords start with `$2a$` or `$2b$` (BCrypt)
  - [ ] **Expected:** NO plain text passwords

- [ ] Try registering with common passwords:
  - [ ] "password" → Should be REJECTED
  - [ ] "123456" → Should be REJECTED
  - [ ] "qwerty" → Should be REJECTED
  - [ ] "letmein" → Should be REJECTED

#### 7.2 SQL Injection Protection
- [ ] Try SQL injection in username:
  - [ ] Username: `admin' OR '1'='1`
  - [ ] **Expected:** Sanitized or rejected
- [ ] Try SQL injection in search:
  - [ ] Search term: `'; DROP TABLE users; --`
  - [ ] **Expected:** No error, no damage to database

#### 7.3 XSS Protection
- [ ] Try XSS in biography:
  - [ ] `<script>alert('XSS')</script>`
  - [ ] `<img src=x onerror=alert('XSS')>`
  - [ ] `<iframe src="http://evil.com"></iframe>`
  - [ ] **Expected:** All sanitized, no script execution

- [ ] Try XSS in interest tags:
  - [ ] `<script>alert('XSS')</script>,music,travel`
  - [ ] **Expected:** Script removed, safe tags kept

#### 7.4 CSRF Protection
- [ ] Check session cookies in browser dev tools
- [ ] **Expected:** Cookie has:
  - [ ] `SameSite=Strict`
  - [ ] `HttpOnly=true`
  - [ ] `Secure=true` (in HTTPS mode)

#### 7.5 File Upload Validation
- [ ] Try uploading PHP file renamed as .jpg
  - [ ] **Expected:** Rejected (magic number check)
- [ ] Try uploading 10MB image
  - [ ] **Expected:** Rejected (max 5MB)
- [ ] Try uploading HTML file
  - [ ] **Expected:** Rejected (MIME type check)
- [ ] Upload valid JPEG
  - [ ] **Expected:** Accepted and saved

#### 7.6 Authorization Checks
- [ ] Try accessing another user's profile edit page:
  - [ ] Go to `/profile/edit` (should only edit YOUR profile)
  - [ ] **Expected:** Cannot edit other users' profiles
- [ ] Try deleting another user's photo
  - [ ] **Expected:** Rejected (ownership check)

#### 7.7 Session Security
- [ ] Login
- [ ] Check session in dev tools
- [ ] Copy session cookie
- [ ] Open incognito window
- [ ] Try using copied session cookie from different IP
  - [ ] **Expected:** Session invalidated (IP/User-Agent validation)

#### 7.8 Security Headers
- [ ] Open browser dev tools → Network tab
- [ ] Check response headers:
  - [ ] `X-Content-Type-Options: nosniff` ✓
  - [ ] `X-Frame-Options: DENY` ✓
  - [ ] `X-XSS-Protection: 1; mode=block` ✓
  - [ ] `Referrer-Policy: strict-origin-when-cross-origin` ✓

---

### 📊 8. DATABASE REQUIREMENTS

#### 8.1 500 Profiles Minimum
```bash
curl http://localhost:5192/api/users/count
```
- [ ] **Expected:** `{"userCount": 500}` or more

Or check database directly:
```sql
SELECT COUNT(*) FROM users;
```
- [ ] **Expected:** 500+

#### 8.2 Manual SQL Queries
- [ ] Open any service file (e.g., `Services/UserService.cs`)
- [ ] Verify NO Entity Framework queries:
  - [ ] ❌ NO `_dbContext.Users.Where(...)`
  - [ ] ❌ NO `_dbContext.SaveChanges()`
  - [ ] ✅ YES `connection.QueryAsync<User>(sql, parameters)`
  - [ ] ✅ YES `const string sql = @"SELECT..."`

#### 8.3 Database Schema
```bash
# Connect to PostgreSQL
psql -U postgres -d webmatcha

# List all tables
\dt

# Expected 11 tables:
# - users
# - user_passwords
# - likes
# - matches
# - messages
# - notifications
# - profile_views
# - blocks
# - reports
# - email_verifications
# - password_resets
```

#### 8.4 Indexes Optimization
```sql
# Check indexes exist
SELECT indexname FROM pg_indexes WHERE schemaname = 'public' ORDER BY indexname;
```
- [ ] **Expected:** 60+ indexes created
- [ ] Check critical indexes:
  - [ ] `idx_users_username`
  - [ ] `idx_users_email`
  - [ ] `idx_likes_both`
  - [ ] `idx_matches_user1_user2`
  - [ ] `idx_messages_conversation`

---

### 🎨 9. UI & COMPATIBILITY

#### 9.1 Responsive Design
- [ ] Open app in browser
- [ ] Open Dev Tools (F12)
- [ ] Toggle device toolbar (mobile view)
- [ ] Test on:
  - [ ] iPhone SE (375x667)
  - [ ] iPad (768x1024)
  - [ ] Desktop (1920x1080)
- [ ] **Expected:** All pages render correctly
- [ ] **Expected:** No horizontal scrolling
- [ ] **Expected:** Buttons are clickable
- [ ] **Expected:** Forms are usable

#### 9.2 Browser Compatibility
- [ ] Test in **Firefox** (latest)
  - [ ] All features work
  - [ ] No console errors
- [ ] Test in **Chrome** (latest)
  - [ ] All features work
  - [ ] No console errors

#### 9.3 Console Errors
- [ ] Open browser Dev Tools → Console
- [ ] Navigate through all pages
- [ ] **Expected:** ❌ ZERO JavaScript errors
- [ ] **Expected:** ❌ ZERO warnings (except Blazor async warnings - those are OK)

#### 9.4 Server Errors
- [ ] Check server console logs
- [ ] Navigate through all pages
- [ ] **Expected:** ❌ ZERO exceptions
- [ ] **Expected:** ❌ ZERO error logs

#### 9.5 Page Layout
All pages should have:
- [ ] Header (navigation bar with logo, menu, logout button)
- [ ] Main content area
- [ ] Footer (optional but recommended)

---

### 🚀 10. PERFORMANCE & RELIABILITY

#### 10.1 Page Load Times
- [ ] Browse page loads in < 2 seconds
- [ ] Profile page loads in < 1 second
- [ ] Chat page loads in < 2 seconds
- [ ] Search results appear in < 3 seconds

#### 10.2 Real-time Latency
- [ ] Message delivery: < 1 second
- [ ] Notification delivery: < 10 seconds (subject requirement)
- [ ] Online status update: < 5 seconds

#### 10.3 Database Performance
```sql
EXPLAIN ANALYZE SELECT * FROM users WHERE username = 'demo';
-- Should use index scan, not sequential scan
```
- [ ] Verify indexes are being used (check "Index Scan" in EXPLAIN output)

---

## 🎯 CRITICAL CHECKLIST (MUST PASS)

These are the absolute requirements that result in **0% if failed**:

- [ ] ✅ **Passwords hashed** (BCrypt in database)
- [ ] ✅ **Common passwords rejected** (test with "password", "123456")
- [ ] ✅ **SQL injection protection** (all queries parameterized)
- [ ] ✅ **XSS protection** (InputSanitizer used, scripts removed)
- [ ] ✅ **CSRF protection** (SameSite=Strict cookies)
- [ ] ✅ **File upload validation** (MIME + magic numbers)
- [ ] ✅ **500 profiles minimum** in database
- [ ] ✅ **Manual SQL queries** (NO Entity Framework LINQ)
- [ ] ✅ **Real-time < 10 sec** (chat and notifications)
- [ ] ✅ **Mobile responsive** (Bootstrap, works on small screens)
- [ ] ✅ **No console errors** (JavaScript)
- [ ] ✅ **No server errors** (exceptions)
- [ ] ✅ **Matching logic** (orientation, distance, tags, fame)

---

## 📋 QUICK SMOKE TEST (5 Minutes)

If you're short on time, run this quick test:

1. [ ] Start app: `dotnet run`
2. [ ] Check user count: `curl http://localhost:5192/api/users/count` → Should be 500+
3. [ ] Login: Go to `/login`, use `demo` / `Demo123!`
4. [ ] Browse: Go to `/browse`, see user suggestions
5. [ ] Like: Click heart on a user
6. [ ] Chat: Go to `/chat`, send a message to a match
7. [ ] Check database:
   ```sql
   SELECT password_hash FROM user_passwords LIMIT 1;
   -- Should start with $2a$ or $2b$ (BCrypt)
   ```
8. [ ] Check console: NO errors

---

## 🐛 COMMON ISSUES & FIXES

### Issue: Database connection error
```bash
# Fix: Ensure PostgreSQL is running
sudo service postgresql start
createdb -U postgres webmatcha
```

### Issue: No users in database
```bash
# Fix: Trigger manual seeding
curl http://localhost:5192/api/seed
```

### Issue: Email verification not working
```
# Fix: Check server logs for verification token
# Token appears as: [CompleteAuthService] Email verification token: abc123...
# Use token in URL: /verify-email?token=abc123...
```

### Issue: Build errors
```bash
# Fix: Clean and rebuild
dotnet clean
dotnet build --no-incremental
```

---

## ✅ FINAL VALIDATION

Before submitting:

- [ ] All critical security tests pass
- [ ] 500+ profiles in database
- [ ] All pages accessible and functional
- [ ] Real-time features work (< 10 sec)
- [ ] Mobile responsive
- [ ] Zero console errors
- [ ] Zero server errors
- [ ] All mandatory features implemented
- [ ] README.md has clear installation instructions
- [ ] .env file excluded from Git
- [ ] Code is clean and well-commented

---

## 📞 SUPPORT

If any test fails:
1. Check server console logs
2. Check browser console (F12)
3. Check PostgreSQL logs
4. Review CLAUDE.md for implementation details
5. Review security patterns in InputSanitizer.cs and CompleteAuthService.cs

---

**Good luck with your project defense! 🚀**
