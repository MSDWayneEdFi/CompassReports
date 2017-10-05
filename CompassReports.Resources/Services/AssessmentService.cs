﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompassReports.Data;
using CompassReports.Data.Entities;
using CompassReports.Resources.Models;

namespace CompassReports.Resources.Services
{
    public interface IAssessmentService
    {
        PercentageTotalBarChartModel ByGoodCauseExcemption(AssessmentFilterModel model);
        PieChartModel<int> ByPerformanceLevel(AssessmentFilterModel model);
        PercentageTotalBarChartModel PerformanceLevelByEnglishLanguageLearner(AssessmentFilterModel model);
        PercentageTotalBarChartModel PerformanceLevelByEthnicity(AssessmentFilterModel model);
        PercentageTotalBarChartModel PerformanceLevelByLunchStatus(AssessmentFilterModel model);
        PercentageTotalBarChartModel PerformanceLevelBySpecialEducation(AssessmentFilterModel model);
    }

    public class AssessmentService : IAssessmentService
    {
        private readonly IRepository<AssessmentFact> _assessmentRepository;
        private readonly IRepository<GoodCauseExemptionJunkDimension> _goodCauseExemptionRepository;

        public AssessmentService(IRepository<AssessmentFact> assessmentRepository,
            IRepository<GoodCauseExemptionJunkDimension> goodCauseExemptionRepository)
        {
            _assessmentRepository = assessmentRepository;
            _goodCauseExemptionRepository = goodCauseExemptionRepository;
        }

        public PieChartModel<int> ByPerformanceLevel(AssessmentFilterModel model)
        {
            var query = BaseQuery(model);

            var results = query.GroupBy(x => x.PerformanceLevel.PerformanceLevel)
                .Select(x => new
                {
                    PerformanceLevel = x.Key,
                    Total = x.Sum(y => y.AssessmentStudentCount)
                }).OrderBy(x => x.PerformanceLevel)
                .ToList();

            return new PieChartModel<int>
            {
                Title = "Performance Level",
                Headers = new List<string> { "", "Performance Level", "Performance Count" },
                Labels = results.Select(x => x.PerformanceLevel).ToList(),
                Data = results.Select(x => x.Total).ToList(),
                ShowChart = true,
                Total = results.Sum(x => x.Total)
            };
        }

        public PercentageTotalBarChartModel PerformanceLevelByEnglishLanguageLearner(AssessmentFilterModel model)
        {
            return PerformanceLevelBy("Performance Level by English Language Learners", "Language Statuses", model, "EnglishLanguageLearnerStatus");
        }

        public PercentageTotalBarChartModel PerformanceLevelByEthnicity(AssessmentFilterModel model)
        {
            return PerformanceLevelBy("Performance Level by Ethnicity", "Ethnicities", model, "Ethnicity");
        }

        public PercentageTotalBarChartModel PerformanceLevelByLunchStatus(AssessmentFilterModel model)
        {
            return PerformanceLevelBy("Performance Level by Free/Reduced Price Meals", "Lunch Statuses", model, "FreeReducedLunchStatus");
        }

        public PercentageTotalBarChartModel PerformanceLevelBySpecialEducation(AssessmentFilterModel model)
        {
            return PerformanceLevelBy("Performance Level by Special Education", "Education Statuses", model, "SpecialEducationStatus");
        }

        public PercentageTotalBarChartModel ByGoodCauseExcemption(AssessmentFilterModel model)
        {
            var baseQuery = BaseQuery(model);

            if (baseQuery.All(x => x.GoodCauseExemptionKey == 3)) return null;

            var results = (
                from cause in
                _goodCauseExemptionRepository.GetAll().Where(x => (new[] {1, 2}).Contains(x.GoodCauseExemptionKey))
                join fact in baseQuery on cause.GoodCauseExemptionKey equals fact.GoodCauseExemptionKey into facts
                from fact in facts.DefaultIfEmpty()
                select new
                {
                    GoodCauseExemptionKey = cause.GoodCauseExemptionKey,
                    GoodCauseExemption = cause.GoodCauseExemption,
                    AssessmentStudentCount = fact == null ? 0 : fact.AssessmentStudentCount
                }
            ).GroupBy(x => new
            {
                GoodCauseExemptionKey = x.GoodCauseExemptionKey,
                GoodCauseExemption = x.GoodCauseExemption
            }).Select(x => new
            {
                GoodCauseExemption = x.Key.GoodCauseExemption,
                GoodCauseExemptionKey = x.Key.GoodCauseExemptionKey,
                Total = x.Sum(y => y.AssessmentStudentCount)
            }).ToList();

            var goodCause = results.Where(x => x.GoodCauseExemptionKey == 1).ToList();

            return new PercentageTotalBarChartModel
            {
                Title = "Good Cause Exemptions",
                Headers = new List<string> { "", "Good Cause Exemptions", "Total"},
                Labels = goodCause.Select(x => x.GoodCauseExemption).ToList(),
                Series = new List<string> { goodCause.First().GoodCauseExemption },
                Data = new List<List<PercentageTotalDataModel>> {
                    goodCause.Select( x => new PercentageTotalDataModel
                    {
                        Percentage = GetPercentage(x.Total, results.Sum(y => y.Total)),
                        Total = x.Total
                    }).ToList()
                },
                ShowChart = true,
                ShowPercentage = true,
                HideTotal = true
            };
        }

