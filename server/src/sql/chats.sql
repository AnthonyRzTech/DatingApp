
-- Create chats tables
CREATE TABLE IF NOT EXISTS chats (
    id SERIAL PRIMARY KEY,
    user1_id INT REFERENCES users(id) ON DELETE CASCADE,
    user2_id INT REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    deleted_by INT[],
    -- Add constraints
    CONSTRAINT fk_user1 FOREIGN KEY (user1_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_user2 FOREIGN KEY (user2_id)
        REFERENCES users(id) ON DELETE CASCADE
);


-- Create messages table
CREATE TABLE IF NOT EXISTS messages (
    id SERIAL PRIMARY KEY,
    content VARCHAR(1000) NOT NULL,
    chat_id INT REFERENCES chats(id) ON DELETE CASCADE,
    from_user_id INT REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_read BOOLEAN DEFAULT FALSE,
    -- Add constraints
    CONSTRAINT fk_chat FOREIGN KEY (chat_id) 
        REFERENCES chats(id) ON DELETE CASCADE,
    CONSTRAINT fk_user FOREIGN KEY (from_user_id)
        REFERENCES users(id) ON DELETE CASCADE
);
