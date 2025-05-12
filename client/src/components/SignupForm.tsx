import Form from "@rjsf/mui";
import {
  getSubmitButtonOptions,
  RJSFSchema,
  SubmitButtonProps,
  UiSchema,
} from "@rjsf/utils";
import validator from "@rjsf/validator-ajv8";
import { customValidate, transformErrors } from "@/utils/formUtils";
import { IChangeEvent } from "@rjsf/core";
import { FormEvent } from "react";
import { UserSignup } from "@/app/interfaces";
import { useState } from "react";
import { Box, Button, Alert, CircularProgress } from "@mui/material";
import { useSignup } from "@/pages/auth/authActions";
import { Link } from "react-router-dom";
import { routes } from "@/utils/routes";

interface CustomSubmitButtonProps extends SubmitButtonProps {
  isLoading?: boolean;
}

const schema: RJSFSchema = {
  required: ["firstName", "lastName", "username", "email", "password"],
  properties: {
    firstName: {
      title: "First name",
      type: "string",
      minLength: 3,
      maxLength: 100,
    },
    lastName: {
      title: "Last name",
      type: "string",
      minLength: 3,
      maxLength: 100,
    },
    username: {
      title: "Username",
      type: "string",
      minLength: 3,
      maxLength: 100,
    },
    email: { title: "Email", type: "string", maxLength: 255 },
    password: {
      title: "Password",
      type: "string",
      minLength: 8,
      maxLength: 255,
      pattern:
        "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&#])[A-Za-z\\d@$!%*?&#]{8,}$",
    },
  },
};

const SignupButton = (props: CustomSubmitButtonProps) => {
  const { uiSchema, isLoading } = props;
  const { norender } = getSubmitButtonOptions(uiSchema);
  if (norender) {
    return null;
  }
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "center",
        mt: 2,
      }}
    >
      <Button
        type="submit"
        variant="contained"
        sx={{
          minWidth: "120px",
        }}
        disabled={isLoading}
      >
        {isLoading ? <CircularProgress size={24} /> : "Sign up"}
      </Button>
    </Box>
  );
};

export const SignupForm = () => {
  const signup = useSignup();
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSuccess, setIsSuccess] = useState(false);
  const [formData, setFormData] = useState<UserSignup>({
    firstName: "",
    lastName: "",
    username: "",
    email: "",
    password: "",
  });

  const handleChange = (data: IChangeEvent<any, RJSFSchema, any>) => {
    setIsSubmitted(true);
    setFormData((prev) => ({ ...prev, ...data.formData }));
  };

  const uiSchema: UiSchema = {
    email: {
      "ui:widget": "email",
      "ui:options": {
        inputType: "email",
      },
    },
    password: {
      "ui:widget": "password",
      "ui:options": {
        inputType: "password",
      },
    },
  };

  const onSubmit = (
    data: IChangeEvent<any, RJSFSchema, any>,
    event: FormEvent<any>
  ) => {
    setIsSubmitted(true);
    setIsLoading(true);

    signup(formData, {
      onSuccess: () => {
        setIsSuccess(true);
        setIsLoading(false);
      },
      onError: () => {
        setIsLoading(false);
      },
    });
  };

  if (isSuccess) {
    return (
      <Alert severity="success" sx={{ mt: 2 }}>
        <div>
          <strong>Registration successful!</strong>
          <p>
            Please check your email inbox to verify your account before logging
            in.
          </p>
          <p>
            <Link to={routes.LOGIN}>Return to Login</Link>
          </p>
        </div>
      </Alert>
    );
  }

  return (
    <Form
      onChange={handleChange}
      schema={schema}
      uiSchema={uiSchema}
      validator={validator}
      formData={formData}
      onSubmit={onSubmit}
      transformErrors={transformErrors}
      customValidate={customValidate}
      templates={{
        ButtonTemplates: {
          SubmitButton: (props) => (
            <SignupButton {...props} isLoading={isLoading} />
          ),
        },
      }}
      liveValidate={isSubmitted}
      showErrorList={false}
      noHtml5Validate
    />
  );
};
