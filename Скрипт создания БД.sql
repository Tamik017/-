USE GrishanAS107d2

CREATE TABLE Roles
(
id INT IDENTITY(1,1) PRIMARY KEY,
roleName VARCHAR(20),
);

CREATE TABLE Users
(
id INT IDENTITY(1,1) PRIMARY KEY,
fullname VARCHAR(50),
email VARCHAR(100),
userPassword VARCHAR(50),
post VARCHAR(50),
roleID INT REFERENCES Roles(id),
);

CREATE TABLE Projects
(
id INT IDENTITY(1,1) PRIMARY KEY,
projectName VARCHAR(100) UNIQUE NOT NULL,
projectEndDate date,
userID INT REFERENCES Users(id)
);

CREATE TABLE Area
(
id INT IDENTITY(1,1) PRIMARY KEY,
areaName VARCHAR(100),
x REAL,
y REAL,
projectID INT REFERENCES Projects(id)
);

CREATE TABLE Profiles
(
id INT IDENTITY(1,1) PRIMARY KEY,
profileName VARCHAR(100),
areaID INT REFERENCES Area(id),
operator INT REFERENCES Users(id)
);

CREATE TABLE Points
(
id INT IDENTITY(1,1) PRIMARY KEY,
x REAL,
y REAL,
induction INT,
shootingTime time(7),
shootingDate date,
number INT NOT NULL,
profileID INT REFERENCES Profiles(id),
);

CREATE TABLE Contracts
(
id INT IDENTITY(1,1) PRIMARY KEY,
price MONEY,
projectID INT UNIQUE,
FOREIGN KEY (projectID) REFERENCES Projects(id),
customer INT REFERENCES Users(id),
);