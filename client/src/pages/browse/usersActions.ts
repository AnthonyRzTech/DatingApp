import { AxiosError, AxiosResponse } from "axios";
import { client } from "@utils/axios";
import { useQuery, QueryObserverResult } from "@tanstack/react-query";
import {
  User,
  Gender,
  UserResponse,
  Tag,
  UserSearchQuery,
  UserBlock,
} from "@app/interfaces";
import { useMutation } from "@tanstack/react-query";
import { queryClient } from "@/app/App";
import { formatCoordinates } from "@/utils/helpers";
import { Image, FormData, ErrorResponse } from "@/app/interfaces";
import { capitalize } from "@/utils/helpers";
import { enqueueSnackbar } from "notistack";
import { useNavigate } from "react-router-dom";

const fetchUsers = async (params?: UserSearchQuery) => {
  return await client.get("/users/search", {
    params: { ...params },
    withCredentials: true,
  });
};

export const useFetchUsers = (params?: UserSearchQuery) => {
  return useQuery<User[], any>({
    queryFn: async () => {
      const response = await fetchUsers(params);
      const data = response.data.data;
      return data.map((user: any) => ({
        id: user.id,
        firstName: user.firstName,
        lastName: user.lastName,
        username: user.username,
        gender: user.gender as Gender,
        preferences: user.preferences as Gender[],
        dateOfBirth: user.dateOfBirth,
        bio: user.bio,
        city: user.city,
        fameRate: user.fameRate,
        lastSeen: user.lastSeen,
        tags: user.tags,
        age: user.age,
        distance: user.distance,
      })) as User[];
    },
    queryKey: ["users", params],
  });
};

const updateUser = async (data: Partial<FormData>) => {
  const coordinates = formatCoordinates(data.location);
  const updates = {
    bio: data.bio,
    first_name: data.firstName,
    last_name: data.lastName,
    username: data.username,
    email: data.email,
    gender: data.gender,
    preferences: data.preferences,
    date_of_birth: data.dateOfBirth,
    location: coordinates,
    city: data.city,
  };
  const user = await client.put<User>(`/users/${data.id}`, updates, {
    withCredentials: true,
  });
  return user.data as User;
};

const fetchCurrentUser = async (): Promise<
  AxiosResponse<UserResponse, any>
> => {
  return await client.get<UserResponse>(`/users/me`, { withCredentials: true });
};

export const useFetchCurrentUser = (): QueryObserverResult<User, any> => {
  return useQuery<User, any>({
    queryFn: async () => {
      const response = await fetchCurrentUser();
      const userData: any = response.data.data;
      return {
        id: userData.id,
        firstName: userData.first_name,
        lastName: userData.last_name,
        email: userData.email,
        username: userData.username,
        gender: userData.gender,
        preferences: userData.preferences,
        dateOfBirth: userData.date_of_birth,
        bio: userData.bio,
        location: userData.location,
        city: userData.city,
        fameRate: userData.fame_rate,
        lastSeen: userData.last_seen,
        tags: userData.tags,
      } as User;
    },
    queryKey: ["currentUser"],
    retry: false,
  });
};

export const useUpdateUserProfile = () => {
  const { mutate: updateUserData, error: updateUserError } = useMutation({
    mutationKey: ["currentUser"],
    mutationFn: updateUser,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
      enqueueSnackbar("Profile updated successfully", { variant: "success" });
    },
    onError: (error: AxiosError<ErrorResponse>) => {
      if (error.response?.data?.status === 500) {
        if (error.response.data?.message?.includes("duplicate key")) {
          const duplicateField = error.response.data.message.match(/"([^"]+)"/);
          if (duplicateField) {
            const fieldName = capitalize(
              duplicateField[0]
                .replace("users_", "")
                .replace("_key", "")
                .replace(/"/g, "")
            );
            enqueueSnackbar(`${fieldName} is already in use, try again`);
            return;
          }
        }
        enqueueSnackbar(`Internal server error, changes not saved`);
        return;
      }
    },
  });

  return { updateUserData, updateUserError };
};

/*TAGS*/

const fetchAllTags = async (): Promise<AxiosResponse> => {
  return await client.get(`/tags`, { withCredentials: true });
};

export const fetchUserTags = async (
  userId?: number
): Promise<AxiosResponse> => {
  return await client.get(`/users/${userId}/tags`, { withCredentials: true });
};

const updateUserTags = async ({
  userId,
  tagId,
}: {
  userId?: number;
  tagId: number;
}) => {
  const response = await client.post(`/users/${userId}/tags`, { tagId });
  return response.data;
};

const deleteUserTags = async ({
  userId,
  tagId,
}: {
  userId?: number;
  tagId: number;
}) => {
  const response = await client.delete(`/users/${userId}/tags`, {
    data: { tagId: tagId },
  });
  return response.data;
};

export const useFetchAllTags = (): QueryObserverResult<Tag[], any> => {
  return useQuery<Tag[], any>({
    queryFn: async () => {
      const { data } = await fetchAllTags();
      return data;
    },
    queryKey: ["tags"],
    retry: false,
  });
};

export const useAddUserTags = () => {
  const { mutate: update } = useMutation({
    mutationKey: ["userTags"],
    mutationFn: (variables: { userId?: number; tagId: number }) =>
      updateUserTags(variables),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["userTags"] });
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
    },
  });
  return update;
};

export const useDeleteUserTags = () => {
  const { mutate: update } = useMutation({
    mutationKey: ["userTags"],
    mutationFn: (variables: { userId?: number; tagId: number }) =>
      deleteUserTags(variables),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["userTags"] });
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
    },
  });
  return update;
};

