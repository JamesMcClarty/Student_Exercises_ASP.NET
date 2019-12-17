using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StudentExercises.Models
{
    public class Cohort
    {
        public int Id { get; set; }
        [Required]
        [StringLength(11, MinimumLength = 5)]
        public string Name { get; set; }
        public List<Student> Students { get; set; }
        public List<Instructor> Instructors { get; set; }

    }
}
