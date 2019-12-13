SELECT s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID, c.id as CohortID, c.name as CohortName, i.id as InstructorID, i.first_name as InstructorFirst, i.last_name as IntructorLast, i.slack_handle as InstructorSlack, i.specialty as InstructorSpecialty, i.cohort_id as InstructorCohortID, e.id as ExerciseID, e.name as ExerciseName, e.language as ExerciseLanguage FROM students as s
LEFT JOIN cohorts as c ON s.cohort_id = c.id
LEFT JOIN instructors as i ON c.id = i.id
LEFT JOIN studentexercises as se ON se.student_id = s.id
INNER JOIN exercises as e ON se.exercise_id = e.id;


SELECT instructors.id as InstructorID, instructors.first_name as InstructorFirst, instructors.last_name as InstructorLast, instructors.slack_handle as InstructorSlack, instructors.cohort_id as InstructorCohortID,
                        c.id as CohortID, c.name as CohortName,
                        s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID
                        FROM instructors
                        LEFT JOIN cohorts as c ON instructors.cohort_id = c.id
                        LEFT JOIN students as s ON c.id = s.cohort_id;