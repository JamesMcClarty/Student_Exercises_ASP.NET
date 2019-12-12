using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace StudentExercises.Controllers
{
    [Route("{api/[controller]}")]
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
        public async Task<IActionResult> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID, " +
                        "c.id as CohortID, c.name as CohortName, " +
                        "i.id as InstructorID, i.first_name as InstructorFirst, i.last_name as IntructorLast, i.slack_handle as InstructorSlack, i.specialty as InstructorSpecialty, i.cohort_id as InstructorCohortID, " +
                        "e.id as ExerciseID, e.name as ExerciseName, e.language as ExerciseLanguage " +
                        "FROM students as s"+
                        "INNER JOIN cohorts as c ON s.cohort_id = c.id " +
                        "INNER JOIN instructors as i ON c.id = i.id " +
                        "INNER JOIN studentexercises as se ON se.student_id = s.id " +
                        "INNER JOIN exercises as e ON se.exercise_id = e.id;";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();
                    while (reader.Read())
                    {
                        // Start this tomorrow
                        int currentStudentID = reader.GetInt32(reader.GetOrdinal("StudentID");
                        if (students)
                        Student newStudent = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slack_handle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("cohort_id"))
                        };

                        students.Add(newStudent);
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