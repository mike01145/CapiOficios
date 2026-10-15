mergeInto(LibraryManager.library, {
  FirebaseRegister: function (emailPtr, passwordPtr, nombrePtr) {
    const email = UTF8ToString(emailPtr);
    const password = UTF8ToString(passwordPtr);
    const name = UTF8ToString(nombrePtr);

    firebase
      .auth()
      .createUserWithEmailAndPassword(email, password, name)
      .then((result) => {
        const user = result.user;


        console.log("Usuario creado:", user.uid);


        return firebase.firestore().collection("usuarios").doc(user.uid).set({
          nombre: name,
          contrasenia: password,
          email: email,
        });
      })
      .then(() => {
        const user = firebase.auth().currentUser;


        SendMessage("FirebaseManager", "OnRegisterSuccess", user.uid);
      })
      .catch((error) => {
        console.error("Error registro:", error);


        SendMessage("FirebaseManager", "OnFirebaseError", error.message);
      });
  },

  FirebaseCreateProgressDocument: function (userId) {
    firebase.firestore.collection("progresos").addDoc({
      misionesCumplidas: 0,
      misionesFalladas: 0,
      nivelAlcanzado: 1,
      tiempoJugado: 0,
      usuarioId: userId,
    })
    .then((result) => {
      return result.id;
    })
    .catch((error) => {
      console.error("FirebaseCreateProgressDocumentError: ", error);
    });
  }

  FirebaseLogin: function (emailPtr, passwordPtr) {
    const email = UTF8ToString(emailPtr);
    const password = UTF8ToString(passwordPtr);

    firebase
      .auth()
      .signInWithEmailAndPassword(email, password)
      .then((result) => {
        console.log("Login correcto:", result.user.uid);


        SendMessage("FirebaseManager", "OnLoginSuccess", result.user.uid);
      })
      .catch((error) => {
        console.error("Error login:", error);


        SendMessage("FirebaseManager", "OnFirebaseError", error.message);
      });
  },

  FirebaseUpdateProgress: function (progressId, mCumplidas, mFallidas, nAlcanzado, tJugado) {
    firebase.firestore().collection("progresos").doc(progressId).set({
      misionesCumplidas: mCumplidas,
      misionesFalladas: mFallidas,
      nivelAlcanzado: nAlcanzado,
      tiempoJugado: tJugado,
    })
    .catch((error) => {
      console.error("Error al actualizar progreso: ", error);
    });
  }

  FirebaseGetProgress: function (userId) {
    const usuariosRef = firebase.firestore().collection("progresos");

    //Define la consulta filtrando por el campo específico (ej. email)
    const q = query(usuariosRef, where("usuarioId", "==", userId));

    try {
      //Ejecuta la consulta y obtén los documentos resultantes
      const querySnapshot = await getDocs(q);

      if (querySnapshot.empty) {
        console.log("No se encontraron documentos que coincidan.");
        return;
      }

      //Itera sobre los resultados (devuelve un array, incluso si es solo un documento)
      querySnapshot.forEach((doc) => {
        // doc.id contiene el ID autogenerado del documento
        // doc.data() contiene los campos del documento
        console.log(`ID: ${doc.id} =>`, doc.data());
      });
      
    } catch (error) {
      console.error("Error al buscar el progreso: ", error);
    }
  }

  FirebaseAddCompletedMission: function (resultado, descripcion, objetivo, tiempo, userId) {
    firebase.firestore.collection("misiones").addDoc({
      resultado: resultado,
      descripcion: descripcion,
      objetivo: objetivo,
      tiempoEmpleado: tiempo,
      usuarioId: userId,
    })
    .then((result) => {
      return result.id;
    })
    .catch((error) => {
      console.error("Error al guardar la misión completada: ", error);
    });
  }
});

