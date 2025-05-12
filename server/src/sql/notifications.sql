-- Notification objects and recipients tables 

-- Create enum for notifications to save space and enforce valid values
DO $$ 
BEGIN
    CREATE TYPE notification_status AS ENUM ('PENDING', 'SENT', 'FAILED');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ 
BEGIN
    CREATE TYPE notification_type AS ENUM ('like', 'view', 'match', 'unlike', 'message');
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

-- Notification objects table
CREATE TABLE IF NOT EXISTS notification_objects (
    id SERIAL PRIMARY KEY,
    type notification_type NOT NULL,
    content JSONB NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_notification_objects_type ON notification_objects(type);
CREATE INDEX IF NOT EXISTS idx_notification_objects_created_at ON notification_objects(created_at);

-- Notification recipients table
CREATE TABLE IF NOT EXISTS notification_recipients (
    id SERIAL PRIMARY KEY,
    to_user_id BIGINT NOT NULL,
    from_user_id BIGINT NOT NULL,
    notification_object_id BIGINT NOT NULL,
    status notification_status NOT NULL DEFAULT 'PENDING',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    read_at TIMESTAMP,
    is_read BOOLEAN DEFAULT FALSE,
    -- Add constraints
    CONSTRAINT fk_recipient FOREIGN KEY (to_user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_actor FOREIGN KEY (from_user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_notification_object FOREIGN KEY (notification_object_id) 
        REFERENCES notification_objects(id) ON DELETE CASCADE
);
-- Add indexes for common queries
CREATE INDEX IF NOT EXISTS idx_notification_recipients_notifier ON notification_recipients(to_user_id);
CREATE INDEX IF NOT EXISTS idx_notification_recipients_status ON notification_recipients(status);
CREATE INDEX IF NOT EXISTS idx_notification_recipients_read ON notification_recipients(is_read);
CREATE INDEX IF NOT EXISTS idx_notification_recipients_actor ON notification_recipients(from_user_id);