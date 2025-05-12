import React, { useState } from "react";
import {
  Box,
  Drawer,
  FormControl,
  Slider,
  Stack,
  TextField,
  Typography,
  Button,
  Zoom,
  colors,
} from "@mui/material";
import {
  Select,
  Chip,
  OutlinedInput,
  MenuItem,
  SelectChangeEvent,
} from "@mui/material";
import { useMediaQuery, useTheme } from "@mui/material";
import ManageSearchIcon from "@mui/icons-material/ManageSearch";
import SortIcon from "@mui/icons-material/Sort";
import SpeedDial from "@mui/material/SpeedDial";
import SpeedDialAction from "@mui/material/SpeedDialAction";
import DateRangeIcon from "@mui/icons-material/DateRange";
import StyleIcon from "@mui/icons-material/Style";
import Diversity1Icon from "@mui/icons-material/Diversity1";
import SocialDistanceIcon from "@mui/icons-material/SocialDistance";

import {
  Order,
  Tag,
  User,
  UserSearchQuery,
  UsersSortParams,
} from "@app/interfaces";
import { MAX_AGE, MAX_FAME, MIN_AGE, MIN_FAME } from "@/utils/config";

interface SearchBarProps {
  browseStatus: boolean;
  userData?: User;
  tags?: Tag[];
  searchParams: UserSearchQuery;
  sortParams: UsersSortParams;
  setSortParams: (params: UsersSortParams) => void;
  onSubmit: (params: UserSearchQuery) => void;
}

