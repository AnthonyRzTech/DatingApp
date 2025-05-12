import { Request, Response } from "express";
import {
  handleBadRequestResponse,
  handleErrorResponse,
} from "@utils/errorHandler";
import { report as ReportModel } from "@src/models/reportModel";

export const reportUser = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = req?.user?.id;
    const reportedUserId = parseInt(req.params.id);
    if (userId === reportedUserId) {
      handleBadRequestResponse(res, "You cannot block yourself");
      return;
    }
    const result = await ReportModel.reportUser(
      userId!,
      reportedUserId,
      "Fake Account"
    );
    if (result === undefined) {
      res.status(200).json({
        status: 200,
        message: "User already reported",
        success: false,
      });
      return;
    }
    res.status(201).json({
      status: 201,
      success: true,
      message: "User reported successfully",
    });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};

export const getUserReport = async (
  req: Request,
  res: Response
): Promise<void> => {
  try {
    const userId = parseInt(req.params.id);
    const userReports = await ReportModel.getUserReport(userId!);
    res.status(200).json({
      status: 200,
      success: true,
      message: "User report retrieved successfully",
      data: userReports,
    });
  } catch (error) {
    handleErrorResponse(res, error);
  }
};
