import { QueryResult } from "pg";
import { pool } from "../settings";

class LikeModel {
  async checkUserLiked(likerId: number, likedId: number): Promise<any> {
    const query = {
      text: `SELECT * FROM likes WHERE liker_user_id = $1 AND liked_user_id = $2`,
      values: [likerId, likedId],
    };

    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async getUserLikes(userId: number): Promise<any[]> {
    const query = {
      text: `SELECT * FROM likes WHERE liked_user_id = $1`,
      values: [userId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async addUserLike(likerUserId: number, likedUserId: number): Promise<any> {
    const query = {
      text: `INSERT INTO likes (liker_user_id, liked_user_id)
            SELECT $1, $2
            WHERE NOT EXISTS (
              SELECT 1 FROM likes WHERE liker_user_id = $1 AND liked_user_id = $2
            )
            RETURNING *
            `,
      values: [likerUserId, likedUserId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async deleteUserLike(likerUserId: number, likedUserId: number): Promise<any> {
    const query = {
      text: `DELETE FROM likes WHERE liker_user_id = $1 AND liked_user_id = $2`,
      values: [likerUserId, likedUserId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }
}

export const likeModel = new LikeModel();
