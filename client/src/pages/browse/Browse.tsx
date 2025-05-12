import {
  useFetchAllTags,
  useFetchBlockedUsers,
  useFetchCurrentUser,
  useFetchUsers,
} from "./usersActions";
import {
  UserSearchQuery,
  UsersSortParams,
  SortUser,
  Order,
} from "@app/interfaces";
import { useState, useEffect } from "react";
import { Pagination, AppBar, Tabs, Tab, Box } from "@mui/material";
import LoadingCup from "@/components/LoadingCup/LoadingCup";
import { UserList } from "@components/UserList";
import { Layout } from "@components/Layout";
import SearchBar from "@components/SearchBar";
import { sortUsersByCommonTags, sortWeightedUsers } from "./usersSorting";
import { DEFAULT_SEARCH_PARAMS } from "@/utils/config";
import { filterBlockedUsers } from "@/utils/helpers";

export const Browse = () => {
  /* _____________________________ State ____________________________ */
  // State
  const [browseStatus, setBrowseStatus] = useState(true);
  const [tabValue, setTabValue] = useState(0);
  const { data: userData, isLoading: userDataLoading } = useFetchCurrentUser();
  const { data: tags, isLoading: tagLoading } = useFetchAllTags();
  // Search and sort state
  const [searchParams, setSearchParams] = useState<UserSearchQuery>({
    // ...DEFAULT_SEARCH_PARAMS,
    gender: userData?.gender || DEFAULT_SEARCH_PARAMS.gender,
    sexualPreferences:
      userData?.preferences || DEFAULT_SEARCH_PARAMS.sexualPreferences,
    distance: DEFAULT_SEARCH_PARAMS.distance,
    latitude: userData?.location?.x ?? DEFAULT_SEARCH_PARAMS.latitude,
    longitude: userData?.location?.y ?? DEFAULT_SEARCH_PARAMS.longitude,
  });
  const [sortParams, setSortParams] = useState<UsersSortParams>({
    age: Order.asc,
  });
  const [sortedUsers, setSortedUsers] = useState<SortUser[]>([]);
  const [browseUsers, setBrowseUsers] = useState<SortUser[]>([]);

  // Pagination state
  const [displayedUsers, setDisplayedUsers] = useState<SortUser[]>([]);
  const [page, setPage] = useState(1);

  /* _____________________________ Search Params ____________________________ */
  // Update search params
  const updateSearchQuery = (params: UserSearchQuery) => {
    setSearchParams(params);
    setSortParams({ age: Order.asc });
    setPage(1);
  };
  // Update search params when UserData loads
  useEffect(() => {
    if (userData) {
      setSearchParams((prev) => ({
        ...prev,
        gender: userData.gender || prev.gender,
        sexualPreferences: userData.preferences || prev.sexualPreferences,
        latitude: userData.location?.x ?? prev.latitude,
        longitude: userData.location?.y ?? prev.longitude,
      }));
    }
  }, [userData]);

  // Fetch users
  let {
    data: users,
    isLoading: usersIsLoading,
    isSuccess: usersIsSuccess,
    isError: usersIsError,
  } = useFetchUsers(searchParams);

  /* _____________________________ Browse Mode ____________________________ */
  /**
   * Browse mode will sort users by weighted score
   * @param browseStatus - browse mode status
   * @param users - users list filtered by search or by default params
   * @param userData - current user data
   * @returns browseUsers - sorted users list by weighted score
   * @returns sortParams - sort params for browse mode
   * @returns page - current page
   * @note browseUsers will be updated when users or userData changes (so when search params changes)
   * @todo Maybe change the way browseUsers is updated not to depend on users like search mode
   */
  useEffect(() => {
    if (browseStatus === true && users && userData) {
      let sorted = [...users!];
      sorted = sortWeightedUsers(userData!, sorted, itemsPerPage * 4);
      setSortParams({
        totalScore: Order.desc,
      });
      setBrowseUsers(sorted);
      setPage(1);
    }
  }, [browseStatus, users, userData]);

  const { data: blockedUsers } = useFetchBlockedUsers(userData?.id);
  /* _____________________________ Sort Params ____________________________ */
  // Sort users
  useEffect(() => {
    let to_sort: SortUser[] = [];
    if (browseStatus) {
      to_sort = [...browseUsers];
    } else {
      to_sort = users ? [...users] : [];
    }
    let sorted: SortUser[] = [];
    if (to_sort && sortParams.commonTags! && userData?.tags) {
      sorted = [...to_sort];
      sorted = sortUsersByCommonTags(
        sorted,
        sortParams.commonTags,
        userData.tags
      );
    } else if (
      to_sort &&
      (sortParams.age! || sortParams.fameRate! || sortParams.distance!)
    ) {
      sorted = [...to_sort].sort((a, b) => {
        if (sortParams.age === Order.desc) {
          return b.age! - a.age!;
        }
        if (sortParams.fameRate === Order.desc) {
          return b.fameRate! - a.fameRate!;
        }
        if (sortParams.distance === Order.desc) {
          return b.distance! - a.distance!;
        }
        if (sortParams.age) {
          return a.age! - b.age!;
        }
        if (sortParams.fameRate) {
          return a.fameRate! - b.fameRate!;
        }
        if (sortParams.distance) {
          return a.distance! - b.distance!;
        }
        return 0;
      });
    } else {
      sorted = to_sort;
    }
    // Update state with new sorted array
    setSortedUsers(sorted);
    setPage(1);
  }, [sortParams, browseStatus, browseUsers, userData, users]); // Do not include sortedUsers here to avoid infinite loop

  /* _____________________________ Pagination ____________________________ */
  const itemsPerPage = 12;
  useEffect(() => {
    // Use sortedUsers as the source for pagination
    if (sortedUsers && blockedUsers) {
      console.log(blockedUsers);
      // Filter the already sorted list
      const filteredUsers = filterBlockedUsers(sortedUsers, blockedUsers);

      const offset = (page - 1) * itemsPerPage;
      const endOffset = offset + itemsPerPage;
      // Slice the filtered AND sorted list
      setDisplayedUsers(filteredUsers.slice(offset, endOffset));
    } else if (sortedUsers) {
      // Handle case where blockedUsers might not be loaded yet
      const offset = (page - 1) * itemsPerPage;
      const endOffset = offset + itemsPerPage;
      setDisplayedUsers(sortedUsers.slice(offset, endOffset));
    } else {
      setDisplayedUsers([]); // Ensure displayedUsers is empty if sortedUsers is not ready
    }
    // The dependency array should primarily react to changes in the source list (sortedUsers) and pagination controls (page)
  }, [page, sortedUsers, blockedUsers]); // Removed 'users' dependency as it's implicitly handled via sortedUsers

  const handlePageChange = (
    event: React.ChangeEvent<unknown>,
    value: number
  ) => {
    setPage(value);
  };

  /* _____________________________ Content ____________________________ */
  let content;
  if (usersIsLoading || userDataLoading || tagLoading) {
    content = <LoadingCup />;
    return content;
  }
  if (usersIsSuccess && users) {
    content = (
      <>
        <UserList users={displayedUsers} match={false} />
        <Pagination
          count={Math.ceil(sortedUsers.length / itemsPerPage)}
          page={page}
          onChange={handlePageChange}
          style={{ marginTop: "16px" }}
        />
      </>
    );
  }

  if (usersIsError) {
    content = "Error fetching users";
  }
  if (!userData) {
    content = "Please log in to view";
  }

  const handleChange = (event: React.SyntheticEvent, newValue: boolean) => {
    setBrowseStatus(!browseStatus);
    setTabValue(newValue ? 1 : 0);
  };

  return (
    <Layout>
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
          alignItems: "center",
          height: "100%",
          width: "100%",
        }}
      >
        <SearchBar
          browseStatus={browseStatus}
          userData={userData}
          tags={tags}
          searchParams={searchParams}
          sortParams={sortParams}
          setSortParams={setSortParams}
          onSubmit={updateSearchQuery}
        />
        <AppBar position="static" color="default">
          <Tabs
            value={tabValue}
            onChange={handleChange}
            indicatorColor="secondary"
            textColor="secondary"
            variant="fullWidth"
            aria-label="browse or search tabs"
          >
            <Tab label="Browse" />
            <Tab label="Search" />
          </Tabs>
        </AppBar>

        {content}
      </Box>
    </Layout>
  );
};
