importScripts('js/underscore.js');

function Collector() {

}

function ExecutionWorker() {
    //TODO

    this._cancelled = false;
    this._job = {};
    this._data = {};
    this._phase = {};

    this.onMessage = function(e) {
        switch (e.data.cmd) {
            case 'start':
                this.start(e.data.options);
                break;
            case 'cancel':
                this.cancel();
                break;
        }
    }

    this.cancel = function() {
        this._cancelled = true;
    }

    this.start = function(options) {
        this._phase = options.phase;
        this._job = eval(options.code);
        this.loadData(options.dataUrl);
    }

    this.loadData = function(dataUrl) {
        this.log("Loading data from " + dataUrl);
        importScripts(dataUrl + '?callback=callback');
    }

    this.onDataLoaded = function(data) {
        this._data = data;
        this.log("Data loaded");
        this.doProcess();
    }

    this.doProcess = function() {
        //TODO
    }

    this.log = function(msg) {
        self.postMessage({'cmd': 'log', 'msg': msg});
    }
}

var execution = new ExecutionWorker();
self.onmessage = execution.onMessage;

function callback(data) {
    execution.onDataLoaded(data);
}