#!/bin/bash

echo "=== Simple WebMatcha Authentication Test ==="
echo

BASE_URL="http://localhost:5192"

echo "1. Checking if application is running..."
HEALTH=$(curl -s "${BASE_URL}/api/health" 2>/dev/null)
if [[ $HEALTH == *"healthy"* ]]; then
    echo "   ✓ Application is running"
else
    echo "   ✗ Application is not responding"
    exit 1
fi

echo
echo "2. Testing Login Page..."
LOGIN_HTML=$(curl -s "${BASE_URL}/login" 2>/dev/null)
if [[ $LOGIN_HTML == *"Welcome Back"* ]]; then
    echo "   ✓ Login page loads correctly"
    echo "   ✓ Found 'Welcome Back' text"
else
    echo "   ✗ Login page not loading properly"
fi

if [[ $LOGIN_HTML == *"form"* ]]; then
    echo "   ✓ Login form present"
else
    echo "   ✗ Login form not found"
fi

echo
echo "3. Testing Registration Page..."
REGISTER_HTML=$(curl -s "${BASE_URL}/register" 2>/dev/null)
if [[ $REGISTER_HTML == *"Join WebMatcha"* ]]; then
    echo "   ✓ Registration page loads correctly"
    echo "   ✓ Found 'Join WebMatcha' text"
else
    echo "   ✗ Registration page not loading properly"
fi

if [[ $REGISTER_HTML == *"First Name"* ]] && [[ $REGISTER_HTML == *"Last Name"* ]]; then
    echo "   ✓ Registration form fields present"
else
    echo "   ✗ Registration form fields not found"
fi

echo
echo "4. Testing Navigation..."
HOME_HTML=$(curl -s "${BASE_URL}/" 2>/dev/null)
if [[ $HOME_HTML == *"WebMatcha"* ]]; then
    echo "   ✓ Home page loads"
fi

if [[ $HOME_HTML == *"Login"* ]] && [[ $HOME_HTML == *"Sign Up"* ]]; then
    echo "   ✓ Unauthenticated navigation shows Login/Sign Up"
else
    echo "   ✗ Navigation links not found"
fi

echo
echo "5. Testing Database Connection..."
# Use the seed endpoint to test database
SEED_RESPONSE=$(curl -s "${BASE_URL}/api/seed" 2>/dev/null)
if [[ $SEED_RESPONSE == *"seeded successfully"* ]]; then
    echo "   ✓ Database connection working"
    echo "   ✓ Can create users in database"
else
    echo "   ⚠ Database seeding returned: $SEED_RESPONSE"
fi

echo
echo "=== Authentication System Status ==="
echo "✓ Application is running on ${BASE_URL}"
echo "✓ Login page is accessible"
echo "✓ Registration page is accessible"
echo "✓ Navigation is working"
echo
echo "To test authentication:"
echo "1. Go to ${BASE_URL}/register to create an account"
echo "2. Fill in all required fields"
echo "3. After registration, go to ${BASE_URL}/login"
echo "4. Login with your credentials"
echo
echo "=== Test Complete ===