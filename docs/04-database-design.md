# Database Design

## Overview

This document provides the complete database schema for the Twitter Clone application. The design uses PostgreSQL but avoids PostgreSQL-specific features to allow migration to SQL Server in the future.

## Design Principles

1. **Normalization**: 3NF (Third Normal Form) to minimize redundancy
2. **Integrity**: Foreign keys and constraints enforce data validity
3. **Performance**: Strategic indexing for common queries
4. **Scalability**: Designed for millions of rows
5. **Database-Agnostic**: Standard SQL features only

## Entity Relationship Diagram

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│    User     │────────<│    Follow    │>────────│    User     │
│             │         │              │         │  (followed) │
└──────┬──────┘         └──────────────┘         └─────────────┘
       │
       │ 1:N
       │
       ├──────────┬─────────────┬──────────────┬───────────────┐
       │          │             │              │               │
       ▼          ▼             ▼              ▼               ▼
  ┌────────┐ ┌────────┐   ┌──────────┐  ┌──────────┐  ┌──────────────┐
  │  Post  │ │ Image  │   │   Like   │  │ Bookmark │  │Notification  │
  └───┬────┘ └────┬───┘   └──────────┘  └──────────┘  └──────────────┘
      │           │
      │           │
      │ N:M       │ N:M
      │           │
  ┌───▼──────┐ ┌─▼────────┐
  │PostImage │ │ Hashtag  │
  └──────────┘ └──────┬───┘
                      │
                  ┌───▼────────┐
                  │PostHashtag │
                  └────────────┘

┌──────────────────────┐
│   RefreshToken       │
│ EmailVerificationToken│
│ PasswordResetToken   │
└──────────────────────┘
```

## Complete Schema

### 1. User

**Purpose**: Represents a user account.

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    bio TEXT,
    profile_image_id UUID,
    background_image_id UUID,
    email_verified BOOLEAN DEFAULT FALSE,
    email_verified_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT uq_users_username UNIQUE (username),
    CONSTRAINT uq_users_email UNIQUE (email),
    CONSTRAINT ck_users_bio_length CHECK (LENGTH(bio) <= 500)
);

CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_created_at ON users(created_at DESC);
CREATE INDEX idx_users_email_verified ON users(email_verified);
```

**Columns**:
- `id`: Primary key (UUID for global uniqueness)
- `username`: Unique, alphanumeric, 1-100 characters
- `email`: Unique, valid email format
- `display_name`: User's display name, 1-100 characters
- `password_hash`: BCrypt hash (60 characters)
- `bio`: Optional bio, max 500 characters
- `profile_image_id`: FK to images table (nullable)
- `background_image_id`: FK to images table (nullable)
- `email_verified`: Email verification status
- `email_verified_at`: When email was verified
- `created_at`: Account creation timestamp
- `updated_at`: Last profile update timestamp

**Constraints**:
- Username must be unique
- Email must be unique
- Bio max 500 characters

**Computed Properties** (not in database):
- `follower_count`: Computed from `follows` table
- `following_count`: Computed from `follows` table
- `post_count`: Computed from `posts` table

---

### 2. Image

**Purpose**: Stores image metadata and storage paths.

```sql
CREATE TABLE images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    uploaded_by_user_id UUID NOT NULL,
    original_filename VARCHAR(255) NOT NULL,
    mime_type VARCHAR(50) NOT NULL,
    file_size INTEGER NOT NULL,
    storage_path VARCHAR(500) NOT NULL,
    width INTEGER NOT NULL,
    height INTEGER NOT NULL,
    alt_text TEXT,
    processing_status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_images_user FOREIGN KEY (uploaded_by_user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT ck_images_processing_status CHECK (
        processing_status IN ('pending', 'processing', 'complete', 'failed')
    ),
    CONSTRAINT ck_images_file_size CHECK (file_size > 0 AND file_size <= 10485760)
);

CREATE INDEX idx_images_uploaded_by ON images(uploaded_by_user_id);
CREATE INDEX idx_images_created_at ON images(created_at DESC);
CREATE INDEX idx_images_processing_status ON images(processing_status);
```

**Columns**:
- `id`: Primary key
- `uploaded_by_user_id`: User who uploaded (FK to users)
- `original_filename`: Original file name
- `mime_type`: MIME type (image/jpeg, image/png, etc.)
- `file_size`: Size in bytes (max 10MB)
- `storage_path`: Path to image directory
- `width`: Original image width
- `height`: Original image height
- `alt_text`: Accessibility description (optional)
- `processing_status`: pending | processing | complete | failed
- `created_at`: Upload timestamp

