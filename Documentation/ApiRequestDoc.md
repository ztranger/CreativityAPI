# API Specification for Messenger Service v1

**Base URL:** `https://api.yourmessenger.com/v1`  
**Authentication:** Bearer Token (`Authorization: Bearer <jwt_token>`)  
**Content-Type:** `application/json` (unless specified otherwise)

---

## Table of Contents

1. [Users](#1-users)
   - [Registration & Profile Management](#registration--profile-management)
   - [Contacts & Friends](#contacts--friends)
2. [Chats](#2-chats)
   - [Chat Management](#chat-management)
   - [Group Participants](#group-participants)
3. [Messages](#3-messages)
   - [Core Operations](#core-operations)
   - [Reactions & Interaction](#reactions--interaction)
4. [Search](#4-search)
5. [Presence](#5-presence)
6. [Settings](#6-settings)
7. [Media & Files](#7-media--files)
8. [Backup & Export](#8-backup--export)
9. [WebSocket Events](#9-websocket-events)
10. [Pagination](#10-pagination)

---

## 1. Users

### Registration & Profile Management

#### `POST /auth/register`

Register a new user with phone number.

**Request Body:**

```json
{
  "phone": "+79001234567",
  "display_name": "John Doe",
  "username": "johndoe" // optional
}
```

**Response: 201 Created**

```json
{
  "user": {
    "id": 123,
    "phone": "+79001234567",
    "username": "johndoe",
    "display_name": "John Doe",
    "avatar": null,
    "created_at": "2024-01-01T12:00:00Z"
  },
  "token": "jwt_token_here"
}
```

---

#### `POST /auth/verify`

Verify phone with confirmation code.

**Request Body:**

```json
{
  "phone": "+79001234567",
  "code": "123456"
}
```

**Response:**

```json
{
  "user": { },
  "token": "jwt_token_here",
  "settings": { }
}
```

---

#### `POST /auth/refresh`

Refresh access token.

**Request Body:**

```json
{
  "refresh_token": "refresh_token_here"
}
```

**Response:**

```json
{
  "access_token": "new_jwt_token"
}
```

---

#### `GET /users/me`

Get current user profile.

**Response:**

```json
{
  "id": 123,
  "phone": "+79001234567",
  "username": "johndoe",
  "display_name": "John Doe",
  "avatar": "https://cdn.example.com/avatars/123.jpg",
  "bio": "Hello there!",
  "settings": {
    "notifications": true,
    "theme": "dark"
  },
  "last_seen": "2024-01-01T12:00:00Z"
}
```

---

#### `PATCH /users/me`

Update profile.

**Request Body (all fields optional):**

```json
{
  "display_name": "John Updated",
  "username": "john_new",
  "bio": "New bio",
  "avatar": "avatar_id_from_upload"
}
```

---

#### `POST /users/me/avatar`

Upload avatar image.

- **Content-Type:** `multipart/form-data`

**Form Data:**

- `avatar`: image file (jpg, png, gif; max 5MB)

**Response:**

```json
{
  "avatar_url": "https://cdn.example.com/avatars/123.jpg"
}
```

---

#### `GET /users/:id`

Get another user's profile.

**Response:**

```json
{
  "id": 456,
  "username": "friend",
  "display_name": "Friend Name",
  "avatar": "https://...",
  "bio": "Their bio",
  "is_online": true,
  "last_seen": null
}
```

---

#### `GET /users/search`

Search users by name/username.

**Query Parameters:**

- `q`: search query  
- `limit`: max results (default 20, max 100)

**Response:**

```json
{
  "users": [
    {
      "id": 123,
      "username": "johndoe",
      "display_name": "John Doe",
      "avatar": "https://..."
    }
  ],
  "total_count": 1
}
```

---

#### `POST /users/me/logout`

Logout from all devices.

**Response:**

```json
{
  "success": true
}
```

---

### Contacts & Friends

#### `GET /contacts`

Get contact list.

**Response:**

```json
{
  "contacts": [
    {
      "id": 456,
      "user_id": 789,
      "display_name": "My Custom Name",
      "user": {
        "id": 789,
        "username": "friend",
        "avatar": "https://..."
      }
    }
  ]
}
```

---

#### `POST /contacts`

Add contact.

**Request Body (provide either `user_id` or `phone`):**

```json
{
  "user_id": 789,
  "phone": "+79001234567",
  "display_name": "Optional name"
}
```

---

#### `DELETE /contacts/:id`

Remove contact.

---

#### `PUT /contacts/:id`

Update contact name.

**Request Body:**

```json
{
  "display_name": "New Name"
}
```

---

#### `POST /contacts/sync`

Sync with phonebook.

**Request Body:**

```json
{
  "phones": ["+79001234567", "+79007654321"]
}
```

**Response:**

```json
{
  "registered_contacts": [
    {
      "user_id": 789,
      "phone": "+79001234567",
      "display_name": "Friend Name"
    }
  ],
  "invites_sent": ["+79007654321"]
}
```

---

#### `GET /blocked`

Get blocked users.

---

#### `POST /blocked/:id`

Block user.

---

#### `DELETE /blocked/:id`

Unblock user.

---

## 2. Chats

### MVP Implemented (Current)

For the current MVP build, the following chat endpoints are implemented and available:

- `POST /chats`
- `GET /chats`
- `GET /chats/:id`
- `PATCH /chats/:id`
- `POST /chats/:id/participants`
- `DELETE /chats/:id/participants/:userId`

Notes:

- All listed endpoints require JWT authentication.
- `chatType` and participant `role` are numeric (`1..3`) and mapped to DB check constraints.
- Access is limited to active chat members.

### Chat Management

#### `GET /chats`

Get user's chats.

**Query Parameters:**

- `offset`: pagination offset (default 0)  
- `limit`: items per page (default 50, max 200)  
- `archived`: include archived chats (default `false`)

**Response:**

```json
{
  "chats": [
    {
      "id": 123,
      "type": "private",
      "title": "John Doe",
      "avatar": "https://...",
      "last_message": {
        "id": 456,
        "content": "Hello!",
        "sender_id": 789,
        "created_at": "2024-01-01T12:00:00Z"
      },
      "unread_count": 2,
      "participants_count": 2,
      "is_online": true,
      "pinned": false,
      "muted_until": null
    }
  ],
  "total_unread_count": 5
}
```

---

#### `GET /chats/:id`

Get chat details.

**Response:**

```json
{
  "id": 123,
  "type": "group",
  "title": "Family Group",
  "avatar": "https://...",
  "created_by": 789,
  "created_at": "2024-01-01T12:00:00Z",
  "participants_count": 5,
  "last_message_id": 456,
  "is_public": false,
  "invite_link": "https://t.me/joinchat/abc123",
  "metadata": {
    "can_send_messages": true,
    "can_send_media": true
  }
}
```

---

#### `POST /chats`

Create chat.

**Request Body:**

```json
{
  "type": "private", // or "group", "channel"
  "user_ids": [123, 456],
  "title": "Group Name", // required for groups
  "avatar": "avatar_id" // optional
}
```

---

#### `PATCH /chats/:id`

Update chat.

**Request Body:**

```json
{
  "title": "New Title",
  "avatar": "new_avatar_id"
}
```

---

#### `DELETE /chats/:id`

Delete/leave chat.

---

#### `POST /chats/:id/leave`

Leave group (groups only).

---

#### `POST /chats/:id/archive`

Archive chat.

---

#### `POST /chats/:id/unarchive`

Unarchive chat.

---

#### `POST /chats/:id/mark-read`

Mark chat as read.

**Request Body:**

```json
{
  "up_to_message_id": 456
}
```

---

### Group Participants

#### `GET /chats/:id/participants`

List participants.

**Query Parameters:**

- `offset`: pagination offset  
- `limit`: items per page

**Response:**

```json
{
  "participants": [
    {
      "user_id": 123,
      "role": "admin",
      "joined_at": "2024-01-01T12:00:00Z",
      "user": {
        "display_name": "John",
        "avatar": "https://..."
      }
    }
  ],
  "count": 10
}
```

---

#### `POST /chats/:id/participants`

Add participants.

**Request Body:**

```json
{
  "user_ids": [456, 789]
}
```

---

#### `DELETE /chats/:id/participants/:userId`

Remove participant.

---

#### `PATCH /chats/:id/participants/:userId`

Change role.

**Request Body:**

```json
{
  "role": "admin" // or "member"
}
```

---

#### `POST /chats/:id/join`

Join via invite link.

**Request Body:**

```json
{
  "invite_link": "https://t.me/joinchat/abc123"
}
```

---

#### `POST /chats/:id/invite-link`

Generate invite link.

**Request Body:**

```json
{
  "expires_in": 604800, // seconds, optional
  "usage_limit": 100 // optional
}
```

---

#### `DELETE /chats/:id/invite-link`

Delete invite link.

---

## 3. Messages

### MVP Implemented (Current)

For the current MVP build, the following message endpoints are implemented and available:

- `POST /chats/:chatId/messages` (text only)
- `GET /chats/:chatId/messages`

Notes:

- All listed endpoints require JWT authentication.
- Sending message is allowed only for active chat members.
- Current MVP supports text messages only (`message_type = 1`).

### Core Operations

#### `GET /chats/:chatId/messages`

Get message history.

**Query Parameters:**

- `before`: message_id (messages older than this)  
- `after`: message_id (messages newer than this)  
- `around`: message_id (center of the page)  
- `limit`: items per page (default 50, max 200)

**Response:**

```json
{
  "messages": [
    {
      "id": 456,
      "chat_id": 123,
      "sender_id": 789,
      "content": "Hello world!",
      "reply_to_message_id": 455,
      "attachments": [
        {
          "id": "file_123",
          "type": "image",
          "url": "https://...",
          "thumbnail_url": "https://...",
          "name": "photo.jpg",
          "size": 1024000
        }
      ],
      "reactions": {
        "❤️": [123, 456],
        "😂": [789]
      },
      "is_pinned": false,
      "is_edited": false,
      "created_at": "2024-01-01T12:00:00Z"
    }
  ],
  "has_more": true
}
```

---

#### `GET /chats/:chatId/messages/:id`

Get single message.

---

#### `POST /chats/:chatId/messages`

Send message.

**Request Body:**

```json
{
  "content": "Hello!", // optional if attachments present
  "reply_to": 455, // optional
  "attachments": [
    {
      "id": "file_123",
      "type": "image",
      "url": "https://...",
      "name": "photo.jpg"
    }
  ],
  "temp_id": "local-uuid-123"
}
```

---

#### `POST /chats/:chatId/messages/file`

Upload file for message.

- **Content-Type:** `multipart/form-data`

**Form Data:**

- `file`: file to upload  
- `chat_id`: optional, for processing

**Response:**

```json
{
  "attachment": {
    "id": "file_123",
    "type": "image",
    "url": "https://...",
    "thumbnail_url": "https://...",
    "name": "photo.jpg",
    "size": 1024000
  }
}
```

---

#### `PATCH /chats/:chatId/messages/:id`

Edit message.

**Request Body:**

```json
{
  "content": "Updated content"
}
```

---

#### `DELETE /chats/:chatId/messages/:id`

Delete message.

**Request Body:**

```json
{
  "for_all": false
}
```

---

#### `POST /chats/:chatId/messages/:id/pin`

Pin message.

---

#### `POST /chats/:chatId/messages/:id/unpin`

Unpin message.

---

#### `GET /chats/:chatId/pinned`

Get pinned messages.

---

### Reactions & Interaction

#### `POST /chats/:chatId/messages/:id/reactions`

Add reaction.

**Request Body:**

```json
{
  "reaction": "❤️"
}
```

---

#### `DELETE /chats/:chatId/messages/:id/reactions`

Remove reaction.

**Request Body:**

```json
{
  "reaction": "❤️"
}
```

---

#### `GET /chats/:chatId/messages/:id/seen-by`

Get read receipts.

**Response:**

```json
{
  "users": [
    {
      "id": 123,
      "display_name": "John",
      "avatar": "https://...",
      "read_at": "2024-01-01T12:00:05Z"
    }
  ],
  "count": 1
}
```

---

## 4. Search

#### `GET /search/global`

Global search across users, chats, messages.

**Query Parameters:**

- `q`: search query  
- `type`: `"all"` | `"messages"` | `"chats"` | `"users"` (default `"all"`)  
- `limit`: per category limit (default 20)

**Response:**

```json
{
  "messages": [
    {
      "id": 456,
      "chat_id": 123,
      "content": "Found message",
      "created_at": "..."
    }
  ],
  "chats": [],
  "users": []
}
```

---

#### `GET /chats/:chatId/search`

Search within specific chat.

**Query Parameters:**

- `q`: search query  
- `from`: filter by sender `user_id`  
- `before`: date filter  
- `limit`: results limit

**Response:**

```json
{
  "messages": [],
  "total_count": 42
}
```

---

#### `GET /search/recent`

Get recent search queries.

---

## 5. Presence

#### `POST /presence/online`

Report online status.

---

#### `POST /presence/offline`

Go offline.

---

#### `GET /presence/:userIds`

Get status for multiple users (comma-separated).

**Example:** `GET /presence/123,456,789`

**Response:**

```json
{
  "presence": {
    "123": {
      "status": "online",
      "last_seen": null
    },
    "456": {
      "status": "offline",
      "last_seen": "2024-01-01T11:55:00Z"
    }
  }
}
```

---

#### `POST /presence/typing/:chatId`

Start typing indicator.

---

#### `POST /presence/stop-typing/:chatId`

Stop typing indicator.

---

## 6. Settings

#### `GET /settings`

Get all settings.

**Response:**

```json
{
  "notifications": {
    "enabled": true,
    "sound": "default",
    "vibrate": true,
    "preview": true
  },
  "privacy": {
    "last_seen": "everyone",
    "read_receipts": true,
    "profile_photo": "everyone",
    "forwarded": "everyone"
  },
  "appearance": {
    "theme": "dark",
    "font_size": "medium",
    "language": "en"
  }
}
```

---

#### `PATCH /settings`

Update specific settings.

**Request Body:**

```json
{
  "notifications.enabled": false,
  "appearance.theme": "light"
}
```

---

#### `GET /settings/privacy`

Get privacy settings only.

---

#### `PUT /settings/privacy`

Update privacy settings.

---

#### `GET /sessions`

Get active sessions.

**Response:**

```json
{
  "sessions": [
    {
      "id": "session_123",
      "device": "iPhone 15 Pro",
      "os": "iOS 17",
      "app_version": "1.2.3",
      "last_active": "2024-01-01T12:00:00Z",
      "location": "Moscow, Russia",
      "ip": "192.168.1.1"
    }
  ]
}
```

---

#### `DELETE /sessions/:id`

Terminate specific session.

---

#### `DELETE /sessions`

Terminate all other sessions.

---

## 7. Media & Files

#### `GET /media/:fileId`

Download file.

**Query Parameters:**

- `thumbnail`: if `true`, returns thumbnail instead of full file

---

#### `GET /media/gallery/:chatId`

Get chat media gallery.

**Query Parameters:**

- `type`: `"photo"` | `"video"` | `"file"` | `"all"` (default `"all"`)  
- `limit`: items per page

**Response:**

```json
{
  "media": [
    {
      "id": "file_123",
      "type": "image",
      "url": "https://...",
      "thumbnail_url": "https://...",
      "name": "photo.jpg",
      "size": 1024000,
      "created_at": "2024-01-01T12:00:00Z",
      "message_id": 456,
      "sender_id": 789
    }
  ]
}
```

---

#### `DELETE /media/:fileId`

Delete file.

---

#### `POST /media/upload`

Upload file (without sending to chat).

- **Content-Type:** `multipart/form-data`

**Form Data:**

- `file`: file to upload  
- `chat_id`: optional, for processing

---

## 8. Backup & Export

#### `POST /export/start`

Start data export.

**Request Body:**

```json
{
  "include": ["messages", "contacts", "media"],
  "format": "json" // or "html"
}
```

**Response:**

```json
{
  "export_id": "export_123",
  "status": "processing"
}
```

---

#### `GET /export/:exportId/status`

Check export status.

**Response:**

```json
{
  "status": "completed",
  "progress": 100,
  "download_url": "https://..."
}
```

---

#### `GET /backup`

List available backups.

---

#### `POST /backup/create`

Create new backup.

---

#### `POST /backup/restore/:backupId`

Restore from backup.

---

## 9. WebSocket Events

**Connection:** `wss://api.yourmessenger.com/v1/ws`

### Client → Server Events

#### Subscribe to channels

```json
{
  "type": "subscribe",
  "channels": ["chat.123", "user.status"]
}
```

#### Send message

```json
{
  "type": "message.send",
  "payload": {
    "chat_id": 123,
    "content": "Hello!",
    "temp_id": "local-uuid-123"
  }
}
```

#### Typing indicators

```json
{
  "type": "typing.start",
  "payload": { "chat_id": 123 }
}
```

```json
{
  "type": "typing.stop",
  "payload": { "chat_id": 123 }
}
```

#### Read receipts

```json
{
  "type": "read.receipt",
  "payload": {
    "chat_id": 123,
    "message_id": 456
  }
}
```

### Server → Client Events

#### New message

```json
{
  "type": "message.new",
  "payload": {
    "message": { },
    "temp_id": "local-uuid-123"
  }
}
```

#### Message updated

```json
{
  "type": "message.updated",
  "payload": {
    "chat_id": 123,
    "message": { }
  }
}
```

#### Message deleted

```json
{
  "type": "message.deleted",
  "payload": {
    "chat_id": 123,
    "message_id": 456,
    "for_all": true
  }
}
```

#### User status

```json
{
  "type": "user.status",
  "payload": {
    "user_id": 789,
    "status": "online",
    "last_seen": null
  }
}
```

#### Typing indicator

```json
{
  "type": "typing",
  "payload": {
    "chat_id": 123,
    "user_id": 789,
    "is_typing": true
  }
}
```

#### Read receipt

```json
{
  "type": "read.receipt",
  "payload": {
    "chat_id": 123,
    "user_id": 789,
    "read_up_to": 456
  }
}
```

---

## 10. Pagination

All list endpoints support cursor-based pagination.

### Request Parameters

| Parameter | Description                          | Example                         |
|----------|--------------------------------------|---------------------------------|
| `before` | Get items before this ID            | `before=100500`                 |
| `after`  | Get items after this ID             | `after=100500`                  |
| `around` | Center page around this ID          | `around=100500`                 |
| `limit`  | Items per page (default 50)         | `limit=30`                      |

### Response Format

```json
{
  "data": [],
  "pagination": {
    "has_more": true,
    "next_cursor": "100600",
    "prev_cursor": "100400",
    "total_count": 1500
  }
}
```

### Examples

```http
GET /chats/123/messages?before=100500&limit=30
GET /chats/123/messages?after=100500&limit=30
GET /chats/123/messages?around=100500&limit=20
```