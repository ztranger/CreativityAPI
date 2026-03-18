CREATE TABLE message_content (
    message_id BIGINT PRIMARY KEY REFERENCES messages(id) ON DELETE CASCADE, -- 1:1 связь с сообщением
    text TEXT, -- Текст сообщения
    entities JSONB NOT NULL DEFAULT '[]', -- Список сущностей: mentions, ссылки, форматирование
    emoji_payload JSONB NOT NULL DEFAULT '[]', -- Emoji-данные (если сообщение состоит только из emoji)
    CONSTRAINT chk_message_content_not_empty CHECK (
        COALESCE(LENGTH(TRIM(text)), 0) > 0 OR JSONB_ARRAY_LENGTH(emoji_payload) > 0
    )
);

-- Индекс для полнотекстового поиска в контенте сообщений
CREATE INDEX idx_message_content_text_search ON message_content USING GIN (to_tsvector('russian', COALESCE(text, '')));
