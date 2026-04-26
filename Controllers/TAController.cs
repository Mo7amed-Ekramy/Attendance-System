using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC_PROJECT.Extensions;
using MVC_PROJECT.Services.Interfaces;
using System.Threading.Tasks;

namespace MVC_PROJECT.Controllers
{
    [Authorize(Roles = "TA")]
    public class TAController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ISectionService _sectionService;

        public TAController(
            IDashboardService dashboardService,
            ISectionService sectionService)
        {
            _dashboardService = dashboardService;
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int taId = GetCurrentTAId();

            if (taId <= 0)
                return Unauthorized("Unable to identify TA.");

            var viewModel = await _dashboardService.GetTADashboardAsync(taId);

            if (viewModel == null)
                return NotFound("TA dashboard data was not found.");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Sections()
        {
            int taId = GetCurrentTAId();

            if (taId <= 0)
                return Unauthorized("Unable to identify TA.");

            var sections = await _sectionService.GetTASectionsAsync(taId);

            return View(sections);
        }

        [HttpGet]
        public async Task<IActionResult> SectionDetails(int sectionId)
        {
            if (sectionId <= 0)
                return BadRequest("Invalid section ID.");

            int taId = GetCurrentTAId();

            if (taId <= 0)
                return Unauthorized("Unable to identify TA.");

            bool isAssigned = await _sectionService.IsTAAssignedToSectionAsync(taId, sectionId);

            if (!isAssigned)
                return Forbid();

            var viewModel = await _sectionService.GetSectionDetailsAsync(sectionId);

            if (viewModel == null || viewModel.CourseSectionId == 0)
                return NotFound("Section details were not found.");

            return View(viewModel);
        }

        private int GetCurrentTAId()
        {
            return User.GetUserId();
        }
    }
}