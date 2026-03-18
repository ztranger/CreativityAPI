CREATE TYPE chat_type AS ENUM ('direct', 'group', 'public'); -- Типы чатов: личный, групповой, общий

CREATE TABLE chats (
    id BIGSERIAL PRIMARY KEY,
    chat_type chat_type NOT NULL, -- Тип чата: direct/group/public
    title VARCHAR(200), -- Название (для group/public)
    description TEXT, -- Описание чата
    avatar_url TEXT, -- Аватар чата
    created_by INTEGER NOT NULL REFERENCES users(id) ON DELETE RESTRICT, -- Пользователь-создатель
    last_message_id BIGINT, -- Денормализация: последнее сообщение в чате
    is_archived BOOLEAN NOT NULL DEFAULT FALSE, -- Архивирован ли чат для всех участников
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Индексы для быстрого списка чатов и фильтрации
CREATE INDEX idx_chats_created_by ON chats(created_by);
CREATE INDEX idx_chats_chat_type ON chats(chat_type);
CREATE INDEX idx_chats_updated_at ON chats(updated_at DESC);