**Storage Structure**:
```
uploads/
  {user_id}/
    {year}/
      {month}/
        {image_id}/
          thumbnail.webp   (150x150, square crop)
          small.webp       (400px width)
          medium.webp      (800px width)
          large.webp       (1200px width)
          original.{ext}   (backup)
```

---

### 3. Post

**Purpose**: Represents a tweet/post.

```sql
CREATE TABLE posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    content TEXT NOT NULL,
    parent_post_id UUID,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_posts_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_posts_parent FOREIGN KEY (parent_post_id) 
        REFERENCES posts(id) ON DELETE CASCADE,
    CONSTRAINT ck_posts_content_length CHECK (LENGTH(content) >= 1 AND LENGTH(content) <= 280)
);

CREATE INDEX idx_posts_user_id ON posts(user_id);
CREATE INDEX idx_posts_parent_post_id ON posts(parent_post_id);
CREATE INDEX idx_posts_created_at ON posts(created_at DESC);
CREATE INDEX idx_posts_user_created ON posts(user_id, created_at DESC);
```

**Columns**:
- `id`: Primary key
- `user_id`: Author of the post (FK to users)
- `content`: Post text, 1-280 characters
- `parent_post_id`: If reply, references parent post (nullable)
- `created_at`: Post creation timestamp

**Computed Properties** (not in database):
- `like_count`: Computed from `likes` table
- `reply_count`: Computed from `posts` where `parent_post_id = this.id`
- `bookmark_count`: Computed from `bookmarks` table (optional)

**Business Rules**:
- Content: 1-280 characters
- Parent post must exist if `parent_post_id` is set
- Replies can reply to replies (nested threading)
- Deleting parent doesn't delete replies (CASCADE can be changed to SET NULL)

---

### 4. PostImage

**Purpose**: Junction table linking posts and images (N:M relationship).

```sql
CREATE TABLE post_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID NOT NULL,
    image_id UUID NOT NULL,
    display_order INTEGER NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_post_images_post FOREIGN KEY (post_id) 
        REFERENCES posts(id) ON DELETE CASCADE,
    CONSTRAINT fk_post_images_image FOREIGN KEY (image_id) 
        REFERENCES images(id) ON DELETE CASCADE,
    CONSTRAINT uq_post_images_post_image UNIQUE (post_id, image_id),
    CONSTRAINT uq_post_images_post_order UNIQUE (post_id, display_order),
    CONSTRAINT ck_post_images_display_order CHECK (display_order >= 1 AND display_order <= 4)
);

CREATE INDEX idx_post_images_post_id ON post_images(post_id);
CREATE INDEX idx_post_images_image_id ON post_images(image_id);
```

**Columns**:
- `id`: Primary key
- `post_id`: FK to posts
- `image_id`: FK to images
- `display_order`: 1-4 (order images appear in post)
- `created_at`: When image was attached

**Constraints**:
- Same image can't be attached twice to same post
- Display order must be unique per post
- Max 4 images per post

---

### 5. Like

**Purpose**: Represents a user liking a post.

```sql
CREATE TABLE likes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    post_id UUID NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_likes_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_likes_post FOREIGN KEY (post_id) 
        REFERENCES posts(id) ON DELETE CASCADE,
    CONSTRAINT uq_likes_user_post UNIQUE (user_id, post_id)
);

CREATE INDEX idx_likes_user_id ON likes(user_id);
CREATE INDEX idx_likes_post_id ON likes(post_id);
CREATE INDEX idx_likes_created_at ON likes(created_at DESC);
```

**Columns**:
- `id`: Primary key
- `user_id`: User who liked (FK to users)
- `post_id`: Post that was liked (FK to posts)
- `created_at`: When like occurred

**Constraints**:
- User can only like a post once

---

### 6. Follow

**Purpose**: Represents user following relationships.

```sql
CREATE TABLE follows (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    follower_id UUID NOT NULL,
    following_id UUID NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_follows_follower FOREIGN KEY (follower_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_follows_following FOREIGN KEY (following_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_follows_follower_following UNIQUE (follower_id, following_id),
    CONSTRAINT ck_follows_no_self_follow CHECK (follower_id != following_id)
);

CREATE INDEX idx_follows_follower_id ON follows(follower_id);
CREATE INDEX idx_follows_following_id ON follows(following_id);
CREATE INDEX idx_follows_created_at ON follows(created_at DESC);
```

