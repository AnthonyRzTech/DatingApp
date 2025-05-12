import { client } from "@utils/axios";
import { useMutation } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { queryClient } from "../../app/App";
import { User, UserLogin, UserSignup, ErrorResponse } from "@app/interfaces";
import { routes } from "@utils/routes";
import { useAuth } from "@utils/authContext";
import { AxiosError } from "axios";
import { enqueueSnackbar } from "notistack";

export const loginUser = async (data: UserLogin) => {
  const user = await client.post<User>("/auth/login", data, {
    withCredentials: true,
  });
  return user.data as User;
};

export const useLogin = () => {
  const navigate = useNavigate();
  const { login: setAuth } = useAuth();
  const { mutate: login } = useMutation({
    mutationKey: ["currentUser"],
    mutationFn: loginUser,
    onSuccess: () => {
      setAuth();
      queryClient.invalidateQueries({ queryKey: ["currentUser"] });
      navigate(routes.ME, { replace: true });
    },
    onError: (error: AxiosError<ErrorResponse>) => {
        if (error.status === 403) {
        enqueueSnackbar(
          "Email not verified. Please check your inbox for the verification link.",
          { variant: "error" }
        );
        return;
      }
      if (error.status === 400 || 401) {
        enqueueSnackbar("Invalid credentials", { variant: "error" });
      }
    },
  });
  return login;
};

export const signupUser = async (data: UserSignup) => {
  const user = await client.post<User>("/auth/signup", data, {
    withCredentials: true,
  });
  return user.data as User;
};

export const useSignup = () => {
  const navigate = useNavigate();
  const { mutate: signup } = useMutation({
    mutationKey: ["currentUser"],
    mutationFn: signupUser,
    onSuccess: (data) => {
      enqueueSnackbar(
        "Registration successful! Please verify your email before logging in.",
        {
          variant: "success",
          autoHideDuration: 6000,
        }
      );
    },
    onError: (error: AxiosError<ErrorResponse>) => {
      if (error.response?.data?.status === 400) {
        if (error.response?.data.message) {
          enqueueSnackbar(error.response.data.message, { variant: "error" });
        }
      }
    },
  });
  return (
    data: UserSignup,
    options?: { onSuccess?: () => void; onError?: () => void }
  ) => {
    signup(data, {
      onSuccess: () => {
        if (options?.onSuccess) options.onSuccess();
      },
      onError: (error) => {
        if (options?.onError) options.onError();
      },
    });
  };
};

export const logoutUser = async () => {
  try {
    const response = await client.post(
      "/auth/logout",
      {},
      { withCredentials: true }
    );
    return response.data;
  } catch (error) {
    console.error("Logout failed:", error);
    throw error;
  }
};

export const useLogout = () => {
  const navigate = useNavigate();
  const { logout: setAuth } = useAuth();

  const { mutate: logout } = useMutation({
    mutationKey: ["currentUser"],
    mutationFn: logoutUser,
    onSuccess: () => {
      console.log("Logout successful");
      setAuth();
      queryClient.clear();
      navigate("/", { replace: true });
    },
  });
  return logout;
};
