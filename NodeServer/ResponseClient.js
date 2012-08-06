var express = util = require('util')
  , azure = require('azure');


var ResponseClient = function (queueName) {
	console.log('Create ResponseClient for SB queue ' + queueName);
	this.serviceBusService = azure.createServiceBusService();
	this.QueueName = queueName;
	var queueOptions = {
		  LockDuration : 'PT8M'
		};

	this.serviceBusService.createQueueIfNotExists(queueName,queueOptions,function(error){
		if(!error){
			console.log('Queue exists');
		}
		else{
			console.log('Error creating queue');
		}
	});  	
}


ResponseClient.prototype.sendMessage = function (msg){
	console.log('Send message to SB queue ' + this.QueueName);
	var message = {
		body: msg.data
	};
	
	this.serviceBusService.sendQueueMessage(this.QueueName, message, function(error){
		if(!error){
			// message sent
		}
	});
}

exports = module.exports = ResponseClient;
