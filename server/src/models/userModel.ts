import { QueryResult } from "pg";
import { pool } from "../settings";
import {
  User,
  PublicUser,
  SortParams,
  Gender,
} from "@interfaces/userInterface";
import { generateHash } from "@utils/bcrypt";
import { UserSearchQuery } from "@interfaces/userSearchQuery";
import { generateVerificationToken } from "@src/utils/jwt";

class UserModel {
  async getUsers(params: SortParams | undefined): Promise<User[]> {
    let query = {
      text: "SELECT * FROM users",
    };

    try {
      const results: QueryResult<User> = await pool.query(query);
      return results.rows;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getClientUserById(id: number): Promise<PublicUser | null> {
    try {
      const query = {
        text: `
          SELECT 
            u.id,
            u.first_name,
            u.last_name, 
            u.username,
            u.gender,
            COALESCE(
              array_to_json(
                array_remove(
                  array_remove(
                    string_to_array(regexp_replace(u.preferences::text, '[{}]', '', 'g'), ','),
                    ''
                  ),
                  NULL
                )
              ),
              '[]'::json
            ) as preferences,
            u.date_of_birth,
            u.bio,
            u.location,
            u.city,
            u.fame_rate,
            u.last_seen,
            COALESCE(array_to_json(ARRAY_AGG(t.tag) FILTER (WHERE t.tag IS NOT NULL)), '[]'::json) as tags
          FROM users u
          LEFT JOIN user_tags ut ON u.id = ut.user_id
          LEFT JOIN tags t ON ut.tag_id = t.id
          WHERE u.id = $1
          GROUP BY u.id
          `,
        values: [id],
      };
      const result: QueryResult<PublicUser> = await pool.query(query);
      return result.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getUsernameById(id: number): Promise<string | null> {
    try {
      const query = {
        text: "SELECT username FROM users WHERE id = $1",
        values: [id],
      };
      const result: QueryResult = await pool.query(query);
      return result.rows[0]?.username || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getUserById(id: number): Promise<User | null> {
    try {
      const query = {
        text: `
          SELECT 
            u.id,
            u.first_name,
            u.last_name, 
            u.username,
            u.email,
            u.gender,
            COALESCE(
              array_to_json(
                array_remove(
                  array_remove(
                    string_to_array(regexp_replace(u.preferences::text, '[{}]', '', 'g'), ','),
                    ''
                  ),
                  NULL
                )
              ),
              '[]'::json
            ) as preferences,
            u.date_of_birth,
            u.bio,
            u.location,
            u.city,
            u.fame_rate,
            u.last_seen,
            COALESCE(array_to_json(ARRAY_AGG(t.tag) FILTER (WHERE t.tag IS NOT NULL)), '[]'::json) as tags
          FROM users u
          LEFT JOIN user_tags ut ON u.id = ut.user_id
          LEFT JOIN tags t ON ut.tag_id = t.id
          WHERE u.id = $1
          GROUP BY u.id
        `,
        values: [id],
      };
      const results: QueryResult<User> = await pool.query(query);
      return results.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getUserByEmail(email: string | undefined): Promise<User | null> {
    try {
      const query = {
        text: "SELECT * FROM users WHERE email = $1",
        values: [email],
      };
      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getUserByUsername(username: string | undefined): Promise<User | null> {
    try {
      const query = {
        text: "SELECT * FROM users WHERE username = $1",
        values: [username],
      };
      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async createUser(userData: Partial<User>): Promise<User> {
    try {
      const password = await generateHash(userData.password);
      const verificationToken = userData.verificationToken;

      const query = {
        text: `
			  INSERT INTO users (first_name, last_name, username, email, password, verification_token, is_verified)
			  VALUES ($1, $2, $3, $4, $5, $6, $7)
			  RETURNING id, first_name, last_name, username, email, verification_token, is_verified
			`,
        values: [
          userData.firstName,
          userData.lastName,
          userData.username,
          userData.email,
          password,
          verificationToken,
          false,
        ],
      };
      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async updateUser(id: number, userData: Partial<User>): Promise<User> {
    try {
      const updates: string[] = [];
      const values: any[] = [];
      let parameterIndex = 1;

      for (const [key, value] of Object.entries(userData)) {
        if (value !== null && value !== undefined) {
          const snakeKey = key.replace(
            /[A-Z]/g,
            (letter) => `_${letter.toLowerCase()}`
          );
          if (
            snakeKey === "password" ||
            snakeKey === "id" ||
            snakeKey === "created_at"
          ) {
            continue;
          }
          updates.push(`${snakeKey} = $${parameterIndex}`);
          values.push(value);
          parameterIndex++;
        }
      }

      if (updates.length === 0) {
        throw new Error("No valid fields to update");
      }

      values.push(id);

      const query = {
        text: `
          UPDATE users
          SET ${updates.join(", ")}, updated_at = NOW()
          WHERE id = $${parameterIndex}
          RETURNING id, first_name, last_name, username, email, gender, preferences, date_of_birth, bio, location, city, fame_rate, last_seen, created_at, updated_at
        `,
        values,
      };

      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async updateUserFameRate(id: number, fameRate: number): Promise<void> {
    try {
      const query = {
        text: `
          UPDATE users
          SET fame_rate = $1
          WHERE id = $2 RETURNING fame_rate
        `,
        values: [fameRate, id],
      };
      await pool.query(query);
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async updateLastSeen(id: number): Promise<void> {
    try {
      const query = {
        text: `
          UPDATE users
          SET last_seen = NOW()
          WHERE id = $1
        `,
        values: [id],
      };
      await pool.query(query);
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getUserByVerificationToken(token: string): Promise<User | null> {
    try {
      const query = {
        text: "SELECT * FROM users WHERE verification_token = $1",
        values: [token],
      };
      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async verifyEmailQuery(token: string): Promise<User | null> {
    try {
      // First, check if the token exists and get user details
      const checkQuery = {
        text: `
          SELECT id, is_verified, verification_token 
          FROM users 
          WHERE verification_token = $1
        `,
        values: [token],
      };
      
      const checkResult = await pool.query(checkQuery);
      console.log("Check token result:", checkResult.rows[0]);
      
      if (!checkResult.rows[0]) {
        console.log("No user found with token:", token);
        return null;
      }
      
      if (checkResult.rows[0].is_verified) {
        console.log("User already verified:", checkResult.rows[0].id);
        const userQuery = {
          text: `
            SELECT id, first_name, last_name, username, email, is_verified
            FROM users
            WHERE id = $1
          `,
          values: [checkResult.rows[0].id],
        };
        const userResult = await pool.query(userQuery);
        return userResult.rows[0];
      }
      
      const updateQuery = {
        text: `
          UPDATE users
          SET is_verified = true, verification_token = NULL
          WHERE verification_token = $1 AND is_verified = false
          RETURNING id, first_name, last_name, username, email, is_verified
        `,
        values: [token],
      };
      
      const result = await pool.query(updateQuery);
      return result.rows[0] || null;
    } catch (error) {
      console.error("Error in verifyEmail:", error);
      throw new Error((error as Error).message);
    }
  }

  async setResetToken(
    email: string,
    token: string
  ): Promise<User | null> {
    try {
      const query = {
        text: `
          UPDATE users
          SET token_password = $1
          WHERE email = $2
        `,
        values: [token, email],
      };
      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async resetPassword(
    token: string,
    newPassword: string,
  ): Promise<User | null> {
    try {
      const hashedPassword = await generateHash(newPassword);
      const query = {
        text: `
          UPDATE users
          SET password = $1, token_password = NULL
          WHERE token_password = $2
          RETURNING id, username, email
        `,
        values: [hashedPassword, token],
      };
      const result: QueryResult<User> = await pool.query(query);
      return result.rows[0] || null;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  /**
   * VIEWS
   */
  async getUserViews(userId: number): Promise<any[]> {
    const query = {
      text: `SELECT * FROM views WHERE viewer_id = $1`,
      values: [userId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }
  async addUserView(
    viewer_user_id: number,
    viewedUserId: number
  ): Promise<any> {
    const query = {
      text: `INSERT INTO views (viewer_user_id, viewed_user_id) VALUES ($1, $2)`,
      values: [viewer_user_id, viewedUserId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async searchUsers(
    params: UserSearchQuery,
    excludeUserId: number
  ): Promise<PublicUser[]> {
    try {
      const conditions: string[] = [];
      const values: any[] = [];
      let parameterIndex = 1;

      // Base query with age calculation
      let query = `
        SELECT DISTINCT u.id, u.first_name, u.last_name, u.username, u.gender, u.preferences,
        u.date_of_birth, u.bio, u.location::text, u.city, u.fame_rate, u.last_seen,
        ARRAY_AGG(t.tag) AS tags,
        EXTRACT(YEAR FROM AGE(NOW(), u.date_of_birth)) as age
      `;

      // Add distance calculation if coordinates provided
      if (
        params.distance !== undefined &&
        !isNaN(params.distance) &&
        params.latitude !== undefined &&
        !isNaN(params.latitude) &&
        params.longitude !== undefined &&
        !isNaN(params.longitude)
      ) {
        query += `,
          earth_distance(
            ll_to_earth(u.location[0], u.location[1]),
            ll_to_earth($${parameterIndex}, $${parameterIndex + 1})
          ) / 1000 AS distance
        `;
        values.push(params.latitude, params.longitude);
        parameterIndex += 2;
      }

      query += ` FROM users u
        LEFT JOIN user_tags ut ON u.id = ut.user_id
        LEFT JOIN tags t ON ut.tag_id = t.id
      `;

      // Remove self from search results
      if (excludeUserId) {
        conditions.push(`u.id != $${parameterIndex}`);
        values.push(excludeUserId);
        parameterIndex++;
      }

      // Add conditions
      if (params.tags && params.tags.length > 0) {
        conditions.push(
          `t.tag IN (${params.tags
            .map((_, i) => `$${parameterIndex + i}`)
            .join(", ")})`
        );
        values.push(...params.tags);
        parameterIndex += params.tags.length;
      }

      if (params.gender !== undefined) {
        conditions.push(`u.gender = ANY($${parameterIndex}::gender[])`);
        values.push(params.sexualPreferences);
        parameterIndex++;

        // Add condition to match users whose preferences include the searcher's gender
        conditions.push(`$${parameterIndex} = ANY(u.preferences)`);
        values.push(params.gender);
        parameterIndex++;
      }

      if (params.ageMin !== undefined && !isNaN(params.ageMin)) {
        conditions.push(
          `EXTRACT(YEAR FROM AGE(NOW(), u.date_of_birth)) >= $${parameterIndex}`
        );
        values.push(params.ageMin);
        parameterIndex++;
      }

      if (params.ageMax !== undefined && !isNaN(params.ageMax)) {
        conditions.push(
          `EXTRACT(YEAR FROM AGE(NOW(), u.date_of_birth)) <= $${parameterIndex}`
        );
        values.push(params.ageMax);
        parameterIndex++;
      }

      if (params.minFameRating !== undefined && !isNaN(params.minFameRating)) {
        conditions.push(`u.fame_rate >= $${parameterIndex}`);
        values.push(params.minFameRating);
        parameterIndex++;
      }

      if (params.maxFameRating !== undefined && !isNaN(params.maxFameRating)) {
        conditions.push(`u.fame_rate <= $${parameterIndex}`);
        values.push(params.maxFameRating);
        parameterIndex++;
      }

      if (
        params.distance !== undefined &&
        !isNaN(params.distance) &&
        params.latitude !== undefined &&
        !isNaN(params.latitude) &&
        params.longitude !== undefined &&
        !isNaN(params.longitude)
      ) {
        conditions.push(`
          earth_distance(
            ll_to_earth(u.location[0], u.location[1]),
            ll_to_earth($${parameterIndex}, $${parameterIndex + 1})
          ) / 1000 <= $${parameterIndex + 2}`);
        values.push(params.latitude, params.longitude, params.distance);
        parameterIndex += 3;
      }

      if (conditions.length > 0) {
        query += ` WHERE ${conditions.join(" AND ")}`;
      }

      query += ` GROUP BY u.id`;

      if (params.tags && params.tags.length > 0) {
        query += ` HAVING COUNT(DISTINCT t.tag) = ${params.tags.length}`;
      }

      if (params.sortBy) {
        const order = params.order || "asc";
        switch (params.sortBy) {
          case "distance":
            if (params.latitude && params.longitude) {
              query += ` ORDER BY distance ${order}`;
            }
            break;
          case "age":
            query += ` ORDER BY age ${order}`;
            break;
          case "fameRating":
            query += ` ORDER BY u.fame_rate ${order}`;
            break;
        }
      }

      if (params.limit) {
        query += ` LIMIT $${parameterIndex}`;
        values.push(params.limit);
        parameterIndex++;
      }

      if (params.offset) {
        query += ` OFFSET $${parameterIndex}`;
        values.push(params.offset);
      }

      const result: QueryResult = await pool.query({
        text: query,
        values: values,
      });

      return result.rows.map((row) => ({
        id: row.id,
        firstName: row.first_name,
        lastName: row.last_name,
        username: row.username,
        gender: row.gender as Gender,
        preferences: row.preferences as Gender[],
        dateOfBirth: row.date_of_birth,
        bio: row.bio,
        city: row.city,
        fameRate: row.fame_rate,
        lastSeen: row.last_seen,
        tags: row.tags,
        age: row.age,
        distance: row.distance,
      })) as PublicUser[];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async omniSearchUsers(params: UserSearchQuery): Promise<PublicUser[]> {
    try {
      const conditions: string[] = [];
      const values: any[] = [];
      let parameterIndex = 1;

      // Base query with age calculation
      let query = `
        SELECT DISTINCT u.id, u.first_name, u.last_name, u.username, u.gender, u.preferences,
        u.date_of_birth, u.bio, u.location::text, u.city, u.fame_rate, u.last_seen,
        ARRAY_AGG(t.tag) AS tags,
        EXTRACT(YEAR FROM AGE(NOW(), u.date_of_birth)) as age
      `;

      // Add distance calculation if coordinates provided
      if (
        params.distance !== undefined &&
        !isNaN(params.distance) &&
        params.latitude !== undefined &&
        !isNaN(params.latitude) &&
        params.longitude !== undefined &&
        !isNaN(params.longitude)
      ) {
        query += `,
          earth_distance(
            ll_to_earth(u.location[0], u.location[1]),
            ll_to_earth($${parameterIndex}, $${parameterIndex + 1})
          ) / 1000 AS distance
        `;
        values.push(params.latitude, params.longitude);
        parameterIndex += 2;
      }

      query += ` FROM users u
        LEFT JOIN user_tags ut ON u.id = ut.user_id
        LEFT JOIN tags t ON ut.tag_id = t.id
      `;

      // Add conditions
      if (params.tags && params.tags.length > 0) {
        conditions.push(
          `t.tag IN (${params.tags
            .map((_, i) => `$${parameterIndex + i}`)
            .join(", ")})`
        );
        values.push(...params.tags);
        parameterIndex += params.tags.length;
      }

      if (params.gender !== undefined) {
        conditions.push(`u.gender = $${parameterIndex}`);
        values.push(params.gender);
        parameterIndex++;
      }

      if (
        params.sexualPreferences !== undefined &&
        params.sexualPreferences.length > 0
      ) {
        conditions.push(
          `u.preferences @> ARRAY[${params.sexualPreferences
            .map((_, i) => `$${parameterIndex + i}`)
            .join(", ")}]::gender[]`
        );
        values.push(...params.sexualPreferences);
        parameterIndex += params.sexualPreferences.length;
      }

      if (params.ageMin !== undefined && !isNaN(params.ageMin)) {
        conditions.push(
          `EXTRACT(YEAR FROM AGE(NOW(), u.date_of_birth)) >= $${parameterIndex}`
        );
        values.push(params.ageMin);
        parameterIndex++;
      }

      if (params.ageMax !== undefined && !isNaN(params.ageMax)) {
        conditions.push(
          `EXTRACT(YEAR FROM AGE(NOW(), u.date_of_birth)) <= $${parameterIndex}`
        );
        values.push(params.ageMax);
        parameterIndex++;
      }

      if (params.minFameRating !== undefined && !isNaN(params.minFameRating)) {
        conditions.push(`u.fame_rate >= $${parameterIndex}`);
        values.push(params.minFameRating);
        parameterIndex++;
      }

      if (params.maxFameRating !== undefined && !isNaN(params.maxFameRating)) {
        conditions.push(`u.fame_rate <= $${parameterIndex}`);
        values.push(params.maxFameRating);
        parameterIndex++;
      }

      if (
        params.distance !== undefined &&
        !isNaN(params.distance) &&
        params.latitude !== undefined &&
        !isNaN(params.latitude) &&
        params.longitude !== undefined &&
        !isNaN(params.longitude)
      ) {
        conditions.push(`
          earth_distance(
            ll_to_earth(u.location[0], u.location[1]),
            ll_to_earth($${parameterIndex}, $${parameterIndex + 1})
          ) / 1000 <= $${parameterIndex + 2}`);
        values.push(params.latitude, params.longitude, params.distance);
        parameterIndex += 3;
      }

      if (conditions.length > 0) {
        query += ` WHERE ${conditions.join(" AND ")}`;
      }

      query += ` GROUP BY u.id`;

      if (params.tags && params.tags.length > 0) {
        query += ` HAVING COUNT(DISTINCT t.tag) = ${params.tags.length}`;
      }

      if (params.sortBy) {
        const order = params.order || "asc";
        switch (params.sortBy) {
          case "distance":
            if (params.latitude && params.longitude) {
              query += ` ORDER BY distance ${order}`;
            }
            break;
          case "age":
            query += ` ORDER BY age ${order}`;
            break;
          case "fameRating":
            query += ` ORDER BY u.fame_rate ${order}`;
            break;
        }
      }

      if (params.limit) {
        query += ` LIMIT $${parameterIndex}`;
        values.push(params.limit);
        parameterIndex++;
      }

      if (params.offset) {
        query += ` OFFSET $${parameterIndex}`;
        values.push(params.offset);
      }

      const result: QueryResult = await pool.query({
        text: query,
        values: values,
      });

      return result.rows.map((row) => ({
        id: row.id,
        firstName: row.first_name,
        lastName: row.last_name,
        username: row.username,
        gender: row.gender as Gender,
        preferences: row.preferences as Gender[],
        dateOfBirth: row.date_of_birth,
        bio: row.bio,
        city: row.city,
        fameRate: row.fame_rate,
        lastSeen: row.last_seen,
        tags: row.tags,
        age: row.age,
        distance: row.distance,
      })) as PublicUser[];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }
}

export const user = new UserModel();
