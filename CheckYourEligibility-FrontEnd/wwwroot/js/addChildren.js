document.addEventListener('DOMContentLoaded', function () {
    var i = 1; // number of child elements

    document.getElementById('addChild').addEventListener('click', function (e) {
        e.preventDefault(); // Prevent form being submitted

        var childInputElement = document.createElement('div')
        childInputElement.id = `childDetails${i}`

        childInputElement.innerHTML =
            `<fieldset id="childDetails${i}" class="govuk-fieldset moj-add-another__item">
                         <legend class= "govuk-fieldset__legend moj-add-another__title govuk-fieldset__legend--m" >
                            Child ${i + 1} details
                         </legend>

                    <div class="button-container">
                        <div class="moj-button-action">
                            <button id="removeChild${i}" class="govuk-button govuk-button--secondary moj-add-another__add-button govuk-!-margin-bottom-4" data-module="govuk-button">
                                Remove child ${i + 1}
                            </button>
                        </div>
                    </div>

                    <div class="govuk-form-group">
                        <label class="govuk-label govuk-!-font-weight-bold" for="ch${i}-first-name">
                            First name
                        </label>
                        <input asp-for="ChildList[${i}].FirstName" class="govuk-input" type="text">
                    </div>

                    <div class="govuk-form-group">
                        <label class="govuk-label govuk-!-font-weight-bold" for="ch${i}-surname">
                            Last name
                        </label>
                        <input asp-for="ChildList[${i}].LastName" class="govuk-input" type="text">
                    </div>

                    <div class="govuk-form-group" >
                        <div class="govuk-form-group" >
                            <label class="govuk-label govuk-!-font-weight-bold" for="school-picker" >
                                School
                            </label>

                            <div id="school-picker-hint" class="govuk-hint" >
                                Type to search
                            </div>
                            <input asp-for="ChildList[${i}].school.Name" aria-expanded="false" aria-owns="school-picker__listbox" aria-autocomplete="both" autocomplete="off" class="autocomplete__input autocomplete__input--default" id="school-picker" type="text" role="combobox" aria-describedby="school-picker__assistiveHint" >
                        </div>
                    </div>

                    <div class="govuk-form-group" >
                            <fieldset class="govuk-fieldset" role="group" aria-describedby="ch${i}-date-of-birth-hint" >
                        <legend class="govuk-fieldset__legend govuk-fieldset__legend--s" >
                            Date of birth
                        </legend>

                        <div id="ch${i}-date-of-birth-hint" class="govuk-hint" >
                            For example, 27 3 2007
                        </div>

                        <div class="govuk-date-input" id="ch${i}-date-of-birth" >
                            <div class="govuk-date-input__item" >
                                <div class="govuk-form-group" >
                                        <label class="govuk-label govuk-date-input__label" for="ch${i}-day" >
                                        Day
                                    </label>
                                    <input asp-for="ChildList[${i}].Day" class="govuk-input govuk-date-input__input govuk-input--width-2" type="text" inputmode="numeric" >
                                </div>
                            </div>

                            <div class="govuk-date-input__item" >
                                <div class="govuk-form-group" >
                                        <label class="govuk-label govuk-date-input__label" for="ch${i}-month" >
                                        Month
                                    </label>
                                        <input asp-for="ChildList[${i}].Month" class= "govuk-input govuk-date-input__input govuk-input--width-2" type="text" inputmode="numeric" >
                                </div>
                            </div>

                            <div class="govuk-date-input__item" >
                                <div class="govuk-form-group" >
                                        <label class="govuk-label govuk-date-input__label" for="ch${i}-year" >
                                        Year
                                    </label>
                                        <input asp-for="ChildList[${i}].Year" class="govuk-input govuk-date-input__input govuk-input--width-4" type="text" inputmode="numeric" >
                                </div>
                            </div>
                        </div>
                    </fieldset>
                    </div>
            </fieldset>`

        var addChildBtn = document.querySelector('.add-button-container');
        addChildBtn.parentNode.insertBefore(childInputElement, addChildBtn);

        i++

        document.getElementById(`removeChild${i - 1}`).addEventListener('click', function (e) {
            e.preventDefault();

            var childFormGroup = document.getElementById(`childDetails${i - 1}`)
            childFormGroup.remove();
            i--
        });
    });
});

