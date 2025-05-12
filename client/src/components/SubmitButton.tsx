import { Button } from "@mui/material";
import { SubmitButtonProps } from "@rjsf/utils";
import { getSubmitButtonOptions } from "@rjsf/utils";

export const SubmitButton = (props: SubmitButtonProps) => {
    const { uiSchema } = props;
    const { norender } = getSubmitButtonOptions(uiSchema);
    if (norender) {
      return null;
    }
    return (
      <Button
          type="submit"
          variant="contained"
        >
          Save Changes
        </Button>
    );
  }