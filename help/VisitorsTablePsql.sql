-- ===== POSTGRESQL DATABASE SETUP SCRIPT =====
-- Run this in your PostgreSQL database to create the visitors table

CREATE TABLE IF NOT EXISTS visitors (
    id SERIAL PRIMARY KEY,
    firstname VARCHAR(50) NOT NULL,
    surname VARCHAR(50) NOT NULL,
    company VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Add index for better query performance
    CONSTRAINT unique_visitor UNIQUE (firstname, surname, email, timestamp)
);

-- Optional: Create index for faster queries by timestamp
CREATE INDEX IF NOT EXISTS idx_visitors_timestamp ON visitors(timestamp DESC);

-- Optional: Create index for faster queries by email
CREATE INDEX IF NOT EXISTS idx_visitors_email ON visitors(email);

-- ===== TEST DATA (OPTIONAL) =====
-- INSERT INTO visitors (firstname, surname, company, email) VALUES 
-- ('John', 'Doe', 'Acme Corp', 'john.doe@acme.com'),
-- ('Jane', 'Smith', 'Tech Solutions', 'jane.smith@tech.com');

-- ===== VERIFY TABLE CREATION =====
-- SELECT * FROM visitors;
