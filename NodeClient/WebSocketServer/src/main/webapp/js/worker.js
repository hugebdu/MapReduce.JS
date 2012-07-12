var done = 0;
var job = setInterval(function() {
    done += 10;
    postMessage({type: 'progress', progress: done});
    if (done == 100) {
        clearInterval(job);
        postMessage({type: 'done'});
        self.stop();
    }
}, 1000);