**Columns**:
- `id`: Primary key
- `follower_id`: User who is following (FK to users)
- `following_id`: User being followed (FK to users)
- `created_at`: When follow occurred

**Constraints**:
- User cannot follow themselves
- User can only follow another user once

---

### 7. Bookmark

**Purpose**: Represents a user bookmarking a post.

```sql
CREATE TABLE bookmarks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    post_id UUID NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_bookmarks_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_bookmarks_post FOREIGN KEY (post_id) 
        REFERENCES posts(id) ON DELETE CASCADE,
    CONSTRAINT uq_bookmarks_user_post UNIQUE (user_id, post_id)
);

CREATE INDEX idx_bookmarks_user_id ON bookmarks(user_id);
CREATE INDEX idx_bookmarks_post_id ON bookmarks(post_id);
CREATE INDEX idx_bookmarks_user_created ON bookmarks(user_id, created_at DESC);
```

**Columns**:
- `id`: Primary key
- `user_id`: User who bookmarked (FK to users)
- `post_id`: Post that was bookmarked (FK to posts)
- `created_at`: When bookmark occurred

**Constraints**:
- User can only bookmark a post once

---

### 8. Notification

**Purpose**: Stores user notifications.

```sql
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    type VARCHAR(20) NOT NULL,
    actor_id UUID NOT NULL,
    post_id UUID,
    is_read BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_notifications_actor FOREIGN KEY (actor_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_notifications_post FOREIGN KEY (post_id) 
        REFERENCES posts(id) ON DELETE CASCADE,
    CONSTRAINT ck_notifications_type CHECK (type IN ('new_follower', 'reply'))
);

CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_is_read ON notifications(is_read);
CREATE INDEX idx_notifications_user_unread ON notifications(user_id, is_read, created_at DESC);
CREATE INDEX idx_notifications_created_at ON notifications(created_at DESC);
```

**Columns**:
- `id`: Primary key
- `user_id`: Recipient of notification (FK to users)
- `type`: 'new_follower' | 'reply'
- `actor_id`: User who triggered notification (FK to users)
- `post_id`: Related post (for replies, nullable)
- `is_read`: Read status
- `created_at`: When notification was created

**Notification Types**:
- **new_follower**: `actor_id` followed `user_id` (`post_id` is NULL)
- **reply**: `actor_id` replied to `user_id`'s post (`post_id` is the reply)

---

### 9. Hashtag

**Purpose**: Stores unique hashtags.

```sql
CREATE TABLE hashtags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tag VARCHAR(100) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT uq_hashtags_tag UNIQUE (tag)
);

CREATE INDEX idx_hashtags_tag ON hashtags(tag);
CREATE INDEX idx_hashtags_created_at ON hashtags(created_at DESC);
```

