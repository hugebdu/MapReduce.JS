importScripts('underscore.js');

var job = {};
var outcome = {};

function log(msg) {
    this.postMessage({
        "cmd": "log",
        "msg": msg
    });
}

function collect(result) {
    outcome.result.push(result);
    if (outcome.result.length > 100) {
        this.postMessage({
            "cmd": "result",
            "outcome": outcome
        });
        outcome.result = [];
    }
}

function run() {
    outcome = {
        "jobId": job.details.jobId,
        "splitId": job.details.splitId,
        "phase": job.details.phase,
        "result": [],
        "done": false
    };

    var call = eval("(" + job.handler + ")")[job.details.phase.toLowerCase()];

    _.each(job.data, function(obj) {
        var result = call(obj);
        collect(result);
    });

    outcome.done = true;
    this.postMessage({
        "cmd": "result",
        "outcome": outcome
    });
}

this.onmessage = function(msg) {
    switch (msg.data.cmd) {
        case 'init':
            job = msg.data.job;
            break;
        case 'start':
            log("Starting " + job.details.name + "/" + job.details.phase);
            run();
            break;
    }
};
