import { QueryResult } from "pg";
import { pool } from "../settings";

class ViewModel {
  private async getLastViewTime(
    userId: number,
    viewedUserId: number
  ): Promise<Date | null> {
    const query = {
      text: `SELECT last_viewed_at FROM views 
             WHERE viewer_user_id = $1 AND viewed_user_id = $2`,
      values: [userId, viewedUserId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows[0]?.last_viewed_at || null;
  }

  async getUserViews(userId: number): Promise<any[]> {
    const query = {
      text: `SELECT * FROM views WHERE viewed_user_id = $1 ORDER BY last_viewed_at DESC`,
      values: [userId],
    };
    const result: QueryResult = await pool.query(query);
    return result.rows;
  }

  async addUserView(userId: number, viewedUserId: number): Promise<boolean> {
    // Future improvement:
    // Add a rate limit to prevent a user from viewing another user's profile too frequently

    // Optimized query to prevent too many updates of view_count and last_viewed_at for a short period of time
    const query = {
      text: `
      WITH last_view AS (
        SELECT last_viewed_at 
        FROM views 
        WHERE viewer_user_id = $1 
        AND viewed_user_id = $2
      )
      INSERT INTO views (viewer_user_id, viewed_user_id, view_count, last_viewed_at)
      SELECT $1, $2, 1, NOW()
      WHERE NOT EXISTS (
        SELECT 1 FROM last_view 
        WHERE last_viewed_at > NOW() - INTERVAL '1 minute'
      )
      ON CONFLICT (viewer_user_id, viewed_user_id)
      DO UPDATE SET 
        view_count = CASE 
          WHEN views.last_viewed_at < NOW() - INTERVAL '1 minute'
          THEN views.view_count + 1 
          ELSE views.view_count 
        END,
        last_viewed_at = CASE 
          WHEN views.last_viewed_at < NOW() - INTERVAL '1 minute'
          THEN NOW() 
          ELSE views.last_viewed_at 
        END
    `,
      values: [userId, viewedUserId],
    };

    const result = await pool.query(query);
    if (result.rowCount === 0) {
      return false;
    } else {
      return true;
    }
  }
}

export const viewModel = new ViewModel();
