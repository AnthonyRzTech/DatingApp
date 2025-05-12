import { Image } from "./interfaces/imageInterface";
import { Gender } from "./interfaces/userInterface";
import { image } from "./models/imageModel";
import { pool } from "./settings";
import fetch from "node-fetch"; // For older versions of Node.js
import { generateHash } from "./utils/bcrypt";
import { user } from "./models/userModel";

interface ApiUser {
  first_name: string;
  last_name: string;
  username: string;
  password: string;
  email: string;
  gender: string;
  preferences: string[];
  date_of_birth: string;
  bio: string;
  location: {
    latitude: number;
    longitude: number;
    city: string;
  };
  image_url: string;
}

interface RandomUserResponse {
  results: {
    name: { first: string; last: string };
    login: { username: string; password: string };
    email: string;
    gender: string;
    dob: { date: string };
    location: {
      city: string;
      coordinates: { latitude: string; longitude: string };
    };
    picture: { large: string };
    // Other fields are ignored
  }[];
  // Other fields are ignored
}

/**
 * Fetches random user data from a public API
 * Here we use the randomuser.me API version 1.4
 *
 * @param url - The base URL of the random user API (defaults to "https://randomuser.me/api/")
 * @param quantitiy - The number of random users to fetch
 * @param seed - Optional seed for consistent random user generation
 * @returns Promise that resolves to an array of ApiUser objects
 * @throws {Error} When the API response is not OK
 *
 * @example
 * ```typescript
 * const users = await fetchRandomUsers("https://randomuser.me/api", 10);
 * ```
 *
 * @remarks
 * - The API call is restricted to users from France, Great Britain, Spain, and Germany
 * - Each user object includes randomly generated gender preferences
 * - Location coordinates are parsed from strings to numbers
 */
async function fetchRandomUsers(
  url: string = "https://randomuser.me/api/",
  quantitiy: number,
  seed: string = "foobar"
): Promise<ApiUser[]> {
  const url_api = `${url}?results=${quantitiy}&seed=${seed}`;
  const response = await fetch(url_api);

  // Check if the response is OK
  if (!response.ok) {
    throw new Error(
      `An error occurred during random user fetching: ${response.status}, ${response.statusText}`
    );
  }

  const data = (await response.json()) as RandomUserResponse;
  const users = data.results.map((user, i: number) => {
    return {
      first_name: user.name.first,
      last_name: user.name.last,
      username: user.login.username,
      password: user.login.password,
      email: user.email,
      gender: user.gender,
      preferences: (() => {
        const genders = [Gender.Male, Gender.Female, Gender.Other];
        const prefCount = Math.floor(Math.random() * 3) + 1; // Random number between 1 and 3
        const shuffled = [...genders].sort(() => 0.5 - Math.random());
        return shuffled.slice(0, prefCount);
      })(),
      date_of_birth: user.dob.date,
      bio: `Hi, I''m ${user.name.first}!`,
      location: {
        city: user.location.city || "Faraway",
        latitude: parseFloat(user.location.coordinates.latitude),
        longitude: parseFloat(user.location.coordinates.longitude),
      },
      image_url: user.picture.large,
    };
  });

  return users;
}

