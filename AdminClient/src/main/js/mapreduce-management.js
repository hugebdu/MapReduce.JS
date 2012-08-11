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
            { "id": 0, "name": "Word count input", "url": "wordcount", "dateCreated": new Date(2012, 7, 1), "status": "ready" },
            { "id": 1, "name": "Inversed index input", "url": "inversedindex", "dateCreated": new Date(2012, 7, 2), "status": "ready" },
            { "id": 2, "name": "Some really hard input", "url": "some_other_input", "dateCreated": new Date(2012, 7, 3), "status": "in_process" }
        ];
    };
});

mapReduceModule.factory('$jobsService', function() {
    return new function() {

        this.jobs = [
            { "id": "0001", "dataSetId" : 0, "name": "Word count", "description": "MapReduce word count demo", "javaScript": "{\n\tmap: function(word) {\n\t\treturn {\"key\": word, \"value\": 1}\n\t},\n\treduce: function(result) {\n\t\treturn {\n\t\t\t\"key\": result.key,\n\t\t\t\"value\": _.reduce(result.value, function(memo, num) { return memo + num }, 0)\n\t\t};\n\t}\n}" },
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

function JobsListCtrl($scope, $http, $jobsService, $dataService) {
    $scope.jobs = $jobsService.jobs;

    $scope.run = function(jobId) {
        var job = $jobsService.getById(jobId);
        var dataSource = _.find($dataService.dataSets, function(d) { return d.id == job.dataSetId});
        var payload = {
            "JobId": jobId,
                    "Name": job.name,
                    "DataSource": dataSource.url,
                    "Handler": job.javaScript
        };

        $http({
            "method": "POST",
            "url": "http://mapreducejs.cloudapp.net/ProxyService.svc/JobRequest/Add",
            "headers": {
                "Content-Type": "text/plain"
            },
            "data": payload
        }).success(function(data, status, headers, config) {
            window.alert("hura!");
        }).error(function(data, status, headers, config) {
            window.alert("failed!");
        });

        //TODO:
    }
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
