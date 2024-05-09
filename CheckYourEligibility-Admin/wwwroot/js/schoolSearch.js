function searchSchool(query, index) {
    var id = `ChildList[${index}].school.Name`

    if (query.length >= 3 && query !== null) {
        fetch('/Check/GetSchoolDetails?query=' + query)
            .then(response => response.json())
            .then(data => {
                document.getElementById(`schoolList${index}`).innerHTML = '';
                let counter = 0;

                // loop response and add li elements with an onclick event listener to select the school
                data.forEach(function (value) {
                    var li = document.createElement('li');
                    li.setAttribute('id', value.id);
                    li.setAttribute('value', `${value.name}`);
                    // Check if the counter is even, if so add the 'autocomplete__option--odd' class
                    li.setAttribute('class', counter % 2 === 0 ? 'autocomplete__option' : 'autocomplete__option autocomplete__option--odd')
                    li.innerHTML = `${value.name}, ${value.id}, ${value.postcode}, ${value.la}`;
                    li.addEventListener('click', function () {
                        selectSchool(`${value.name}`, value.id, value.la, value.postcode, index);
                    });

                    document.getElementById(`schoolList${index}`).appendChild(li);
                    counter++;
                });
            });
    } else {
        document.getElementById(`schoolList${index}`).innerHTML = '';
    }
}

function selectSchool(school, urn, la, postcode, index) {
    // element_ids
    var schoolName = `ChildList[${index}].School.Name`;
    var schoolURN = `ChildList[${index}].School.URN`;
    var schoolPostcode = `ChildList[${index}].School.Postcode`;
    var schoolLA = `ChildList[${index}].School.LA`;

    // set values
    document.getElementById(schoolName).value = school;
    document.getElementById(schoolURN).value = urn;
    document.getElementById(schoolPostcode).value = postcode;
    document.getElementById(schoolLA).value = la;

    // set in local storage
    localStorage.setItem(`schoolName${index}`, school);
    localStorage.setItem(`schoolURN${index}`, urn);
    localStorage.setItem(`schoolPostcode${index}`, la);
    localStorage.setItem(`schoolLA${index}`, postcode);

    // clear options
    document.getElementById(`schoolList${index}`).innerHTML = '';
}

window.onload = function () {
    let i = 0;
    while (true) {
        let schoolName = localStorage.getItem(`schoolName${i}`);
        let schoolURN = localStorage.getItem(`schoolURN${i}`);
        let schoolPostcode = localStorage.getItem(`schoolPostcode${i}`);
        let schoolLA = localStorage.getItem(`schoolLA${i}`);

        if (schoolName === null && schoolURN === null && schoolPostcode === null && schoolLA === null) {
            // No more items in localStorage, break the loop
            break;
        }

        if (document.getElementById(`ChildList[${i}].School.Name`)) {
            // If the element exists, populate the fields
            document.getElementById(`ChildList[${i}].School.Name`).value = schoolName || '';
            document.getElementById(`ChildList[${i}].School.URN`).value = schoolURN || '';
            document.getElementById(`ChildList[${i}].School.Postcode`).value = schoolPostcode || '';
            document.getElementById(`ChildList[${i}].School.LA`).value = schoolLA || '';
        }

        i++;
    }
}

