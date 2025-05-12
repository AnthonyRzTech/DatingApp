import { Pool, QueryResult } from "pg";
import dotenv from "dotenv";
import { promises as fs } from "fs";
import { truncate } from "fs";
import { validTags } from "./interfaces/tagInterface";
import path from "path";
import { truncateAndInsertFixtures } from "./seed";

dotenv.config();

export const NODE_ENV = process.env.NODE_ENV || "development";

export const FRONTEND_ORIGIN =
  process.env.FRONTEND_URL || "http://localhost:3000";
export const SERVER_PORT = process.env.SERVER_PORT || 8000;
export const JWT_SECRET_KEY = process.env.JWT_SECRET_KEY || "default secret";
export const ACCESSTOKEN_EXPIRES_IN: number =
  parseInt(process.env.ACCESS_TOKEN_EXPIRES_IN!) || 60; // in minutes
export const REFRESHTOKEN_EXPIRES_IN: number =
  parseInt(process.env.REFRESH_TOKEN_EXPIRES_IN!) || 7; // in days

const db_config = {
  user: process.env.POSTGRES_USER as string,
  host: process.env.POSTGRES_HOST as string,
  database: process.env.POSTGRES_DB as string,
  password: process.env.POSTGRES_PASSWORD as string,
  port: parseInt(process.env.DB_PORT as string, 10),
};

const pool = new Pool(db_config);

export interface DbQuery {
  query: <T extends QueryResult = any>(
    text: string,
    params?: any[]
  ) => Promise<QueryResult<T>>;
}

export const db: DbQuery = {
  query: <T extends QueryResult = any>(
    text: string,
    params?: any[]
  ): Promise<QueryResult<T>> => pool.query(text, params),
};

export { pool };

