{
    map: function(word) {
        return {"key": word, "value": 1}
    },
    reduce: function(result) {
        return {
            "key": result.key,
            "value": _.reduce(result.value, function(memo, num) { return memo + num }, 0)
        };
    }
}