mergeInto(LibraryManager.library, {
  OpenGoogleAuthWindow: function () {
    var popup = window.open("about:blank", "_blank");
    if (!popup) return 0;

    window.__platuGoogleAuthWindow = popup;
    if (!window.__platuGoogleAuthMessageHandler) {
      window.__platuGoogleAuthMessageHandler = function (event) {
        if (event.origin !== "https://patu-salmon.onrender.com" ||
            event.source !== window.__platuGoogleAuthWindow ||
            !event.data || event.data.type !== "platu-google-login") return;
        SendMessage("GoogleController", "HandleOAuthMessage", JSON.stringify(event.data));
        window.__platuGoogleAuthWindow = null;
      };
      window.addEventListener("message", window.__platuGoogleAuthMessageHandler);
    }
    try {
      popup.document.open();
      popup.document.write(
        "<!doctype html><title>Google sign-in</title>" +
        "<body style='font:16px sans-serif;text-align:center;padding:3em'>" +
        "Connecting to Google sign-in…</body>"
      );
      popup.document.close();
    } catch (e) {
      // The blank tab is still available even if its loading message cannot be written.
    }

    return 1;
  },

  NavigateGoogleAuthWindow: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    var parsed;
    var popup = window.__platuGoogleAuthWindow;
    function fail(message, error) {
      console.error(message, error || "");
      if (popup && !popup.closed) {
        try { popup.document.body.textContent = message; } catch (ignored) {}
      }
      return 0;
    }

    try {
      parsed = new URL(url);
    } catch (e) {
      return fail("Could not read the Google sign-in URL.", e);
    }

    if (parsed.protocol !== "https:" || !parsed.pathname.includes("/auth/v1/authorize")) {
      return fail("The sign-in URL from the server is not a valid Google authorization URL: " + parsed.origin + parsed.pathname);
    }

    if (!popup || popup.closed)
      return fail("The sign-in tab was closed before it could be opened.");

    try {
      popup.location.href = parsed.href;
      return 1;
    } catch (e) {
      return fail("Could not navigate the sign-in tab. Please reload the game and try again.", e);
    }
  },

  CloseGoogleAuthWindow: function () {
    var popup = window.__platuGoogleAuthWindow;
    window.__platuGoogleAuthWindow = null;
    if (popup && !popup.closed) popup.close();
  }
});
