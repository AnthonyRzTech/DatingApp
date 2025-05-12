import nodemailer from "nodemailer";
import { FRONTEND_ORIGIN } from "../settings";

let transporter: nodemailer.Transporter;

export const initializeEmailTransport = async (): Promise<void> => {
  try {
      console.log("Using real SMTP service for email transport");
      transporter = nodemailer.createTransport({
        host: process.env.SMTP_HOST || "smtp.gmail.com",
        port: parseInt(process.env.SMTP_PORT || "587"),
        secure: process.env.SMTP_SECURE === "true",
        auth: {
          user: process.env.SMTP_USER,
          pass: process.env.SMTP_PASS,
        },
      });

      transporter.verify(function (error, success) {
        if (error) {
          console.log(error);
        } else {
          console.log("Server is ready to send emails");
        }
      });
  } catch (error) {
    console.error("Failed to initialize email transport:", error);
  }
};

export const safelySendEmail = async (
  mailOptions: nodemailer.SendMailOptions
): Promise<{ success: boolean; error?: any; previewUrl?: string }> => {
  try {
    if (!transporter) {
      await initializeEmailTransport();
    }

    await transporter.sendMail(mailOptions);

    return { success: true };
  } catch (error) {
    console.error("Error sending email:", error);
    return { success: false, error };
  }
};

export const sendVerificationEmail = async (
  email: string,
  token: string
): Promise<void> => {
  const verificationLink = `${FRONTEND_ORIGIN}/verify-email?token=${token}`;

  const mailOptions = {
    from: `"Matcha Dating App" <${
      process.env.SMTP_USER
    }>`,
    to: email,
    subject: "Verify your Matcha account",
    html: `
      <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
        <h1 style="color: #4CAF50;">Welcome to Matcha!</h1>
        <p>Thank you for registering with us. To complete your registration, please click the button below to verify your email address:</p>
        <div style="text-align: center; margin: 30px 0;">
          <a href="${verificationLink}" 
             style="background-color: #4CAF50; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;">
            Verify Email
          </a>
        </div>
        <p>Or copy and paste the following link in your browser:</p>
        <p><a href="${verificationLink}">${verificationLink}</a></p>
        <p>If you didn't create an account, you can safely ignore this email.</p>
        <p>Best regards,<br>The Matcha Team</p>
      </div>
    `,
  };

  const { success, error, previewUrl } = await safelySendEmail(mailOptions);

  if (success) {
    console.log(`Verification email sent to: ${email}`);
    if (previewUrl) {
      console.log(`View verification email at: ${previewUrl}`);
    }
  } else {
    console.error(`Failed to send verification email to ${email}:`, error);
  }
};

export const sendResetPasswordEmail = async (
    email: string,
    token: string
): Promise<void> => {
    const resetLink = `${FRONTEND_ORIGIN}/reset-password?token=${token}`;
    
    const mailOptions = {
        from: `"Matcha Dating App" <${
        process.env.SMTP_USER
        }>`,
        to: email,
        subject: "Reset your Matcha password",
        html: `
        <div style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;">
            <h1 style="color: #4CAF50;">Reset Your Password</h1>
            <p>We received a request to reset your password. To proceed, please click the button below:</p>
            <div style="text-align: center; margin: 30px 0;">
            <a href="${resetLink}" 
                 style="background-color: #4CAF50; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;">
                Reset Password
            </a>
            </div>
            <p>Or copy and paste the following link in your browser:</p>
            <p><a href="${resetLink}">${resetLink}</a></p>
            <p>If you didn't request this, please ignore this email.</p>
            <p>Best regards,<br>The Matcha Team</p>
        </div>
        `,
    };
    
    const { success, error, previewUrl } = await safelySendEmail(mailOptions);
    
    if (success) {
        console.log(`Password reset email sent to: ${email}`);
        if (previewUrl) {
        console.log(`View password reset email at: ${previewUrl}`);
        }
    } else {
        console.error(`Failed to send password reset email to ${email}:`, error);
    }
    }