async function truncateAndInsertFixtures(shouldClosePool = false) {
  try {
    console.log("Starting database fixtures...");

    // Cleaning Tables content
    console.log("Cleaning existing data...");
    await pool.query("TRUNCATE TABLE chats RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE messages RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE users RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE images RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE likes RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE blocked RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE matches RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE user_tags RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE views RESTART IDENTITY CASCADE;");
    await pool.query("TRUNCATE TABLE reported_users RESTART IDENTITY CASCADE;");

    // Insert data into the `users` table
    console.log("Inserting users...");
    const insertUsersQuery = `
      INSERT INTO users (first_name, last_name, username, email, password, gender, preferences, date_of_birth, bio, location, fame_rate, last_seen, is_verified)
      VALUES
        ('John', 'Doe', 'johndoe', 'john@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"female"}', '1990-01-01', 'Bio of John', POINT(48.862842, 2.342815), 10, NOW(), true),
        ('Jane', 'Doe', 'janedoe', 'jane@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"female"}', '1992-02-02', 'Bio of Jane', POINT(48.853119, 2.349494), 20, NOW(), true),
        ('Alice', 'Smith', 'alicesmith', 'alice@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"male", "female", "other"}', '1995-05-05', 'Bio of Alice', POINT(37.7749, -122.4194), 30, NOW(), true),
        ('Bob', 'Smith', 'bobsmith', 'bobsmith@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'other', '{"male", "female"}', '1998-08-08', 'Bio of Bob', POINT(41.8781, -87.6298), 40, NOW(), true),
        ('Charlie', 'Brown', 'charliebrown', 'charliebrown@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"male"}', '2000-10-10', 'Bio of Charlie', POINT(29.7604, -95.3698), 50, NOW(), true),
        ('David', 'White', 'davidwhite', 'davidwhite@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"other"}', '2002-12-12', 'Bio of David', POINT(34.0522, -118.2437), 60, NOW(), true),
        ('Eve', 'Black', 'eveblack', 'eveblack@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"male"}', '1991-03-03', 'Bio of Eve', POINT(40.7128, -74.0060), 70, NOW(), true),
        ('Frank', 'Green', 'frankgreen', 'frankgreen@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"female"}', '1989-04-04', 'Bio of Frank', POINT(34.0522, -118.2437), 80, NOW(), true),
        ('Grace', 'Blue', 'graceblue', 'graceblue@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"male", "female"}', '1993-06-06', 'Bio of Grace', POINT(51.5074, -0.1278), 90, NOW(), true),
        ('Hank', 'Yellow', 'hankyellow', 'hankyellow@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"female", "other"}', '1994-07-07', 'Bio of Hank', POINT(35.6895, 139.6917), 100, NOW(), true),
        ('Ivy', 'Red', 'ivyred', 'ivyred@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"male"}', '1996-09-09', 'Bio of Ivy', POINT(48.8566, 2.3522), 110, NOW(), true),
        ('Jack', 'White', 'jackwhite', 'jackwhite@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"female"}', '1988-11-11', 'Bio of Jack', POINT(40.7306, -73.9352), 120, NOW(), true),
        ('Karen', 'Black', 'karenblack', 'karenblack@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"male", "other", "female"}', '1997-12-12', 'Bio of Karen', POINT(34.0522, -118.2437), 130, NOW(), true),
        ('Leo', 'Green', 'leogreen', 'leogreen@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"female"}', '1999-01-01', 'Bio of Leo', POINT(51.5074, -0.1278), 140, NOW(), true),
        ('Mia', 'Blue', 'miablue', 'miablue@example.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'female', '{"male"}', '1993-02-02', 'Bio of Mia', POINT(35.6895, 139.6917), 150, NOW(), true),
        ('Test', 'User', 'test', 'test@test.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"male", "other", "female"}', '1995-05-05', 'Bio of Test', POINT(48.853119, 2.349494), 20, NOW(), true),
        ('Server', 'User', 'server', 'server@test.com', '$2b$10$twwxqovQ5nYrbcYKJ35qOOuaXpPApgnk90cdu6irUC.tKQiBV7v0G', 'male', '{"male", "other", "female"}', '1995-05-05', 'Bio of Server', POINT(48.853119, 2.349494), 20, NOW(), true)
        ;
    `;
    await pool.query(insertUsersQuery);

    // Insert loads of users for testing
    console.log("Fetching random users...");
    const users = await fetchRandomUsers("https://randomuser.me/api/", 500);
    console.log("Creating random users (can take a while)...");
    for (const user of users) {
      const query = `
        INSERT INTO users (first_name, last_name, username, email, password, gender, preferences, date_of_birth, bio, city, location)
        VALUES ($1, $2, $3, $4, $5, $6, $7::Gender[], $8, $9, $10, POINT($11, $12))
      `;
      const hashedPassword = await generateHash(user.password);
      const values = [
        user.first_name,
        user.last_name,
        user.username,
        user.email,
        hashedPassword,
        user.gender,
        user.preferences,
        user.date_of_birth,
        user.bio,
        user.location.city,
        user.location.latitude,
        user.location.longitude,
      ];
      await pool.query(query, values);
      process.stdout.write(`\r${users.indexOf(user) + 1} / ${users.length}`);
    }
    console.log("\nRandom users created!");

    // Insert data into the `images` table
    console.log("Inserting images...");
    const insertImagesQuery = `
      INSERT INTO images (user_id, image_url, is_profile)
      VALUES
      (1, 'https://randomuser.me/api/portraits/men/1.jpg', TRUE),
      (2, 'https://randomuser.me/api/portraits/women/1.jpg', TRUE),
      (3, 'https://randomuser.me/api/portraits/women/2.jpg', TRUE),
      (4, 'https://randomuser.me/api/portraits/men/2.jpg', TRUE),
      (5, 'https://randomuser.me/api/portraits/men/3.jpg', TRUE),
      (6, 'https://randomuser.me/api/portraits/men/6.jpg', TRUE),
      (7, 'https://randomuser.me/api/portraits/women/3.jpg', TRUE),
      (8, 'https://randomuser.me/api/portraits/men/4.jpg', TRUE),
      (9, 'https://randomuser.me/api/portraits/women/4.jpg', TRUE),
      (10, 'https://randomuser.me/api/portraits/men/5.jpg', TRUE),
      (11, 'https://randomuser.me/api/portraits/women/5.jpg', TRUE),
      (12, 'https://randomuser.me/api/portraits/men/7.jpg', TRUE),
      (13, 'https://randomuser.me/api/portraits/women/7.jpg', TRUE),
      (14, 'https://randomuser.me/api/portraits/men/8.jpg', TRUE),
      (15, 'https://randomuser.me/api/portraits/women/8.jpg', TRUE);
    `;
    await pool.query(insertImagesQuery);
    for (const user of users) {
      const query = `
        INSERT INTO images (user_id, image_url, is_profile)
        VALUES ($1, $2, $3)`;
      const values = [users.indexOf(user) + 17, user.image_url, true];
      await pool.query(query, values);
    }

    // Insert data into the `user_tags` table
    console.log("Inserting user tags...");
    const insertUsersTagsQuery = `
      INSERT INTO user_tags (user_id, tag_id)
      VALUES
        (1, 30),
        (1, 7),
        (1, 8),
        (2, 2),
        (2, 5),
        (3, 3),
        (4, 4),
        (5, 5);
    `;
    await pool.query(insertUsersTagsQuery);

    // Insert data into the `likes` table
    console.log("Inserting likes...");
    const insertLikesQuery = `
      INSERT INTO likes (liker_user_id, liked_user_id)
      VALUES
        (16, 2),
        (2, 16),
        (3, 16),
        (4, 16);
    `;
    await pool.query(insertLikesQuery);

    // Insert data into the `matches` table
    console.log("Inserting matches...");
    const insertMatchesQuery = `
      INSERT INTO matches (user_id1, user_id2)
      VALUES
        (2, 16);
    `;
    await pool.query(insertMatchesQuery);

    // Insert data into the `chats` table
    console.log("Inserting chats...");
    const insertChatsQuery = `
      INSERT INTO chats (user1_id, user2_id, deleted_by)
      VALUES
        (16, 2, '{}');
    `;
    await pool.query(insertChatsQuery);

    // Insert data into the `msgs` table
    console.log("Inserting messages...");
    const insertMsgsQuery = `
      INSERT INTO messages (content, chat_id, from_user_id, created_at)
      VALUES
      ('Hello, how are you?', 1, 2, TIMESTAMP '2023-10-01 10:00:00'),
      ('I am fine, thank you.', 1, 16, TIMESTAMP '2023-10-01 10:01:00'),
      ('I''m looking for a long-term contract.', 1, 2, TIMESTAMP '2023-10-01 10:02:00'),
      ('Sounds good. Send me your resume.', 1, 16, TIMESTAMP '2023-10-01 10:03:00'),
      ('I''m an excellent cook.', 1, 2, TIMESTAMP '2023-10-01 10:04:00'),
      ('I love to eat. Can you cook for me?', 1, 16, TIMESTAMP '2023-10-01 10:05:00'),
      ('It depends on what you want to eat.', 1, 2, TIMESTAMP '2023-10-01 10:06:00'),
      ('I love Italian food.', 1, 16, TIMESTAMP '2023-10-01 10:07:00'),
      ('I''m not Italian, but I can cook Italian food.', 1, 2, TIMESTAMP '2023-10-01 10:08:00'),
      ('Nothing fishy, I hope.', 1, 16, TIMESTAMP '2023-10-01 10:09:00'),
      ('No, I don''t like fish.', 1, 2, TIMESTAMP '2023-10-01 10:10:00');
    `;
    await pool.query(insertMsgsQuery);

    console.log("Fixtures completed successfully!");
  } catch (error) {
    console.error("Error inserting fixtures:", error);
    throw error;
  } finally {
    if (shouldClosePool) {
      await pool.end();
    }
  }
}

// Execute if running directly
if (require.main === module) {
  truncateAndInsertFixtures(true)
    .then(() => process.exit(0))
    .catch((error) => {
      console.error(error);
      process.exit(1);
    });
}

export { truncateAndInsertFixtures };
