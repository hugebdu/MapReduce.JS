/**
 * Created with IntelliJ IDEA.
 * User: daniels
 * Date: 7/13/12
 * Time: 5:46 PM
 * To change this template use File | Settings | File Templates.
 */

function InversedIndex() {

    this.map = function(record, collector) {
        //TODO:
        alert("mapping " + record.key);

        for (var value in record.values) {
            collector.collect(value, record.key);
        }
    };

    this.reduce = function(record, collector) {
        //TODO:
        alert("reducing " + record.key);
    };
}

