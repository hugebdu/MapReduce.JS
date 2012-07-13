var sio = require('socket.io')
  , util = require('util')
  , events = require("events")
  , sql = require('node-sqlserver')
  , jobsmonitor = require('./jobsmonitor');
  
var conn_str = "Driver={SQL Server Native Client 10.0};Server=tcp:yloh7tw5n9.database.windows.net,1433;Database=mrjs;Uid=cloud@yloh7tw5n9;Pwd=MapReduce1;Encrypt=yes;Connection Timeout=30;";
var exports = module.exports = Curator;

/**
* Creates a new Curator object.
*
* @param {object} [app]          The server object.
*/
function Curator(app) {
	util.log("<Curator>: Create new Curator object");
	if (!app) {
		throw new Error("<Curator>: Application cannot be null");
	}
	_initialize(this, app);
}


function _initialize(curator, app){
	curator.server = app;
	curator.curatorId = generateGuid();
}

Curator.prototype.start = function (){
	util.log("<Curator>: Curator.start");
	this.io = sio.listen(this.server);
	this.activeNodes = {};
	this.jobMonitor = new jobsmonitor();
	
	this.jobMonitor.on('jobReceived',function(msg){
			util.log('<Curator>: Curator.receive job ' + msg);
		}
	);
	
	this.io.sockets.on('connection', function (socket) {
		// Node connected
		util.log('<Curator>: Connected new socket id: ' + socket.id);

		socket.nodeId = registerNode(this);
		util.log('<Curator>: Emit register for nodeId:' + socket.nodeId);
		socket.emit('register',{ nodeId: socket.nodeId});
		
		// Got node info
		socket.on('userInfo', function (info) {
			util.log('<Curator>: Client ' + socket.nodeId + ' sent info: ' + info);
			updateNodeInfo(this,socket.nodeId,info);
		});

		// Got node info
		socket.on('jobProgress', function (jobInfo) {
			util.log('<Curator>: Client ' + socket.nodeId + ' sent job progress update: ' + jobInfo);
			updateNodeJobProgress(this,socket.nodeId,jobInfo);
		});

		// Node disconnected
		socket.on('disconnect', function () {
			util.log('<Curator>: Socket disconnected: ' + socket.nodeId)
			unregisterNode(this,socket.nodeId);
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
}

Curator.prototype.processJob = function(job){
	util.log("<Curator>: Curator.processJob");
}

function registerNode(curator){
	var nodeId = generateGuid();
	util.log('<Curator>:   Assign socket ID = ' + nodeId);
	curator.activeNodes[nodeId] = {};
	updateNodeStatus(nodeId,'new');
	return nodeId;
}

function unregisterNode(curator, nodeId){
	if(!nodeId)return;
	delete curator.activeNodes[nodeId];
	updateNodeStatus(nodeId,'closed');
}

function updateNodeStatus(curator, nodeId, status) {
	try{
		util.log('<Curator>: Update status to: ' + status + ' for node ' + nodeId); 
		var command;
		if(status=='new'){
			command = util.format("INSERT INTO Node (NodeId, Status, CuratorId) VALUES ('%s','%s','%s')",nodeId, status, curator.curatorId);
		}
		else if(status=='closed'){
			command = util.format("DELETE FROM Node WHERE NodeId = '%s' AND CuratorId = '%s'",nodeId,curator.curatorId);
		}
		else{
			command = util.format("UPDATE Node SET Status = '%s' WHERE NodeId = '%s' AND CuratorId = '%s'",status,nodeId,curator.curatorId);
		}
		
		updateDb(command);
	}
	catch(e){
		util.log('<Curator>: Status update failed: ' + e.message); 
	}
}

function updateNodeInfo(curator, nodeId, nodeInfo) {
	try{
		util.log('<Curator>: Update node info: ' + nodeInfo + ' for node ' + nodeId); 
		curator.activeNodes[nodeId] = nodeInfo;
		var command = util.format("UPDATE Node SET NodeInfo = '%s', Status = 'idle' WHERE NodeId = '%s' AND CuratorId = '%s'",nodeInfo,nodeId,curator.curatorId);
		updateDb(command);
	}
	catch(e){
		util.log('<Curator>: Node info update failed: ' + e.message); 
	}
}

function updateNodeJobProgress(curator,nodeId, jobInfo) {
	try{
		util.log('<Curator>: Update job info: ' + jobInfo + ' for node ' + nodeId); 
		//TODO: Update DB
		//var command = util.format("UPDATE Node SET NodeInfo = '%s', Status = 'idle' WHERE NodeId = '%s' AND CuratorId = '%s'",nodeInfo,nodeId,curatorId);
		//updateDb(command);
	}
	catch(e){
		util.log('<Curator>: Node info update failed: ' + e.message); 
	}
}

function updateDb(command) {
	try{
		util.log('<Curator>:   Perform DB update command: ' + command)
		
		sql.query(conn_str, command, function (err, results) {
			if (err) {
				util.log("Got error :-( " + err);
				return;
			}
			util.log("<Curator>: DB updated OK:" + results.length);
		});
	}
	catch(e){
		util.log('<Curator>: DB update failed: ' + e.message); 
	}
}

function generateGuid() {
    var S4 = function () {
        return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
}
