import { User, UserView, UserLike } from "@/app/interfaces";
import { client } from "@/utils/axios";
import { useQuery } from "@tanstack/react-query";

const fetchUserById = async (id: number) => {
  return await client.get(`/users/${id}`, { withCredentials: true });
};

export const useFetchUserById = (id: number) => {
  return useQuery<User, any>({
    queryFn: async ({ queryKey }) => {
      const userId = queryKey[1] as number;
      const response = await fetchUserById(userId);
      const user = response.data.data;
      return {
        id: user.id,
        firstName: user.first_name,
        lastName: user.last_name,
        username: user.username,
        city: user.city,
        bio: user.bio,
        gender: user.gender,
        preferences: user.preferences,
        dateOfBirth: user.date_of_birth,
        fameRate: user.fame_rate,
        lastSeen: user.last_seen,
        tags: user.tags,
      } as User;
    },
    queryKey: ["user", id],
  });
};

/* __________________________________ Views __________________________________ */

interface ApiViewResponse {
  id: number;
  viewer_user_id: number;
  viewed_user_id: number;
  view_count: number;
  last_viewed_at: string;
  created_at: string;
}

const fetchUserViews = async (id: number) => {
  if (!id || id === 0 || id === undefined) {
    return { data: { data: [] } };
  }
  return await client.get(`/users/${id}/views`, {
    withCredentials: true,
  });
};

export const useViews = (id: number | undefined) => {
  return useQuery<UserView[], any>({
    queryFn: async () => {
      const response = await fetchUserViews(id!);
      const data = response.data.data.map((view: ApiViewResponse) => ({
        id: view.id,
        viewerId: view.viewer_user_id,
        viewedId: view.viewed_user_id,
        viewedAt: new Date(view.last_viewed_at),
      })) as UserView[];
      return data;
    },
    queryKey: ["views", id],
  });
};

/* __________________________________ Likes __________________________________ */

interface ApiLikeResponse {
  id: number;
  liker_user_id: number;
  liked_user_id: number;
  created_at: string;
}

const fetchUserLikes = async (id: number) => {
  if (!id || id === 0 || id === undefined) {
    return { data: { data: [] } };
  }
  return await client.get(`/users/${id}/likes`, {
    withCredentials: true,
  });
};

export const useLikes = (id: number | undefined) => {
  return useQuery<UserLike[], any>({
    queryFn: async () => {
      const response = await fetchUserLikes(id!);
      const data = response.data.data.map((like: ApiLikeResponse) => ({
        id: like.id,
        likerId: like.liker_user_id,
        likedId: like.liked_user_id,
        likedAt: new Date(like.created_at),
      })) as UserLike[];
      return data;
    },
    queryKey: ["likes", id],
  });
};
