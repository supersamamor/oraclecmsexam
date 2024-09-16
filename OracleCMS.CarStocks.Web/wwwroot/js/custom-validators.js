function setValidationValues(options, ruleName, value) {
    options.rules[ruleName] = value;
    if (options.message) {
        options.messages[ruleName] = options.message;
    }
}

function escapeAttributeValue(value) {
    // As mentioned on http://api.jquery.com/category/selectors/
    return value.replace(/([!"#$%&'()*+,./:;<=>?@\[\\\]^`{|}~])/g, "\\$1");
}

function getModelPrefix(fieldName) {
    return fieldName.substr(0, fieldName.lastIndexOf(".") + 1);
}

function appendModelPrefix(value, prefix) {
    if (value.indexOf("*.") === 0) {
        value = value.replace("*.", prefix);
    }
    return value;
}

$.validator.unobtrusive.adapters.add('lessThan', ['other'], function (options) {
    var prefix = getModelPrefix(options.element.name),
        other = options.params.other,
        fullOtherName = appendModelPrefix(other, prefix),
        element = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(fullOtherName) + "']")[0];

    setValidationValues(options, "lessThan", element);
});

$.validator.unobtrusive.adapters.add('lessThanEqual', ['other'], function (options) {
    var prefix = getModelPrefix(options.element.name),
        other = options.params.other,
        fullOtherName = appendModelPrefix(other, prefix),
        element = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(fullOtherName) + "']")[0];

    setValidationValues(options, "lessThanEqual", element);
});

$.validator.unobtrusive.adapters.add('greaterThan', ['other'], function (options) {
    var prefix = getModelPrefix(options.element.name),
        other = options.params.other,
        fullOtherName = appendModelPrefix(other, prefix),
        element = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(fullOtherName) + "']")[0];

    setValidationValues(options, "greaterThan", element);
});

$.validator.unobtrusive.adapters.add('greaterThanEqual', ['other'], function (options) {
    var prefix = getModelPrefix(options.element.name),
        other = options.params.other,
        fullOtherName = appendModelPrefix(other, prefix),
        element = $(options.form).find(":input").filter("[name='" + escapeAttributeValue(fullOtherName) + "']")[0];

    setValidationValues(options, "greaterThanEqual", element);
});

// Adding custom validation methods
$.validator.addMethod("decimalRange", function (value, element, params) {
    if (this.optional(element)) {
        return true;
    }
    var min = parseFloat(params.min);
    var max = parseFloat(params.max);
    var val = parseFloat(value);

    return !isNaN(val) && val >= min && val <= max;
}, "The field must be between the specified range.");

$.validator.unobtrusive.adapters.add('decimalRange', ['min', 'max'], function (options) {   
    var params = {
        min: options.params.min,
        max: options.params.max
    };
    options.rules['decimalRange'] = params;
    if (options.message) {
        options.messages['decimalRange'] = options.message;
    }
});