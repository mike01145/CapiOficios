mergeInto(LibraryManager.library, {
  Auth_IsReady: function () { return window.authReady ? 1 : 0; },
  Auth_IsLoggedIn: function () { return window.authUser ? 1 : 0; },
  Auth_SignOut: function () {
    firebase.auth().signOut().catch(function (e) { console.error("signOut falló", e); });
  }
});