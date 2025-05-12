import { User } from "@app/interfaces";
import { UserCard } from "./UserCard";
import { Box } from "@mui/material";
import Grid from "@mui/material/Grid2";

export const UserList = ({
  users,
  match,
}: {
  users: User[];
  match: boolean;
}) => {
  if (users.length === 0) {
    return (
      <Box sx={{ flexGrow: 1, p: 2 }}>
        <p>No users corresponding to your search criteria.</p>
      </Box>
    );
  }

  return (
    <Box sx={{ flexGrow: 1, p: 2 }}>
      <Grid container spacing={2}>
        {users.map((user) => (
          <Grid
            key={user.id}
            size={{
              xs: 12,
              sm: users.length > 2 ? 6 : 12,
              md: users.length > 2 ? 4 : 12,
            }}
          >
            <UserCard user={user} match={match} />
          </Grid>
        ))}
      </Grid>
    </Box>
  );
};
