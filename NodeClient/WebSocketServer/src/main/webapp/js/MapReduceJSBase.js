/**
 * Created with IntelliJ IDEA.
 * User: daniels
 * Date: 7/13/12
 */

function Collector(jobDescriptor, socket) {

    this.threshold = 3;
    this.buffer = [];

    this.collect = function (value, key) {
        this.buffer.push({
            "value":value,
            "key":key
        });

        if (this.buffer.length > this.threshold) {
            this.flushBuffer();
        }
    };

    this.flushBuffer = function () {
        socket.emit('interm-result', {
            "jobDescriptor" : jobDescriptor,
            "data" : this.buffer
        });
        this.buffer.clean();
    };
}

/**
 *
 * @param jobDescriptor
 * @param mapReduce
 * @param data
 * @constructor
 */
function MapReduce(jobDescriptor, mapReduce, data, socket) {

    this.collector = new Collector(jobDescriptor, socket);

    this.start = function () {
        var executor =
                jobDescriptor.phase == "map" ?
                        mapReduce.map :
                        mapReduce.reduce;

        for (var record in data) {
            executor(record, this.collector);
        }
    }
}