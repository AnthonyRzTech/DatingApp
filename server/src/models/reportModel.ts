import { pool } from "@src/settings";

class ReportModel {
  async reportUser(
    reportUserId: number,
    reportedUserId: number,
    reason: string
  ) {
    try {
      const query = {
        text: `
          INSERT INTO reported_users (reporter_id, reported_id, reason)
          VALUES ($1, $2, $3)
          ON CONFLICT (reporter_id, reported_id) DO NOTHING
          RETURNING *
        `,
        values: [reportUserId, reportedUserId, reason],
      };
      const result = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getUserReport(userId: number) {
    try {
      const query = {
        text: `
          SELECT COUNT(*) AS report_count
          FROM reported_users
          WHERE reported_id = $1
        `,
        values: [userId],
      };
      const result = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }
}

export const report = new ReportModel();
