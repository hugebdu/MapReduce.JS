process.env["AZURE_STORAGE_ACCOUNT"] = "vainshteinalexander";
process.env["AZURE_STORAGE_ACCESS_KEY"] = "NL3ag1ZkU7794cAspW+HBHlKXcmg+j0XpyY6TOK5X89KIqB/Rog+Yn4NdpHQ20YEpJ/p/lewHXLWoMpESFRLnw==";
process.env["AZURE_SERVICEBUS_NAMESPACE"] = "MapReduceJS";
process.env["AZURE_SERVICEBUS_ACCESS_KEY"] = "kpt3ZfiSyEiC1GKZ9ZkjwuFG2kNMEv9Bl+aHt4Z7tNw=";
process.env["EMULATED"] = "true";
var conn_str = "Driver={SQL Server Native Client 10.0};Server=tcp:yloh7tw5n9.database.windows.net,1433;Database=mrjs;Uid=cloud@yloh7tw5n9;Pwd=MapReduce1;Encrypt=yes;Connection Timeout=30;";

var sio = require('socket.io')
  , util = require('util')
  , events = require("events")
  , sql = require('node-sqlserver')
  , jobsmonitor = require('./jobsmonitor')
  , azure = require('azure')
  , ResponseClient = require('./ResponseQueueClient');
  
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
}

var activeCurator;
var blobService = azure.createBlobService();
 
Curator.prototype.start = function (){
	activeCurator = this;
	this.curatorId = generateGuid();
	util.log("<Curator>: Curator.start");
	this.io = sio.listen(this.server,{ log: false });
	this.activeNodes = {};
	this.jobMonitor = new jobsmonitor();
	this.responseClients = {};
	this.messages = {};
	
	this.jobMonitor.on('jobReceived',function(msg){
			util.log('<Curator>: Receive job chunk');
			var freeNode = activeCurator.getFreeNode(true);
			if(freeNode){
				var srvMsg = JSON.parse(msg);
				
				blobService.getBlobToText(srvMsg.BlobContainer,srvMsg.BlobName, function(error,text, blobResult, response){
					if(!error){
						util.log('<Curator>: Send data from blob to client. Mode: ' + srvMsg.Mode);
						var clientMsg = {
									"details" : {
										"name" : srvMsg.ChunkUid.JobName,
										"jobId" : srvMsg.ChunkUid.JobId,
										"splitId" : srvMsg.ChunkUid.ChunkId,
										"phase" : srvMsg.Mode
									},
									"data" : JSON.parse(text),
									"handler" : srvMsg.Handler
						};

						if(!activeCurator.responseClients[srvMsg.ResponseQueueName])
							activeCurator.responseClients[srvMsg.ResponseQueueName] = new ResponseClient(srvMsg.ResponseQueueName);
						activeCurator.messages[freeNode.nodeId] = srvMsg;
						
						util.log('<Curator>: Emit job event for free node');
						util.log('<Curator>: Send JSON to free node');
						freeNode.currentStatus = "busy";
						updateJobMasterReadState(activeCurator);	
						freeNode.results = [];
						freeNode.emit('job', clientMsg);					
					}
					else{
						util.log("Error reading blog from storage");
					}
				});
			}
			else{
				updateJobMasterReadState(activeCurator);
				//activeCurator.jobMonitor.returnMessage(msg);
			}
		}
	);
		
	this.io.sockets.on('connection', function (socket) {
		// Node connected
		util.log('<Curator>: Connected new socket');
		socket.nodeId = registerNode(activeCurator);
		socket.currentStatus = 'new';
		util.log('<Curator>: Emit register new socket');
		socket.emit('register',{ nodeId: socket.nodeId});
		
		// Got node info
		socket.on('userInfo', function (info) {
			util.log('<Curator>: Got UserInfo event ');
			socket.currentStatus = 'idle';
			updateJobMasterReadState(activeCurator);
			updateNodeInfo(activeCurator,socket.nodeId,info);
		});

		// Got node info
		socket.on('jobProgress', function (jobInfo) {
			util.log('<Curator>: Client ' + socket.nodeId + ' sent job progress update: ' + jobInfo);
			updateNodeJobProgress(activeCurator,socket.nodeId,jobInfo);
		});

		// Node completed the work
		socket.on('done', function (partialResult) {
			try
			{
				util.log('<Curator>: Done of job chunk. Socke id: ' + socket.id);
				if(!partialResult)return;

				if(partialResult.done || true){
					util.log('<Curator>: Job chunk is partially done. Send results');
					var srvMsg = activeCurator.messages[socket.nodeId];
					
					var resultMessage = {
											"ChunkUid" : {
												"JobId" : partialResult.jobId, 
												"JobName" : partialResult.name,
												"ChunkId" : partialResult.splitId, 
											},
											"Mode" : partialResult.phase, 
											"Data" : partialResult.result,
											"Done" : partialResult.done,
											"ProcessorNodeId" : activeCurator.curatorId
										};
					
					activeCurator.responseClients[srvMsg.ResponseQueueName].sendMessage(resultMessage);
					if(partialResult.done){
						util.log('<Curator>: Job chunk is fully done.');
						socket.currentStatus = 'idle';
						updateJobMasterReadState(activeCurator); 
					}
				}
			}
			catch(e)
			{
				util.log('ERROR IN DONE:' + e.message);
			}
		});

		// Node disconnected
		socket.on('disconnect', function () {
			util.log('<Curator>: Socket disconnected: ' + socket.nodeId)
			socket.currentStatus = 'disconnected';
			unregisterNode(activeCurator,socket.nodeId);
		});
	});
}

