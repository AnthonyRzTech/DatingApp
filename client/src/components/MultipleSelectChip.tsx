import { Tag } from "@/app/interfaces";
import { tagChipColors } from "@/components/theme";
import { WidgetProps } from "@rjsf/utils";
import { InputLabel, Box, OutlinedInput, MenuItem, ListItemText,SelectChangeEvent, Select, Chip, Checkbox } from "@mui/material";

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP,
      width: 250,
    },
  },
};

interface MultipleSelectChipProps extends WidgetProps {
  options: {
    items: Tag[];
    userTags: Tag[];
    handleChange: (event: SelectChangeEvent<string[]>) => void;
  };
}

function isTagInBothArrays(tag: string, array1?: Tag[], array2?: Tag[]) {
  // Check if the tag exists in both arrays
  if (!array1 || !array2) return;
  const isInArray1 = array1.some((item) => item.tag === tag);
  const isInArray2 = array2.some((item) => item.tag === tag);

  return isInArray1 && isInArray2;
}

export const MultipleSelectChip = ({
  options
}: MultipleSelectChipProps) => {
  const onSelectChange = (event: SelectChangeEvent<string[]>) => {
    options.handleChange(event);
  };
  const tagsArr = options.userTags?.map((tag: Tag) => tag.tag) || [];
  return (
    <>
    <InputLabel id="interests-label">Interests</InputLabel>
      <Select
        labelId="Interests"
        id="Interests"
        multiple
        value={tagsArr}
        onChange={onSelectChange}
        input={<OutlinedInput id="select-multiple-chip" label="Interests" />}
        renderValue={(selected) => (
          <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
            {selected.map((value, id) => (
              <Chip
                key={value}
                label={`#${value}`}
                sx={{
                  bgcolor: tagChipColors[id % tagChipColors.length],
                }}
              />
            ))}
          </Box>
        )}
        MenuProps={MenuProps}
      >
        {options.items?.map((item: any) => (
          <MenuItem key={item.id} value={item.tag}>
            <Checkbox checked={isTagInBothArrays(item.tag, options.items, options.userTags)} />
            <ListItemText primary={item.tag} />
          </MenuItem>
        ))}
      </Select>
    </>
  );
};
