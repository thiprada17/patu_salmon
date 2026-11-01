mergeInto(LibraryManager.library, {
  OpenGoogleAuthWindow: function (urlPtr) {
    var url = UTF8ToString(urlPtr);
    var parsed;
    try {
      parsed = new URL(url);
    } catch (e) {
      console.error("Invalid Google sign-in endpoint", e);
      return 0;
    }
    if (parsed.origin !== "https://patu-salmon.onrender.com" ||
        parsed.pathname !== "/auth/google/redirect") {
      console.error("Unexpected Google sign-in endpoint", parsed.origin, parsed.pathname);
      return 0;
    }

    var popup = window.open("about:blank", "_blank");
    if (!popup) {
      window.alert("Google sign-in was blocked by this browser. Open the game in Safari or Chrome and allow pop-ups, then try again.");
      return 0;
    }

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
      popup.document.write("<!doctype html><title>Google sign-in</title><p>Connecting to Google…</p>");
      popup.document.close();
      // Navigate during the original click. This avoids waiting for UnityWebRequest
      // before redirecting, which can leave mobile browser popups on about:blank.
      popup.location.href = parsed.href;
    } catch (e) {
      console.error("Could not start Google sign-in", e);
      try { popup.close(); } catch (ignored) {}
      return 0;
    }

    return 1;
  },

  CloseGoogleAuthWindow: function () {
    var popup = window.__platuGoogleAuthWindow;
    window.__platuGoogleAuthWindow = null;
    if (popup && !popup.closed) popup.close();
  }
});
