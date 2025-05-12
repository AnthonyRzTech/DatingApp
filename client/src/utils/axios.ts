import axios from "axios";
import { SERVER_URL } from "./config";

export const client = axios.create({
  baseURL: SERVER_URL,
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});