const SearchBar = ({
  browseStatus,
  userData,
  tags,
  searchParams,
  sortParams,
  setSortParams,
  onSubmit,
}: SearchBarProps) => {
  const [inputAge, setInputAge] = useState<number[]>([
    searchParams.ageMin || MIN_AGE,
    searchParams.ageMax || MAX_AGE,
  ]);
  const [inputFame, setInputFame] = useState<number[]>([
    searchParams.minFameRating || MIN_FAME,
    searchParams.maxFameRating || MAX_FAME,
  ]);
  const [localParams, setLocalParams] = useState<UserSearchQuery>(searchParams);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

  const getSortSpeedDialColors = (
    sortType: string,
    lightColor: string,
    darkColor: string
  ): string => {
    const defaultColor = colors.grey[50];
    switch (sortType) {
      case "age":
        if (sortParams.age === Order.asc) return darkColor;
        if (sortParams.age === Order.desc) return lightColor;
        else return defaultColor;
      case "fameRate":
        if (sortParams.fameRate === Order.asc) return darkColor;
        if (sortParams.fameRate === Order.desc) return lightColor;
        else return defaultColor;
      case "distance":
        if (sortParams.distance === Order.asc) return darkColor;
        if (sortParams.distance === Order.desc) return lightColor;
        else return defaultColor;
      case "commonTags":
        if (sortParams.commonTags === Order.asc) return darkColor;
        if (sortParams.commonTags === Order.desc) return lightColor;
        else return defaultColor;
      default:
        return defaultColor;
    }
  };

  const speedDialActions = [
    {
      icon: <StyleIcon />,
      name: "Tags",
      sx: {
        backgroundColor: getSortSpeedDialColors(
          "commonTags",
          "#f0f4c3",
          "#9ccc65"
        ),
      }, // light green
    },
    {
      icon: <Diversity1Icon />,
      name: "Fame",
      sx: {
        backgroundColor: getSortSpeedDialColors(
          "fameRate",
          "#f8bbd0",
          "#f06292"
        ),
      }, // light pink
    },
    {
      icon: <SocialDistanceIcon />,
      name: "Distance",
      sx: {
        backgroundColor: getSortSpeedDialColors(
          "distance",
          "#b3e5fc",
          "#03a9f4"
        ),
      }, // light blue
    },
    {
      icon: <DateRangeIcon />,
      name: "Age",
      sx: {
        backgroundColor: getSortSpeedDialColors("age", "#ffcc80", "#ff9800"),
      }, // light orange
    },
  ];

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    onSubmit(localParams);
    setDrawerOpen(false);
  };

  const handleAgeSlider = (
    e: Event,
    newValue: number | number[],
    activeThumb: number
  ) => {
    setInputAge(newValue as number[]);
    if (typeof newValue !== "object") return;
    localParams.ageMin = newValue[0];
    localParams.ageMax = newValue[1];
    setLocalParams({ ...localParams });
  };

  const handleFameSlider = (
    e: Event,
    newValue: number | number[],
    activeThumb: number
  ) => {
    setInputFame(newValue as number[]);
    if (typeof newValue !== "object") return;
    localParams.minFameRating = newValue[0];
    localParams.maxFameRating = newValue[1];
    setLocalParams({ ...localParams });
  };

  const handleDistanceChange = (
    event: Event,
    value: number | number[],
    activeThumb: number
  ) => {
    const numValue = typeof value === "number" ? value : value[0];
    setLocalParams({
      ...localParams,
      distance: numValue <= 0 ? 1 : numValue,
      latitude: userData!.location?.x,
      longitude: userData!.location?.y,
    });
  };

  const handleTagsChange = (e: SelectChangeEvent<string[]>) => {
    const value = e.target.value as string[];
    setLocalParams({ ...localParams, tags: value });
  };

  const handleSort = (actionName: string) => {
    switch (actionName) {
      case "Age":
        setSortParams({
          age: sortParams.age === Order.asc ? Order.desc : Order.asc,
        });
        break;
      case "Fame":
        setSortParams({
          fameRate: sortParams.fameRate === Order.asc ? Order.desc : Order.asc,
        });
        break;
      case "Distance":
        setSortParams({
          distance: sortParams.distance === Order.asc ? Order.desc : Order.asc,
        });
        break;
      case "Tags":
        setSortParams({
          commonTags:
            sortParams.commonTags === Order.asc ? Order.desc : Order.asc,
        });
        break;
      default:
        break;
    }
  };

  // Show the FAB sort params only
  if (browseStatus) {
    return (
      <SpeedDial
        ariaLabel="Sort options"
        sx={
          isMobile
            ? { position: "fixed", bottom: 72, left: 16 }
            : { position: "fixed", bottom: 16, left: 16 }
        }
        icon={<SortIcon />}
      >
        {speedDialActions.map((action) => (
          <SpeedDialAction
            key={action.name}
            icon={action.icon}
            tooltipTitle={`Sort by ${action.name}`}
            sx={action.sx}
            onClick={() => handleSort(action.name)}
          />
        ))}
      </SpeedDial>
    );
  }

  // Show the FAB search params
  else {
    return (
      <>
        <Zoom
          in={true}
          style={{
            transitionDelay: "100ms",
          }}
          unmountOnExit
        >
          <SpeedDial
            ariaLabel="Manage search and sort options"
            sx={
              isMobile
                ? { position: "fixed", bottom: 72, left: 16 }
                : { position: "fixed", bottom: 16, left: 16 }
            }
            icon={<ManageSearchIcon />}
            title="Advanced search"
            // TODO: Add a way to open the drawer on click / long press instead of temporary double click
            // Prevent the drawer from opening on with each speed dial action
            onDoubleClick={() => setDrawerOpen(true)}
          >
            {speedDialActions.map((action) => (
              <SpeedDialAction
                key={action.name}
                icon={action.icon}
                tooltipTitle={`Sort by ${action.name}`}
                sx={action.sx}
                onClick={() => handleSort(action.name)}
              />
            ))}
          </SpeedDial>
        </Zoom>
        <Drawer
          anchor="top"
          open={drawerOpen}
          onClose={() => setDrawerOpen(false)}
        >
          <Box component="form" onSubmit={handleSubmit} sx={{ p: 2 }}>
            <FormControl fullWidth sx={{ gap: 2 }}>
              <Stack
                spacing={2}
                direction="row"
                alignItems="center"
                sx={{ ml: 5, mr: 5, mt: 4 }}
              >
                <Typography variant="h6" sx={{ minWidth: 80 }}>
                  Age
                </Typography>
                {userData?.dateOfBirth ? (
                  <Slider
                    getAriaLabel={() => "Age Range"}
                    valueLabelDisplay="on"
                    value={[inputAge[0], inputAge[1]]}
                    onChange={handleAgeSlider}
                    min={MIN_AGE}
                    max={MAX_AGE}
                    marks={[
                      {
                        value:
                          new Date().getFullYear() -
                          new Date(userData!.dateOfBirth).getFullYear(),
                        label: "Me",
                      },
                    ]}
                    disableSwap
                  />
                ) : (
                  <Slider
                    getAriaLabel={() => "Age Range"}
                    valueLabelDisplay="on"
                    sx={{ m: 1 }}
                    value={[inputAge[0], inputAge[1]]}
                    onChange={handleAgeSlider}
                    min={MIN_AGE}
                    max={MAX_AGE}
                    disableSwap
                  />
                )}
              </Stack>

              {userData?.location === null ? (
                <TextField
                  label="Allow location in your profile to filter by distance"
                  disabled
                />
              ) : (
                <Stack
                  spacing={2}
                  direction="row"
                  alignItems="center"
                  sx={{ ml: 5, mr: 5, mt: 4 }}
                >
                  <Typography variant="h6" sx={{ minWidth: 80 }}>
                    Distance
                  </Typography>
                  <Slider
                    getAriaLabel={() => "Distance"}
                    valueLabelDisplay="auto"
                    value={localParams.distance || 1}
                    onChange={handleDistanceChange}
                    min={1}
                    max={1000}
                    step={null}
                    marks={[
                      { value: 1 },
                      { value: 5 },
                      { value: 10 },
                      { value: 15 },
                      { value: 20 },
                      { value: 30 },
                      { value: 40 },
                      { value: 50 },
                      { value: 75 },
                      { value: 100 },
                      { value: 150 },
                      { value: 200 },
                      { value: 350 },
                      { value: 500 },
                      { value: 750 },
                      { value: 1000 },
                    ]}
                    valueLabelFormat={(value) => `${value} km`}
                  />
                </Stack>
              )}

              <Stack
                spacing={2}
                direction="row"
                alignItems="center"
                sx={{ ml: 5, mr: 5, mt: 4 }}
              >
                <Typography variant="h6" sx={{ minWidth: 80 }}>
                  Fame
                </Typography>
                <Slider
                  getAriaLabel={() => "Fame Range"}
                  valueLabelDisplay="on"
                  value={[inputFame[0], inputFame[1]]}
                  onChange={handleFameSlider}
                  min={MIN_FAME}
                  max={MAX_FAME}
                  marks={[
                    {
                      value: userData?.fameRate || 0,
                      label: "Me",
                    },
                  ]}
                  disableSwap
                  valueLabelFormat={(value) => `${value}%`}
                />
              </Stack>

              <Select
                sx={{ m: 1 }}
                multiple
                displayEmpty
                value={localParams.tags?.map((tag) => tag) || []}
                onChange={handleTagsChange}
                input={<OutlinedInput />}
                renderValue={(selected) => {
                  if ((selected as string[]).length === 0) {
                    return <em>Tags</em>;
                  } else
                    return (
                      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                        {selected.map((value) => (
                          <Chip
                            key={value}
                            color="primary"
                            label={value}
                            onMouseDown={(event) => event.stopPropagation()}
                            onDelete={(event) => {
                              event.stopPropagation();
                              event.preventDefault();
                              const newTags = localParams.tags?.filter(
                                (tag) => tag !== value
                              );
                              setLocalParams({ ...localParams, tags: newTags });
                            }}
                          />
                        ))}
                      </Box>
                    );
                }}
              >
                <MenuItem disabled value="">
                  Tags
                </MenuItem>
                {tags?.map((tag) => (
                  <MenuItem key={tag.tag} value={tag.tag}>
                    {tag.tag}
                  </MenuItem>
                ))}
              </Select>

              <Button type="submit" variant="contained">
                Search
              </Button>
            </FormControl>
          </Box>
        </Drawer>
      </>
    );
  }
};

export default SearchBar;
