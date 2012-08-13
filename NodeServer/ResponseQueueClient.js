var express = util = require('util')
  , azure = require('azure');


var ResponseClient = function (queueName) {
	console.log('Create ResponseQueueClient for queue ' + queueName);

	this.queueService = azure.createQueueService();
	this.QueueName = queueName;
	this.queueService.createQueueIfNotExists(queueName,function(error){
		if(!error){
			util.log('<ResponseQueueClient>: Queue exists');
		}
		else{
			util.log('<ResponseQueueClient>: Error creating queue');
		}
	});  		
}


ResponseClient.prototype.sendMessage = function (msg){
	
	console.log('Send message to SB queue ' + this.QueueName);	
	this.queueService.createMessage(this.QueueName, JSON.stringify(msg).toString(), function(error){
		if(!error){
			// message sent
			console.log('Message sent to response queue');
		}
		else
			console.log('ERROR: Message NOT sent to response queue');
	});
}

exports = module.exports = ResponseClient;