async function seed() {
  try {
    await pool.connect();
    console.log("Connected to the database.");

    const dropTablesQuery = `
      DROP TABLE IF EXISTS user_tags CASCADE;
      DROP TABLE IF EXISTS refresh_tokens CASCADE;
      DROP TABLE IF EXISTS matches CASCADE;
      DROP TABLE IF EXISTS views CASCADE;
      DROP TABLE IF EXISTS likes CASCADE;
      DROP TABLE IF EXISTS blocked CASCADE;
      DROP TABLE IF EXISTS chats CASCADE;
      DROP TABLE IF EXISTS messages CASCADE;
      DROP TABLE IF EXISTS reported_users CASCADE;
      DROP TABLE IF EXISTS users CASCADE;
    `;
    // await pool.query(dropTablesQuery);

    // Drop existing enum types if they exist
    const dropEnumTypesQuery = `
      DROP TYPE IF EXISTS gender CASCADE;
      DROP TYPE IF EXISTS preferences CASCADE;
    `;
    // await pool.query(dropEnumTypesQuery);

    /*INTERESTS TAGS TABLE*/
    const createInterestsTagTableQuery = `
        CREATE TABLE IF NOT EXISTS tags (
          id INTEGER PRIMARY KEY,
          tag VARCHAR(100) UNIQUE NOT NULL
        );
      `;
    await pool.query(createInterestsTagTableQuery);

    const insertTagsQuery = `
        INSERT INTO tags (id, tag)
        VALUES
          ${validTags.map((tag) => `(${tag.id}, '${tag.tag}')`).join(",")}
        ON CONFLICT (id) DO UPDATE 
        SET tag = EXCLUDED.tag;
      `;
    await pool.query(insertTagsQuery);

    /*USERS TABLE*/
    const setupEnumTypes = `
      -- Handle gender type
      DO $$ 
      BEGIN
        CREATE TYPE gender AS ENUM ('male', 'female', 'other');
      EXCEPTION
        WHEN duplicate_object THEN
          NULL;
      END $$;
    `;
    await pool.query(setupEnumTypes);

    const createTableQuery = `
      CREATE TABLE IF NOT EXISTS users (
        id SERIAL PRIMARY KEY,
        first_name VARCHAR(100) NOT NULL,
        last_name VARCHAR(100) NOT NULL,
        username VARCHAR(100) UNIQUE NOT NULL,
        email VARCHAR(255) UNIQUE NOT NULL,
        password VARCHAR(255) NOT NULL,
        gender gender DEFAULT 'other',
        preferences gender[] DEFAULT ARRAY['other']::gender[],
        date_of_birth DATE,
        bio VARCHAR(500),
        location POINT,
        city VARCHAR(100),
        fame_rate INT DEFAULT 0,
        last_seen TIMESTAMP,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        is_verified BOOLEAN DEFAULT FALSE,
        verification_token VARCHAR(255),
        token_password VARCHAR(255)
      );
    `;

    await pool.query(createTableQuery);

    /*USERS TAGS TABLE*/

    const createUserTagsTableQuery = `
      CREATE TABLE IF NOT EXISTS user_tags (
        user_id INTEGER REFERENCES users(id) ON DELETE CASCADE,
        tag_id INTEGER REFERENCES tags(id) ON DELETE CASCADE,
        PRIMARY KEY (user_id, tag_id)
      );
    `;
    await pool.query(createUserTagsTableQuery);

    const createMatchesTableQuery = `
      CREATE TABLE IF NOT EXISTS matches (
        id SERIAL PRIMARY KEY,
        user_id1 INTEGER REFERENCES users(id) ON DELETE CASCADE,
        user_id2 INTEGER REFERENCES users(id) ON DELETE CASCADE,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        UNIQUE(user_id1, user_id2)
      );
    `;
    await pool.query(createMatchesTableQuery);

    const createViewsTableQuery = `
      CREATE TABLE IF NOT EXISTS views (
        id SERIAL PRIMARY KEY,
        viewer_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        viewed_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        view_count INTEGER DEFAULT 1 NOT NULL CHECK (view_count > 0),
        last_viewed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
        UNIQUE(viewer_user_id, viewed_user_id),
        CHECK (viewer_user_id != viewed_user_id)
      );
    `;
    await pool.query(createViewsTableQuery);

    const createLikesTableQuery = `
      CREATE TABLE IF NOT EXISTS likes (
        id SERIAL PRIMARY KEY, 
        liker_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        liked_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
        UNIQUE(liker_user_id, liked_user_id),
        CHECK (liker_user_id != liked_user_id)
      );
    `;
    await pool.query(createLikesTableQuery);

    const createBlockedTableQuery = `
      CREATE TABLE IF NOT EXISTS blocked (
        id SERIAL PRIMARY KEY,
        blocker_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        blocked_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        UNIQUE(blocker_id, blocked_user_id)
      );
    `;
    await pool.query(createBlockedTableQuery);

    /*REPORTED USERS TABLE*/
    const createReportedUsersQuery = `
        CREATE TABLE IF NOT EXISTS reported_users (
            id SERIAL PRIMARY KEY,
            reporter_id INT REFERENCES users(id) ON DELETE CASCADE,
            reported_id INT REFERENCES users(id) ON DELETE CASCADE,
            reason VARCHAR(255) NOT NULL,
            reported_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UNIQUE (reporter_id, reported_id),
            CHECK (reporter_id != reported_id)
        );
    `;
    await pool.query(createReportedUsersQuery);

    /* IMAGES TABLE */
    const createImagesQuery = `
        CREATE TABLE IF NOT EXISTS images (
            id SERIAL PRIMARY KEY,
            user_id INT REFERENCES users(id) ON DELETE CASCADE,
            image_url bpchar NOT NULL,
            is_profile BOOLEAN DEFAULT FALSE
        );
    `;

    await pool.query(createImagesQuery);

    /* REFRESH TOKENS TABLE */
    const createRefreshTokensQuery = `
        CREATE TABLE IF NOT EXISTS refresh_tokens (
            id SERIAL PRIMARY KEY,
            user_id INT REFERENCES users(id) ON DELETE CASCADE UNIQUE,
            token VARCHAR(512) NOT NULL
        );
    `;
    await pool.query(createRefreshTokensQuery);

    /* __________________________ Chats Table ___________________________ */
    const chatsSchema = await fs.readFile(
      path.join(__dirname, "sql/chats.sql"),
      "utf-8"
    );
    await pool.query(chatsSchema);

    /* _______________________ Notifcations Table _______________________ */
    const notificationsSchema = await fs.readFile(
      path.join(__dirname, "sql/notifications.sql"),
      "utf-8"
    );
    await pool.query(notificationsSchema);

    // Insert users (if needed)
    await initDatabase();
  } catch (err) {
    console.error("Error seeding the database:", err);
  } finally {
    // Close the database connection
    // await pool.release();
  }
}

/**
 * Postgres extensions
 *
 * Enable the cube and earthdistance extensions
 * @see https://www.postgresql.org/docs/17/contrib-cube.html
 * @see https://www.postgresql.org/docs/17/earthdistance.html
 */
async function enableExtensions() {
  try {
    await pool.query("CREATE EXTENSION IF NOT EXISTS cube;");
    await pool.query("CREATE EXTENSION IF NOT EXISTS earthdistance;");
    console.log("Postgresql Extensions enabled successfully.");
  } catch (error) {
    console.error("Error enabling extensions:", error);
  }
}

// Enable the extensions
enableExtensions();

// Seed users and users images to the database (when < 500 users)
async function initDatabase() {
  try {
    console.log("Starting database initialization check...");
    const result = await pool.query("SELECT COUNT(*) FROM users");
    const userCount = parseInt(result.rows[0].count, 10);
    console.log(`Current user count: ${userCount}`);

    if (userCount < 500) {
      await truncateAndInsertFixtures();
      console.log("Database seeded successfully.");
    } else {
      console.log("Database already seeded. No action taken.");
    }
  } catch (error) {
    console.error("Error checking user count:", error);
  }
}

// Run the seed function
seed();
