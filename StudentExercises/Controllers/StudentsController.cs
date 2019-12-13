using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Controllers
{
    [Route("{controller}/{action}")]
    [ApiController]
    public class StudentsController : Controller
    {
        private readonly IConfiguration config;

        public StudentsController(IConfiguration _config)
        {
            config = _config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(config.GetConnectionString("defaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStudents()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT students.id as StudentID, students.first_name as StudentFirst, students.last_name as StudentLast, students.slack_handle as StudentSlack, students.cohort_id as StudentCohortID, " +
                        "c.id as CohortID, c.name as CohortName, " +
                        "i.id as InstructorID, i.first_name as InstructorFirst, i.last_name as InstructorLast, i.slack_handle as InstructorSlack, i.specialty as InstructorSpecialty, i.cohort_id as InstructorCohortID, " +
                        "e.id as ExerciseID, e.name as ExerciseName, e.language as ExerciseLanguage, " +
                        "se.student_id as StudentExerciseStudentID, se.exercise_id as StudentExerciseExerciseID " +
                        "FROM students " +
                        "LEFT JOIN cohorts as c ON students.cohort_id = c.id " +
                        "LEFT JOIN instructors as i ON c.id = i.id " +
                        "LEFT JOIN studentexercises as se ON se.student_id = students.id " +
                        "INNER JOIN exercises as e ON se.exercise_id = e.id;";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        //Check if student is in
                        int currentStudentID = reader.GetInt32(reader.GetOrdinal("StudentID"));
                        Student newStudent = students.FirstOrDefault(s => s.Id == currentStudentID);

                        //If the student is not on the list
                        if (newStudent == null)
                        {
                            newStudent = new Student
                            {
                                Id = currentStudentID,
                                FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohortID")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>()
                                },
                                Exercises = new List<Exercise>()
                            };

                            students.Add(newStudent);
                        }
                        //Check each student's cohort if cohort matches student's cohort id
                        foreach(Student stud in students)
                        {
                            if(stud.Cohort.Id == reader.GetInt32(reader.GetOrdinal("StudentCohortId")) && stud.Cohort.Students.FirstOrDefault(c => c.Id == currentStudentID) == null){
                                stud.Cohort.Students.Add(new Student {
                                    Id = currentStudentID,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                });
                            }
                        }

                        //Check each student's cohort if cohort matches instructor cohort id
                        int currentInstructorId = reader.GetInt32(reader.GetOrdinal("InstructorID"));

                        foreach (Student stud in students)
                        {
                            if(stud.Cohort.Id == reader.GetInt32(reader.GetOrdinal("InstructorCohortID")) && stud.Cohort.Instructors.FirstOrDefault(c => c.Id == currentInstructorId) == null){
                                stud.Cohort.Instructors.Add(new Instructor {
                                    Id = currentInstructorId,
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                });
                            }
                        }

                        //Check each student's exercise list if exercise matches ids.

                        int currentExerciseID = reader.GetInt32(reader.GetOrdinal("ExerciseID"));

                        foreach (Student stud in students)
                        {
                            if (stud.Exercises.FirstOrDefault(e => e.Id == currentExerciseID) == null && currentStudentID == reader.GetInt32(reader.GetOrdinal("StudentExerciseStudentID")))
                            {
                                stud.Exercises.Add(new Exercise
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("ExerciseID")),
                                    Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                    Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage")),
                                }
                                );
                            }
                        }
                    }

                    reader.Close();

                    return Ok(students);
                }
            }
        }

        [HttpGet("{id}", Name = "GettingStudent")]
        public async Task<IActionResult> GetOneStudent([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, first_name, last_name, slack_handle, cohort_id FROM students WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Student student = new Student();
                    while (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slack_handle"))
                        };
                    }

                    reader.Close();

                    return Ok(student);
                }
            }
        }
    }
}