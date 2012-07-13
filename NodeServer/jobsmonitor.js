var azure = require('azure')
  , util = require('util')
  , events = require('events');

process.env["AZURE_STORAGE_ACCOUNT"] = "vainshteinalexander";
process.env["AZURE_STORAGE_ACCESS_KEY"] = "NL3ag1ZkU7794cAspW+HBHlKXcmg+j0XpyY6TOK5X89KIqB/Rog+Yn4NdpHQ20YEpJ/p/lewHXLWoMpESFRLnw==";

var queueCheckDelay = 5000;
var jobsQueueName = 'jobsqueue';
  
exports = module.exports = JobsMonitor;

/**
* Creates a new JobsMaster object.
*
* @param {object} [app]          The server object.
*/
function JobsMonitor() {
	util.log("<JobsMonitor>: Create new Jobs object");
	
	this.queueService = azure.createQueueService();

	this.queueService.createQueueIfNotExists(jobsQueueName,function(error){
		if(!error){
			util.log('<JobsMonitor>: Queue exists');
		}
		else{
			util.log('<JobsMonitor>: Error creating queue');
		}
	});  

	setInterval(checkJobQueue, queueCheckDelay, this);
	//setTimeout(sendMessage, queueCheckDelay);
}

util.inherits(JobsMonitor, events.EventEmitter);  


function sendMessage(JobsMonitor){
	util.log("<JobsMonitor>: Send new message....");
	var creatingMessageOptions = {visibilitytimeout: 30 * 60};
	var message = '{"name":"new message","age":20}';
	JobsMonitor.queueService.createMessage(
		jobsQueueName,
		message,
		//creatingMessageOptions,
		function(error, queueMessageResult, response){
			if(!error){
				util.log("<JobsMonitor>: Message sent. " + queueMessageResult + ". Reponse: " + response);
			}
			else{
				util.log("<JobsMonitor>: Message sending error. " + queueMessageResult + ". Reponse: " + response);
			}
		});
}

function checkJobQueue(JobsMonitor){
	util.log("<JobsMonitor>: Check for new message....");
	var gettingMessageOptions = {numofmessages: 1, visibilitytimeout: 5};
	JobsMonitor.queueService.getMessages(
		jobsQueueName, 
		gettingMessageOptions,
		function(error, messages){
			if(!error){
				// Message received and locked
				for(var index in messages){
					var message = messages[index];
					util.log("Got message " + message.messageid);
					// text is available in messages[index].messagetext
					processJobMessage(JobsMonitor,message.messagetext);
					util.log("Delete message " + message.messageid);
					
					JobsMonitor.queueService.deleteMessage(
						jobsQueueName,
						message.messageid,
						message.popreceipt,
						function (deleteError){
							if(!deleteError){
								// Message deleted
								util.log("Message deleted...");
							}
						});
				}
			}
		});
}

function processJobMessage(JobsMonitor, message){
	util.log("<JobsMonitor>: Start process message");
	JobsMonitor.emit('jobReceived', message)
}

