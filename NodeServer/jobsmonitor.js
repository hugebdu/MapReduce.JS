var azure = require('azure')
  , util = require('util')
  , events = require('events');


var queueCheckDelay = 1000;
var jobsQueueName = 'jobchunks';
  
/**
* Creates a new JobsMaster object.
*
* @param {object} [app]          The server object.
*/
var JobsMonitor = function () {
	util.log("<JobsMonitor>: Create new Jobs object");
	
	this.queueService = azure.createQueueService();
	this.Read = false;

	this.queueService.createQueueIfNotExists(jobsQueueName,function(error){
		if(!error){
			util.log('<JobsMonitor>: Queue exists');
		}
		else{
			util.log('<JobsMonitor>: Error creating queue');
		}
	});  

	setInterval(this.checkJobQueue, queueCheckDelay, this);
	//setTimeout(sendMessage, queueCheckDelay);
}

util.inherits(JobsMonitor, events.EventEmitter);  

JobsMonitor.prototype.returnMessage = function(msg){
	util.log("<JobsMonitor>: Return message....");
	var creatingMessageOptions = {visibilitytimeout: 30 * 60};
	this.queueService.createMessage(
		jobsQueueName,
		msg,
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

JobsMonitor.prototype.checkJobQueue = function(JobsMonitor){
	if(!JobsMonitor.Read)return;
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
					util.log("Delete message " + message.messageid);
					
					JobsMonitor.queueService.deleteMessage(
						jobsQueueName,
						message.messageid,
						message.popreceipt,
						function (deleteError){
							if(!deleteError){
								// Message deleted
								util.log("Message deleted...");
								processJobMessage(JobsMonitor,message.messagetext);
							}
							else{
								util.log("<JobsMonitor>: Error deleting message");
							}
						});
					
				}
			}
			else
			{
				util.log("<JobsMonitor>: Error getting message");
			}
		});
}

function processJobMessage(JobsMonitor, message){
	util.log("<JobsMonitor>: Start process message");
	JobsMonitor.emit('jobReceived', message)
}


exports = module.exports = JobsMonitor;

