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
    try {
      parsed = new URL(url);
    } catch (e) {
      return;
    }

    if (parsed.protocol !== "https:" || !parsed.pathname.includes("/auth/v1/authorize"))
      return;

    var popup = window.__platuGoogleAuthWindow;
    if (!popup || popup.closed) return;

    popup.location.replace(parsed.href);
  },

  CloseGoogleAuthWindow: function () {
    var popup = window.__platuGoogleAuthWindow;
    window.__platuGoogleAuthWindow = null;
    if (popup && !popup.closed) popup.close();
  }
});
