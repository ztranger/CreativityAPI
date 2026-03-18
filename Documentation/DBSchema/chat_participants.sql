CREATE TYPE participant_role AS ENUM ('member', 'admin', 'owner');
CREATE TYPE membership_status AS ENUM ('active', 'invited', 'left', 'kicked');

CREATE TABLE chat_participants (
    chat_id BIGINT REFERENCES chats(id) ON DELETE CASCADE,
    user_id BIGINT REFERENCES users(id) ON DELETE CASCADE,
    role participant_role DEFAULT 'member',
    status membership_status DEFAULT 'active',
    joined_at TIMESTAMPTZ DEFAULT NOW(),
    last_read_message_id BIGINT, -- До какого сообщения прочитано пользователем в этом чате
    muted_until TIMESTAMPTZ NULL, -- До какого времени отключены уведомления
    PRIMARY KEY (chat_id, user_id) -- Составной первичный ключ
);

-- Индексы для быстрого получения чатов пользователя и участников чата
CREATE INDEX idx_participants_user_id ON chat_participants(user_id) WHERE status = 'active';
CREATE INDEX idx_participants_chat_id ON chat_participants(chat_id) WHERE status = 'active';