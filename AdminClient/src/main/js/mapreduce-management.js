var mapReduceModule = angular.module('mapreduce-management', [], function($routeProvider, $locationProvider, $provide) {

    $routeProvider.when('/jobs/:jobId', {
        templateUrl: 'job.html',
        controller: JobCtrl
    });

    $routeProvider.when('/data', {
        templateUrl: 'data.html',
        controller: DataCtrl
    });

    $routeProvider.when('/account', {
        templateUrl: 'account.html',
        controller: MyAccountCtrl
    });

    $routeProvider.when('/jobs', {
        templateUrl: 'all-jobs.html',
        controller: JobsListCtrl
    });

});

mapReduceModule.factory('$dataService', function() {
    return new function() {

        this.Status =

        this.dataSets = [
            { "id": 0, "name": "Word count input", "url": "http://s3.amazon.com/hugebdu/mapreducejs/wordcount/", "dateCreated": new Date(2012, 7, 1), "status": "ready" },
            { "id": 1, "name": "Inversed index input", "url": "http://s3.amazon.com/hugebdu/mapreducejs/inversed_index/", "dateCreated": new Date(2012, 7, 2), "status": "ready" },
            { "id": 2, "name": "Some really hard input", "url": "http://s3.amazon.com/hugebdu/mapreducejs/some_other_input/", "dateCreated": new Date(2012, 7, 3), "status": "in_process" }
        ];
    };
});

mapReduceModule.factory('$jobsService', function() {
    return new function() {

        this.jobs = [
            { "id": "0001", "dataSetId" : 0, "name": "Word count", "description": "job 1 description", "javaScript": "function CountWords() {\r\n\tmap: function(record, collector) {\r\n\t\t\/\/do some map\r\n\t};\r\n\t\r\n\treduce: function(record, collector) {\r\n\t\t\/\/do some reduce\r\n\t};\r\n}" },
            { "id": "0002", "dataSetId" : 1, "name": "Inversed index", "description": "job 2 description", "javaScript": "function CountWords() {\r\n\tmap: function(record, collector) {\r\n\t\t\/\/do some map\r\n\t};\r\n\t\r\n\treduce: function(record, collector) {\r\n\t\t\/\/do some reduce\r\n\t};\r\n}" },
        ];

        this.getById = function(id) {
            var job = _.find(this.jobs, function(j) { return j.id == id });
            if (job)
                job = _.clone(job);
            return job;
        };

        this.save = function(job) {
            if (!this.getById(job.id))
                this.jobs.push(job);
            else
                this.jobs = _.map(this.jobs, function(elem) { return elem.id == job.id ? job : elem });
        }
    };
});

/** controllers ****************************************************/

function MyAccountCtrl($scope) {

}

function DataCtrl($scope, $dataService) {
    $scope.dataSets = $dataService.dataSets
    $scope.statuses = {
        "ready": {"label": "Ready", "icon": "icon-ok"},
        "in_process": {"label": "Pre-processing", "icon": "icon-fire"}
    };
}

function JobsListCtrl($scope, $jobsService) {
    $scope.jobs = $jobsService.jobs;
}

function JobCtrl($scope, $routeParams, $jobsService, $location, $dataService) {

    $scope.job = $jobsService.getById($routeParams["jobId"]);
    $scope.dataSets = $dataService.dataSets;

    window.setTimeout(function() {
        if (!$scope.javaScriptEditor) {
            $scope.javaScriptEditor = window.CodeMirror.fromTextArea(document.getElementById("javaScript"), {
                lineNumbers: true,
                matchBrackets: true,
                indentWithTabs: true,
                indentUnit: 4
            });
        }
    }, 10);

    $scope.save = function() {
        $scope.javaScriptEditor.save();
        $scope.job.javaScript = $("#javaScript").val();
        $jobsService.save($scope.job);
        window.location = "#/jobs";
    };
}
