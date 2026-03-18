CREATE TYPE chat_member_role AS ENUM ('member', 'admin', 'owner'); -- Роли в чате

CREATE TABLE chat_members (
    chat_id BIGINT NOT NULL REFERENCES chats(id) ON DELETE CASCADE,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role chat_member_role NOT NULL DEFAULT 'member', -- Роль участника в чате
    joined_at TIMESTAMPTZ NOT NULL DEFAULT NOW(), -- Когда пользователь вступил в чат
    left_at TIMESTAMPTZ NULL, -- Когда пользователь покинул чат
    mute_until TIMESTAMPTZ NULL, -- До какого времени отключены уведомления
    last_read_message_id BIGINT NULL, -- Курсор прочтения в рамках чата
    is_pinned BOOLEAN NOT NULL DEFAULT FALSE, -- Закреплен ли чат у пользователя
    PRIMARY KEY (chat_id, user_id) -- Один пользователь может иметь одну активную запись в чате
);

-- Индексы для быстрого получения чатов пользователя и участников чата
CREATE INDEX idx_chat_members_user_id_chat_id ON chat_members(user_id, chat_id);
CREATE INDEX idx_chat_members_chat_id_user_id ON chat_members(chat_id, user_id);
CREATE INDEX idx_chat_members_active_user_id ON chat_members(user_id) WHERE left_at IS NULL;