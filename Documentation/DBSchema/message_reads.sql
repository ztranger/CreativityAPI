CREATE TABLE message_reads (
    message_id BIGINT NOT NULL REFERENCES messages(id) ON DELETE CASCADE, -- Какое сообщение прочитано
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE, -- Кто прочитал
    read_at TIMESTAMPTZ NOT NULL DEFAULT NOW(), -- Когда прочитано
    PRIMARY KEY (message_id, user_id)
);

-- Индексы для быстрого получения подтверждений прочтения
CREATE INDEX idx_message_reads_user_id_read_at ON message_reads(user_id, read_at DESC);
CREATE INDEX idx_message_reads_message_id ON message_reads(message_id);
