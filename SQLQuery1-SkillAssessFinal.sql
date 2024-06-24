ALTER TABLE Employees ADD ReportingManagerEmail VARCHAR(100);

ALTER TABLE Employees ADD CONSTRAINT FK_Employees_AuthUsers FOREIGN KEY (ReportingManagerEmail) REFERENCES AuthUsers(Email);

ALTER TABLE SkillAssessment ADD ProficiencyLevel VARCHAR(20);

CREATE TABLE SkillAssessment (
    AssessmentID INT IDENTITY(1,1) PRIMARY KEY,
    EmpID INT,
    SkillCategory VARCHAR(100),
    SkillName VARCHAR(100),
    BasicUnderstanding INT,
    WorkingExperience VARCHAR(100),
    ExtensiveExperience INT CHECK (ExtensiveExperience >= 0 AND ExtensiveExperience <= 10),
    SubjectMatterExperience INT CHECK (SubjectMatterExperience >= 0 AND SubjectMatterExperience <= 10),
    ProficiencyLevel VARCHAR(100),
    Status VARCHAR(50)
);
