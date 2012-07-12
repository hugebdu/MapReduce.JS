/**
 * Created with IntelliJ IDEA.
 * User: daniels
 * Date: 7/12/12
 */

function NodeCtrl($scope, $filter)
{
    $scope.StatusLabel = {
        Idle : {"label" : "Connected, idle.", "cssClass" : "label-info"},
        Running : {"label" : "Connected, executing task.", "cssClass" : "label-warning"},
        Disconnected : {"label" : "Disconnected.", "cssClass" : ""}
    }

    $scope.status = $scope.StatusLabel.Disconnected;

    $scope.general = {};
    $scope.general.server = "111.222.333.444:1234";
    $scope.general.account = "AlexanderVa (alexanderva@gmail.com)";

    $scope.job = {
        "name" : "Job name here...",
        "phase" : "Executing Map",
        "progress" : 20
    };

    $scope.run = function() {
        //TODO:
        $scope.status = $scope.StatusLabel.Running;
        $scope.log.println("Started job '" + $scope.job.name + "'");
    }

    $scope.connect = function() {
        //TODO: connect
        $scope.status = $scope.StatusLabel.Idle;
        $scope.log.println("Connected to " + $scope.general.server);
    }

    $scope.disconnect = function() {
        //TODO: disconnect
        $scope.status = $scope.StatusLabel.Disconnected;
        $scope.log.println("Disconnected from " + $scope.general.server);
    }

    $scope.log = {
        data : "",
        println : function(msg) {
            if (this.data)
                this.data += '\n';
            this.data += $filter('date')(new Date(), "HH:mm:ss");
            this.data += '\t' + msg;
        }
    };
}
