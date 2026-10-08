using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using techninxa.Models;
using Techninxa;
using Techninxa.Models;

namespace techninxa.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        private readonly IConfiguration Configuration;
        public HomeController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            Configuration = configuration;
        }

        // ==============================
        // HOME
        // ==============================
        public async Task<IActionResult> Index()
        {
            var projects = await dbContext.Projects
                .Where(p => p.Status == "published")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Projects = projects;


            // Load active team members
            var teamMembers = await dbContext.TeamMembers
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();

            ViewBag.TeamMembers = teamMembers;


            return View(new ContactMessage());
        }
        // ==============================
        // MAKE A SCHEDULE
        // ==============================
        [Route("/makeschedule")]
        [HttpGet]
        public async Task<IActionResult> MakeASchedule()
        {
            var holidays = await dbContext.HolidaySettings
                .Where(x => x.IsHoliday)
                .Select(x => x.Day)
                .ToListAsync();

            ViewBag.Holidays = holidays;

            return View();
        }
        // ==============================
        // GET AVAILABLE TIME SLOTS
        // ==============================
        [Route("/makeschedule/slots")]
        [HttpGet]
        public async Task<IActionResult> GetScheduleSlots(DateTime date)
        {
            date = date.Date;

            // ==============================
            // PAST DATE CHECK
            // ==============================
            if (date < DateTime.Today)
            {
                return Json(new
                {
                    success = false,
                    message = "You cannot select a past date.",
                    slots = new object[] { }
                });
            }

            // ==============================
            // HOLIDAY CHECK
            // ==============================
            var weekday = (Weekday)(((int)date.DayOfWeek + 1) % 7);

            var holiday = await dbContext.HolidaySettings
                .FirstOrDefaultAsync(x => x.Day == weekday);

            if (holiday != null && holiday.IsHoliday)
            {
                return Json(new
                {
                    success = false,
                    holiday = true,
                    message = "This day is unavailable.",
                    slots = new object[] { }
                });
            }

            // ==============================
            // GET ACTIVE TIME SLOTS
            // ==============================
            var timeSlots = await dbContext.TimeSlots
                .Where(x => x.IsActive)
                .OrderBy(x => x.StartTime)
                .ToListAsync();

            // ==============================
            // GET BOOKED SLOTS
            // ==============================
            var bookedSlots = await dbContext.Meetings
                .Where(x =>
                    x.MeetingDate == date &&
                    !x.IsCancelled)
                .Select(x => new
                {
                    x.StartTime,
                    x.EndTime
                })
                .ToListAsync();

            // ==============================
            // REMOVE BOOKED SLOTS
            // ==============================
            var availableSlots = timeSlots
                .Where(slot =>
                    !bookedSlots.Any(booked =>
                        booked.StartTime == slot.StartTime &&
                        booked.EndTime == slot.EndTime))
                .Select(slot => new
                {
                    id = slot.Id,

                    startTime = slot.StartTime
                        .ToString(@"hh\:mm"),

                    endTime = slot.EndTime
                        .ToString(@"hh\:mm"),

                    displayStart = DateTime.Today
                        .Add(slot.StartTime)
                        .ToString("h:mm tt"),

                    displayEnd = DateTime.Today
                        .Add(slot.EndTime)
                        .ToString("h:mm tt")
                })
                .ToList();

            return Json(new
            {
                success = true,
                holiday = false,
                date = date.ToString("yyyy-MM-dd"),
                slots = availableSlots
            });
        }
        // ==============================
        // BOOK MEETING
        // ==============================
        [Route("/makeschedule/book")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookMeeting(
            string name,
            string email,
            string? projectType,
            string? message,
            DateTime meetingDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            // ==============================
            // BASIC VALIDATION
            // ==============================
            if (string.IsNullOrWhiteSpace(name))
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter your name."
                });
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter your email."
                });
            }

            if (meetingDate.Date < DateTime.Today)
            {
                return Json(new
                {
                    success = false,
                    message = "You cannot book a past date."
                });
            }

            if (startTime >= endTime)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid time slot."
                });
            }

            meetingDate = meetingDate.Date;

            // ==============================
            // CHECK HOLIDAY AGAIN
            // ==============================
            var weekday = (Weekday)(((int)meetingDate.DayOfWeek + 1) % 7);

            var holiday = await dbContext.HolidaySettings
                .FirstOrDefaultAsync(x => x.Day == weekday);

            if (holiday != null && holiday.IsHoliday)
            {
                return Json(new
                {
                    success = false,
                    message = "This date is unavailable."
                });
            }

            // ==============================
            // CHECK TIME SLOT
            // ==============================
            var timeSlot = await dbContext.TimeSlots
                .FirstOrDefaultAsync(x =>
                    x.StartTime == startTime &&
                    x.EndTime == endTime &&
                    x.IsActive);

            if (timeSlot == null)
            {
                return Json(new
                {
                    success = false,
                    message = "This time slot is no longer available."
                });
            }

            // ==============================
            // CHECK IF ALREADY BOOKED
            // ==============================
            var alreadyBooked = await dbContext.Meetings
                .AnyAsync(x =>
                    x.MeetingDate == meetingDate &&
                    x.StartTime == startTime &&
                    x.EndTime == endTime &&
                    !x.IsCancelled);

            if (alreadyBooked)
            {
                return Json(new
                {
                    success = false,
                    message = "Sorry, this time slot has already been booked."
                });
            }

            // ==============================
            // CREATE MEETING
            // ==============================
            var meeting = new Meeting
            {
                Name = name.Trim(),

                Email = email.Trim(),

                ProjectType = string.IsNullOrWhiteSpace(projectType)
                    ? null
                    : projectType.Trim(),

                Message = string.IsNullOrWhiteSpace(message)
                    ? null
                    : message.Trim(),

                MeetingDate = meetingDate,

                StartTime = startTime,

                EndTime = endTime,

                CreatedAt = DateTime.Now,

                IsRead = false,

                IsCancelled = false
            };

            dbContext.Meetings.Add(meeting);

            await dbContext.SaveChangesAsync();

            // ==============================
            // SUCCESS RESPONSE
            // ==============================
            return Json(new
            {
                success = true,

                message = "Your meeting has been scheduled successfully.",

                meeting = new
                {
                    id = meeting.Id,

                    name = meeting.Name,

                    date = meeting.MeetingDate
                        .ToString("dddd, MMMM d, yyyy"),

                    startTime = DateTime.Today
                        .Add(meeting.StartTime)
                        .ToString("h:mm tt"),

                    endTime = DateTime.Today
                        .Add(meeting.EndTime)
                        .ToString("h:mm tt")
                }
            });
        }

        // ==============================
        // INSERT PROJECT - PROTECTED
        // ==============================
        [Route("/InsertProject")]
        public IActionResult AddProjects()
        {
            // Check if admin is logged in
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }

            return View();
        }
        // ==============================
        // ADD PROJECT - POST
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProject(ProjectCreateViewModel model)
        {
            // ==========================================
            // CHECK ADMIN LOGIN
            // ==========================================

            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }


            // ==========================================
            // HERO IMAGE REQUIRED
            // ==========================================

            if (model.HeroImage == null || model.HeroImage.Length == 0)
            {
                ModelState.AddModelError(
                    "HeroImage",
                    "Hero image is required."
                );
            }


            // ==========================================
            // IF MODEL VALIDATION FAILED
            // ==========================================

            //if (!ModelState.IsValid)
            //{
            //    return View("AddProjects", model);
            //}


            // ==========================================
            // ALLOWED FILE TYPES
            // ==========================================

            string[] allowedImageTypes =
            {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

            string[] allowedVideoTypes =
            {
        "video/mp4",
        "video/webm",
        "video/quicktime"
    };


            // ==========================================
            // VALIDATE HERO IMAGE
            // ==========================================

            if (!allowedImageTypes.Contains(
                model.HeroImage!.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError(
                    "HeroImage",
                    "Only JPG, PNG and WEBP images are allowed."
                );

                return View("AddProjects", model);
            }


            // Maximum 5 MB
            const long maxHeroSize = 5 * 1024 * 1024;

            if (model.HeroImage.Length > maxHeroSize)
            {
                ModelState.AddModelError(
                    "HeroImage",
                    "Hero image cannot be larger than 5 MB."
                );

                return View("AddProjects", model);
            }


            // ==========================================
            // GENERATE SLUG
            // ==========================================

            string slug;

            if (string.IsNullOrWhiteSpace(model.Slug))
            {
                slug = GenerateSlug(model.Title);
            }
            else
            {
                slug = GenerateSlug(model.Slug);
            }


            // ==========================================
            // MAKE SLUG UNIQUE
            // ==========================================

            string baseSlug = slug;
            int slugNumber = 1;

            while (await dbContext.Projects.AnyAsync(
                p => p.Slug == slug))
            {
                slug = $"{baseSlug}-{slugNumber}";
                slugNumber++;
            }


            // ==========================================
            // CREATE PROJECT
            // ==========================================

            var project = new Project
            {
                Title = model.Title.Trim(),

                Category = model.Category.Trim(),

                Client = string.IsNullOrWhiteSpace(model.Client)
                    ? null
                    : model.Client.Trim(),

                Year = model.Year,

                ProjectUrl = string.IsNullOrWhiteSpace(model.ProjectUrl)
                    ? null
                    : model.ProjectUrl.Trim(),

                ShortDescription =
                    string.IsNullOrWhiteSpace(model.ShortDescription)
                        ? null
                        : model.ShortDescription.Trim(),

                Overview =
                    string.IsNullOrWhiteSpace(model.Overview)
                        ? null
                        : model.Overview.Trim(),

                Slug = slug,

                Status = model.Status == "published"
                    ? "published"
                    : "draft",

                NextProjectId = model.NextProject,

                CreatedAt = DateTime.UtcNow,

                UpdatedAt = null,

                PublishedAt =
                    model.Status == "published"
                        ? DateTime.UtcNow
                        : null
            };


            // ==========================================
            // ADD PROJECT TO DATABASE
            // ==========================================

            dbContext.Projects.Add(project);

            await dbContext.SaveChangesAsync();


            // At this point project.Id exists
            // ==========================================
            // PROJECT FOLDER
            // ==========================================

            string projectFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "projects",
                project.Id.ToString()
            );

            Directory.CreateDirectory(projectFolder);


            try
            {
                // ======================================
                // HERO IMAGE
                // ======================================

                string heroExtension =
                    Path.GetExtension(
                        model.HeroImage.FileName
                    ).ToLowerInvariant();

                string heroFileName =
                    $"{Guid.NewGuid():N}{heroExtension}";

                string heroPhysicalPath =
                    Path.Combine(
                        projectFolder,
                        heroFileName
                    );

                using (var stream = new FileStream(
                    heroPhysicalPath,
                    FileMode.Create))
                {
                    await model.HeroImage.CopyToAsync(stream);
                }

                project.HeroImage =
                    $"/uploads/projects/{project.Id}/{heroFileName}";


                // ======================================
                // PROJECT IMAGES
                // ======================================

                int imageOrder = 0;

                if (model.Images != null)
                {
                    foreach (var image in model.Images)
                    {
                        if (image == null || image.Length == 0)
                            continue;


                        // Validate image type

                        if (!allowedImageTypes.Contains(
                            image.ContentType.ToLowerInvariant()))
                        {
                            continue;
                        }


                        string extension =
                            Path.GetExtension(
                                image.FileName
                            ).ToLowerInvariant();

                        string fileName =
                            $"{Guid.NewGuid():N}{extension}";

                        string physicalPath =
                            Path.Combine(
                                projectFolder,
                                fileName
                            );


                        using (var stream = new FileStream(
                            physicalPath,
                            FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }


                        var media = new ProjectMedia
                        {
                            ProjectId = project.Id,

                            MediaType = "image",

                            FilePath =
                                $"/uploads/projects/{project.Id}/{fileName}",

                            OriginalFileName =
                                image.FileName,

                            ContentType =
                                image.ContentType,

                            FileSize =
                                image.Length,

                            DisplayOrder =
                                imageOrder++,

                            CreatedAt =
                                DateTime.UtcNow
                        };


                        dbContext.ProjectMedia.Add(media);
                    }
                }


                // ======================================
                // PROJECT VIDEOS
                // ======================================

                int videoOrder = 0;

                if (model.Videos != null)
                {
                    foreach (var video in model.Videos)
                    {
                        if (video == null || video.Length == 0)
                            continue;


                        // Validate video type

                        if (!allowedVideoTypes.Contains(
                            video.ContentType.ToLowerInvariant()))
                        {
                            continue;
                        }


                        string extension =
                            Path.GetExtension(
                                video.FileName
                            ).ToLowerInvariant();

                        string fileName =
                            $"{Guid.NewGuid():N}{extension}";

                        string physicalPath =
                            Path.Combine(
                                projectFolder,
                                fileName
                            );


                        using (var stream = new FileStream(
                            physicalPath,
                            FileMode.Create))
                        {
                            await video.CopyToAsync(stream);
                        }


                        var media = new ProjectMedia
                        {
                            ProjectId = project.Id,

                            MediaType = "video",

                            FilePath =
                                $"/uploads/projects/{project.Id}/{fileName}",

                            OriginalFileName =
                                video.FileName,

                            ContentType =
                                video.ContentType,

                            FileSize =
                                video.Length,

                            DisplayOrder =
                                videoOrder++,

                            CreatedAt =
                                DateTime.UtcNow
                        };


                        dbContext.ProjectMedia.Add(media);
                    }
                }


                // ======================================
                // TECHNOLOGIES
                // ======================================

                if (model.Technologies != null)
                {
                    int technologyOrder = 0;

                    foreach (var technology in model.Technologies)
                    {
                        if (string.IsNullOrWhiteSpace(technology))
                            continue;


                        string cleanTechnology =
                            technology.Trim();


                        // Prevent duplicate technologies

                        bool alreadyExists =
                            model.Technologies
                                .Take(technologyOrder)
                                .Any(x =>
                                    string.Equals(
                                        x.Trim(),
                                        cleanTechnology,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                );


                        if (alreadyExists)
                            continue;


                        var projectTechnology =
                            new ProjectTechnology
                            {
                                ProjectId = project.Id,

                                Name = cleanTechnology,

                                DisplayOrder =
                                    technologyOrder++
                            };


                        dbContext.ProjectTechnologies.Add(
                            projectTechnology
                        );
                    }
                }


                // ======================================
                // FEATURES
                // ======================================

                if (model.FeatureTitles != null)
                {
                    for (
                        int i = 0;
                        i < model.FeatureTitles.Count;
                        i++
                    )
                    {
                        string title =
                            model.FeatureTitles[i]?.Trim()
                            ?? string.Empty;


                        if (string.IsNullOrWhiteSpace(title))
                            continue;


                        string? description = null;


                        if (
                            model.FeatureDescriptions != null &&
                            i < model.FeatureDescriptions.Count
                        )
                        {
                            string cleanDescription =
                                model.FeatureDescriptions[i]
                                ?.Trim()
                                ?? string.Empty;


                            if (!string.IsNullOrWhiteSpace(
                                cleanDescription))
                            {
                                description =
                                    cleanDescription;
                            }
                        }


                        var feature =
                            new ProjectFeature
                            {
                                ProjectId = project.Id,

                                Title = title,

                                Description = description,

                                DisplayOrder = i
                            };


                        dbContext.ProjectFeatures.Add(feature);
                    }
                }


                // ======================================
                // SAVE ALL CHILD DATA
                // ======================================

                await dbContext.SaveChangesAsync();


                // ======================================
                // UPDATE PROJECT WITH HERO IMAGE
                // ======================================

                dbContext.Projects.Update(project);

                await dbContext.SaveChangesAsync();


                // ======================================
                // SUCCESS
                // ======================================

                TempData["ProjectSuccess"] =
                    project.Status == "published"
                        ? "Project published successfully!"
                        : "Project saved as draft successfully!";


                // ======================================
                // RETURN TO ADD PROJECT PAGE
                // ======================================
                TempData["ProjectSuccess"] =
     project.Status == "published"
         ? "Project published successfully!"
         : "Project saved as draft successfully!";
                return RedirectToAction("AddProjects");
            }
            catch (Exception)
            {
                // ======================================
                // DELETE UPLOADED FILES
                // ======================================

                if (Directory.Exists(projectFolder))
                {
                    try
                    {
                        Directory.Delete(
                            projectFolder,
                            true
                        );
                    }
                    catch
                    {
                        // Ignore cleanup failure
                    }
                }


                // ======================================
                // DELETE PROJECT FROM DATABASE
                // ======================================

                var savedProject =
                    await dbContext.Projects
                        .FirstOrDefaultAsync(
                            p => p.Id == project.Id
                        );

                if (savedProject != null)
                {
                    dbContext.Projects.Remove(
                        savedProject
                    );

                    await dbContext.SaveChangesAsync();
                }


                ModelState.AddModelError(
                    "",
                    "Something went wrong while saving the project."
                );


                return View("AddProjects", model);
            }
        }

        // ==============================
        // PROJECTS - PUBLIC
        // ==============================
        [Route("/Projects")]
        public async Task<IActionResult> Projects()
        {
            var projects = await dbContext.Projects
                .Where(p => p.Status == "published")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(projects);
        }

        // ==============================
        // PROJECT DETAILS - PUBLIC
        // ==============================
        [Route("/Projects/Details/{slug}")]
        public async Task<IActionResult> ProjectDetails(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            var project = await dbContext.Projects
                .Include(p => p.Media)
                .Include(p => p.Technologies)
                .Include(p => p.Features)
                .Include(p => p.NextProject)
                .FirstOrDefaultAsync(p =>
                    p.Slug == slug &&
                    p.Status == "published");

            if (project == null)
            {
                return NotFound();
            }

            // Organize media
            project.Media = project.Media
                .OrderBy(m => m.MediaType == "image" ? 0 : 1)
                .ThenBy(m => m.DisplayOrder)
                .ToList();

            // Organize technologies
            project.Technologies = project.Technologies
                .OrderBy(t => t.DisplayOrder)
                .ToList();

            // Organize features
            project.Features = project.Features
                .OrderBy(f => f.DisplayOrder)
                .ToList();

            // Only show next project if it is published
            if (project.NextProject != null &&
                project.NextProject.Status != "published")
            {
                project.NextProject = null;
            }

            return View("ProjectDetails", project);
        }


        // ==============================
        // ADMIN LOGIN PAGE
        // ==============================
        [Route("/AdminAccess")]
        public IActionResult AdminAccess()
        {
            return View();
        }

        // ==============================
        // ADMIN DASHBOARD - PROTECTED
        // ==============================
        [Route("/Dashboard")]
        public async Task<IActionResult> DashboardAsync()
        {
            // Check if admin is logged in
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }
            var unreadMeetings = await dbContext.Meetings
    .Where(x => !x.IsRead && !x.IsCancelled)
    .OrderByDescending(x => x.MeetingDate)
    .ThenBy(x => x.StartTime)
    .ToListAsync();

            return View(unreadMeetings);
        }
        [Route("/meetingstojoin")]
        [HttpGet]
        public async Task<IActionResult> MeetingstojoinAsync()
        {
            // Check if admin is logged in
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }
            var meetings = await dbContext.Meetings
      .Where(x => !x.IsCancelled)
      .OrderByDescending(x => x.MeetingDate)
      .ThenBy(x => x.StartTime)
      .ToListAsync();
            return View(meetings);
        }
        [Route("/clientsmesseges")]
        [HttpGet]
        public async Task<IActionResult> ClientMesseges()
        {
            // Check if admin is logged in
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }
            var msg = await dbContext.ContactMessages.ToListAsync();
            return View(msg);
        }

        [Route("/timeslots")]
        [HttpGet]
        public async Task<IActionResult> SetTimeSlots()
        {
            // Check admin login
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }
            var timeSlots = await dbContext.TimeSlots
               .OrderBy(x => x.StartTime)
               .ToListAsync();

            return View(timeSlots);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(
           TimeSpan startTime,
           TimeSpan endTime)
        {
            if (startTime >= endTime)
            {
                TempData["TimeSlotError"] =
                    "End time must be later than start time.";

                return RedirectToAction(nameof(SetTimeSlots));
            }

            var exists = await dbContext.TimeSlots
                .AnyAsync(x =>
                    x.StartTime == startTime &&
                    x.EndTime == endTime);

            if (exists)
            {
                TempData["TimeSlotError"] =
                    "This time slot already exists.";

                return RedirectToAction(nameof(SetTimeSlots));
            }

            var timeSlot = new TimeSlot
            {
                StartTime = startTime,
                EndTime = endTime,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            dbContext.TimeSlots.Add(timeSlot);

            await dbContext.SaveChangesAsync();

            TempData["TimeSlotSuccess"] =
                "Time slot added successfully.";

            return RedirectToAction(nameof(SetTimeSlots));
        }


        // ==============================
        // DELETE
        // ==============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var timeSlot = await dbContext.TimeSlots
                .FindAsync(id);

            if (timeSlot == null)
            {
                return NotFound();
            }

            dbContext.TimeSlots.Remove(timeSlot);

            await dbContext.SaveChangesAsync();

            TempData["TimeSlotSuccess"] =
                "Time slot deleted successfully.";

            return RedirectToAction(nameof(SetTimeSlots));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var timeSlot = await dbContext.TimeSlots
                .FindAsync(id);

            if (timeSlot == null)
            {
                return NotFound();
            }

            timeSlot.IsActive = !timeSlot.IsActive;

            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(SetTimeSlots));
        }

        // ==============================
        // SET HOLIDAYS
        // ==============================
        [Route("/setholydays")]
        [HttpGet]
        public async Task<IActionResult> Setholydays()
        {
            // Check admin login
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }

            var existingSettings = await dbContext.HolidaySettings
                .ToListAsync();

            var model = Enum.GetValues<Weekday>()
                .Select(day => new HolidayDayViewModel
                {
                    Day = day,
                    IsHoliday = existingSettings
                        .Any(x => x.Day == day && x.IsHoliday)
                })
                .ToList();

            return View(model);
        }
        // ==============================
        // COMPANY CHAT
        // ==============================

        [Route("/CompanyChat")]
        [HttpGet]
        public IActionResult CompanyChat()
        {
            if (HttpContext.Session.GetString("CompanyChatLoggedIn") != "true")
            {
                return View("CompanyChatLogin");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CompanyChatLogin(
    Techninxa.Models.CompanyChatLoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var companyUsername =
                Configuration["CompanyChat:Username"];

            var companyPassword =
                Configuration["CompanyChat:Password"];

            // Validate shared company credentials
            if (model.Username == companyUsername &&
                model.Password == companyPassword)
            {
                // Make sure employee name is not empty
                if (string.IsNullOrWhiteSpace(model.DisplayName))
                {
                    ModelState.AddModelError(
                        "DisplayName",
                        "Please enter your name.");

                    return View(model);
                }

                // User successfully entered the company chat
                HttpContext.Session.SetString(
                    "CompanyChatLoggedIn",
                    "true");

                // IMPORTANT:
                // Store the employee's individual name,
                // NOT the shared company username.
                HttpContext.Session.SetString(
                    "CompanyChatUsername",
                    model.DisplayName.Trim());

                return RedirectToAction("CompanyChat");
            }

            ModelState.AddModelError(
                "",
                "Invalid company username or password.");

            return View(model);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/CompanyChat/Logout")]
        public IActionResult CompanyChatLogout()
        {
            HttpContext.Session.Remove("CompanyChatLoggedIn");
            HttpContext.Session.Remove("CompanyChatUsername");

            return RedirectToAction("CompanyChat");
        }

        [HttpGet]
        [Route("/CompanyChat/Messages")]
        public async Task<IActionResult> CompanyChatMessages()
        {
            if (HttpContext.Session.GetString("CompanyChatLoggedIn") != "true")
            {
                return Unauthorized();
            }

            var messages = await dbContext.ChatMessages
                .OrderBy(x => x.SentAt)
                .Select(x => new
                {
                    id = x.Id,
                    username = x.Username,
                    message = x.Message,
                    sentAt = x.SentAt.ToString("hh:mm tt")
                })
                .ToListAsync();

            return Json(messages);
        }

        // ==============================
        // SAVE HOLIDAYS
        // ==============================
        [HttpPost]
        [Route("/setholydays")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setholydays(
            List<HolidayDayViewModel> model)
        {
            // Check admin login
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminAccess");
            }

            if (model == null || model.Count == 0)
            {
                return RedirectToAction("Setholydays");
            }

            // Get existing settings
            var existingSettings = await dbContext.HolidaySettings
                .ToListAsync();

            // Update existing records
            foreach (var item in model)
            {
                var setting = existingSettings
                    .FirstOrDefault(x => x.Day == item.Day);

                if (setting != null)
                {
                    setting.IsHoliday = item.IsHoliday;
                }
                else
                {
                    dbContext.HolidaySettings.Add(new HolidaySetting
                    {
                        Day = item.Day,
                        IsHoliday = item.IsHoliday
                    });
                }
            }

            await dbContext.SaveChangesAsync();

            TempData["HolidaySuccess"] =
                "Holiday schedule saved successfully!";

            return RedirectToAction("Setholydays");
        }
        // ==============================
        // LOGIN
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AdminAccess", model);
            }

            var user = await dbContext.User
                .FirstOrDefaultAsync(x => x.Username == model.Username);

            // Invalid login
            if (user == null || user.Password != model.Password)
            {
                TempData["LoginError"] = "Invalid Username or Password!";

                return RedirectToAction("AdminAccess");
            }

            // ==============================
            // LOGIN SUCCESS
            // ==============================
            HttpContext.Session.SetString("AdminLoggedIn", "true");
            HttpContext.Session.SetString("AdminUsername", user.Username);

            // Send user to protected page
            return RedirectToAction("Dashboard");
        }

        
        // ==============================
        // LOGOUT
        // ==============================
        [Route("/AdminLogout")]
        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Dashboard");
        }


        // ==============================
        // CONTACT MESSAGE
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitMessege(ContactMessage model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            model.CreatedAt = DateTime.UtcNow;

            dbContext.ContactMessages.Add(model);

            await dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your message has been sent successfully!";

            return RedirectToAction("Index");
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Guid.NewGuid().ToString("N");

            text = text.ToLowerInvariant();

            text = Regex.Replace(
                text,
                @"[^a-z0-9\s-]",
                ""
            );

            text = Regex.Replace(
                text,
                @"\s+",
                "-"
            );

            text = Regex.Replace(
                text,
                @"-+",
                "-"
            );

            return text.Trim('-');
        }
        // =========================
        // TEAM MEMBERS
        // =========================

        [HttpGet]
        [Route("/Team")]
        public async Task<IActionResult> Team()
        {

            var members = await dbContext.TeamMembers
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id)
                .ToListAsync();

            return View(members);
        }


        // =========================
        // CREATE TEAM MEMBER
        // =========================

        [HttpGet]
        [Route("/Team/Create")]
        public IActionResult CreateTeam()
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
                return RedirectToAction("AdminAccess");

            return View();
        }


        [HttpPost]
        [Route("/Team/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeam(
            TeamMember model,
            IFormFile? ImageFile)
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
                return RedirectToAction("AdminAccess");

            if (!ModelState.IsValid)
                return View(model);

            // Image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string[] allowedTypes =
                {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

                if (!allowedTypes.Contains(
                    ImageFile.ContentType.ToLowerInvariant()))
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Only JPG, PNG and WEBP images are allowed.");

                    return View(model);
                }

                const long maxSize = 5 * 1024 * 1024;

                if (ImageFile.Length > maxSize)
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Image cannot be larger than 5 MB.");

                    return View(model);
                }

                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "team");

                Directory.CreateDirectory(folder);

                string extension =
                    Path.GetExtension(ImageFile.FileName)
                        .ToLowerInvariant();

                string fileName =
                    $"{Guid.NewGuid():N}{extension}";

                string filePath =
                    Path.Combine(folder, fileName);

                using (var stream =
                       new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                model.Image =
                    $"/uploads/team/{fileName}";
            }

            model.Name = model.Name.Trim();
            model.Department = model.Department.Trim();
            model.Role = model.Role.Trim();

            if (!string.IsNullOrWhiteSpace(model.AltText))
                model.AltText = model.AltText.Trim();

            model.CreatedAt = DateTime.UtcNow;

            dbContext.TeamMembers.Add(model);

            await dbContext.SaveChangesAsync();

            TempData["TeamSuccess"] =
                "Team member added successfully.";

            return RedirectToAction(nameof(Team));
        }


        // =========================
        // EDIT TEAM MEMBER
        // =========================

        [HttpGet]
        [Route("/Team/Edit/{id:int}")]
        public async Task<IActionResult> EditTeam(int id)
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
                return RedirectToAction("AdminAccess");

            var member =
                await dbContext.TeamMembers.FindAsync(id);

            if (member == null)
                return NotFound();

            return View(member);
        }


        [HttpPost]
        [Route("/Team/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeam(
            int id,
            TeamMember model,
            IFormFile? ImageFile)
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
                return RedirectToAction("AdminAccess");

            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var member =
                await dbContext.TeamMembers.FindAsync(id);

            if (member == null)
                return NotFound();


            // If new image uploaded
            if (ImageFile != null && ImageFile.Length > 0)
            {
                string[] allowedTypes =
                {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

                if (!allowedTypes.Contains(
                    ImageFile.ContentType.ToLowerInvariant()))
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Only JPG, PNG and WEBP images are allowed.");

                    return View(model);
                }

                const long maxSize = 5 * 1024 * 1024;

                if (ImageFile.Length > maxSize)
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Image cannot be larger than 5 MB.");

                    return View(model);
                }


                // Delete old image
                if (!string.IsNullOrWhiteSpace(member.Image))
                {
                    string oldFile =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            member.Image.TrimStart('/'));

                    if (System.IO.File.Exists(oldFile))
                    {
                        try
                        {
                            System.IO.File.Delete(oldFile);
                        }
                        catch
                        {
                            // Ignore delete errors
                        }
                    }
                }


                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "team");

                Directory.CreateDirectory(folder);

                string extension =
                    Path.GetExtension(ImageFile.FileName)
                        .ToLowerInvariant();

                string fileName =
                    $"{Guid.NewGuid():N}{extension}";

                string filePath =
                    Path.Combine(folder, fileName);

                using (var stream =
                       new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                member.Image =
                    $"/uploads/team/{fileName}";
            }


            // Update fields
            member.Name = model.Name.Trim();

            member.Department =
                model.Department.Trim();

            member.Role =
                model.Role.Trim();

            member.AltText =
                string.IsNullOrWhiteSpace(model.AltText)
                    ? null
                    : model.AltText.Trim();

            member.DisplayOrder =
                model.DisplayOrder;

            member.IsActive =
                model.IsActive;


            await dbContext.SaveChangesAsync();

            TempData["TeamSuccess"] =
                "Team member updated successfully.";

            return RedirectToAction(nameof(Team));
        }


        // =========================
        // DELETE TEAM MEMBER
        // =========================

        [HttpPost]
        [Route("/Team/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
                return RedirectToAction("AdminAccess");

            var member =
                await dbContext.TeamMembers.FindAsync(id);

            if (member == null)
                return NotFound();


            // Delete image
            if (!string.IsNullOrWhiteSpace(member.Image))
            {
                string filePath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        member.Image.TrimStart('/'));

                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        System.IO.File.Delete(filePath);
                    }
                    catch
                    {
                        // Ignore delete errors
                    }
                }
            }


            dbContext.TeamMembers.Remove(member);

            await dbContext.SaveChangesAsync();

            TempData["TeamSuccess"] =
                "Team member deleted successfully.";

            return RedirectToAction(nameof(Team));
        }


        // =========================
        // ACTIVE / INACTIVE
        // =========================

        [HttpPost]
        [Route("/Team/Toggle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTeam(int id)
        {
            if (HttpContext.Session.GetString("AdminLoggedIn") != "true")
                return RedirectToAction("AdminAccess");

            var member =
                await dbContext.TeamMembers.FindAsync(id);

            if (member == null)
                return NotFound();

            member.IsActive = !member.IsActive;

            await dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Team));
        }
    }
}