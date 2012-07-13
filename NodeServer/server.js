var express = require('express'),
	util = require('util'),
    curator = require('./curator');

process.on('uncaughtException1', function (err) {
  util.log('<Server>: Caught unhandled exception: ' + err);
});

util.log('<Server>: Start server');
var app = express.createServer();

app.listen(8082, function () {
  var addr = app.address();
});

util.log('<Server>: Create curator');
var curatorInstance = new curator(app);
util.log('<Server>: Start curator');
curatorInstance.start(app);
