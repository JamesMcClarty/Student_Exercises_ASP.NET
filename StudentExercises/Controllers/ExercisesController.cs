using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudentExercises.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class ExercisesController : Controller
    {
        private readonly IConfiguration config;

        public ExercisesController(IConfiguration _config)
        {
            config = _config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet] //Get all exercises
        public async Task<IActionResult> GetAllExercises([FromQuery]string? include, [FromQuery]string? search)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT exercises.id as ExerciseID, exercises.name as ExerciseName, exercises.language as ExerciseLanguage";
                    if (include == "students")
                    {
                        cmd.CommandText += ", se.student_id as StudentExerciseStudentID, se.exercise_id as StudentExerciseExerciseID, " +
                        "s.id as StudentID, s.first_name as StudentFirst, s.last_name as StudentLast, s.slack_handle as StudentSlack, s.cohort_id as StudentCohortID";
                    }
                    cmd.CommandText += " FROM exercises";
                    if (include == "students")
                    {
                        cmd.CommandText += " LEFT JOIN studentexercises as se ON exercises.id = se.exercise_id" +
                          " INNER JOIN students as s ON se.student_id = s.id";
                    }
                    if (search != null)
                    {
                        cmd.CommandText += " WHERE exercises.name LIKE @search OR exercises.language LIKE @search";
                        cmd.Parameters.Add(new SqlParameter("@search", "%" + search + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Exercise> exercises = new List<Exercise>();

                    while (reader.Read())
                    {
                        //Check if exercise is in
                        int currentExerciseID = reader.GetInt32(reader.GetOrdinal("ExerciseID"));
                        Exercise newExercise = exercises.FirstOrDefault(e => e.Id == currentExerciseID);

                        if (newExercise == null)
                        {
                            newExercise = new Exercise
                            {
                                Id = currentExerciseID,
                                Name = reader.GetString(reader.GetOrdinal("ExerciseName")),
                                Language = reader.GetString(reader.GetOrdinal("ExerciseLanguage")),
                                Students = new List<Student>()
                            };
                            exercises.Add(newExercise);
                        }

                        if (include == "students")
                        {
                            //Add new student in appropriate exercise list IF Include = "students"
                            int currentStudentID = reader.GetInt32(reader.GetOrdinal("StudentID"));
                            foreach (Exercise exer in exercises)
                            {
                                if (exer.Id == reader.GetInt32(reader.GetOrdinal("StudentExerciseExerciseID")) && exer.Students.FirstOrDefault(s => s.Id == currentStudentID) == null)
                                {
                                    exer.Students.Add(new Student
                                    {
                                        Id = currentStudentID,
                                        FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                                        LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                                        SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                                    });
                                }
                            }
                        }
                    }
                    reader.Close();

                    return Ok(exercises);
                }
            }
        }

        [HttpGet("{id})", Name = "GetExercise")]
        public async Task<IActionResult> GetOneExercise([FromRoute]int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, name, language FROM exercises WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Exercise exercise = new Exercise();

                    while (reader.Read())
                    {
                        exercise = new Exercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Language = reader.GetString(reader.GetOrdinal("language"))
                        };
                    }

                    reader.Close();

                    if (exercise == null)
                    {
                        return NotFound();
                    }

                    return Ok(exercise);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostExercise([FromBody] Exercise exercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO exercises (name, language)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @languagee)";
                    cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                    cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));

                    int newId = (int)cmd.ExecuteScalar();
                    exercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, exercise);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutExercise([FromRoute] int id, [FromBody] Exercise exercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE exercises
                                            SET name = @name,
                                                language = @language
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", exercise.Name));
                        cmd.Parameters.Add(new SqlParameter("@language", exercise.Language));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest();
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM exercises WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        return BadRequest();
                    }
                }
            }
            catch (Exception)
            {
                if (!ExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT id, name, language
                        FROM exercises
                        WHERE id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}