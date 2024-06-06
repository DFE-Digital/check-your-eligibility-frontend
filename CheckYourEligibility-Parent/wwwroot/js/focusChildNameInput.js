document.addEventListener("DOMContentLoaded", function () {
    // Select all input fields with IDs starting with 'ChildList' and ending with 'FirstName'
    var inputs = document.querySelectorAll("input[id^='ChildList'][id$='FirstName']");
    // Focus on the last input field
    if (inputs.length > 0) {
        inputs[inputs.length - 1].focus();
    }
});