document.body.className += ' js-enabled' + ('noModule' in HTMLScriptElement.prototype ? ' govuk-frontend-supported' : '');

import { initAll } from './govuk-frontend.min.js'
initAll();

const COOKIE_NAME = 'analytics-cookies-consent';

function initCookieConsent() {
    const hasChoice = cookie.read(COOKIE_NAME);
    const body = document.getElementsByTagName("body")[0];

    if (!hasChoice) {
        document.getElementById('cookie-banner').style.display = 'block';
    } else if (hasChoice === 'true') {
        initializeClarity();
    }

    // Add button handlers inside initCookieConsent
    document.getElementById('accept-cookies').onclick = acceptCookies;
    document.getElementById('reject-cookies').onclick = rejectCookies;
}

function acceptCookies() {
    cookie.create(COOKIE_NAME, 'true', 365);
    document.getElementById('cookie-banner').remove();
    initializeClarity();
}

function rejectCookies() {
    cookie.create(COOKIE_NAME, 'false', 365);
    document.getElementById('cookie-banner').remove();
    cookie.erase('_clarity');
    cookie.erase('CLARITY_MASTERID');
}

function initializeClarity() {
    const clarityId = document.body.getAttribute("data-clarity");
    if (clarityId) {
        (function (c, l, a, r, i, t, y) {
            c[a] = c[a] || function () { (c[a].q = c[a].q || []).push(arguments) };
            t = l.createElement(r); t.async = 1; t.src = "https://www.clarity.ms/tag/" + i;
            y = l.getElementsByTagName(r)[0]; y.parentNode.insertBefore(t, y);
        })(window, document, "clarity", "script", clarityId);
    }
}

// Initialize on load
window.onload = initCookieConsent;

// Button handlers
document.getElementById('accept-cookies').onclick = acceptCookies;
document.getElementById('reject-cookies').onclick = rejectCookies;