import bcrypt from "bcrypt";

export const generateHash = async (password: string | undefined): Promise<string> => {
  if (!password) {
    throw new Error("Password is required");
  }
  const salt = await bcrypt.genSalt(10);
  const hashedPassword = await bcrypt.hash(password, salt);
  return hashedPassword;
};

export const validatePassword = async (password: string, hashedPassword: string): Promise<boolean> => {
  return await bcrypt.compare(password, hashedPassword);
};