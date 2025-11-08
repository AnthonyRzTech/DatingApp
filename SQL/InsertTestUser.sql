-- Script pour ajouter un utilisateur de test (SMTP non configuré)
-- Username: testuser
-- Password: Test123!

-- Supprimer l'utilisateur s'il existe déjà
DELETE FROM users WHERE username = 'testuser' OR email = 'test@example.com';

-- Insérer l'utilisateur de test avec email déjà vérifié
INSERT INTO users (
    username,
    email,
    first_name,
    last_name,
    birth_date,
    gender,
    sexual_preference,
    biography,
    interest_tags,
    profile_photo_url,
    photo_urls,
    latitude,
    longitude,
    fame_rating,
    is_email_verified,
    is_active,
    is_online,
    last_seen,
    created_at
) VALUES (
    'testuser',
    'test@example.com',
    'Test',
    'User',
    '1995-01-01',
    'male',
    'bisexual',
    'Utilisateur de test pour développement local. Profil créé automatiquement.',
    '',      -- Tags vides pour l'instant
    '',      -- Pas de photo de profil
    '',      -- Pas de photos supplémentaires
    48.8566, -- Paris latitude
    2.3522,  -- Paris longitude
    50,
    true,    -- Email déjà vérifié (pas besoin de SMTP)
    true,    -- Compte actif
    false,   -- Pas en ligne par défaut
    NOW(),
    NOW()
);

-- Insérer le mot de passe dans la table user_passwords
INSERT INTO user_passwords (
    user_id,
    password_hash,
    created_at
) VALUES (
    (SELECT id FROM users WHERE username = 'testuser'),
    '$2a$12$LQyUBJVatJAw8lNfbQGIVuVoHkyFtHusp5MzEvi5HHYf5Cxtqwvuy', -- BCrypt hash pour "Test123!" (workfactor 12)
    NOW()
);

-- Afficher l'utilisateur créé
SELECT
    id,
    username,
    email,
    first_name,
    last_name,
    is_email_verified,
    is_active,
    created_at
FROM users
WHERE username = 'testuser';

-- Message de confirmation
DO $$
BEGIN
    RAISE NOTICE 'Utilisateur de test créé avec succès !';
    RAISE NOTICE 'Username: testuser';
    RAISE NOTICE 'Password: Test123!';
    RAISE NOTICE 'Email vérifié: OUI (is_email_verified = true)';
END $$;
