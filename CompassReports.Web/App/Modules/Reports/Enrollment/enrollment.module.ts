﻿module App.Reports.Enrollment {

    class EnrollmentController {
        static $inject = ['$rootScope', 'api', 'services', '$mdSidenav', 'englishLanguageLearnerStatuses', 'ethnicities',
            'grades', 'lunchStatuses', 'specialEducationStatuses', 'schoolYears'];

        displaySchoolYears: any = {};
        charts = [
            new PieChartModel<number>('byGrade'),
            new PieChartModel<number>('byEthnicity'),
            new PieChartModel<number>('byLunchStatus'),
            new PieChartModel<number>('bySpecialEducation'),
            new PieChartModel<number>('byEnglishLanguageLearner')
        ];

        filters = new Models.EnrollmentFilterModel(this.schoolYears[0].Value);

        toggleFilters = () => this.$mdSidenav('filternav').toggle();

        reset = () => {
            this.filters = new Models.EnrollmentFilterModel(this.schoolYears[0].Value);
        }

        resetColors = () => {
            angular.forEach(this.charts, chart => {
                if (chart.Chart) {
                    chart.Options.animation = { duration: 1500 },
                    chart.Colors = this.services.colorGradient.getColors(chart.Chart.Data.length);
                }
            });
        }

        apply = () => {

            angular.forEach(this.charts, chart => {

                return this.api.enrollment[chart.ChartCall](this.filters)
                    .then((result: Models.EnrollmentChartModel<number>) => {
                        //Sets the current card state to default on the first call
                        if (!chart.Chart) {
                            chart.ShowChart = result.ShowChart;
                            chart.Chart = result;
                            chart.Options = {
                                responsive: true,
                                legend: { display: true, position: 'left' }
                            };
                        } else {
                            chart.Options.animation = {duration: 1000},
                            chart.Chart.Labels = result.Labels;
                            chart.Chart.Data = result.Data;
                            chart.Chart.Total = result.Total;
                        }

                        chart.Colors = this.services.colorGradient.getColors(result.Data.length);

                        // Workout around redrawing causes messup animation
                         //this.services.timeout(() => chart.Options.animation = false, 1500);
                    });
            });

        }

        constructor(
            public $rootScope,
            private readonly api: IApi,
            private readonly services: IServices,
            private readonly $mdSidenav: ng.material.ISidenavService,
            public englishLanguageLearnerStatuses: string[],
            public ethnicities: string[],
            public grades: string[],
            public lunchStatuses: string[],
            public specialEducationStatuses: string[],
            public schoolYears: Models.FilterModel<number>[]
        ) {

            this.services.timeout(() => {
                $rootScope.$on('theme-change', () => {
                    this.resetColors();
                });
            }, 1000);

            angular.forEach(schoolYears, year => {
                this.displaySchoolYears[year.Value] = year.Display;
            });

            this.apply();
        }
    }

    class EnrollmentConfig {
        static $inject = ['$stateProvider', 'settings'];

        constructor($stateProvider: ng.ui.IStateProvider, settings: ISystemSettings) {

            $stateProvider.state('app.reports.enrollment', {
                url: '/enrollment',
                views: {
                    'report@app.reports': {
                        templateUrl: `${settings.moduleBaseUri}/reports/enrollment/enrollment.view.html`,
                        controller: EnrollmentController,
                        controllerAs: 'ctrl',
                        resolve: {
                            englishLanguageLearnerStatuses: ['api', (api: IApi) => {
                                return api.enrollmentFilters.getEnglishLanguageLearnerStatuses();
                            }],
                            ethnicities: ['api', (api: IApi) => {
                                return api.enrollmentFilters.getEthnicities();
                            }],
                            grades: ['api', (api: IApi) => {
                                return api.enrollmentFilters.getGrades();
                            }],
                            lunchStatuses: ['api', (api: IApi) => {
                                return api.enrollmentFilters.getLunchStatuses();
                            }],
                            specialEducationStatuses: ['api', (api: IApi) => {
                                return api.enrollmentFilters.getSpecialEducationStatuses();
                            }],
                            schoolYears: ['api', (api: IApi) => {
                                return api.enrollmentFilters.getSchoolYears();
                            }]
                        }
                    }
                }
            });
        }
    }

    angular
        .module('app.reports.enrollment', [])
        .config(EnrollmentConfig);
}