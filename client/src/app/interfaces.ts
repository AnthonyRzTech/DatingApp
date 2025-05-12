import { SelectChangeEvent } from "@mui/material";
import { ReactNode } from "react";

export enum Order {
  asc = "asc",
  desc = "desc",
}

export interface UserLogin {
  username: string;
  password: string;
}

export interface UserSignup extends UserLogin {
  firstName: string;
  lastName: string;
  email: string;
}

export interface User {
  id: number;
  firstName: string;
  lastName: string;
  email?: string;
  username: string;
  gender: Gender;
  preferences: Gender[];
  dateOfBirth: Date;
  bio: string;
  location?: { x: number; y: number };
  city?: string;
  fameRate: number;
  tags: Tag[];
  lastSeen: Date;
  age?: number;
  distance?: number;
}

export interface UserCardProps {
  user: User;
  match: boolean;
}

export interface UserListProps {
  users: SortUser[];
}

export interface FormData {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  gender: Gender;
  preferences: Gender[];
  dateOfBirth: Date;
  bio: string;
  location?: { x: number; y: number };
  city?: string;
  interests: Tag[];
}

export interface UserResponse {
  data: User;
  status: number;
}

export interface Props {
  children: ReactNode;
}

export interface Tag {
  id: number;
  tag: string;
}

export interface ViewProfileProps {
  me: boolean;
  user: Partial<User>;
  tags: Tag[] | undefined;
  images: Image[] | undefined;
}

export interface EditProfileProps {
  user: Partial<User>;
  userTags?: Tag[];
  images?: Image[];
  setEditMode: () => void;
}

export interface Image {
  id?: number;
  userId: number;
  url: string;
  isProfilePic?: boolean;
}

export enum Gender {
  Male = "male",
  Female = "female",
  Other = "other",
}

export interface UserSearchQuery {
  ageMin?: number;
  ageMax?: number;
  gender?: Gender;
  sexualPreferences?: Gender[];
  distance?: number;
  latitude?: number;
  longitude?: number;
  tags?: string[];
  minFameRating?: number;
  maxFameRating?: number;
  sortBy?: "distance" | "age" | "fameRating";
  order?: Order;
  limit?: number;
  offset?: number;
}

export interface SortUser extends User {
  totalScore?: number;
  commonTags?: number;
  ageScore?: number;
  distanceScore?: number;
  fameRateScore?: number;
}

export interface UsersSortParams {
  age?: Order;
  fameRate?: Order;
  distance?: Order;
  commonTags?: Order;
  totalScore?: Order;
}

export interface UserUpdateFormProps {
  user?: Partial<User>;
  tags?: Tag[];
  userTags: Tag[];
  oldTags?: Tag[];
  onDateChange: (newValue: string) => void;
  onTagsChange: (event: SelectChangeEvent<string[]>) => void;
}

export interface ValidationError {
  code: string;
  message: string;
  path: string[];
  validation: string;
}

export interface ErrorResponse {
  error?: ValidationError[];
  message?: string;
  status: number;
}

export interface UserView {
  id: number;
  viewerId: number;
  viewedId: number;
  viewedAt: Date;
}

export interface UserLike {
  id: number;
  likerId: number;
  likedId: number;
  likedAt: Date;
}

export interface UserBlock {
  id: number;
  blockerId: number;
  blockedUserId: number;
}
