using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class InstructorsController : Controller
    {
        private readonly IConfiguration config;

        public InstructorsController(IConfiguration _config)
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
        public async Task<IActionResult> GetAllInstructors([FromQuery]string? search)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT instructors.id as InstructorID, instructors.first_name as InstructorFirst, instructors.last_name as InstructorLast, instructors.slack_handle as InstructorSlack, instructors.cohort_id as InstructorCohortID, " +
                        "c.id as CohortID, c.name as CohortName, " +
                        "s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID " +
                        "FROM instructors " +
                        "LEFT JOIN cohorts as c ON instructors.cohort_id = c.id " +
                        "LEFT JOIN students as s ON c.id = s.cohort_id ";
                    if (search != null)
                        cmd.CommandText += $" WHERE instructors.first_name LIKE '%{search}%' OR instructors.last_name LIKE '%{search}%' OR instructors.slack_handle LIKE '%{search}%'";

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        //Check if instructor is in
                        int currentInstructorID = reader.GetInt32(reader.GetOrdinal("InstructorID"));
                        Instructor newInstructor = instructors.FirstOrDefault(i => i.Id == currentInstructorID);

                        //If the instructor is not on the list
                        if (newInstructor == null)
                        {
                            newInstructor = new Instructor
                            {
                                Id = currentInstructorID,
                                FirstName = reader.GetString(reader.GetOrdinal("InstructorFirst")),
                                LastName = reader.GetString(reader.GetOrdinal("InstructorLast")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohortID")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortID")),
                                    Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                    Students = new List<Student>(),
                                    Instructors = new List<Instructor>()
                                },
                            };
                            instructors.Add(newInstructor);
                        }

                        //Add new student in Instructor's cohort list
                        int currentStudentID = reader.GetInt32(reader.GetOrdinal("StudentID"));
                        foreach (Instructor inst in instructors)
                        {
                            if (inst.Cohort.Id == reader.GetInt32(reader.GetOrdinal("StudentCohortId")) && inst.Cohort.Students.FirstOrDefault(s => s.Id == currentStudentID) == null)
                            {
                                inst.Cohort.Students.Add(new Student
                                {
                                    Id = currentStudentID,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                });
                            }
                        }

                        //Check each instructors's cohort if cohort matches instructor cohort id
                        foreach (Instructor inst in instructors)
                        {
                            if (inst.Cohort.Id == reader.GetInt32(reader.GetOrdinal("InstructorCohortID")) && inst.Cohort.Instructors.FirstOrDefault(c => c.Id == currentInstructorID) == null)
                            {
                                inst.Cohort.Instructors.Add(new Instructor
                                {
                                    Id = currentInstructorID,
                                    FirstName = reader.GetString(reader.GetOrdinal("InstructorFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("InstructorLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                                });
                            }
                        }
                    }

                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        [HttpGet("{id}", Name = "GettingInstructor")]
        public async Task<IActionResult> GetOneInstructor([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, first_name, last_name, slack_handle, specialty, cohort_id FROM instructors WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    Instructor instructor = new Instructor();
                    while (reader.Read())
                    {
                        instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slack_handle"))
                        };
                    }

                    reader.Close();

                    return Ok(instructor);
                }
            }
        }
    }
}