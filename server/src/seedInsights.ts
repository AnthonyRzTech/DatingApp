import { pool } from "./settings";

async function truncateAndInsertFixtures(shouldClosePool = false) {
  try {
    console.log("Starting database insights fixtures...");

    // Cleaning Insights Tables content
    console.log("Cleaning existing data...");
    await pool.query("TRUNCATE TABLE views RESTART IDENTITY CASCADE;");

    // Insert data into the `views` table
    console.log("Inserting views...");
    const insertViewsQuery = `
      INSERT INTO views (viewer_user_id, viewed_user_id)
      VALUES
        (1, 16),
        (2, 16),
        (3, 16),
        (4, 16),
        (5, 16),
        (6, 16),
        (7, 16),
        (8, 16),
        (9, 16),
        (10, 16),
        (11, 16),
        (12, 16),
        (13, 16),
        (14, 16),
        (15, 16);
    `;
    await pool.query(insertViewsQuery);

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
