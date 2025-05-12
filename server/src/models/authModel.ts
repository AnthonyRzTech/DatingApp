import { generateHash } from "@utils/bcrypt";
import { User } from "@interfaces/userInterface";
import { QueryResult } from "pg";
import { pool } from "../settings";

class AuthModel {
  async createRefreshToken(user: any): Promise<string> {
    try {
      const query = {
        text: `
		INSERT INTO refresh_tokens (user_id, token)
		VALUES ($1, $2)
		ON CONFLICT (user_id)
		DO UPDATE SET token = $2
		RETURNING token
		`,
        values: [user.id, user.token],
      };
      const result: QueryResult = await pool.query(query);
      return result.rows[0].token;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }
}

export const auth = new AuthModel();
