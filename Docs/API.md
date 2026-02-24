# API Reference

Base URL (local): `http://localhost:5000`

All authenticated endpoints require an `Authorization: Bearer <token>` header.

---

## Authentication

### Register

**METHOD:** `POST`  
**ROUTE:** `/api/auth/register`

**Authentication:** Not required

**Description:**  
Creates a new user account. Optionally accepts a `portfolioSlug` to set a custom public portfolio identifier. If omitted, no slug is assigned at registration.

**Request (JSON):**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "portfolioSlug": "my-portfolio"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `email` | string | ✅ | Must be unique |
| `password` | string | ✅ | |
| `portfolioSlug` | string | ❌ | Optional; auto-normalized to lowercase-slug format |

**Response:**
```json
{
  "token": "<JWT>"
}
```

| Status | Meaning |
|--------|---------|
| `201 Created` | Account created, token returned |
| `400 Bad Request` | Email already in use or validation error |

---

### Login

**METHOD:** `POST`  
**ROUTE:** `/api/auth/login`

**Authentication:** Not required

**Description:**  
Authenticates an existing user and returns a signed JWT.

**Request (JSON):**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response:**
```json
{
  "token": "<JWT>"
}
```

| Status | Meaning |
|--------|---------|
| `200 OK` | Login successful, token returned |
| `401 Unauthorized` | Invalid credentials |

---

## User

### Get Current User

**METHOD:** `GET`  
**ROUTE:** `/api/me`

**Authentication:** Required

**Description:**  
Returns the authenticated user's profile derived from the JWT `owner_id` claim.

**Request:** No body.

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "portfolioSlug": "my-portfolio"
}
```

| Status | Meaning |
|--------|---------|
| `200 OK` | User profile returned |
| `401 Unauthorized` | Missing or invalid token |
| `404 Not Found` | User not found in database |

---

## Projects (Admin)

> All project admin endpoints require authentication. `OwnerId` is always derived from the JWT — never from the request body.

---

### List All Projects (Admin)

**METHOD:** `GET`  
**ROUTE:** `/api/projects/admin`

**Authentication:** Required

**Description:**  
Returns all projects belonging to the authenticated user, regardless of status (Draft, Published, Archived).

**Request:** No body.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ownerId": "...",
    "name": "My Project",
    "shortDescription": "A brief description.",
    "longDescription": "Extended description.",
    "liveUrl": "https://example.com",
    "repoUrl": "https://github.com/example/repo",
    "status": "Published",
    "isPinned": true,
    "displayOrder": 1,
    "coverAssetId": "...",
    "demoVideoAssetId": "...",
    "coverUrl": "https://res.cloudinary.com/...",
    "demoVideoUrl": "https://res.cloudinary.com/...",
    "createdAt": "2026-02-24T00:00:00Z",
    "updatedAt": "2026-02-24T00:00:00Z",
    "technologies": [
      {
        "id": "...",
        "name": "React",
        "slug": "react",
        "createdAt": "...",
        "updatedAt": "..."
      }
    ]
  }
]
```

| Status | Meaning |
|--------|---------|
| `200 OK` | List returned (empty array if none) |
| `401 Unauthorized` | Missing or invalid token |

---

### Create Project

**METHOD:** `POST`  
**ROUTE:** `/api/projects`

**Authentication:** Required

**Description:**  
Creates a new project. Requires a multipart/form-data body. A cover image is mandatory. Demo video is optional. Assets are uploaded to Cloudinary before the project record is persisted; if persistence fails, uploaded assets are automatically cleaned up.

**Request (multipart/form-data):**

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `name` | string | ✅ | Project name |
| `shortDescription` | string | ✅ | Brief summary |
| `longDescription` | string | ❌ | Extended description |
| `liveUrl` | string | ❌ | URL to live demo |
| `repoUrl` | string | ❌ | URL to source repository |
| `status` | string | ✅ | `Draft`, `Published`, or `Archived` |
| `isPinned` | boolean | ❌ | Default: `false` |
| `displayOrder` | int | ❌ | Ordering hint |
| `technologyIds` | string | ❌ | Comma-separated GUIDs |
| `cover` | file | ✅ | Cover image (JPEG, PNG, WebP) |
| `demoVideo` | file | ❌ | Demo video (MP4, etc.) |

**Response:** Returns the created `ProjectDto` (same structure as admin list item).

| Status | Meaning |
|--------|---------|
| `201 Created` | Project created, DTO returned |
| `400 Bad Request` | Missing cover, validation error |
| `401 Unauthorized` | Missing or invalid token |

---

### Update Project

