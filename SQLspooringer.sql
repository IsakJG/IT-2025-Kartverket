--Database sql spørringer 

CREATE DATABASE IF NOT EXISTS kartverket_db
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;
USE kartverket_db;


--  Organistasjon 
CREATE TABLE Organisation (
OrgID INT AUTO_INCREMENT PRIMARY KEY, 
OrgName VARCHAR(50) NOT NULL
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- User 
CREATE TABLE Users (
  UserID INT AUTO_INCREMENT PRIMARY KEY,
  OrgID INT NOT NULL,
  Email VARCHAR(255) UNIQUE,
  Username VARCHAR(50) NOT NULL,
  PasswordSalt VARCHAR(255) NOT NULL,
  CONSTRAINT fk_user_org
    FOREIGN KEY (OrgID)
    REFERENCES Organisation(OrgID)
    ON UPDATE CASCADE
    ON DELETE RESTRICT
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- Role 
CREATE TABLE Role (
  RoleID INT AUTO_INCREMENT PRIMARY KEY,
  RoleName VARCHAR(50) NOT NULL,
  RoleDescription TINYTEXT
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- UserRole 
CREATE TABLE UserRoles (
  UserID INT NOT NULL,
  RoleID INT NOT NULL,
  PRIMARY KEY (UserID, RoleID),
  FOREIGN KEY (UserID) REFERENCES Users(UserID)
    ON UPDATE CASCADE
    ON DELETE CASCADE,
  FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
    ON UPDATE CASCADE
    ON DELETE RESTRICT
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;





-- Images 
CREATE TABLE Images (
  ImageID INT AUTO_INCREMENT PRIMARY KEY,
  ImageURL VARCHAR(255) NOT NULL,
  ImageHeight INT,
  ImageLength INT
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- Status 
CREATE TABLE Statuses (
  StatusID INT AUTO_INCREMENT PRIMARY KEY,
  StatusName VARCHAR(50) NOT NULL
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- Categories 
CREATE TABLE Categories (
  CategoryID INT AUTO_INCREMENT PRIMARY KEY,
  CategoryName VARCHAR(50) NOT NULL
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- Timestamp 
CREATE TABLE DateTimestamp (
  DateID INT AUTO_INCREMENT PRIMARY KEY,
  DateCreated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  DateOfLastChange TIMESTAMP NULL
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- Report 
CREATE TABLE Report (
  ReportID     INT AUTO_INCREMENT PRIMARY KEY,
  UserID       INT NOT NULL,
  Title        VARCHAR(100) NOT NULL,
  GeoLocation  POINT NULL,
  HeightInFeet Decimal(8,1) NOT NULL CHECK (HeightInFeet >= 0), -- trenger ikke desimaltall, men ha de med for hvis de skriver inn noe med komma
  
  ReportDescription  MEDIUMTEXT NULL,
  ImageID      INT,
  StatusID     INT NOT NULL,
  CategoryID   INT NOT NULL,
  Feedback     MEDIUMTEXT,
  DateID       INT,
  AssignedToUser INT, 
  AssignedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  DecisionByUser INT,
  DecisionAt TIMESTAMP NULL,  
  FOREIGN KEY (UserID)    REFERENCES Users(UserID),
  FOREIGN KEY (ImageID)   REFERENCES Images(ImageID),
  FOREIGN KEY (StatusID)  REFERENCES Statuses(StatusID),
  FOREIGN KEY (CategoryID)REFERENCES Categories(CategoryID),
  FOREIGN KEY (DateID)    REFERENCES DateTimestamp(DateID),
  FOREIGN KEY (AssignedToUser) REFERENCES Users(UserID) ,
  FOREIGN KEY (DecisionByUser) REFERENCES Users(UserID)
)ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