Curator.prototype.processJob = function(job){
	util.log("<Curator>: Curator.processJob");
}


Curator.prototype.getFreeNode = function (pickNode){
	util.log("Look for free node");
	var clients = this.io.sockets.clients();
	util.log("Total client: " + clients.length);

	if(!pickNode){
		for(var i=0; i<clients.length; i++) {
			if (clients[i].currentStatus == 'idle'){
				util.log("[Found free node]");
				return clients[i];
			}
		}
		return null;
	}
	
	var minJobs = 999999999;
	var selectedNode = null;
	for(var i=0; i<clients.length; i++) {
        if (clients[i].currentStatus == 'idle'){
			if(!clients[i].jobs)
				clients[i].jobs = 0;
			
			if(clients[i].jobs<minJobs){
				selectedNode = clients[i];
				minJobs = clients[i].jobs;
			}
		}
    }
	
	if(selectedNode){
		util.log("[Found free node]");
		selectedNode.jobs++;
		return selectedNode;
	}
	
	util.log("No free nodes");
	return null;
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
	updateJobMasterReadState(curator); 
	updateNodeStatus(nodeId,'closed');
}

function updateJobMasterReadState(curator){
	if(!curator)return;
	curator.jobMonitor.Read = curator.getFreeNode(false)?true:false;
	util.log('<Curator>: Set jobMonitor.Read = ' + curator.jobMonitor.Read);
}

function updateNodeStatus(nodeId, status) {
	try{
		util.log('<Curator>: Update status to: ' + status); 
		var command;
		if(status=='new'){
			command = util.format("INSERT INTO Node (NodeId, Status, CuratorId) VALUES ('%s','%s','%s')",nodeId, status, activeCurator.curatorId);
		}
		else if(status=='closed'){
			command = util.format("DELETE FROM Node WHERE NodeId = '%s' AND CuratorId = '%s'",nodeId,activeCurator.curatorId);
		}
		else{
			command = util.format("UPDATE Node SET Status = '%s' WHERE NodeId = '%s' AND CuratorId = '%s'",status,nodeId,activeCurator.curatorId);
		}
		
		updateDb(command);
	}
	catch(e){
		util.log('<Curator>: Status update failed: ' + e.message); 
	}
}

function updateNodeInfo(curator, nodeId, nodeInfo) {
	try{
		util.log('<Curator>: Update node info to db'); 
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
		var command = util.format("UPDATE Node SET NodeInfo = '%s', Status = 'idle' WHERE NodeId = '%s' AND CuratorId = '%s'",nodeInfo,nodeId,curatorId);
		//updateDb(command);
	}
	catch(e){
		util.log('<Curator>: Node info update failed: ' + e.message); 
	}
}

function updateDb(command) {
	return;
	
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

