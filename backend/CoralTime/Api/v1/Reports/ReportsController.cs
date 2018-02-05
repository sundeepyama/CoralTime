using CoralTime.BL.Interfaces.Reports;
using CoralTime.Common.Constants;
using CoralTime.Common.Middlewares;
using CoralTime.Services;
using CoralTime.ViewModels.Reports.Request.Grid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace CoralTime.Api.v1.Reports
{
    [Authorize]
    [Route("api/v1/[controller]")]
    public class ReportsController : BaseController<ReportsController, IReportsService>
    {
        private IReportsSettingsService _reportsSettingsService;

        public ReportsController(IReportsService service, ILogger<ReportsController> logger, IReportsSettingsService reportsSettingsService)
            : base(logger, service)
        {
            _reportsSettingsService = reportsSettingsService;
        }

        [HttpGet]
        public IActionResult GetDropdowns()
        {
            try
            {
                return new JsonResult(_service.ReportsDropDowns());
            }
            catch (Exception e)
            {
                _logger.LogError($"GetReportsDropdowns method {e}");
                var errors = ExceptionsChecker.CheckProjectsException(e);
                return BadRequest(errors);
            }
        }

        [HttpPost]
        public IActionResult GetGridAndSaveCurrentQuery([FromBody] ReportsGridView reportsGridData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Model");
            }

            try
            {
                _reportsSettingsService.SaveCurrentQuery(reportsGridData.ValuesSaved);

                // 0 - Default(none), 1 - Projects, 2 - Users, 3 - Dates, 4 - Clients.
                switch (reportsGridData.ValuesSaved.GroupById)
                {
                    case (int) Constants.ReportsGroupBy.Project:
                    {
                        return new JsonResult(_service.ReportsGridGroupByProjects(reportsGridData));
                    }

                    case (int) Constants.ReportsGroupBy.User:
                    {
                        return new JsonResult(_service.ReportsGridGroupByUsers(reportsGridData));
                    }

                    case (int) Constants.ReportsGroupBy.Date:
                    {
                        return new JsonResult(_service.ReportsGridGroupByDates(reportsGridData));
                    }

                    case (int) Constants.ReportsGroupBy.Client:
                    {
                        return new JsonResult(_service.ReportsGridGroupByClients(reportsGridData));
                    }

                    default:
                    {
                        return BadRequest("Invalid Grouping value");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning($"PostReportsGrid method with parameters ({reportsGridData});\n {e}");
                var errors = ExceptionsChecker.CheckProjectsException(e);
                return BadRequest(errors);
            }
        }
    }
}
