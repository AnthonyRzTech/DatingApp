import { QueryResult } from "pg";
import { pool } from "../settings";
import { chatModel } from "./chatModel";
import { PublicUser } from "@src/interfaces/userInterface";

class MatchModel {
  async getUserMatches(userId: number): Promise<any[]> {
    // TODO: add tags in the query
    const query = {
      text: `SELECT * FROM (
                SELECT us.id, us.first_name, us.last_name, us.email, us.username, us.gender, us.preferences, us.date_of_birth, us.bio, us.city, us.fame_rate, us.last_seen, us.created_at, ma.id AS match_id, ma.created_at AS match_date
                FROM users AS us
                JOIN matches AS ma
                ON us.id = ma.user_id1
                WHERE ma.user_id2 = $1
                UNION
                SELECT us.id, us.first_name, us.last_name, us.email, us.username, us.gender, us.preferences, us.date_of_birth, us.bio, us.city, us.fame_rate, us.last_seen, us.created_at, ma.id AS match_id, ma.created_at AS match_date
                FROM users AS us
                JOIN matches AS ma
                ON us.id = ma.user_id2
                WHERE ma.user_id1 = $1
              ) AS matched_users
            ORDER BY match_date DESC`,
      values: [userId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async addUserMatch(userId: number, matchedUserId: number): Promise<any> {
    const query = {
      text: `INSERT INTO matches (user_id1, user_id2)
            SELECT $1, $2
            WHERE NOT EXISTS (
              SELECT 1 FROM matches WHERE user_id1 = $1 AND user_id2 = $2)
            AND $1 <> $2
            AND NOT EXISTS (
              SELECT 1 FROM matches WHERE user_id1 = $2 AND user_id2 = $1
            )
            RETURNING *
            `,
      values: [userId, matchedUserId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async checkForMatch(
    likerUserId: number,
    likedUserId: number
  ): Promise<boolean> {
    // Maybe: Better check if the user has already liked the other user

    // Check if the liked user has also liked the liker user
    const query = {
      text: `SELECT (liker_user_id, liked_user_id) FROM likes WHERE liker_user_id = $1 AND liked_user_id = $2`,
      values: [likedUserId, likerUserId],
    };
    const result: QueryResult = await pool.query(query);
    // If the liked user has also liked the liker user, create a match
    if (result.rows.length > 0) {
      await this.addUserMatch(likerUserId, likedUserId);
      return true;
    }
    return false;
  }

  async removeUserMatch(userId: number, matchedUserId: number): Promise<any> {
    const query = {
      text: `DELETE FROM matches WHERE (user_id1 = $1 AND user_id2 = $2) OR (user_id1 = $2 AND user_id2 = $1)`,
      values: [userId, matchedUserId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }
}

export const matchModel = new MatchModel();
