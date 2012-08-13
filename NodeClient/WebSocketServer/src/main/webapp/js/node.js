/**
 * Created with IntelliJ IDEA.
 * User: daniels
 * Date: 7/12/12
 */

var nodeModule = angular.module("NodeModule", []);

nodeModule.directive("ngLog", function factory() {
    return {
        "restrict": 'A',
        "link": function(scope, element, attrs, controller) {
            scope.$watch(attrs['ngLog'], function(newValue, oldValue) {
                element.text(newValue);
                element.prop('scrollTop', element.prop('scrollHeight'));
            });
        } 
    };
});

function onDataLoaded(data) {
    alert("Hurra: " + data.length + ' records');
}

function NodeCtrl($scope, $filter, $http)
{
    $scope.worker = {};

    $scope.StatusLabel = {
        Idle : {"label" : "Connected, idle.", "cssClass" : "label-info"},
        Running : {"label" : "Connected, executing task.", "cssClass" : "label-warning"},
        Disconnected : {"label" : "Disconnected.", "cssClass" : ""}
    };

    $scope.status = $scope.StatusLabel.Disconnected;

    $scope.general = {};
    $scope.general.server = window.server;
    $scope.general.account = "AlexanderVa (alexanderva@gmail.com)";

    $scope.socket = {};

    $scope.nodeId = {};

    $scope.job = {};

    $scope.run = function() {

        $scope.status = $scope.StatusLabel.Running;
        $scope.log.println("Started job '" + $scope.job.details.name + "' phase '" + $scope.job.details.phase + "' split '" + $scope.job.details.splitId + "'");
        $scope.job.progress = 0;

        var worker = new Worker('js/worker.js');
        var resultsCount = 0;

        worker.onmessage = function(msg) {
            $scope.$apply(function() {
                switch (msg.data.cmd) {
                    case 'log':
//                        $scope.log.println("(worker) " + msg.data.msg);
                        break;
                    case 'result':
//                        $scope.log.println("(worker) got results: " + msg.data.outcome.result.length);
                        resultsCount += msg.data.outcome.result.length;
                        $scope.job.progress = parseInt(resultsCount / $scope.job.data.length * 100);
                        $scope.socket.emit('done', msg.data.outcome);
                        if (msg.data.outcome.done) {
                            $scope.log.println("Finished job '" + $scope.job.details.name + "' phase '" + $scope.job.details.phase + "' split '" + $scope.job.details.splitId + "'");
                            $scope.status = $scope.StatusLabel.Idle;
                            worker.terminate();
                        }
                        break;
                }
            });
        };

        worker.postMessage({
            "cmd": "init",
            "job": $scope.job
        });

        worker.postMessage({"cmd": "start"});
//        //TODO:
//        $scope.status = $scope.StatusLabel.Running;
//        $scope.job.progress = 0;
//        var job = $scope.job;
//
//        $scope.log.println("Started job '" + job.details.name + "' phase '" + job.details.phase + "'");
//        var f = eval("(" + job.handler + ")");
//        var outcome = {
//            "jobId": job.details.jobId,
//            "splitId": job.details.splitId,
//            "result": [],
//            "done": false
//        };
//
//        for (var i = 0; i < job.data.length; i++)
//        {
//            outcome.result.push(f[job.details.phase.toLowerCase()](job.data[i]));
//            job.progress = parseInt(job.data.length / 100 * i);
//            if (outcome.result.length == 10)
//            {
//                $scope.socket.emit('done', outcome);
//                outcome.result = [];
//            }
//        }
//        outcome.done = true;
//        $scope.socket.emit('done', outcome);
//        $scope.log.println("Finished job '" + job.details.name + "' phase '" + job.details.phase + "'");
//        $scope.status = $scope.StatusLabel.Idle;

    };

    $scope.connect = function() {
        $scope.log.println("Trying to connect to server " + $scope.general.server);

        var userInfo = {
            "browser" : $.browser
        };

        $scope.socket = io.connect($scope.general.server);

        $scope.socket.on('connect', function() {
            $scope.$apply(function() {
                $scope.status = $scope.StatusLabel.Idle;
                $scope.log.println("Connected to " + $scope.general.server);
            });
        });

        $scope.socket.on('job', function(job) {
            $scope.$apply(function() {
                $scope.job = job;
                $scope.run();
            });
        });

        $scope.socket.on('disconnect', function() {
            $scope.log.println("Disconnected from " + $scope.general.server);
            $scope.status = $scope.StatusLabel.Disconnected;
            $scope.socket = {};
        });                                                                      ;

        $scope.socket.on('register', function(msg) {
            $scope.$apply(function() {
                $scope.nodeId = msg.nodeId;
                $scope.log.println("Client node id [" + msg.nodeId + "]");
                $scope.socket.emit('userInfo', $filter('json')(userInfo));
            });
        });
    };

    $scope.disconnect = function() {
        if ($scope.socket) {
            $scope.socket.disconnect();
        }
    };

    $scope.log = {

        data : "",

        _timestamp : function() {
            var padZeros = function(val, padSize) {
                var str = val.toString();
                while (str.length < padSize)
                    str = "0" + str;
                return str;
            };
            var date = new Date();
            return padZeros(date.getHours(), 2) + ":" +
                    padZeros(date.getMinutes(), 2) + ":" +
                    padZeros(date.getSeconds(), 2) + "." +
                    padZeros(date.getMilliseconds(), 3);
        },

        println : function(msg) {
            if (this.data)
                this.data += '\n';
            this.data += this._timestamp() + "   " + msg;
        },

        clear: function() {
            this.data = "";
        }
    };

    $scope.connect();
}

