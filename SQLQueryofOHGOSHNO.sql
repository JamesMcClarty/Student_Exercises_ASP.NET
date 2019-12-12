SELECT s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID, c.id as CohortID, c.name as CohortName, i.id as InstructorID, i.first_name as InstructorFirst, i.last_name as IntructorLast, i.slack_handle as InstructorSlack, i.specialty as InstructorSpecialty, i.cohort_id as InstructorCohortID, e.id as ExerciseID, e.name as ExerciseName, e.language as ExerciseLanguage FROM students as s
INNER JOIN cohorts as c ON s.cohort_id = c.id
INNER JOIN instructors as i ON c.id = i.id
INNER JOIN studentexercises as se ON se.student_id = s.id
INNER JOIN exercises as e ON se.exercise_id = e.id;