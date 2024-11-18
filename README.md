# Herzkompass Datenbank


```sql
CREATE DATABASE IF NOT EXISTS Herzkompass;
USE Herzkompass;

CREATE TABLE accounts (
    id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    passwort VARCHAR(100),
    email VARCHAR(100),
    benutzername VARCHAR(45)
);

CREATE TABLE kundenprofil (
    id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    account_id INT,
    beruf VARCHAR(100),
    geburtstag DATE,
    wohnort VARCHAR(100),
    ueber_mich TEXT,
    profilbild VARCHAR(150),
    CONSTRAINT UNIQUE (account_id),
    CONSTRAINT fk_kundenprofil_account_id
        FOREIGN KEY (account_id)
        REFERENCES accounts (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);

CREATE TABLE profile_likes (
    id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    liker_id INT,
    liked_profile_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_liker_account
        FOREIGN KEY (liker_id) REFERENCES accounts (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_liked_profile
        FOREIGN KEY (liked_profile_id) REFERENCES kundenprofil (id)
        ON DELETE CASCADE
);

CREATE TABLE profile_favorites (
    id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    favoriter_id INT,
    favorite_profile_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_favoriter_account
        FOREIGN KEY (favoriter_id) REFERENCES accounts (id)
        ON DELETE CASCADE,
    CONSTRAINT fk_favorite_profile
        FOREIGN KEY (favorite_profile_id) REFERENCES kundenprofil (id)
        ON DELETE CASCADE
);

CREATE TABLE tickets (
    id INT PRIMARY KEY NOT NULL AUTO_INCREMENT,
    account_id INT NOT NULL,
    ticket_text TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_ticket_account
        FOREIGN KEY (account_id) REFERENCES accounts (id)
        ON DELETE CASCADE
        ON UPDATE CASCADE
);
