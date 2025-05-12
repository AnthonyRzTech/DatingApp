import React from "react";
import { Sidebar } from "@components/Sidebar";
import { Props } from "@app/interfaces";
import { Container, Box } from "@mui/material";

export const Layout: React.FC<Props> = (props) => {
  return (
    <Box
      sx={{
        display: "flex",
        minHeight: "100vh",
        paddingRight: { xs: 0, sm: "150px" }, // Add padding for sidebar width on non-mobile
      }}
    >
      <Sidebar />
      <Container
        sx={{
          flexGrow: 2,
          pb: { xs: "56px", sm: 0 }, // Add bottom padding on mobile for the bottom nav
        }}
      >
        <Box component="main">{props.children}</Box>
      </Container>
    </Box>
  );
};