        private IQueryable<AssessmentFact> BaseQuery(AssessmentFilterModel model)
        {
            var query = _assessmentRepository
                .GetAll()
                .Where(x => x.SchoolYearKey == model.SchoolYear);

            if (model.Assessments != null && model.Assessments.Any())
                query = query.Where(x => model.Assessments.Contains(x.AssessmentKey));
            else
                query = query.Where(x => x.Assessment.AssessmentTitle == model.AssessmentTitle && x.Assessment.AcademicSubject == model.Subject);

            if (model.EnglishLanguageLearnerStatuses != null && model.EnglishLanguageLearnerStatuses.Any())
                query = query.Where(x => model.EnglishLanguageLearnerStatuses.Contains(x.Demographic.EnglishLanguageLearnerStatus));

            if (model.Ethnicities != null && model.Ethnicities.Any())
                query = query.Where(x => model.Ethnicities.Contains(x.Demographic.Ethnicity));

            if (model.GoodCauseExcemptions != null && model.GoodCauseExcemptions.Any())
                query = query.Where(x => model.GoodCauseExcemptions.Contains(x.GoodCauseExemptionKey));

            if (model.LunchStatuses != null && model.LunchStatuses.Any())
                query = query.Where(x => model.LunchStatuses.Contains(x.Demographic.FreeReducedLunchStatus));

            if (model.PerformanceLevels != null && model.PerformanceLevels.Any())
                query = query.Where(x => model.PerformanceLevels.Contains(x.PerformanceLevelKey));

            if (model.SpecialEducationStatuses != null && model.SpecialEducationStatuses.Any())
                query = query.Where(x => model.SpecialEducationStatuses.Contains(x.Demographic.SpecialEducationStatus));

            return query;
        }

        private static double GetPercentage(int subTotal, int total)
        {
            return Math.Round(100 * ((double) subTotal / (double) total), 2);
        }

        private static object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        private PercentageTotalBarChartModel PerformanceLevelBy(string title, string header, AssessmentFilterModel model, string by)
        {
            var query = BaseQuery(model).ToList();

            var results = query.GroupBy(x => new { x.PerformanceLevel.PerformanceLevel, Property = GetPropertyValue(x.Demographic, by) })
                .Select(x => new
                {
                    PerformanceLevel = x.Key.PerformanceLevel,
                    Property = x.Key.Property,
                    Total = x.Sum(y => y.AssessmentStudentCount)
                }).ToList();

            var performanceLevels = results.Select(x => x.PerformanceLevel).Distinct().OrderBy(x => x).ToList();

            var headers = new List<string> { "", header };
            headers.AddRange(performanceLevels);

            var propertyValues = results.Select(x => x.Property).Distinct().OrderBy(x => x).ToList();

            var data = new List<List<PercentageTotalDataModel>>();
            foreach (var value in propertyValues)
            {
                var values = new List<PercentageTotalDataModel>();
                var propertyTotal = results.Where(x => x.Property == value).Sum(x => x.Total);

                foreach (var performanceLevel in performanceLevels)
                {
                    var row = results.FirstOrDefault(x => x.Property == value && x.PerformanceLevel == performanceLevel);
                    var rowTotal = row == null ? 0 : row.Total;
                    values.Add(new PercentageTotalDataModel
                    {
                        Percentage = rowTotal == 0 ? 0 : GetPercentage(rowTotal, propertyTotal),
                        Total = rowTotal
                    });
                }
                data.Add(values);
            }

            var total = results.Sum(x => x.Total);
            var totals = results.GroupBy(x => x.PerformanceLevel)
                .OrderBy(x => x.Key)
                .Select(x => new PercentageTotalDataModel
                {
                    Percentage = GetPercentage(x.Sum(y => y.Total), total),
                    Total = x.Sum(y => y.Total)
                }).ToList();

            return new PercentageTotalBarChartModel
            {
                Title = title,
                Headers = headers,
                Labels = performanceLevels,
                Series = propertyValues.Select(x => x.ToString()).ToList(),
                Data = data,
                ShowChart = true,
                ShowPercentage = true,
                TotalRowTitle = "Performance Level",
                Totals = totals
            };
        }
    }
}