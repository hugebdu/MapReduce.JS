var express = require('express')
  , sio = require('socket.io')
  , sql = require('node-sqlserver')
  , util = require('util');

var conn_str = "Driver={SQL Server Native Client 10.0};Server=tcp:yloh7tw5n9.database.windows.net,1433;Database=mrjs;Uid=cloud@yloh7tw5n9;Pwd=MapReduce1;Encrypt=yes;Connection Timeout=30;";

process.on('uncaughtException', function (err) {
  console.log('Caught unhandled exception: ' + err);
});

var curatorId = generateGuid();
var app = express.createServer();
var io = sio.listen(app);
app.listen(8082, function () {
  var addr = app.address();
});


app.get('/', function (req, res) {
  res.sendfile(__dirname + '/index.html');
});



var activeNodes = {};

io.sockets.on('connection', function (socket) {
	// Node connected
	console.log('Connected new socket id: ' + socket.id);

	socket.nodeId = registerNode();
	console.log('Emit register for nodeId:' + socket.nodeId);
	socket.emit('register',{ nodeId: socket.nodeId});
	
	// Got node info
	socket.on('userInfo', function (info) {
		console.log('Client ' + socket.nodeId + ' sent info: ' + info);
		updateNodeInfo(socket.nodeId,info);
	});

	// Got node info
	socket.on('jobProgress', function (jobInfo) {
		console.log('Client ' + socket.nodeId + ' sent job progress update: ' + jobInfo);
		updateNodeJobProgress(socket.nodeId,jobInfo);
	});

	// Node disconnected
	socket.on('disconnect', function () {
		console.log('Socket disconnected: ' + socket.nodeId)
		unregisterNode(socket.nodeId);
	});
	
	setTimeout(function() { 
				socket.emit(
					'job',
					{ 
						"details" : {
							"name" : "Daniloop",
							"jobId" : generateGuid(),
							"splitId" : generateGuid(),
							"phase" : "Map"
						},
						"data" : "http://wix.com",
						"handler" : "function (d) {alert('Hello from mapper');}"
					}
					); 
				}, 
				5000);
});


function registerNode(){
	var nodeId = generateGuid();
	console.log('  Assign socket ID = ' + nodeId);
	activeNodes[nodeId] = {};
	updateNodeStatus(nodeId,'new');
	return nodeId;
}

function unregisterNode(nodeId){
	if(!nodeId)return;
	delete activeNodes[nodeId];
	updateNodeStatus(nodeId,'closed');
}

function updateNodeStatus(nodeId, status) {
	try{
		console.log('Update status to: ' + status + ' for node ' + nodeId); 
		var command;
		if(status=='new'){
			command = util.format("INSERT INTO Node (NodeId, Status, CuratorId) VALUES ('%s','%s','%s')",nodeId, status, curatorId);
		}
		else if(status=='closed'){
			command = util.format("DELETE FROM Node WHERE NodeId = '%s' AND CuratorId = '%s'",nodeId,curatorId);
		}
		else{
			command = util.format("UPDATE Node SET Status = '%s' WHERE NodeId = '%s' AND CuratorId = '%s'",status,nodeId,curatorId);
		}
		
		updateDb(command);
	}
	catch(e){
		console.log('Status update failed: ' + e.message); 
	}
}

function updateNodeInfo(nodeId, nodeInfo) {
	try{
		console.log('Update node info: ' + nodeInfo + ' for node ' + nodeId); 
		activeNodes[nodeId] = nodeInfo;
		var command = util.format("UPDATE Node SET NodeInfo = '%s', Status = 'idle' WHERE NodeId = '%s' AND CuratorId = '%s'",nodeInfo,nodeId,curatorId);
		updateDb(command);
	}
	catch(e){
		console.log('Node info update failed: ' + e.message); 
	}
}

function updateNodeJobProgress(nodeId, jobInfo) {
	try{
		console.log('Update job info: ' + jobInfo + ' for node ' + nodeId); 
		//TODO: Update DB
		//var command = util.format("UPDATE Node SET NodeInfo = '%s', Status = 'idle' WHERE NodeId = '%s' AND CuratorId = '%s'",nodeInfo,nodeId,curatorId);
		//updateDb(command);
	}
	catch(e){
		console.log('Node info update failed: ' + e.message); 
	}
}

function updateDb(command) {
	try{
		console.log('  Perform DB update command: ' + command)
		
		sql.query(conn_str, command, function (err, results) {
			if (err) {
				console.log("Got error :-( " + err);
				return;
			}
			console.log("OK:" + results.length);
		});
	}
	catch(e){
		console.log('DB update failed: ' + e.message); 
	}
}

function generateGuid() {
    var S4 = function () {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
}
