using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace StudentExercises.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class CohortsController : Controller
    {
        private readonly IConfiguration config;

        public CohortsController(IConfiguration _config)
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
        public async Task<IActionResult> GetAllCohorts([FromQuery]string? search)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT cohorts.id as CohortID, cohorts.name as CohortName," +
                        "i.id as InstructorID, i.first_name as InstructorFirst, i.last_name as InstructorLast, i.slack_handle as InstructorSlack, i.cohort_id as InstructorCohortID, " +
                        "s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID " +
                        "FROM cohorts " +
                        "LEFT JOIN students as s ON cohorts.id = s.cohort_id " +
                        "LEFT JOIN instructors as i ON cohorts.id = i.cohort_id";
                    if (search != null)
                    {
                        cmd.CommandText += " WHERE cohorts.name LIKE @search";
                        cmd.Parameters.Add(new SqlParameter("@search", "%" + search + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Cohort> cohorts = new List<Cohort>();
                    while (reader.Read())
                    {
                        int currentCohortID = reader.GetInt32(reader.GetOrdinal("CohortID"));
                        Cohort newCohort = cohorts.FirstOrDefault(i => i.Id == currentCohortID);
                        //If there's no cohort, create one and add it to the list.
                        if (newCohort == null)
                        {
                            newCohort = new Cohort
                            {
                                Id = currentCohortID,
                                Name = reader.GetString(reader.GetOrdinal("CohortName")),
                                Students = new List<Student>(),
                                Instructors = new List<Instructor>()
                            };
                            cohorts.Add(newCohort);
                        }

                        //Add new student in Instructor's cohort list
                        int currentStudentID = reader.GetInt32(reader.GetOrdinal("StudentID"));
                        foreach (Cohort cohort in cohorts)
                        {
                            if (cohort.Id == reader.GetInt32(reader.GetOrdinal("StudentCohortId")) && cohort.Students.FirstOrDefault(s => s.Id == currentStudentID) == null)
                            {
                                cohort.Students.Add(new Student
                                {
                                    Id = currentStudentID,
                                    FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                    LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                    SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                });
                            }
                        }

                        //Check if cohort matches instructor cohort id
                        int currentInstructorID = reader.GetInt32(reader.GetOrdinal("InstructorID"));
                        foreach (Cohort cohort in cohorts)
                        {
                            if (cohort.Id == reader.GetInt32(reader.GetOrdinal("InstructorCohortID")) && cohort.Instructors.FirstOrDefault(c => c.Id == currentInstructorID) == null)
                            {
                                cohort.Instructors.Add(new Instructor
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

                    return Ok(cohorts);
                }
            }
        }

        [HttpGet("{id})", Name = "GetCohort")]
        public async Task<IActionResult> GetOneCohort([FromRoute]int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, name FROM cohorts WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = new Cohort();

                    while (reader.Read())
                    {
                        cohort = new Cohort()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                        };
                    }

                    reader.Close();

                    if (cohort == null)
                    {
                        return NotFound();
                    }

                    return Ok(cohort);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostCohort([FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                Regex dayRegex = new Regex(@"(?:day?)+ +\d{1,2}");
                Regex eveningRegex = new Regex(@"(?:evening?)+ +\d{1,2}");

                bool dayMatched = dayRegex.IsMatch(cohort.Name.ToLower());
                bool eveningMatched = dayRegex.IsMatch(cohort.Name.ToLower());

                if (dayMatched || eveningMatched)
                {

                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"INSERT INTO cohorts (name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));
                        int newId = (int)cmd.ExecuteScalar();

                        cohort.Id = newId;
                        return CreatedAtRoute("GetCohort", new { id = newId }, cohort);
                    }
                }

                else
                {
                    return BadRequest("Cohort name should be in the format of [Day|Evening] [number]");
                }
            }
        }
    }
}