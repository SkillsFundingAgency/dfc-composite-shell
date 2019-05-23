using Microsoft.AspNetCore.Mvc;
using Child1.Models;
using Child1.Services;
using System;

namespace Child1.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public IActionResult Index()
        {
            var vm = new CourseIndexViewModel();

            vm.Courses = _courseService.GetCourses();

            return View(vm);
        }

        public IActionResult Search(string searchClue)
        {
            if (!string.IsNullOrEmpty(searchClue))
            {
                return RedirectToAction(nameof(Index), new { searchClue });
            }

            var vm = new SearchViewModel();

            return View(vm);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _courseService.GetCourse(id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Course course)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }
    }
}
