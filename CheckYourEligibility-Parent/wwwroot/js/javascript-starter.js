document.body.className += ' js-enabled' + ('noModule' in HTMLScriptElement.prototype ? ' govuk-frontend-supported' : '');

import { initAll } from './govuk-frontend.min.js'
initAll();

function initializeClarity() {
    let clarityId = document.getElementsByTagName("body")[0].getAttribute("data-clarity");
    if (clarityId) {
        (function (c, l, a, r, i, t, y) {
            c[a] = c[a] || function () {
                (c[a].q = c[a].q || []).push(arguments)
            };
            t = l.createElement(r);
            t.async = 1;
            t.src = "https://www.clarity.ms/tag/" + encodeURIComponent(i);
            y = l.getElementsByTagName(r)[0];
            y.parentNode.insertBefore(t, y);
        })(window, document, "clarity", "script", clarityId);
    }
}

function initCookieConsent() {
    const hasChoice = cookie.read("cookie");
    if (hasChoice==="true") {
        initializeClarity();
    }
    
    else if (hasChoice !== "false") {
        document.getElementById('cookie-banner').style.display = 'block';
    }
    
    console.log(hasChoice);
}

document.getElementById('accept-cookies').onclick = function() {
    cookie.create("cookie", "true", 365);
    document.getElementById('cookie-banner').style.display = 'none';
    initializeClarity();
};
document.getElementById('reject-cookies').onclick = function() {
    cookie.create("cookie", "false", 365);
    document.getElementById('cookie-banner').style.display = 'none';
};

var cookie = {
    create: function (name, value, days) {
        let expires = "";
        if (days) {
            const date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = "; expires=" + date.toUTCString();
        }
        document.cookie = name + "=" + value + expires + "; path=/";
    },

    read: function (name) {
        const nameEQ = name + "=";
        const ca = document.cookie.split(';');
        for (let i = 0; i < ca.length; i++) {
            let c = ca[i].trim();
            if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length);
        }
        return null;
    },

    erase: function (name) {
        this.create(name, "", -1);
    }
};

initCookieConsent();