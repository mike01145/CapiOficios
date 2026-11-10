mergeInto(LibraryManager.library, {

  FirebaseRegister: function (emailPtr, passwordPtr, namePtr) {
    var email = UTF8ToString(emailPtr);
    var password = UTF8ToString(passwordPtr);
    var nombre = UTF8ToString(namePtr);

    firebase.auth().createUserWithEmailAndPassword(email, password)
      .then(function (result) {
        var u = result.user;
        var db = firebase.firestore();
        return Promise.all([
          db.collection("usuarios").doc(u.uid).set({ nombre: nombre, email: email }),
          db.collection("progresos").doc(u.uid).set({
            misionesCumplidas: 0,
            misionesFalladas: 0,
            nivelAlcanzado: 1,
            tiempoJugado: 0,
            usuarioId: u.uid
          })
        ]).then(function () {
          window.unityInstance.SendMessage("FirebaseManager", "OnAuthSuccess",
            JSON.stringify({ userId: u.uid, email: email, nombre: nombre }));
        });
      })
      .catch(function (error) {
        console.error("Error registro:", error);
        window.unityInstance.SendMessage("FirebaseManager", "OnAuthError", error.code || "unknown");
      });
  },

  FirebaseLogin: function (emailPtr, passwordPtr) {
    var email = UTF8ToString(emailPtr);
    var password = UTF8ToString(passwordPtr);

    firebase.auth().signInWithEmailAndPassword(email, password)
      .then(function (result) {
        var u = result.user;
        return firebase.firestore().collection("usuarios").doc(u.uid).get()
          .then(function (doc) {
            var d = doc.exists ? doc.data() : {};
            window.unityInstance.SendMessage("FirebaseManager", "OnAuthSuccess",
              JSON.stringify({ userId: u.uid, email: u.email, nombre: d.nombre || "" }));
          });
      })
      .catch(function (error) {
        console.error("Error login:", error);
        window.unityInstance.SendMessage("FirebaseManager", "OnAuthError", error.code || "unknown");
      });
  },

  FirebaseLogout: function () {
    firebase.auth().signOut();
  },

  FirebaseUpdateProgress: function (userIdPtr, mCumplidas, mFallidas, nAlcanzado, tJugado) {
    var userId = UTF8ToString(userIdPtr);

    firebase.firestore().collection("progresos").doc(userId).set({
      misionesCumplidas: mCumplidas,
      misionesFalladas: mFallidas,
      nivelAlcanzado: nAlcanzado,
      tiempoJugado: tJugado,
      usuarioId: userId
    }, { merge: true })
      .catch(function (error) {
        console.error("Error al actualizar progreso:", error);
      });
  },

  FirebaseGetProgress: function (userIdPtr) {
    var userId = UTF8ToString(userIdPtr);

    firebase.firestore().collection("progresos").doc(userId).get()
      .then(function (doc) {
        var d = doc.exists ? doc.data() : {};
        window.unityInstance.SendMessage("FirebaseManager", "OnProgressLoaded", JSON.stringify({
          misionesCumplidas: d.misionesCumplidas || 0,
          misionesFalladas: d.misionesFalladas || 0,
          nivelAlcanzado: d.nivelAlcanzado || 1,
          tiempoJugado: d.tiempoJugado || 0
        }));
      })
      .catch(function (error) {
        console.error("Error al buscar el progreso:", error);
      });
  },

  FirebaseAddCompletedMission: function (resultadoPtr, descripcionPtr, objetivoPtr, tiempo, userIdPtr) {
    firebase.firestore().collection("misiones").add({
      resultado: UTF8ToString(resultadoPtr),
      descripcion: UTF8ToString(descripcionPtr),
      objetivo: UTF8ToString(objetivoPtr),
      tiempoEmpleado: tiempo,
      usuarioId: UTF8ToString(userIdPtr),
      fecha: firebase.firestore.FieldValue.serverTimestamp()
    })
      .catch(function (error) {
        console.error("Error al guardar la misión completada:", error);
      });
  }
});