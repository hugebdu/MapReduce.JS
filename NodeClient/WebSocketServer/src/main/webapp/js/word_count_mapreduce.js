function map(word) {
    return {"key": word, "value": 1};
}

function reduce(result) {
    return {
        "key": result.key,
        "value":_.reduce(result.values, function(memo, num) { return memo + num }, 0)
    };
}