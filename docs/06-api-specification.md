# API Specification

## Base Information

### Base URL
```
Development: http://localhost:5038/api
Production: https://api.yourdomain.com/api
```

### API Versioning
No versioning in URL (MVP). Future: Header-based versioning if needed.

### Authentication
Bearer token authentication (JWT):
```
Authorization: Bearer {access_token}
```

### Content Type
- **Request**: `application/json` (except file uploads: `multipart/form-data`)
- **Response**: `application/json`
- **Character Encoding**: UTF-8

### Timestamps
All timestamps in **ISO 8601** format (UTC):
```
2025-11-10T17:30:00Z
```

---

## Response Formats

### Success Response
```json
{
  "data": {
    // Response payload
  },
  "meta": {
    // Optional metadata (pagination, etc.)
  }
}
```

### Error Response
```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      // Optional additional context
    }
  }
}
```

### HTTP Status Codes
- `200 OK` - Successful GET, PATCH, DELETE
- `201 Created` - Successful POST (resource created)
- `204 No Content` - Successful DELETE (no response body)
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Authentication required/failed
- `403 Forbidden` - Authenticated but not authorized
- `404 Not Found` - Resource doesn't exist
- `409 Conflict` - Resource already exists
- `413 Payload Too Large` - File size exceeded
- `415 Unsupported Media Type` - Invalid file format
- `422 Unprocessable Entity` - Business logic error
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server-side error

---

## Rate Limiting

### Limits (per user per hour)
- **Image uploads**: 50/hour
- **Post creation**: 100/hour
- **Follow/unfollow**: 200/hour
- **General API**: 1000/hour

### Response Headers
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 2025-11-10T18:00:00Z
```

### Rate Limit Exceeded Response (429)
```json
{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Too many requests. Try again later.",
    "details": {
      "retryAfter": "2025-11-10T18:00:00Z"
    }
  }
}
```

---

## Pagination

### Cursor-Based (Feeds/Timelines)
**Request**:
```
GET /api/feed?limit=20&before_id=post_123
```

**Response**:
```json
{
  "posts": [...],
  "meta": {
    "has_more": true,
    "next_cursor": "post_456"
  }
}
```

### Offset-Based (Lists)
**Request**:
```
GET /api/users/johndoe/followers?limit=20&offset=40
```

**Response**:
```json
{
  "followers": [...],
  "meta": {
    "total": 150,
    "limit": 20,
    "offset": 40
  }
}
```

---

## Authentication Endpoints

### Register

**POST** `/api/auth/register`

Creates new user account and sends verification email.

**Request Body**:
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "displayName": "John Doe"
}
```

**Validation**:
- `username`: 1-100 chars, alphanumeric only, unique
- `email`: Valid email format, unique
- `password`: Min 8 chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
- `displayName`: 1-100 chars

**Response 201**:
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "email": "john@example.com",
    "displayName": "John Doe",
    "bio": null,
    "profileImage": null,
    "backgroundImage": null,
    "followerCount": 0,
    "followingCount": 0,
    "postCount": 0,
    "emailVerified": false,
    "createdAt": "2025-11-10T17:30:00Z"
  },
  "token": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "550e8400-e29b-41d4-a716-446655440001",
    "expiresAt": "2025-11-10T17:45:00Z"
  }
}
```

**Error 400** (Validation):
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": {
      "username": ["Username is already taken"],
      "password": ["Password must contain at least one uppercase letter"]
    }
  }
}
```

---

### Login

**POST** `/api/auth/login`

Authenticates user and returns JWT tokens.

**Request Body**:
```json
{
  "usernameOrEmail": "johndoe",
  "password": "SecurePass123!"
}
```

**Response 200**:
```json
{
  "user": { /* user object */ },
  "token": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "550e8400-e29b-41d4-a716-446655440002",
    "expiresAt": "2025-11-10T17:45:00Z"
  }
}
```

**Error 401**:
```json
{
  "error": {
    "code": "INVALID_CREDENTIALS",
    "message": "Invalid username or password"
  }
}
```

**Error 403** (Email not verified):
```json
{
  "error": {
    "code": "EMAIL_NOT_VERIFIED",
    "message": "Please verify your email address"
  }
}
```

---

