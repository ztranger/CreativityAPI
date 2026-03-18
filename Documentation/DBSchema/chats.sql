CREATE TYPE chat_type AS ENUM ('private', 'group', 'channel'); -- Типы чатов

CREATE TABLE chats (
    id BIGSERIAL PRIMARY KEY,
    type chat_type NOT NULL, -- Тип: личка, группа, канал
    title VARCHAR(200), -- Название (для групп и каналов)
    avatar_url TEXT, -- Аватар группы
    created_by BIGINT REFERENCES users(id) ON DELETE SET NULL, -- Кто создал
    is_public BOOLEAN DEFAULT FALSE, -- Публичная ли группа (для поиска/вступления)
    invite_link VARCHAR(255) UNIQUE, -- Ссылка-приглашение
    last_message_id BIGINT, -- Денормализация: ID последнего сообщения (обновляется триггером)
    metadata JSONB DEFAULT '{}', -- Доп. настройки (кто может писать, прикреплять файлы и т.д.)
    created_at TIMESTAMPTZ DEFAULT NOW(),
    deleted_at TIMESTAMPTZ NULL -- Мягкое удаление чата
);