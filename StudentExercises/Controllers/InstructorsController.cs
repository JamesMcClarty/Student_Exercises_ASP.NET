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
        public async Task<IActionResult> GetAll()
        {
            using (SqlConnection conn = Connection)
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, first_name, last_name, slack_handle, specialty, cohort_id FROM instructors";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Instructor> instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("slack_handle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
                        };

                        instructors.Add(instructor);
                    }

                    reader.Close();

                    return Ok(instructors);
                }
            }
        }

        [HttpGet("{id}", Name = "GettingInstructor")]
        public async Task<IActionResult> GetOne([FromRoute] int id)
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