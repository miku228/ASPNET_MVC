using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Final22F.Models.DataAccess;
using Final22F.Models.ViewModels;

namespace Final22F.Controllers
{
    public class CoursesController : Controller
    {
        private readonly RegistrationDBContext _context;

        public CoursesController(RegistrationDBContext context)
        {
            _context = context;
        }

        // GET: Courses
        public IActionResult Search()
        {
            return View();
        }

        //Add your code here to implement the requirements listed in the final exam document.

        // Hi Professor,
        // thank you for whole lecture that was fun and I found c# and asp.net especially about MVC is interesting
        // in next term Ill do eCo-op because i couldnt find the co-op position for coming term
        // I hope I can see you 2023 Fall term
        // Thank you and Merry Christmas



        // Since I use Parallels and Im not sure about the database settings,
        // I wrote whole code without debugging 
        // it might have issues connecting database related to appsettings.json file


        // --------------------------------------
        // ------ Search Courses(10 point) ------
        // --------------------------------------
        // POST: Courses/Search
        [HttpPost] 
        public IActionResult Search(SearchViewModel titleSearchViewModel)
        {
            
            if (ModelState.IsValid)
            {
                string searchString = titleSearchViewModel.SearchString;
                List<Course> searchResults = _context.Courses.Include(c => c.Students).Where(c => c.CourseTitle.Contains(searchString)).ToList();

                // check wherther course contained SearchString or not
                if(searchResults.Count == 0)
                {
                    // when there is no courses contained SearchString, set the error message
                    ModelState.AddModelError("SearchString", "No course found with specific search characters");
                }
                else
                {
                    // when there is courses contained SearchString,
                    // return the searchResults and searchString(put it in ViewData) and pass it to index page
                    ViewData["searchString"] = searchString;
                    return View("Index", searchResults);
                }

            }
            return View(titleSearchViewModel);


        }

        // --------------------------------------
        // ------        Index             ------
        // --------------------------------------
        
        // GET: Courses/Index
        public async Task<IActionResult> Index()
        {
            // when the user clicked Show all courses link in the search page, 
            // return all courses in the database
            return View(await _context.Courses.Include(c => c.Students).ToListAsync());
        }

        // --------------------------------------
        // ------  Edit Registrations     ------
        // --------------------------------------

        // GET: Courses/Edit/5(Id)
        public async Task<IActionResult> EditRegistrations(string? CourseId)
        {
            if (CourseId == null || _context.Courses == null)
            {
                return NotFound();
            }
            // get the Course which has same courseId from database 
            var course = await _context.Courses.Include(c => c.Students).SingleOrDefaultAsync(c => c.CourseId == CourseId);

            // get all Student from the database
            var students = await _context.Students.ToListAsync();
            // make studentSelection List from the data
            var studentSelections = new List<StudentSelection>();
            foreach (Student st in students)
            {
                studentSelections.Add(new StudentSelection(st));
            }

            
            // whether the selected course is exist or not
            if(course == null)
            {
                return NotFound();
            }
            // pass the selected courses courseViewModel to the page 
            CourseViewModel CourseVM = new CourseViewModel(course, studentSelections);
            return View(CourseVM);

        }


        // POST: Courses/Edit/5(Id)
        [HttpPost]
        public async Task<IActionResult> EditRegistrations(CourseViewModel courseVM)
        {
            if (ModelState.IsValid)
            {
                Course course = _context.Courses.Include(c => c.Students).SingleOrDefault(c => c.CourseId == courseVM.TheCourse.CourseId);
                List<StudentSelection> studentSelection = courseVM.StudentSelections;
                var selectedStudent = new List<StudentSelection>();
                foreach (StudentSelection sts in studentSelection)
                {
                    if (sts.Selected) selectedStudent.Add(sts);
                }

                // check whether the number of selected student is greter than the maximum registration of the course
                if(selectedStudent.Count > course.MaxRegistrations) ModelState.AddModelError("StudentSelections", "Your selection exceeds the courses max registration!");

                if(course != null)
                {
                    // update course
                    course.CourseId = courseVM.TheCourse.CourseId;
                    course.CourseTitle = courseVM.TheCourse.CourseTitle;
                }

                // update course table in the database
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }

            // after registration and update database is finished,
            // redirect to index page displaying all courses

            return RedirectToAction(nameof(Index));
        }


    }
}
