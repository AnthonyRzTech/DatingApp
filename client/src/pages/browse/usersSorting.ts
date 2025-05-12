import { SortUser, User, Tag, Order } from "@/app/interfaces";

const calculateCommonTags = (
  currentUserTags: Tag[],
  usersToUpdate: SortUser[]
): SortUser[] => {
  usersToUpdate.forEach((user) => {
    let tagsScore = 0;
    if (!user.tags || !Array.isArray(user.tags) || user.tags.length === 0) {
      user.commonTags = 0;
      return;
    }
    currentUserTags.forEach((tag) => {
      if (tag && user.tags?.some((userTag) => userTag && userTag === tag)) {
        tagsScore++;
      }
    });
    user.commonTags = tagsScore;
  });
  return usersToUpdate;
};

export const sortUsersByCommonTags = (
  users: User[],
  commonTagsOrder: Order,
  currentUserTags: Tag[]
): SortUser[] => {
  let sortedUsers: SortUser[] = users;
  if (!currentUserTags || currentUserTags.length === 0) {
    return sortedUsers;
  }

  // Make a score for each user based on common tags
  sortedUsers = calculateCommonTags(currentUserTags, sortedUsers);

  // Sort users by score
  if (commonTagsOrder === Order.asc) {
    sortedUsers.sort((a, b) => (a.commonTags ?? 0) - (b.commonTags ?? 0));
  } else {
    sortedUsers.sort((a, b) => (b.commonTags ?? 0) - (a.commonTags ?? 0));
  }
  return sortedUsers;
};

/**
 * Criteria weights for sorting users
 * @param age - Age difference - 5000 score is the best with only 1 or no difference
 * @param fameRate - FameRate difference - 1000 score is the best with only 1 difference
 * @param distance - Distance score - 5000 score is the best with 0-5km distance
 * @param tags - Common tags score - 5000 score is the best with 10 common tags
 */
const criteriaWeights = {
  age: 5000,
  // age: 0,
  // fameRate: 1000,
  fameRate: 0,
  distance: 5000,
  // tags: 500,
  tags: 0,
};

const calculateUserDistanceScore = (userDistance: number): number => {
  let score = 0;
  if (userDistance <= 5) {
    score += criteriaWeights.distance; // Maximum score for very close users (0-5km)
  } else if (userDistance <= 10) {
    score += criteriaWeights.distance - 1000 * 1; // 5-10km
  } else if (userDistance <= 20) {
    score += criteriaWeights.distance - 1000 * 2; // 10-20km
  } else if (userDistance <= 50) {
    score += criteriaWeights.distance - 1000 * 3; // 20-50km
  } else if (userDistance <= 100) {
    score += criteriaWeights.distance - 1000 * 4; // 50-100km
  } else {
    // For distances > 100km, score decreases linearly
    // Base score of 1000 decreasing by 10 points per km
    score += Math.max(
      0,
      criteriaWeights.distance - 1000 * 4 - userDistance / 10
    );
  }
  return score;
};

const calculateAgeScore = (userAge: number, currentUserAge: number): number => {
  let ageDifference = Math.abs(userAge - currentUserAge);
  const ageScore = ageDifference * 300;
  const score = Math.max(0, criteriaWeights.age - ageScore);
  return score;
};

const calculateUserScore = (user: SortUser, currentUser: User): number => {
  let score: number = 0;
  if (user.age && currentUser.age) {
    score += calculateAgeScore(user.age, currentUser.age);
  }
  if (user.fameRate !== undefined && currentUser.fameRate !== undefined) {
    let fameRateDifference = Math.abs(user.fameRate - currentUser.fameRate);
    // If the user has no fameRate, give artificial score
    if (currentUser.fameRate === 0 && user.fameRate !== 0) {
      fameRateDifference = Math.abs(user.fameRate - 5);
    }
    if (fameRateDifference === 0) {
      score += criteriaWeights.fameRate;
    } else {
      score += criteriaWeights.fameRate / fameRateDifference;
    }
  }
  if (user.distance !== undefined) {
    score += calculateUserDistanceScore(user.distance);
  }
  if (user.commonTags) {
    if (user.commonTags >= 10) {
      score += criteriaWeights.tags * 10;
    } else {
      score += criteriaWeights.tags * user.commonTags;
    }
  }
  return score;
};

export const sortWeightedUsers = (
  currentUser: User,
  users: SortUser[],
  limit?: number
): SortUser[] => {
  currentUser.age =
    new Date().getFullYear() - new Date(currentUser.dateOfBirth).getFullYear();
  let sortedUsers: SortUser[] = users;
  sortedUsers = calculateCommonTags(currentUser.tags, sortedUsers);
  sortedUsers.forEach((user) => {
    user.totalScore = calculateUserScore(user, currentUser);
  });
  sortedUsers.sort((a, b) => (b.totalScore ?? 0) - (a.totalScore ?? 0));
  if (limit) {
    sortedUsers = sortedUsers.slice(0, limit);
  }
  return sortedUsers;
};
