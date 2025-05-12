import {
  RJSFSchema,
  UiSchema,
  RJSFValidationError,
  FormValidation,
} from "@rjsf/utils";
import { isValidUsername, isValidEmail } from "@/utils/helpers";

export const customValidate = (formData: any, errors: FormValidation) => {
  if (!isValidEmail(formData.email)) {
    errors.email?.addError("Invalid email address");
  }
  if (!isValidUsername(formData.username)) {
    errors.username?.addError(
      "Username can only contain letters, numbers, and underscores"
    );
  }
  return errors;
};

export const transformErrors = (
  errors: RJSFValidationError[],
  uiSchema?: UiSchema<any, RJSFSchema, any>
): RJSFValidationError[] => {
  return errors.map((error: any) => {
    if (error.name === "type" || error.name === "required") {
      error.message = "This field is required";
    }
    if (error.name === "minLength") {
      error.message = `Must be at least ${error.params.limit} characters long`;
    }
    if (error.name === "maxLength") {
      error.message = `Must be at most ${error.params.limit} characters long`;
    }
    if (error.name === "minItems") {
      error.message = "Choose at least one option";
    }
    if (error.name === "pattern") {
      error.message =
        "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character (@$!%*?&#)";
    }
    return error;
  });
};
