
function JobCtrl($scope) {

    $scope.update = function(job) {
        alert(angular.toJson(job));
    }
}