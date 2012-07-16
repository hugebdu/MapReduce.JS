<?php
header("Access-Control-Allow-Origin: *");
?>
<!DOCTYPE html>
<html ng-app>
<head>
    <title></title>
    <link href="bootstrap/css/bootstrap.css" type="text/css" rel="stylesheet">
    <script type="text/javascript" src="js/angular-1.0.1.js"></script>
    <script type="text/javascript" src="js/node.js"></script>
    <script type="text/javascript" src="js/socket.io.js"></script>
    <script type="text/javascript" src="http://code.jquery.com/jquery-1.7.2.js"></script>
    <script type="text/javascript">
        var server = "http://192.168.1.140:8082/"
    </script>
    <style type="text/css">
        .quater {
            width: 420px;
            height: 150px;
        }

        .log-panel {
            width: 900px;
            height: 250px;
        }

        .log-panel pre {
            height: 195px;
            margin-top: 5px;
            background-color: #000000;
            overflow-y: scroll;
            color: #32cd32;
        }
    </style>
</head>
<body>
<div class="container" ng-controller="NodeCtrl">
    <img src="images/logo.png" alt="logo">

    <div class="row">
        <div class="span6 well quater">
            <h2>Status</h2>
            <span class="label {{status.cssClass}}">{{status.label}}</span>
            <dl ng-hide="status == StatusLabel.Disconnected">
                <dt>Remote server:</dt>
                <dd>{{general.server}}</dd>
                <dt>Account:</dt>
                <dd>{{general.account}}</dd>
            </dl>
            <div class="control-group">
                <button class="btn btn-primary" ng-click="connect()">Connect</button>
                <button class="btn" ng-click="disconnect()">Disconnect</button>
                <button class="btn" ng-click="run()">Run</button>
            </div>
        </div>
        <div class="span6 well quater">
            <h2>Job Details</h2>
            <dl style="margin-top: 1px;" ng-show="status == StatusLabel.Running">
                <dt>Name:</dt>
                <dd>{{job.name}}</dd>
                <dt>Phase:</dt>
                <dd>{{job.phase}}</dd>
                <dt>Progress:</dt>
                <dd>
                    <div class="progress progress-striped" style="margin-top: 5px;">
                        <div class="bar"
                             style="width: {{job.progress}}%;"></div>
                    </div>
                </dd>
            </dl>
        </div>
        <div class="span12 well log-panel">
            <h2>Log:</h2>
            <pre>{{log.data}}</pre>
        </div>

        <fieldset>
            <input type="radio" ng-model="file" value="{{files[0]}}">{{files[0]}}<br>
            <input type="radio" ng-model="file" value="{{files[1]}}">{{files[1]}}<br>
            <input type="radio" ng-model="file" value="{{files[2]}}">{{files[2]}}<br>
            <input type="radio" ng-model="file" value="{{files[3]}}">{{files[3]}}<br>
        </fieldset>
        <button class="btn" ng-click="download()">Download '{{file}}'</button>
    </div>
</div>

</body>
</html>
