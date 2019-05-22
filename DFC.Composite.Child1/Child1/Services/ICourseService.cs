using Child1.Models;
using System.Collections.Generic;

namespace Child1.Services
{
    public interface ICourseService
    {
        Course GetCourse(int id);
        List<Course> GetCourses(string city = null, string category = null, bool filterThisMonth = false, bool filterNextMonth = false, string searchClue = null);
    }
}
