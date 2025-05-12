import { Image } from "@/app/interfaces";
import {
  Avatar,
  AvatarGroup,
  Box,
  Radio,
  RadioGroup,
  Stack,
  Typography,
  Badge,
  FormControl,
  Button,
} from "@mui/material";
import { MuiFileInput } from "mui-file-input";
import { useState } from "react";
import { useUploadImage } from "@/pages/browse/usersActions";
import DeleteForeverIcon from "@mui/icons-material/DeleteForever";
import {
  useUpdateImageStatus,
  useDeleteImage,
} from "@/pages/browse/usersActions";
import AttachFileIcon from "@mui/icons-material/AttachFile";
import { useEffect } from "react";

interface ProfilePicturesProps {
  images?: Image[];
  userId?: number;
  editMode?: boolean;
  editPictures?: boolean;
  setEditPictures?: () => void;
}
const MAX_FILE_SIZE = 1 * 1024 * 1024;

export const ProfilePictures = ({
  images,
  userId,
  editMode = true,
  editPictures,
  setEditPictures,
}: ProfilePicturesProps) => {
  const uploadImage = useUploadImage();
  const updateImageStatus = useUpdateImageStatus();
  const deleteImage = useDeleteImage();
  const [files, setFiles] = useState<File[]>([]);
  const [filesError, setFilesError] = useState("");
  const [selectedProfilePic, setSelectedProfilePic] = useState<number | null>(
    images?.find((img) => img.isProfilePic)?.id ?? images?.[0]?.id ?? null
  );

  useEffect(() => {
    if (images && images.length > 0 && selectedProfilePic === null) {
      const profilePic = images.find((img) => img.isProfilePic);
      if (profilePic) {
        setSelectedProfilePic(profilePic.id ?? null);
      }
    }
    if (images && images.length === 1 && !images[0].isProfilePic) {
      updateImageStatus({
        userId: userId!,
        id: images[0].id,
        isProfilePic: true,
      });
      setSelectedProfilePic(images[0].id ?? null);
    }
  }, [images, userId, updateImageStatus, selectedProfilePic]);

  const handleFileUpload = (newFiles: File[]) => {
    if (!newFiles || !newFiles.length) {
      setFiles([]);
      return;
    }
    if (newFiles.length + (images?.length || 0) > 5) {
      setFilesError("You can  upload up to 5 images only");
      return;
    }
    const validFiles = newFiles.filter((file) => {
      if (file.size > MAX_FILE_SIZE) {
        const oversizedFiles = newFiles.filter(
          (file) => file.size > MAX_FILE_SIZE
        );
        setFilesError(
          `Files must be less than 1MB. These files exceed the limit: ${oversizedFiles
            .map((f) => f.name)
            .join(", ")}`
        );
        return false;
      } else if (!file.type.startsWith("image/")) {
        const nonImageFiles = newFiles.filter(
          (file) => !file.type.startsWith("image/")
        );
        setFilesError(
          `Only images are allowed. These files have the wrong format: ${nonImageFiles
            .map((f) => f.name)
            .join(", ")}`
        );
        return false;
      } else {
        setFilesError("");
      }

      return true;
    });

    setFiles(validFiles);

    validFiles.forEach((file) => {
      const reader = new FileReader();

      reader.onload = () => {
        const result = reader.result as string;
        uploadImage({
          userId: userId!,
          url: result,
        });
      };

      reader.onerror = () => {
        console.error(`Error reading file ${file.name}`);
      };

      try {
        reader.readAsDataURL(file);
      } catch (error) {
        console.error(`Failed to read file ${file.name}:`, error);
      }
    });
  };

  const handleProfilePicChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newSelectedId = Number(e.target.value);
    setSelectedProfilePic(newSelectedId);

    images?.forEach((image) => {
      updateImageStatus({
        userId: userId!,
        id: image.id,
        isProfilePic: image.id === newSelectedId,
      });
    });
  };

  return (
    <>
      {!editMode && (
        <>
          <Avatar
            sx={{ width: 100, height: 100 }}
            src={
              (images &&
                images.filter((image) => image.isProfilePic)[0]?.url) ||
              ""
            }
          />
          <AvatarGroup spacing={1}>
            {images &&
              images.length > 1 &&
              images
                .filter((image) => !image.isProfilePic)
                .map((image) => (
                  <Avatar
                    key={image.id}
                    sx={{ width: 65, height: 65 }}
                    src={image.url}
                  />
                ))}
          </AvatarGroup>
        </>
      )}
      {editMode && (
        <Stack spacing={3} sx={{ alignItems: "center" }}>
          <Typography variant="h4">Profile Pictures</Typography>
          {images && images?.length < 5 && (
            <MuiFileInput
              value={files}
              onChange={handleFileUpload}
              multiple
              slotProps={{
                htmlInput: {
                  accept: "image/*",
                },
              }}
              placeholder="Click here to upload pictures"
              error={!!filesError}
              helperText={filesError}
              InputProps={{
                endAdornment: <AttachFileIcon />,
              }}
            />
          )}

          {images && images.length > 0 && (
            <>
              <Typography variant="h6">Select profile picture</Typography>
              <Box
                sx={{
                  display: "grid",
                  gridTemplateColumns: "repeat(auto-fill, minmax(150px, 1fr))",
                  gap: 1,
                  width: "100%",
                  "& .MuiFormControl-root": {
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center",
                    padding: 1,
                  },
                }}
              >
                {images.map((image) => (
                  <Stack key={image.id}>
                    <FormControl>
                      <RadioGroup
                        value={selectedProfilePic ?? ""}
                        onChange={(event) => handleProfilePicChange(event)}
                      >
                        <Badge
                          anchorOrigin={{
                            vertical: "top",
                            horizontal: "right",
                          }}
                          badgeContent={
                            <Box
                              key={image.id}
                              sx={{
                                bgcolor: "primary.main",
                                borderRadius: "50%",
                                p: 0.5,
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center",
                                cursor: "pointer",
                              }}
                            >
                              <DeleteForeverIcon
                                fontSize="small"
                                onClick={() => {
                                  deleteImage({
                                    userId: userId!,
                                    id: image.id,
                                  });
                                }}
                              />
                            </Box>
                          }
                        >
                          <Avatar
                            key={image.id}
                            sx={{ width: 100, height: 100 }}
                            src={image.url}
                          />
                        </Badge>
                        <Radio
                          disableRipple
                          disableTouchRipple
                          value={image.id}
                          checked={selectedProfilePic === image.id}
                        />
                      </RadioGroup>
                    </FormControl>
                  </Stack>
                ))}
              </Box>
            </>
          )}
          <Button
            variant="outlined"
            onClick={() => setEditPictures && setEditPictures()}
          >
            Done
          </Button>
        </Stack>
      )}
    </>
  );
};
