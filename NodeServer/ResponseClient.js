var express = util = require('util')
  , azure = require('azure');


var ResponseClient = function (queueName) {
	console.log('Create ResponseClient for SB queue ' + queueName);
	this.serviceBusService = azure.createServiceBusService();
	this.QueueName = queueName;
	var queueOptions = {
		  LockDuration : 'PT8M'
		};

	this.serviceBusService.createQueueIfNotExists(queueName,function(error){
		if(!error){
			console.log('Queue exists');
		}
		else{
			console.log('Error creating queue: ' + queueName);
		}
	});  	
}


ResponseClient.prototype.sendMessage = function (msg){
	
	console.log('Send message to SB queue ' + this.QueueName);	
	this.serviceBusService.sendQueueMessage(this.QueueName, JSON.stringify(msg).toString(), function(error){
		if(!error){
			// message sent
			console.log('Message sent to SB queue');
		}
		else
			console.log('ERROR: Message NOT sent to SB queue');
	});
}

exports = module.exports = ResponseClient;
