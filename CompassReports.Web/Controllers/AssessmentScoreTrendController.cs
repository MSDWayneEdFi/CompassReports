﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using CompassReports.Resources.Models;
using CompassReports.Resources.Services;

namespace CompassReports.Web.Controllers
{
    /// <summary>
    /// The EnrollmentFilters resource endpoint.
    /// </summary>
    [RoutePrefix("api/assessment-score-trend")]
    public class AssessmentScoreTrendController : ApiController
    {
        private readonly IAssessmentScoreTrendService _assessmentScoreTrendService;

        public AssessmentScoreTrendController(IAssessmentScoreTrendService assessmentScoreTrendService)
        {
            _assessmentScoreTrendService = assessmentScoreTrendService;
        }

        [Route("")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> Get(AssessmentTrendFilterModel model)
        {
            var chart = await _assessmentScoreTrendService.Get(model);
            return Ok(chart);
        }

        [Route("by-language-learner")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByEnglishLanguageLearner(AssessmentTrendFilterModel model)
        {
            var chart = await _assessmentScoreTrendService.ByEnglishLanguageLearner(model);
            return Ok(chart);
        }

        [Route("by-ethnicity")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByEthnicity(AssessmentTrendFilterModel model)
        {
            var chart = await _assessmentScoreTrendService.ByEthnicity(model);
            return Ok(chart);
        }

        [Route("by-lunch-status")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> ByLunchStatus(AssessmentTrendFilterModel model)
        {
            var chart = await _assessmentScoreTrendService.ByLunchStatus(model);
            return Ok(chart);
        }

        [Route("by-special-education")]
        [AcceptVerbs("POST")]
        public async Task<IHttpActionResult> BySpecialEducation(AssessmentTrendFilterModel model)
        {
            var chart = await _assessmentScoreTrendService.BySpecialEducation(model);
            return Ok(chart);
        }
    }
}