**Columns**:
- `id`: Primary key
- `tag`: Hashtag text (lowercase, no #), unique
- `created_at`: When first used

**Computed Properties**:
- `post_count`: Computed from `post_hashtags` table

**Business Rules**:
- Tags stored in lowercase for consistency
- Extracted from post content using regex: `#[a-zA-Z0-9_]+`
- Max 100 characters per tag

---

### 10. PostHashtag

**Purpose**: Junction table linking posts and hashtags (N:M relationship).

```sql
CREATE TABLE post_hashtags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID NOT NULL,
    hashtag_id UUID NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_post_hashtags_post FOREIGN KEY (post_id) 
        REFERENCES posts(id) ON DELETE CASCADE,
    CONSTRAINT fk_post_hashtags_hashtag FOREIGN KEY (hashtag_id) 
        REFERENCES hashtags(id) ON DELETE CASCADE,
    CONSTRAINT uq_post_hashtags_post_hashtag UNIQUE (post_id, hashtag_id)
);

CREATE INDEX idx_post_hashtags_post_id ON post_hashtags(post_id);
CREATE INDEX idx_post_hashtags_hashtag_id ON post_hashtags(hashtag_id);
```

**Columns**:
- `id`: Primary key
- `post_id`: FK to posts
- `hashtag_id`: FK to hashtags
- `created_at`: When hashtag was added to post

---

### 11. RefreshToken

**Purpose**: Stores refresh tokens for authentication.

```sql
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    revoked_at TIMESTAMP,
    replaced_by_token_id UUID,
    
    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_refresh_tokens_replaced_by FOREIGN KEY (replaced_by_token_id) 
        REFERENCES refresh_tokens(id) ON DELETE SET NULL,
    CONSTRAINT uq_refresh_tokens_token UNIQUE (token)
);

CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
```

**Columns**:
- `id`: Primary key
- `user_id`: Token owner (FK to users)
- `token`: Refresh token string (hashed)
- `expires_at`: Token expiration (30 days default)
- `created_at`: When token was issued
- `revoked_at`: When token was revoked (nullable)
- `replaced_by_token_id`: If rotated, points to new token (nullable)

---

### 12. EmailVerificationToken

**Purpose**: Stores tokens for email verification.

```sql
CREATE TABLE email_verification_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_email_verification_tokens_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_email_verification_tokens_token UNIQUE (token)
);

CREATE INDEX idx_email_verification_tokens_user_id ON email_verification_tokens(user_id);
CREATE INDEX idx_email_verification_tokens_token ON email_verification_tokens(token);
CREATE INDEX idx_email_verification_tokens_expires_at ON email_verification_tokens(expires_at);
```

**Columns**:
- `id`: Primary key
- `user_id`: User to verify (FK to users)
- `token`: Verification token (URL-safe random string)
- `expires_at`: Token expiration (24 hours default)
- `created_at`: When token was generated

---

### 13. PasswordResetToken

**Purpose**: Stores tokens for password reset.

```sql
CREATE TABLE password_reset_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    used_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_password_reset_tokens_user FOREIGN KEY (user_id) 
        REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT uq_password_reset_tokens_token UNIQUE (token)
);

CREATE INDEX idx_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX idx_password_reset_tokens_token ON password_reset_tokens(token);
CREATE INDEX idx_password_reset_tokens_expires_at ON password_reset_tokens(expires_at);
```

**Columns**:
- `id`: Primary key
- `user_id`: User requesting reset (FK to users)
- `token`: Reset token (URL-safe random string)
- `expires_at`: Token expiration (1 hour default)
- `used_at`: When token was used (nullable)
- `created_at`: When token was generated

---

## Database Indexes Summary

### Performance Optimization Strategy

**Primary Keys**: All tables use UUID primary keys
- Pros: Globally unique, no ID collision, good for distributed systems
- Cons: Larger than INT, random (not sequential)

**Indexes Created**:

1. **Users**: username, email, created_at, email_verified
2. **Images**: uploaded_by_user_id, created_at, processing_status
3. **Posts**: user_id, parent_post_id, created_at, (user_id + created_at) composite
4. **PostImages**: post_id, image_id
5. **Likes**: user_id, post_id, created_at
6. **Follows**: follower_id, following_id, created_at
7. **Bookmarks**: user_id, post_id, (user_id + created_at) composite
8. **Notifications**: user_id, is_read, (user_id + is_read + created_at) composite
9. **Hashtags**: tag, created_at
10. **PostHashtags**: post_id, hashtag_id
11. **RefreshTokens**: user_id, token, expires_at
12. **EmailVerificationTokens**: user_id, token, expires_at
13. **PasswordResetTokens**: user_id, token, expires_at

**Composite Indexes** for common queries:
- `posts(user_id, created_at DESC)` - User timeline
- `bookmarks(user_id, created_at DESC)` - User bookmarks
- `notifications(user_id, is_read, created_at DESC)` - Unread notifications

---

## Common Queries & Optimization

### 1. Get User Feed (Following Timeline)

```sql
SELECT p.*
FROM posts p
INNER JOIN follows f ON p.user_id = f.following_id
WHERE f.follower_id = :current_user_id
  AND p.parent_post_id IS NULL
ORDER BY p.created_at DESC
LIMIT 20;
```

**Indexes Used**:
- `idx_follows_follower_id`
- `idx_posts_user_created`

---

### 2. Get Post with Like/Bookmark Status

```sql
SELECT 
    p.*,
    EXISTS(SELECT 1 FROM likes WHERE post_id = p.id AND user_id = :current_user_id) as is_liked,
    EXISTS(SELECT 1 FROM bookmarks WHERE post_id = p.id AND user_id = :current_user_id) as is_bookmarked,
    (SELECT COUNT(*) FROM likes WHERE post_id = p.id) as like_count,
    (SELECT COUNT(*) FROM posts WHERE parent_post_id = p.id) as reply_count
FROM posts p
WHERE p.id = :post_id;
```

**Indexes Used**:
- `idx_likes_post_id`
- `idx_bookmarks_post_id`
- `idx_posts_parent_post_id`

---

### 3. Search Posts by Hashtag

```sql
SELECT p.*
FROM posts p
INNER JOIN post_hashtags ph ON p.id = ph.post_id
INNER JOIN hashtags h ON ph.hashtag_id = h.id
WHERE h.tag = :hashtag
ORDER BY p.created_at DESC
LIMIT 20;
```

**Indexes Used**:
- `idx_hashtags_tag`
- `idx_post_hashtags_hashtag_id`
- `idx_posts_created_at`

---

### 4. Get User's Unread Notifications

```sql
SELECT n.*, u.username as actor_username
FROM notifications n
INNER JOIN users u ON n.actor_id = u.id
WHERE n.user_id = :current_user_id
  AND n.is_read = FALSE
ORDER BY n.created_at DESC;
```

**Indexes Used**:
- `idx_notifications_user_unread` (composite index)

---

## Foreign Key Relationships

### Cascade Behaviors

**ON DELETE CASCADE**:
- When user is deleted, delete all their posts, likes, follows, etc.
- When post is deleted, delete all likes, bookmarks, notifications, etc.

**ON DELETE SET NULL**:
- When profile image is deleted, set `profile_image_id` to NULL
- When refresh token is replaced, keep reference history

---

## Migration Strategy

### Entity Framework Core Migrations

**Initial Migration**:
```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/API
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

**Adding New Feature**:
```bash
dotnet ef migrations add AddBookmarks --project src/Infrastructure --startup-project src/API
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

**Rollback**:
```bash
dotnet ef database update PreviousMigration --project src/Infrastructure --startup-project src/API
dotnet ef migrations remove --project src/Infrastructure --startup-project src/API
```

---

## Database-Agnostic Considerations

### PostgreSQL → SQL Server Compatibility

**Compatible Types**:
- `UUID` (PostgreSQL) → `UNIQUEIDENTIFIER` (SQL Server)
- `TEXT` (PostgreSQL) → `NVARCHAR(MAX)` (SQL Server)
- `TIMESTAMP` (PostgreSQL) → `DATETIME2` (SQL Server)
- `BOOLEAN` (PostgreSQL) → `BIT` (SQL Server)

**Avoid PostgreSQL-Specific**:
- ❌ `SERIAL` / `BIGSERIAL` (use UUID or IDENTITY in EF Core)
- ❌ Arrays `TEXT[]`
- ❌ JSONB columns
- ❌ PostgreSQL functions (`string_agg`, `array_agg`)

**EF Core Handles Automatically**:
- Data type mapping
- `DEFAULT` values
- `AUTO_INCREMENT` / `IDENTITY`
- `CURRENT_TIMESTAMP` / `GETDATE()`

---

## Backup & Recovery

### Development

**Backup**:
```bash
docker exec twitter-clone-postgres pg_dump -U twitter_user twitter_clone_dev > backup.sql
```

**Restore**:
```bash
docker exec -i twitter-clone-postgres psql -U twitter_user twitter_clone_dev < backup.sql
```

### Production

- Automated daily backups
- Point-in-time recovery
- Offsite backup storage
- Regular restore testing

---

## Security Considerations

1. **No Plain Text Passwords**: Only `password_hash` stored
2. **Token Security**: Refresh tokens hashed before storage
3. **Cascade Deletes**: Prevent orphaned data
4. **Constraints**: Enforce data integrity
5. **Indexes**: No sensitive data in index names
6. **Connection Strings**: Stored in User Secrets (development) / Azure Key Vault (production)

---

## Monitoring & Maintenance

### Recommended Monitoring

1. **Table Sizes**: Track growth over time
2. **Index Usage**: Remove unused indexes
3. **Slow Queries**: Log queries > 1 second
4. **Connection Pool**: Monitor active connections
5. **Deadlocks**: Log and investigate

### Maintenance Tasks

1. **VACUUM** (PostgreSQL): Weekly for deleted rows
2. **ANALYZE**: Update statistics for query planner
3. **REINDEX**: Rebuild fragmented indexes
4. **Archive Old Data**: Move old posts to archive table

---

## Summary

This database design provides:
- ✅ **Normalized structure** (3NF) for data integrity
- ✅ **Proper indexing** for performance
- ✅ **Foreign keys** for referential integrity
- ✅ **Computed properties** to avoid denormalization
- ✅ **Database-agnostic** design for flexibility
- ✅ **Scalable** to millions of rows
- ✅ **Secure** token storage
- ✅ **Migration ready** with EF Core

**Total Tables**: 13
**Total Indexes**: ~40
**Total Constraints**: ~30

This schema supports all MVP features and provides a foundation for future enhancements.
