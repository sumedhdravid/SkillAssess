DROP TABLE SkillAssessment;

CREATE TABLE SkillAssessment (
    AssessmentID INT PRIMARY KEY IDENTITY,
    EmpID INT,
    DatabaseRating INT CHECK (DatabaseRating >= 0 AND DatabaseRating <= 10),
	ProgrammingRating INT CHECK (ProgrammingRating >=0 AND ProgrammingRating <=10),
	JavaRating INT CHECK (JavaRating >=0 AND JavaRating <=10),
	CSRating INT CHECK (CSRating >=0 AND CSRating <=10),
	PythonRating INT CHECK (PythonRating >=0 AND PythonRating <=10),
	WebProgrammingRating INT CHECK (WebProgrammingRating >=0 AND WebProgrammingRating <=10),
    OtherTechnical NVARCHAR(100),
    VerbalCommunication NVARCHAR(50),
	WrittenCommunication NVARCHAR(50),
	Teamwork NVARCHAR(50),
	ProblemSolving NVARCHAR(50),
	DecisionMaking NVARCHAR(50),
	Leadership NVARCHAR(50),
    AnyForeignLanguage NVARCHAR(100),
    Status NVARCHAR(50)
);
