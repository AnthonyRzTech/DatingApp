import { QueryResult } from "pg";
import { pool } from "../settings";
import { Tag } from "@interfaces/tagInterface"

class TagModel {
  async getUserTags(userId: number): Promise<any[]> {
    const query = {
      text: `SELECT t.* FROM tags t JOIN user_tags ut ON t.id = ut.tag_id WHERE ut.user_id = $1`,
      values: [userId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async addUserTag(userId: number, tagId: number): Promise<any> {
    const query = {
      text: `INSERT INTO user_tags (user_id, tag_id)
         SELECT $1, $2
         WHERE NOT EXISTS (
           SELECT 1 FROM user_tags WHERE user_id = $1 AND tag_id = $2
         )`,
      values: [userId, tagId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows[0];
  }

  async deleteUserTag(userId: number, tagId: number): Promise<any> {
    const query = {
      text: `DELETE FROM user_tags WHERE user_id = $1 AND tag_id = $2`,
      values: [userId, tagId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async getAllTags(): Promise<Tag[]> {
    const query = {
        text: `Select * FROM tags`,
        values: [] as any[]
    }
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

}

export const tagModel = new TagModel();
