import { QueryResult } from "pg";
import { pool } from "../settings";
import { Image } from "@interfaces/imageInterface";

class ImageModel {
  async createImage(params: Partial<Image>): Promise<Image> {
    const query = {
      text: "INSERT INTO images (user_id, image_url) VALUES ($1, $2) RETURNING *",
      values: [params.userId, params.url],
    };

    try {
      const result: QueryResult<Image> = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getProfileImageByUserId(userId: number): Promise<Image> {
    const query = {
      text: "SELECT * FROM images WHERE user_id = $1 AND is_profile = true",
      values: [userId],
    };

    try {
      const result: QueryResult<any> = await pool.query(query);
      if (result.rows.length === 0) {
        return {
          id: -1,
          userId: userId,
          url: "",
          isProfilePic: false,
        };
      } else {
        const data = result.rows[0];
        return {
          id: data.id,
          userId: data.user_id,
          url: data.image_url,
          isProfilePic: data.is_profile,
        };
      }
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async getImagesByUserId(userId: number): Promise<Image[]> {
    const query = {
      text: "SELECT * FROM images WHERE user_id = $1",
      values: [userId],
    };

    try {
      const result: QueryResult<Image> = await pool.query(query);
      return result.rows;
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async deleteImageById(imageId: number): Promise<Image> {
    const query = {
      text: "DELETE FROM images WHERE id = $1 RETURNING *",
      values: [imageId],
    };

    try {
      const result: QueryResult<Image> = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }

  async updateImageStatus(
    imageId: number,
    isProfilePic: boolean
  ): Promise<Image> {
    const query = {
      text: "UPDATE images SET is_profile = $1 WHERE id = $2 RETURNING *",
      values: [isProfilePic, imageId],
    };

    try {
      const result: QueryResult<Image> = await pool.query(query);
      return result.rows[0];
    } catch (error) {
      throw new Error((error as Error).message);
    }
  }
}

export const image = new ImageModel();