**METHOD:** `PUT`  
**ROUTE:** `/api/projects/{projectId}`

**Authentication:** Required

**Description:**  
Updates an existing project owned by the authenticated user. Assets (cover, demo video) are replaced if new files are provided. Old Cloudinary assets are deleted post-commit. If the update fails, any newly uploaded assets are rolled back.  
Pass `removeDemoVideo=true` (with no `demoVideo` file) to explicitly remove the demo video.

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `projectId` | GUID | ID of the project to update |

**Request (multipart/form-data):**

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `name` | string | ✅ | |
| `shortDescription` | string | ✅ | |
| `longDescription` | string | ❌ | |
| `liveUrl` | string | ❌ | |
| `repoUrl` | string | ❌ | |
| `status` | string | ✅ | `Draft`, `Published`, or `Archived` |
| `isPinned` | boolean | ❌ | |
| `displayOrder` | int | ❌ | |
| `technologyIds` | string | ❌ | Comma-separated GUIDs |
| `cover` | file | ❌ | If provided, replaces existing cover |
| `demoVideo` | file | ❌ | If provided, replaces existing demo video |
| `removeDemoVideo` | boolean | ❌ | `true` to remove demo video without replacement |

**Response:** Returns the updated `ProjectDto`.

| Status | Meaning |
|--------|---------|
| `200 OK` | Project updated |
| `400 Bad Request` | Validation error |
| `401 Unauthorized` | Missing or invalid token |
| `404 Not Found` | Project not found or not owned by caller |

---

### Delete Project

**METHOD:** `DELETE`  
**ROUTE:** `/api/projects/{projectId}`

**Authentication:** Required

**Description:**  
Deletes a project and its associated assets. Asset IDs are resolved from the database before Cloudinary deletion (DB-first cleanup).

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `projectId` | GUID | ID of the project to delete |

**Request:** No body.

**Response:** Empty body.

| Status | Meaning |
|--------|---------|
| `204 No Content` | Project deleted successfully |
| `401 Unauthorized` | Missing or invalid token |
| `404 Not Found` | Project not found or not owned by caller |

---

## Technologies

### List Technologies

**METHOD:** `GET`  
**ROUTE:** `/api/technologies`

**Authentication:** Required

**Description:**  
Returns all technologies available in the catalog.

**Request:** No body.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "React",
    "slug": "react",
    "createdAt": "2026-02-24T00:00:00Z",
    "updatedAt": "2026-02-24T00:00:00Z"
  }
]
```

| Status | Meaning |
|--------|---------|
| `200 OK` | List returned |
| `401 Unauthorized` | Missing or invalid token |

---

### Create Technology

**METHOD:** `POST`  
**ROUTE:** `/api/technologies`

**Authentication:** Required

**Description:**  
Adds a new technology to the catalog. The slug is auto-generated from the name.

**Request (JSON):**
```json
{
  "name": "React"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "React",
  "slug": "react",
  "createdAt": "2026-02-24T00:00:00Z",
  "updatedAt": "2026-02-24T00:00:00Z"
}
```

| Status | Meaning |
|--------|---------|
| `201 Created` | Technology created |
| `400 Bad Request` | Validation error or duplicate name |
| `401 Unauthorized` | Missing or invalid token |

---

## Public Portfolio

### Get Published Projects by Portfolio Slug

**METHOD:** `GET`  
**ROUTE:** `/api/portfolio/{portfolioSlug}/projects`

**Authentication:** Not required

**Description:**  
Resolves a `portfolioSlug` to a user and returns all **Published** projects for that user. This is the public-facing endpoint intended for portfolio frontends. Rate limited to **60 requests/minute** per fixed window.

**Path Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `portfolioSlug` | string | The portfolio slug assigned to a user |

**Request:** No body.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "ownerId": "...",
    "name": "My Project",
    "shortDescription": "A brief description.",
    "longDescription": null,
    "liveUrl": "https://example.com",
    "repoUrl": null,
    "status": "Published",
    "isPinned": true,
    "displayOrder": 1,
    "coverAssetId": "...",
    "demoVideoAssetId": null,
    "coverUrl": "https://res.cloudinary.com/...",
    "demoVideoUrl": null,
    "createdAt": "2026-02-24T00:00:00Z",
    "updatedAt": "2026-02-24T00:00:00Z",
    "technologies": []
  }
]
```

| Status | Meaning |
|--------|---------|
| `200 OK` | Published projects returned (empty array if none) |
| `404 Not Found` | Slug not found or invalid |
| `429 Too Many Requests` | Rate limit exceeded (60 req/min) |