export const useFetchUserTags = (userId?: number) => {
  return useQuery<Tag[], any>({
    queryFn: async () => {
      const { data } = await fetchUserTags(userId);
      return data.data;
    },
    queryKey: ["userTags"],
    enabled: !!userId,
  });
};

/*IMAGES*/

const fetchImages = async (userId?: number) => {
  if (!userId) return [];
  const response = await client.get(`/users/${userId}/images`, {
    params: { userId },
    withCredentials: true,
  });
  return response.data;
};
const uploadImage = async (data: Image) => {
  const response = await client.post(
    `/users/${data.userId}/images`,
    { userId: data.userId, url: data.url },
    {
      withCredentials: true,
    }
  );
  return response.data;
};

const deleteImage = async (data: Partial<Image>) => {
  const response = await client.delete(`/users/${data.userId}/images`, {
    params: { user_id: data.userId, imageId: data.id },
    withCredentials: true,
  });
  return response.data;
};

const updateImageStatus = async (data: Partial<Image>) => {
  const response = await client.put(
    `/users/${data.userId}/images`,
    { isProfile: data.isProfilePic, imageId: data.id },
    {
      withCredentials: true,
    }
  );
  return response.data;
};

const fetchProfilePic = async (userId?: number) => {
  const response = await client.get(`/users/${userId}/images/profile`, {
    params: { userId },
    withCredentials: true,
  });
  if (response.status !== 200) {
    throw new Error("Failed to fetch profile picture");
  }
  return response.data;
};

export const useFetchUserProfilePic = (userId?: number) => {
  return useQuery<Image, any>({
    queryFn: async () => {
      const { data } = await fetchProfilePic(userId);
      return {
        id: data.id,
        userId: data.userId,
        url: data.url,
        isProfilePic: data.isProfilePic,
      };
    },
    queryKey: ["userProfilePic", userId],
    enabled: !!userId,
  });
};

export const useFetchUserImages = (userId?: number) => {
  return useQuery<Image[], any>({
    queryFn: async () => {
      const { data } = await fetchImages(userId);
      return data.map((image: any) => ({
        id: image.id,
        userId: image.user_id,
        url: image.image_url,
        isProfilePic: image.is_profile,
      }));
    },
    queryKey: ["currentUserImages", userId],
    enabled: !!userId,
    initialData: [],
  });
};

export const useUploadImage = () => {
  const { mutate: upload } = useMutation({
    mutationKey: ["currentUserImages"],
    mutationFn: uploadImage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["currentUserImages"] });
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
    },
  });
  return upload;
};

export const useDeleteImage = () => {
  const { mutate: deleteImageMutation } = useMutation({
    mutationKey: ["currentUserImages"],
    mutationFn: deleteImage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["currentUserImages"] });
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
    },
  });
  return deleteImageMutation;
};

export const useUpdateImageStatus = () => {
  const { mutate: updateImage } = useMutation({
    mutationKey: ["currentUserImages"],
    mutationFn: updateImageStatus,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["currentUserImages"] });
    },
  });
  return updateImage;
};

export const useFetchBlockedUsers = (userId?: number) => {
  return useQuery<UserBlock[]>({
    queryKey: ["blockedUsers", userId],
    queryFn: async () => {
      if (!userId) return [];
      
      const response = await client.get(`/users/${userId}/blocked`);
      
      return response.data.data.map((block: any) => ({
        id: block.id,
        blockerId: block.blocker_id,
        blockedUserId: block.blocked_user_id,
      }));
    },
    enabled: !!userId,
  });
};

export const useBlockUser = () => {
  const { mutate: blockUser } = useMutation({
    mutationKey: ["blockedUsers"],
    mutationFn: async (userId: number) => {
      await client.post(`/users/${userId}/blocked/`, {
        blockedUserId: userId
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users"] });
      queryClient.invalidateQueries({ queryKey: ["blockedUsers"] });
      queryClient.invalidateQueries({ queryKey: ["matches"] });
    },
  });
  return blockUser;
}

export const useResetPassword = () => { 
  const { mutate: resetPassword } = useMutation({
    mutationKey: ["resetPassword"],
    mutationFn: async (email: string) => {
      await client.post(`/auth/reset-password`, { email });
    },
    onSuccess: () => {
      enqueueSnackbar("Password reset link sent to your email", {
        variant: "success",
      });
    },
    onError: (error: AxiosError<ErrorResponse>) => {
      if (error.response?.data?.status === 400) {
        if (error.response?.data.message) {
          enqueueSnackbar(error.response.data.message, { variant: "error" });
        }
      }
    },
  });
  return resetPassword;
}

export const useUpdatePassword = () => {
  const navigate = useNavigate();
  const { mutate: updatePassword } = useMutation({
    mutationKey: ["updatePassword"],
    mutationFn: async (data: { token: string; password: string }) => {
      await client.put(`/auth/reset-password`, data);
    },
    onSuccess: () => {
      enqueueSnackbar("Password updated successfully, redirecting to log in...", {
        variant: "success",
      });
      setTimeout(() => {
        navigate("/auth/login");
      }, 3000);
    },
    onError: (error: AxiosError<ErrorResponse>) => {
      if (error.response?.data?.status === 400) {
        if (error.response?.data.message) {
          enqueueSnackbar(error.response.data.message, { variant: "error" });
        }
      }
    },
  });
  return updatePassword;
}