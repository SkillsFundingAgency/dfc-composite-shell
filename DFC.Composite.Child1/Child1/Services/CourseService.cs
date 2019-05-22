using Child1.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Child1.Services
{
    public class CourseService : ICourseService
    {
        private List<Course> _courses;

        public CourseService()
        {
            _courses = new List<Course>();

            _courses.Add(new Course()
            {
                Id = 1,
                Description = "This course will give the apprentice the skills needed to work in a variety of job roles such as Assistant Accountant & Trainee Accounting Technician",
                MaxAttendeeCount = 20,
                Start = DateTime.Parse("1 Sep 2019"),
                Title = "Accounting",
                City = "Coventry",
                Category = "Professional"
            });

            _courses.Add(new Course()
            {
                Id = 2,
                Description = "This course is designed to give you a grounding the professional skills needed to begin a career in the creative industries.",
                MaxAttendeeCount = 15,
                Start = DateTime.Parse("2 Sep 2019"),
                Title = "Art & Design",
                City = "Coventry",
                Category = "Arts"
            });

            _courses.Add(new Course()
            {
                Id = 3,
                Description = "The basic skills covered are speaking, listening, reading and writing in Spanish.",
                MaxAttendeeCount = 50,
                Start = DateTime.Parse("12 Sep 2019"),
                Title = "Beginners Spanish",
                City = "Solihull",
                Category = "Languages"
            });

            _courses.Add(new Course()
            {
                Id = 4,
                Description = "This course will immerse you in to an intricate and detailed insight of a photography studio",
                MaxAttendeeCount = 80,
                Start = DateTime.Parse("2 Sep 2019"),
                Title = "Practical Photography",
                City = "Birmingham",
                Category = "Media"
            });

            _courses.Add(new Course()
            {
                Id = 5,
                Description = "This course will immerse you in to an intricate and detailed insight of an engineering company",
                MaxAttendeeCount = 80,
                Start = DateTime.Today,
                Title = "Practical Engineering",
                City = "Birmingham",
                Category = "Professional"
            });

            _courses.Add(new Course()
            {
                Id = 6,
                Description = "This course will immerse you in to an intricate and detailed insight of a cinema photography",
                MaxAttendeeCount = 80,
                Start = DateTime.Today.AddDays(10),
                Title = "Cinema Photography",
                City = "Coventry",
                Category = "Arts"
            });

            _courses.Add(new Course()
            {
                Id = 7,
                Description = "This course will immerse you in to an intricate and detailed world of policing",
                MaxAttendeeCount = 80,
                Start = DateTime.Today.AddDays(20),
                Title = "Policing",
                City = "Birmingham",
                Category = "Professional"
            });

            _courses.Add(new Course()
            {
                Id = 8,
                Description = "This course will immerse you in to an intricate and detailed insight of the legal services",
                MaxAttendeeCount = 80,
                Start = DateTime.Today.AddDays(30),
                Title = "Legal Services",
                City = "Coventry",
                Category = "Professional"
            });
        }

        public List<Course> GetCourses(string city = null, string category = null, bool filterThisMonth = false, bool filterNextMonth = false, string searchClue = null)
        {
            var results = _courses;

            if (!string.IsNullOrEmpty(city))
            {
                results = results.Where(x => string.Compare(x.City, city, true) == 0).ToList();
            }

            if (!string.IsNullOrEmpty(category))
            {
                results = results.Where(x => string.Compare(x.Category, category, true) == 0).ToList();
            }

            if (filterThisMonth)
            {
                results = results.Where(x => x.Start.Year == DateTime.Now.Year && x.Start.Month == DateTime.Now.Month).ToList();
            }

            if (filterNextMonth)
            {
                var testDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);

                results = results.Where(x => x.Start.Year == testDate.Year && x.Start.Month == testDate.Month).ToList();
            }

            if (!string.IsNullOrEmpty(searchClue))
            {
                results = results.Where(x => x.Description.IndexOf(searchClue, StringComparison.InvariantCultureIgnoreCase) >= 0).ToList();
            }

            return results;
        }

        public Course GetCourse(int id)
        {
            return _courses.FirstOrDefault(x => x.Id == id);
        }
    }
}
