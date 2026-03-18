CREATE TABLE messages (
    id BIGSERIAL PRIMARY KEY,
    chat_id BIGINT NOT NULL REFERENCES chats(id) ON DELETE CASCADE,
    sender_user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    message_type SMALLINT NOT NULL, -- 1=text, 2=system, 3=emoji_only
    reply_to_message_id BIGINT REFERENCES messages(id) ON DELETE SET NULL, -- Ответ на другое сообщение
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    edited_at TIMESTAMPTZ NULL,
    deleted_at TIMESTAMPTZ NULL, -- Для "удалить для всех" (мягкое удаление)
    version INTEGER NOT NULL DEFAULT 1, -- Версия сообщения для контроля правок
    CONSTRAINT chk_messages_message_type CHECK (message_type IN (1, 2, 3))
);

-- Индексы для быстрой выборки истории
CREATE INDEX idx_messages_chat_id_created_at_id ON messages(chat_id, created_at DESC, id DESC);
CREATE INDEX idx_messages_sender_user_id ON messages(sender_user_id);
CREATE INDEX idx_messages_reply_to ON messages(reply_to_message_id) WHERE reply_to_message_id IS NOT NULL;
CREATE INDEX idx_messages_chat_id_created_at_not_deleted ON messages(chat_id, created_at DESC) WHERE deleted_at IS NULL;