### Refresh Token

**POST** `/api/auth/refresh`

Exchanges refresh token for new access token.

**Request Body**:
```json
{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440002"
}
```

**Response 200**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440003",
  "expiresAt": "2025-11-10T18:00:00Z"
}
```

**Error 401**:
```json
{
  "error": {
    "code": "INVALID_REFRESH_TOKEN",
    "message": "Refresh token is invalid or expired"
  }
}
```

---

### Verify Email

**POST** `/api/auth/verify-email`

Verifies user's email address using token from email.

**Request Body**:
```json
{
  "token": "abc123def456"
}
```

**Response 200**:
```json
{
  "message": "Email verified successfully"
}
```

**Error 400**:
```json
{
  "error": {
    "code": "INVALID_TOKEN",
    "message": "Verification token is invalid or expired"
  }
}
```

---

### Forgot Password

**POST** `/api/auth/forgot-password`

Sends password reset email to user.

**Request Body**:
```json
{
  "email": "john@example.com"
}
```

**Response 200** (Always succeeds for security):
```json
{
  "message": "If an account exists, a password reset email has been sent"
}
```

---

### Reset Password

**POST** `/api/auth/reset-password`

Resets password using token from email.

**Request Body**:
```json
{
  "token": "abc123def456",
  "newPassword": "NewSecurePass456!"
}
```

**Response 200**:
```json
{
  "message": "Password reset successfully"
}
```

**Error 400**:
```json
{
  "error": {
    "code": "INVALID_TOKEN",
    "message": "Reset token is invalid or expired"
  }
}
```

---

### Check Username Availability

**GET** `/api/auth/check-username?username=johndoe`

Checks if username is available.

**Response 200**:
```json
{
  "available": false,
  "username": "johndoe"
}
```

---

### Logout

**POST** `/api/auth/logout`

**Authorization**: Required

Revokes refresh token.

**Request Body**:
```json
{
  "refreshToken": "550e8400-e29b-41d4-a716-446655440002"
}
```

**Response 200**:
```json
{
  "message": "Logged out successfully"
}
```

---

## User Endpoints

### Get User Profile

**GET** `/api/users/{username}`

Retrieves public user profile.

**Response 200**:
```json
{
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "johndoe",
    "displayName": "John Doe",
    "bio": "Software developer ☕",
    "profileImage": {
      "id": "img_123",
      "urls": {
        "thumbnail": "http://localhost:5038/uploads/.../thumbnail.webp",
        "small": "http://localhost:5038/uploads/.../small.webp",
        "medium": "http://localhost:5038/uploads/.../medium.webp"
      },
      "altText": "Profile picture"
    },
    "backgroundImage": {
      "id": "img_456",
      "urls": {
        "large": "http://localhost:5038/uploads/.../large.webp"
      },
      "altText": null
    },
    "followerCount": 150,
    "followingCount": 80,
    "postCount": 342,
    "isFollowing": false,
    "isFollowedBy": true,
    "createdAt": "2025-01-15T10:00:00Z"
  }
}
```

**Notes**:
- `isFollowing` / `isFollowedBy` only included if requester is authenticated
- Images null if not set

---

### Update Current User Profile

**PATCH** `/api/users/me`

**Authorization**: Required

Updates authenticated user's profile.

**Request Body** (all fields optional):
```json
{
  "displayName": "John D.",
  "bio": "Full-stack developer",
  "profileImageId": "img_123",
  "backgroundImageId": "img_456"
}
```

**Response 200**:
```json
{
  "user": { /* updated user object */ }
}
```

**Notes**:
- Send `null` to remove profile/background image
- Images must be owned by user

---

### Change Password

**PATCH** `/api/users/me/password`

**Authorization**: Required

**Request Body**:
```json
{
  "currentPassword": "OldPass123!",
  "newPassword": "NewSecurePass456!"
}
```

**Response 200**:
```json
{
  "message": "Password updated successfully"
}
```

**Error 400**:
```json
{
  "error": {
    "code": "INVALID_PASSWORD",
    "message": "Current password is incorrect"
  }
}
```

---

### Delete Account

**DELETE** `/api/users/me`

**Authorization**: Required

Permanently deletes user account.

**Request Body**:
```json
{
  "password": "SecurePass123!"
}
```

**Response 200**:
```json
{
  "message": "Account deleted successfully"
}
```

**Notes**:
- Requires password confirmation
- Cascades to delete user's posts, likes, follows, etc.
- Uploaded images remain (other users may reference)

---

### Get User's Posts

**GET** `/api/users/{username}/posts?limit=20&beforeId={postId}`

Retrieves user's posts (not replies).

**Response 200**:
```json
{
  "posts": [
    {
      "id": "post_123",
      "user": { /* user object */ },
      "content": "Hello world! 🚀",
      "images": [ /* image objects */ ],
      "parentPostId": null,
      "likeCount": 42,
      "replyCount": 5,
      "isLiked": false,
      "isBookmarked": false,
      "createdAt": "2025-11-10T15:00:00Z"
    }
  ],
  "meta": {
    "hasMore": true
  }
}
```

---

### Get User's Followers

**GET** `/api/users/{username}/followers?limit=20&offset=0`

**Response 200**:
```json
{
  "followers": [
    {
      "id": "user_789",
      "username": "janedoe",
      "displayName": "Jane Doe",
      "profileImage": { /* image object */ },
      "bio": "Designer",
      "isFollowing": false
    }
  ],
  "meta": {
    "total": 150,
    "limit": 20,
    "offset": 0
  }
}
```

---

### Get User's Following

**GET** `/api/users/{username}/following?limit=20&offset=0`

**Response 200**: Same format as followers.

---

## Post Endpoints

### Create Post

**POST** `/api/posts`

**Authorization**: Required

Creates new post or reply.

**Request Body**:
```json
{
  "content": "Just deployed my first app! 🚀 #coding",
  "parentPostId": null,
  "imageIds": ["img_123", "img_456"]
}
```

**Validation**:
- `content`: 1-280 chars, required
- `imageIds`: Max 4, must be owned by user, must exist and be processed
- `parentPostId`: Must exist if provided

**Response 201**:
```json
{
  "post": {
    "id": "post_123",
    "user": { /* user object */ },
    "content": "Just deployed my first app! 🚀 #coding",
    "images": [
      {
        "id": "img_123",
        "urls": { /* all variants */ },
        "altText": "Screenshot",
        "displayOrder": 1
      }
    ],
    "parentPostId": null,
    "likeCount": 0,
    "replyCount": 0,
    "isLiked": false,
    "isBookmarked": false,
    "createdAt": "2025-11-10T15:00:00Z"
  }
}
```

**Notes**:
- Hashtags automatically extracted and stored
- If reply, creates notification for parent post author

---

### Get Post

**GET** `/api/posts/{id}`

Retrieves post details.

**Response 200**:
```json
{
  "post": { /* post object */ },
  "parentPost": { /* parent post if reply */ }
}
```

---

### Delete Post

**DELETE** `/api/posts/{id}`

**Authorization**: Required (must be post author)

**Response 200**:
```json
{
  "message": "Post deleted successfully"
}
```

**Notes**:
- Only author can delete
- Cascades: likes, bookmarks, post-image links, post-hashtag links
- Images remain (not deleted)
- Decrements user's post count
- Decrements parent's reply count (if reply)

---

### Get Post Replies

**GET** `/api/posts/{id}/replies?limit=20&beforeId={replyId}`

**Response 200**:
```json
{
  "replies": [ /* array of post objects */ ],
  "meta": {
    "hasMore": true
  }
}
```

**Notes**:
- Returns direct replies only (not nested)
- Ordered chronologically (oldest first)

---

### Get Post Likes

**GET** `/api/posts/{id}/likes?limit=20&offset=0`

**Response 200**:
```json
{
  "users": [
    {
      "id": "user_789",
      "username": "janedoe",
      "displayName": "Jane Doe",
      "profileImage": { /* image object */ },
      "likedAt": "2025-11-10T15:10:00Z"
    }
  ],
  "meta": {
    "total": 42
  }
}
```

---

### Like Post

**POST** `/api/posts/{id}/like`

**Authorization**: Required

**Response 200**:
```json
{
  "postId": "post_123",
  "isLiked": true,
  "likeCount": 43
}
```

**Notes**:
- Idempotent (liking twice has no effect)
- Increments post.likeCount
- No notification generated

---

### Unlike Post

**DELETE** `/api/posts/{id}/like`

**Authorization**: Required

**Response 200**:
```json
{
  "postId": "post_123",
  "isLiked": false,
  "likeCount": 42
}
```

---

## Image Endpoints

### Upload Image

**POST** `/api/images`

**Authorization**: Required

**Content-Type**: `multipart/form-data`

**Request Form Data**:
```
image: [binary file]
altText: "Description of image" (optional)
```

**Response 201**:
```json
{
  "image": {
    "id": "img_123",
    "urls": {
      "thumbnail": "http://localhost:5038/uploads/.../thumbnail.webp",
      "small": "http://localhost:5038/uploads/.../small.webp",
      "medium": "http://localhost:5038/uploads/.../medium.webp",
      "large": "http://localhost:5038/uploads/.../large.webp"
    },
    "width": 1920,
    "height": 1080,
    "altText": "Description of image",
    "processingStatus": "complete",
    "createdAt": "2025-11-10T14:30:00Z"
  }
}
```

**Validation**:
- Max file size: 10MB
- Supported formats: JPEG, PNG, GIF, WebP
- Processing may be async; check `processingStatus`
- URLs available immediately but may 404 until processing complete

**Error 413**:
```json
{
  "error": {
    "code": "FILE_TOO_LARGE",
    "message": "File size exceeds 10MB limit"
  }
}
```

**Error 415**:
```json
{
  "error": {
    "code": "UNSUPPORTED_FILE_TYPE",
    "message": "File type not supported. Allowed: JPEG, PNG, GIF, WebP"
  }
}
```

---

### Get Image Details

**GET** `/api/images/{id}`

**Response 200**:
```json
{
  "image": {
    "id": "img_123",
    "urls": { /* all variants */ },
    "width": 1920,
    "height": 1080,
    "altText": "Description",
    "uploadedBy": {
      "id": "user_123",
      "username": "johndoe"
    },
    "processingStatus": "complete",
    "createdAt": "2025-11-10T14:30:00Z"
  }
}
```

---

### Update Image Metadata

**PATCH** `/api/images/{id}`

**Authorization**: Required (must be uploader)

**Request Body**:
```json
{
  "altText": "Updated description"
}
```

**Response 200**:
```json
{
  "image": { /* updated image object */ }
}
```

---

### Delete Image

**DELETE** `/api/images/{id}`

**Authorization**: Required (must be uploader)

**Response 200**:
```json
{
  "message": "Image deleted successfully"
}
```

**Error 409** (if in use):
```json
{
  "error": {
    "code": "IMAGE_IN_USE",
    "message": "Cannot delete image while in use",
    "details": {
      "usedIn": ["post:post_123", "profile:user_456"]
    }
  }
}
```

---

## Feed Endpoints

### Get Personalized Feed

**GET** `/api/feed?limit=20&beforeId={postId}`

**Authorization**: Required

Returns posts from followed users.

**Response 200**:
```json
{
  "posts": [ /* array of post objects */ ],
  "meta": {
    "hasMore": true
  }
}
```

**Notes**:
- Returns posts from followed users only
- Excludes replies (`parentPostId IS NULL`)
- Chronological order (newest first)
- Empty if user follows nobody

---

### Get Public Feed

**GET** `/api/posts/public?limit=20&beforeId={postId}`

Returns all public posts.

**Response 200**: Same format as personalized feed.

**Notes**:
- Returns all posts (not just following)
- Excludes replies
- Available without authentication

---

## Search Endpoints

### Search Users

**GET** `/api/search/users?q={query}&limit=20&offset=0`

**Response 200**:
```json
{
  "users": [ /* array of user objects */ ],
  "meta": {
    "total": 42,
    "query": "john"
  }
}
```

**Notes**:
- Searches username and display name
- Case-insensitive
- Uses LIKE search (simple, no ranking)

---

### Search Posts

**GET** `/api/search/posts?q={query}&limit=20&beforeId={postId}`

**Response 200**:
```json
{
  "posts": [ /* array of post objects */ ],
  "meta": {
    "hasMore": true,
    "query": "coding"
  }
}
```

**Notes**:
- Searches post content
- Case-insensitive
- Uses LIKE search

---

### Search Hashtags

**GET** `/api/search/hashtags?q={query}&limit=20`

**Response 200**:
```json
{
  "hashtags": [
    {
      "id": "hashtag_123",
      "tag": "coding",
      "postCount": 1234,
      "createdAt": "2025-01-01T00:00:00Z"
    }
  ],
  "meta": {
    "query": "cod"
  }
}
```

**Notes**:
- Searches hashtag tags
- Returns tags containing query
- Ordered by post count (most popular first)

---

## Other Endpoints

### Follow User

**POST** `/api/users/{username}/follow`

**Authorization**: Required

**Response 200**:
```json
{
  "user": { /* followed user object */ },
  "isFollowing": true
}
```

**Notes**:
- Idempotent
- Creates notification for followed user
- Increments follower/following counts

---

### Unfollow User

**DELETE** `/api/users/{username}/follow`

**Authorization**: Required

**Response 200**:
```json
{
  "user": { /* unfollowed user object */ },
  "isFollowing": false
}
```

---

### Get Bookmarks

**GET** `/api/bookmarks?limit=20&beforeId={bookmarkId}`

**Authorization**: Required

**Response 200**:
```json
{
  "bookmarks": [
    {
      "id": "bookmark_123",
      "post": { /* post object */ },
      "createdAt": "2025-11-10T15:00:00Z"
    }
  ],
  "meta": {
    "hasMore": true
  }
}
```

---

### Bookmark Post

**POST** `/api/bookmarks`

**Authorization**: Required

**Request Body**:
```json
{
  "postId": "post_123"
}
```

**Response 201**:
```json
{
  "bookmark": {
    "id": "bookmark_123",
    "postId": "post_123",
    "createdAt": "2025-11-10T15:00:00Z"
  }
}
```

---

### Remove Bookmark

**DELETE** `/api/bookmarks/{id}`

**Authorization**: Required

**Response 200**:
```json
{
  "message": "Bookmark removed successfully"
}
```

---

### Get Notifications

**GET** `/api/notifications?limit=20&unreadOnly=false&beforeId={notificationId}`

**Authorization**: Required

**Response 200**:
```json
{
  "notifications": [
    {
      "id": "notif_123",
      "type": "new_follower",
      "actor": { /* user who followed */ },
      "post": null,
      "isRead": false,
      "createdAt": "2025-11-10T15:00:00Z"
    },
    {
      "id": "notif_456",
      "type": "reply",
      "actor": { /* user who replied */ },
      "post": { /* reply post */ },
      "isRead": true,
      "createdAt": "2025-11-10T14:00:00Z"
    }
  ],
  "meta": {
    "hasMore": true,
    "unreadCount": 5
  }
}
```

**Notification Types**:
- `new_follower`: Actor followed you
- `reply`: Actor replied to your post

---

### Mark Notification as Read

**PATCH** `/api/notifications/{id}/read`

**Authorization**: Required

**Response 200**:
```json
{
  "notification": { /* updated notification object */ }
}
```

---

### Mark All Notifications as Read

**PATCH** `/api/notifications/read-all`

**Authorization**: Required

**Response 200**:
```json
{
  "message": "All notifications marked as read",
  "count": 5
}
```

---

### Get Posts by Hashtag

**GET** `/api/hashtags/{tag}/posts?limit=20&beforeId={postId}`

**Response 200**:
```json
{
  "posts": [ /* array of post objects */ ],
  "meta": {
    "hasMore": true,
    "hashtag": "coding"
  }
}
```

---

## WebSocket (Future)

Real-time features (SignalR) - Future enhancement:
- New notifications
- New posts in feed
- Typing indicators (DMs)
- Online status

---

## Summary

**Total Endpoints**: ~40

**Authentication**: 9 endpoints
**Users**: 6 endpoints
**Posts**: 6 endpoints
**Images**: 4 endpoints
**Feed**: 2 endpoints
**Search**: 3 endpoints
**Social**: 7 endpoints (follow, bookmark, notifications)
**Hashtags**: 1 endpoint

All endpoints documented with:
- Request/response formats
- Validation rules
- Error responses
- Business logic notes

**Complete interactive documentation**: http://localhost:5038/swagger
