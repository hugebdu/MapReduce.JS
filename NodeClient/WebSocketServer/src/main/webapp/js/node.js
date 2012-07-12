/**
 * Created with IntelliJ IDEA.
 * User: daniels
 * Date: 7/12/12
 */

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

    $scope.job = {
        "name" : "Job name here...",
        "phase" : "Executing Map",
        "progress" : 0
    };

    $scope.run = function() {
        //TODO:
        $scope.status = $scope.StatusLabel.Running;
        $scope.job.progress = 0;
        $scope.log.println("Started job '" + $scope.job.name + "'");
        $scope.worker = new Worker("js/worker.js");
        $scope.worker.onmessage = function(msg) {
            $scope.$apply(function() {
                switch (msg.data.type) {
                    case 'progress' :
                        $scope.job.progress = msg.data.progress;
                        break;
                    case 'done' :
                        $scope.log.println("Completed running job '" + $scope.job.name + "'");
                        $scope.status = $scope.StatusLabel.Idle;
                        $scope.worker.terminate();
                        $scope.worker = {};
                        break;
                }
            });
        };
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
                $scope.job.name = job.details.name;
                $scope.job.phase = job.details.phase;
                //TODO: data & handler
                $scope.run();
            });
        });

        $scope.socket.on('register', function(msg) {
            $scope.$apply(function() {
                $scope.nodeId = msg.nodeId;
                $scope.log.println("Client node id [" + msg.nodeId + "]");
                $scope.socket.emit('userInfo', $filter('json')(userInfo));
            });
        });
    };

    $scope.disconnect = function() {
        //TODO: disconnect
        if ($scope.socket) {
            $scope.socket.disconnect();
            $scope.socket = {};
        }

        $scope.status = $scope.StatusLabel.Disconnected;
        $scope.log.println("Disconnected from " + $scope.general.server);
    };

    $scope.download = function(fileName) {
        $http.jsonp('http://shmuglindaniel.blob.core.windows.net/mapreducejs/' + fileName).
                success(function(data, status, headers, config) {
                    // this callback will be called asynchronously
                    // when the response is available
                    window.alert("success " + status);
                }).
                error(function(data, status, headers, config) {
                    // called asynchronously if an error occurs
                    // or server returns response with status
                    // code outside of the <200, 400) range
                    window.alert("error " + status);
                });
    };

    $scope.log = {
        data : "",
        println : function(msg) {
            if (this.data)
                this.data += '\n';
            this.data += $filter('date')(new Date(), "HH:mm:ss");
            this.data += "   " + msg;
        }
    };
}

