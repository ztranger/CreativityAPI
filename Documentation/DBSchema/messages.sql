CREATE TABLE messages (
    id BIGSERIAL PRIMARY KEY,
    chat_id BIGINT NOT NULL REFERENCES chats(id) ON DELETE CASCADE,
    sender_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    reply_to_message_id BIGINT REFERENCES messages(id) ON DELETE SET NULL, -- Ответ на другое сообщение
    content TEXT, -- Текст сообщения
    attachments JSONB DEFAULT '[]', -- Массив вложений (файлы, фото, гео)
    reactions JSONB DEFAULT '{}', -- { "like": [user1, user2], "heart": [user3] }
    metadata JSONB DEFAULT '{}', -- Служебная инфа (кому адресовано, запросы и т.д.)
    is_pinned BOOLEAN DEFAULT FALSE, -- Закреплено ли в чате
    is_edited BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMPTZ NULL, -- Для "удалить для всех" (мягкое удаление)
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Индексы для быстрой выборки истории
CREATE INDEX idx_messages_chat_id_created_at ON messages(chat_id, created_at DESC);
CREATE INDEX idx_messages_sender_id ON messages(sender_id);
CREATE INDEX idx_messages_reply_to ON messages(reply_to_message_id) WHERE reply_to_message_id IS NOT NULL;

-- Gin индекс для полнотекстового поиска по сообщениям в чате
CREATE INDEX idx_messages_content_search ON messages USING GIN (to_tsvector('russian', content));

-- !!! ВАЖНО: Для таблицы messages нужно настроить партиционирование (секционирование) по chat_id или по дате,
-- так как она будет содержать миллиарды записей.