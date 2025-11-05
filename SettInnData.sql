-- sql sporring legge og fjerne data alle disse er dummmy data for å teste funksjonalitet i applikasjonen
-- Kan endres eller fjernes etteer behov om hva gruppen ønsker å teste

USE kartverket_db;


-- 1) Organisasjon - Lag én felles organisasjon
INSERT INTO Organisation (OrgName) VALUES ('NRL');

-- 2) Finn ID-en (si at den ble 1)
SELECT * FROM Organisation;

-- 3) Legg inn brukere (pilot, registrarfører, admin) med OrgID=1
INSERT INTO Users (OrgID, Email, Username, PasswordSalt) VALUES
(1, 'pilot@test.no',      'pilot',      'TEMP_ONLY'),
(1, 'registrar@test.no',  'registrar',  'TEMP_ONLY'),
(1, 'admin@test.no',      'admin',      'TEMP_ONLY');

INSERT INTO Role (RoleName) VALUES ('Pilot'), ('Registerfører'), ('Admin');

-- Koble brukere til roller (bruk riktige UserID-er fra SELECT)
-- Eksempel: anta at pilot=1, registrar=2, admin=3
INSERT INTO UserRoles (UserID, RoleID) VALUES
(1, 1), -- pilot -> Pilot
(2, 2), -- registrar -> Registerfører
(3, 3); -- admin -> Admin

-- Status 
INSERT INTO Statuses (StatusName) VALUES ('Submitted'), ('Approved'), ('Rejected');

-- Kategorier
INSERT INTO Categories (CategoryName) VALUES ('Obstacle');

-- Dato/tid 
INSERT INTO DateTimestamp (DateCreated) VALUES (NOW());

-- Eksempelrapport fra pilot (Status = Submitted)

INSERT INTO Report
(Title, UserID, GeoLocation, HeightInFeet, ReportDescription, StatusID, CategoryID, DateID, AssignedToUser, AssignedAt)
VALUES
('Wind turbines', 1, ST_GeomFromText('POINT(7.955571 58.112380)', 4326), 150.0, 'A wind turbine in the way of the path', 1, 1, 1, 2, NOW());

-- Eksempelrapport fra registrarfører (Status = Approved)
INSERT INTO Report
(Title, UserID, GeoLocation, HeightInFeet, ReportDescription,
 StatusID, CategoryID, DateID, AssignedToUser, AssignedAt, DecisionByUser, DecisionAt)
VALUES
('Tower near Lund bro', 
 1, 
 ST_GeomFromText('POINT(8.0125, 58.1580)', 4326),
 210.0, 
 'Report of a tall tower near Lund Bridge — approved by the registrar.',
 2, 1, 1, 2, NOW(), 2, NOW());

-- Eksempelrapport fra registrarfører (Status = Rejected)
INSERT INTO Report
(Title, UserID, GeoLocation, HeightInFeet, ReportDescription,
 StatusID, CategoryID, DateID, AssignedToUser, AssignedAt, DecisionByUser, DecisionAt)
VALUES
('Incorrect height entry', 
 1, 
 ST_GeomFromText('POINT((7.9805, 58.1350)', 4326),
 5000.0, 
 'Pilot entered an incorrect height; the report was rejected by the registrar.',
 3, 1, 1, 2, NOW(), 2, NOW());