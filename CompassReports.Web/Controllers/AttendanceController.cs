﻿using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CompassReports.Resources.Models;
using CompassReports.Resources.Services;

namespace CompassReports.Web.Controllers
{
    /// <summary>
    /// The EnrollmentFilters resource endpoint.
    /// </summary>
    [RoutePrefix("api/attendance")]
    public class AttendanceController : ApiController
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [Route("by-english-language-learner")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByEnglishLanguageLearnerStatus(EnrollmentFilterModel model)
        {
            var chart = await _attendanceService.ByEnglishLanguageLearnerStatus(model);
            return Ok(chart);
        }

        [Route("by-ethnicity")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByEthnicity(EnrollmentFilterModel model)
        {
            var chart = await _attendanceService.ByEthnicity(model);
            return Ok(chart);
        }

        [Route("by-grade")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByGrade(EnrollmentFilterModel model)
        {
            var chart = await _attendanceService.ByGrade(model);
            return Ok(chart);
        }

        [Route("by-lunch-status")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByLunchStatus(EnrollmentFilterModel model)
        {
            var chart = await _attendanceService.ByLunchStatus(model);
            return Ok(chart);
        }

        [Route("by-special-education")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> BySpecialEducationStatus(EnrollmentFilterModel model)
        {
            var chart = await _attendanceService.BySpecialEducationStatus(model);
            return Ok(chart);
        }
    }
}
