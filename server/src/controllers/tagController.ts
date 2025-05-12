import { Request, Response } from "express";
import {
  handleErrorResponse,
  handleBadRequestResponse,
  handleForbiddenResponse,
  handleNotFoundResponse,
} from "@utils/errorHandler";
import { tagModel } from "@models/tagModel";
import { TagSchema } from "@interfaces/tagValidationSchema";

export const getUserTags = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = parseInt(req.params.id);
    const tags = await tagModel.getUserTags(userId!);
    res.status(200).json({ status: 200, data: tags });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const addUserTag = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    if (userId !== parseInt(req.params.id)) {
      handleForbiddenResponse(
        res,
        "You are not allowed to add tags to this user"
      );
      return;
    }

    const result = TagSchema.safeParse(req.body);
    if (!result.success) {
      handleBadRequestResponse(res, "Invalid tag ID format");
      return;
    }
    const { tagId } = result.data;
    const tag = await tagModel.addUserTag(userId!, tagId);
    res.status(201).json({ status: 201, message: "Tag added", data: tag });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const deleteUserTag = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    if (userId !== parseInt(req.params.id)) {
      handleForbiddenResponse(res, "You are not allowed to delete this tag");
      return;
    }

    const result = TagSchema.safeParse(req.body);
    if (!result.success) {
      handleBadRequestResponse(res, "Invalid tag ID format");
      return;
    }
    const { tagId } = result.data;
    const tag = await tagModel.deleteUserTag(userId!, tagId);
    res.status(200).json({ status: 200, message: "Tag deleted", data: tag });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const getAllTags = async (req: Request,
    res: Response): Promise<void> => {
    try {
        const tags = await tagModel.getAllTags();
        res.status(200).json(tags)
    } catch (error) {
        handleErrorResponse(res, error);

    }
}