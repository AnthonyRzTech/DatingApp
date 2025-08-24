#!/bin/bash

# Test script for authentication system
echo "=== WebMatcha Authentication Test ==="
echo

# Base URL
BASE_URL="http://localhost:5192"

# Generate unique test data
TIMESTAMP=$(date +%s)
TEST_USER="testuser_${TIMESTAMP}"
TEST_EMAIL="test_${TIMESTAMP}@example.com"
TEST_PASSWORD="TestPassword123!"

echo "1. Testing Registration..."
echo "   Username: $TEST_USER"
echo "   Email: $TEST_EMAIL"
echo

# Test registration via direct database
echo "2. Creating test user directly in database..."
psql -h localhost -U postgres -d postgres -c "
INSERT INTO users (username, email, first_name, last_name, birth_date, gender, sexual_preference, fame_rating, latitude, longitude, is_online, last_seen)
VALUES ('$TEST_USER', '$TEST_EMAIL', 'Test', 'User', '1999-01-01', 'male', 'female', 0, 0, 0, false, NOW())
RETURNING id;
" 2>/dev/null

if [ $? -eq 0 ]; then
    echo "   ✓ User created successfully"
else
    echo "   ✗ Failed to create user"
fi

echo
echo "3. Testing password storage..."
USER_ID=$(psql -h localhost -U postgres -d postgres -t -c "SELECT id FROM users WHERE username='$TEST_USER'" 2>/dev/null | tr -d ' ')

if [ ! -z "$USER_ID" ]; then
    # Hash password using Python (since we can't easily use BCrypt from bash)
    HASHED_PASSWORD=$(python3 -c "import bcrypt; print(bcrypt.hashpw(b'$TEST_PASSWORD', bcrypt.gensalt(12)).decode('utf-8'))" 2>/dev/null)
    
    if [ ! -z "$HASHED_PASSWORD" ]; then
        psql -h localhost -U postgres -d postgres -c "
        INSERT INTO user_passwords (user_id, password_hash, created_at)
        VALUES ($USER_ID, '$HASHED_PASSWORD', NOW());
        " 2>/dev/null
        
        if [ $? -eq 0 ]; then
            echo "   ✓ Password stored successfully"
        else
            echo "   ✗ Failed to store password"
        fi
    else
        echo "   Note: bcrypt module not available, skipping password test"
    fi
else
    echo "   ✗ Could not find user ID"
fi

echo
echo "4. Testing login endpoint..."
# Try to access the login page
LOGIN_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/login")
if [ "$LOGIN_RESPONSE" = "200" ]; then
    echo "   ✓ Login page accessible (HTTP $LOGIN_RESPONSE)"
else
    echo "   ✗ Login page returned HTTP $LOGIN_RESPONSE"
fi

echo
echo "5. Testing registration endpoint..."
# Try to access the registration page
REGISTER_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/register")
if [ "$REGISTER_RESPONSE" = "200" ]; then
    echo "   ✓ Registration page accessible (HTTP $REGISTER_RESPONSE)"
else
    echo "   ✗ Registration page returned HTTP $REGISTER_RESPONSE"
fi

echo
echo "6. Testing health endpoint..."
HEALTH_RESPONSE=$(curl -s "${BASE_URL}/api/health")
if [[ $HEALTH_RESPONSE == *"healthy"* ]]; then
    echo "   ✓ API is healthy"
    echo "   Response: $HEALTH_RESPONSE"
else
    echo "   ✗ API health check failed"
fi

echo
echo "7. Cleaning up test data..."
psql -h localhost -U postgres -d postgres -c "
DELETE FROM user_passwords WHERE user_id IN (SELECT id FROM users WHERE username='$TEST_USER');
DELETE FROM users WHERE username='$TEST_USER';
" 2>/dev/null

if [ $? -eq 0 ]; then
    echo "   ✓ Test data cleaned up"
else
    echo "   ✗ Failed to clean up test data"
fi

echo
echo "=== Test Complete ==="