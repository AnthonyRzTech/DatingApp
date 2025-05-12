import { Box, Button } from "@mui/material";
import Form from "@rjsf/mui";
import {
  RJSFSchema,
  UiSchema,
  SubmitButtonProps,
  getSubmitButtonOptions,
} from "@rjsf/utils";
import validator from "@rjsf/validator-ajv8";
import { customValidate, transformErrors } from "@/utils/formUtils";
import { useLogin } from "@/pages/auth/authActions";
import { IChangeEvent } from "@rjsf/core";

const schema: RJSFSchema = {
  required: ["username", "password"],
  properties: {
    username: {
      title: "Username",
      type: "string",
      minLength: 3,
      maxLength: 100,
    },
    password: {
      title: "Password",
      type: "string",
    },
  },
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

const LoginButton = (props: SubmitButtonProps) => {
  const { uiSchema } = props;
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
      >
        Login
      </Button>
    </Box>
  );
};

export const LoginForm = () => {
  const login = useLogin();
  const handleSubmit = (
    data: IChangeEvent<any, RJSFSchema, any>,
    event: React.FormEvent<HTMLFormElement>
  ) => {
    const { formData } = data;
    login(formData);
  };
  return (
    <Form
      schema={schema}
      uiSchema={uiSchema}
      validator={validator}
      onSubmit={handleSubmit}
      transformErrors={transformErrors}
      customValidate={customValidate}
      templates={{
        ButtonTemplates: {
          SubmitButton: LoginButton,
        },
      }}
      noHtml5Validate
      liveValidate
      showErrorList={false}
    />
  );
};
