$(function () {
    function ReplaceFieldErrorClasses(field) {
        var fieldErrorClassName = 'field-validation-error';
        var govukGroupErrorClassName = 'govuk-form-group--error';
        var govukGroupClassName = '.govuk-form-group';
        var govukInputClassName = '.govuk-input';
        var govukErrorClassName = 'govuk-input--error';

        if (field.classList.contains(fieldErrorClassName)) {
            $(field).closest(govukGroupClassName).addClass(govukGroupErrorClassName);
            $(field).next(govukInputClassName).addClass(govukErrorClassName);
        } else {
            $(field).closest(govukGroupClassName).removeClass(govukGroupErrorClassName);
            $(field).next(govukInputClassName).removeClass(govukErrorClassName);
        }
    }

    // override add/remove class to trigger a change event
    (function (func) {
        $.fn.addClass = function () {
            func.apply(this, arguments);
            this.trigger('classChanged');
            return this;
        }
    })($.fn.addClass); // pass the original function as an argument

    (function (func) {
        $.fn.removeClass = function () {
            func.apply(this, arguments);
            this.trigger('classChanged');
            return this;
        }
    })($.fn.removeClass);

    // trigger the add/remove class changes to add validation error class to the parent form group
    $('.field-validation-error, .field-validation-valid').on('classChanged', function () {
        ReplaceFieldErrorClasses(this);
    });

    $('.field-validation-error, .field-validation-valid').each(function () {
        ReplaceFieldErrorClasses(this);
    });
});
