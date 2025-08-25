-- Create a test user for debugging
-- Password: test123 (hashed with BCrypt)

-- First, insert the user
INSERT INTO users (username, email, first_name, last_name, birth_date, gender, sexual_preference, biography, interest_tags, profile_photo_url, photo_urls, latitude, longitude, fame_rating, is_online, last_seen, is_email_verified, is_active, created_at)
VALUES (
    'testuser',
    'testuser@example.com',
    'Test',
    'User',
    '1990-01-01',
    'male',
    'female',
    'This is a test user for debugging',
    '[]'::jsonb,
    '/images/default-avatar.png',
    '[]'::jsonb,
    48.8566,
    2.3522,
    50,
    false,
    NOW(),
    true,
    true,
    NOW()
) ON CONFLICT (username) DO NOTHING;

-- Insert the password
INSERT INTO user_passwords (user_id, password_hash, created_at)
SELECT id, '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewKyNiGSzLmFXfbe', NOW()
FROM users 
WHERE username = 'testuser'
ON CONFLICT (user_id) DO UPDATE SET password_hash = EXCLUDED.password_hash;