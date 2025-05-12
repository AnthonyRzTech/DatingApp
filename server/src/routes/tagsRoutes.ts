import express, { Router } from "express";
import * as TagViews from "@src/controllers/tagController";

const router: Router = express.Router();

router.get("/", TagViews.getAllTags);

export default router;