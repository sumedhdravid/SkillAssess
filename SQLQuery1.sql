CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    FirstName VARCHAR(50),
    LastName VARCHAR(50),
    Email VARCHAR(100) UNIQUE,
    Password VARCHAR(100),
    JobRole VARCHAR(50),
    Gender VARCHAR(10),
    Photo VARBINARY(MAX),
    DOB DATE,
    ContactNumber VARCHAR(20),
    Address VARCHAR(200),
    DateOfJoining DATE,
    TotalExperience INT,
    BachelorDegree VARCHAR(50),
    BachelorSpecialization VARCHAR(100),
    MastersDegree VARCHAR(50),
    MastersSpecialization VARCHAR(100),
    Certifications VARCHAR(MAX)
);

CREATE TABLE AuthUsers (
    Email VARCHAR(100) PRIMARY KEY,
    Password VARCHAR(100),
    Role VARCHAR(50)
);

CREATE TABLE SkillAssessment (
    AssessmentID INT PRIMARY KEY,
    EmpID INT,
    SkillCategory VARCHAR(100),
    SkillName VARCHAR(100),
    BasicUnderstanding INT,
    WorkingExperience VARCHAR(100),
    ExtensiveExperience INT CHECK (ExtensiveExperience >= 0 AND ExtensiveExperience <= 10),
    SubjectMatterExperience INT CHECK (SubjectMatterExperience >= 0 AND SubjectMatterExperience <= 10),
    Status VARCHAR(50)
);
