(function (window) {
  "use strict";

  const GET_ON_CALL_DETAILS_API_ENDPOINT = "https://qk98ppo7la.execute-api.eu-central-1.amazonaws.com/production/operators/current/details";
  const TEST_ON_CALL_API_ENDPOINT = "https://lfhpq8hjq3.execute-api.eu-central-1.amazonaws.com/production/calls/test";

  const PERSISTENT_MODAL = {
    backdrop: "static",
    keyboard: false,
    show: true
  };

  const POOL_DATA = {
    UserPoolId: ServerlessTelephonist.AWS.UserPoolId,
    ClientId: ServerlessTelephonist.AWS.ClientId
  };

  var ErrorBoxTimeoutId = 0;

  const USER_POOL = new AWSCognito.CognitoIdentityServiceProvider.CognitoUserPool(POOL_DATA);

  var Session = 0;
  var User = USER_POOL.getCurrentUser();

  function showError(message) {
    if (ErrorBoxTimeoutId > 0) {
      clearTimeout(ErrorBoxTimeoutId);
    }

    $("#error-box").html($("#error-box-template").html().replace("{{CONTENT}}", message));

    ErrorBoxTimeoutId = setTimeout(function () {
      ErrorBoxTimeoutId = 0;

      $("#error-box").children(".alert").alert("close");
    }, 5000);
  }

  function fetchCurrentOnCallDetails() {
    $.ajax({
      url: GET_ON_CALL_DETAILS_API_ENDPOINT,
      type: "GET",

      beforeSend: function (xhr) {
        xhr.setRequestHeader("Authorization", Session.getIdToken().getJwtToken());
      },

      success: function (response) {
        console.log("Getting on-call details: ", response);

        $("#on-call-name").text(response.Name);
        $("#on-call-tz").text(response.TimeZone);
        $("#on-call-phone").text(response.PhoneNumber);
      },

      error: function (error) {
        showError("Getting on-call details failed.");
      }
    });
  }

  function testOnCallNumber() {
    $.ajax({
      url: TEST_ON_CALL_API_ENDPOINT,
      type: "POST",
      contentType: "application/json; charset=utf-8",

      beforeSend: function (xhr) {
        xhr.setRequestHeader("Authorization", Session.getIdToken().getJwtToken());
      },

      success: function (response) {
        $("#test-on-call-modal").modal(PERSISTENT_MODAL);
      },

      error: function (error) {
        showError("Testing on-call number failed.");
      }
    });
  }

  function hideContent() {
    User = null;

    $("#content").addClass("none");

    $("#sign-in-box").removeClass("none");
    $("#sign-out-box").addClass("none");
  }

  function showContent() {
    $("#content").removeClass("none");

    fetchCurrentOnCallDetails();

    $("#sign-in-box").addClass("none");
    $("#sign-out-box").removeClass("none");
  }

  $("#sign-out-box-submit").click(function (event) {
    User.signOut();
    hideContent();
  });

  $("#test-on-call").click(function (event) {
    testOnCallNumber();
  });

  $("#test-on-call-approved").click(function (event) {
    $("#test-on-call-modal").modal("hide");
  });

  $("#sign-in-box-submit").click(function (event) {
    event.preventDefault();

    var username = $("#sign-in-box-login").val();

    var authenticationData = {
      Username: username,
      Password: $("#sign-in-box-password").val(),
    };

    var authenticationDetails = new AWSCognito.CognitoIdentityServiceProvider.AuthenticationDetails(authenticationData);

    var userData = {
        Username: username,
        Pool: USER_POOL
    };

    var cognitoUser = new AWSCognito.CognitoIdentityServiceProvider.CognitoUser(userData);

    cognitoUser.authenticateUser(authenticationDetails, {
      onSuccess: function (result) {
        var loginServiceSpecification = {};

        loginServiceSpecification[ServerlessTelephonist.AWS.LoginService] = result.getIdToken().getJwtToken();

        AWS.config.region = ServerlessTelephonist.AWS.Region;

        AWS.config.credentials = new AWS.CognitoIdentityCredentials({
          IdentityPoolId: ServerlessTelephonist.AWS.IdentityPoolId,
          Logins: loginServiceSpecification
        });

        AWS.config.credentials.refresh((error) => {
          if (error) {
            showError(error.toString());
            hideContent();
          } else {
            User = cognitoUser;
            showContent(cognitoUser);
          }
        });
      },

      onFailure: function (error) {
        showError(error.toString());
        hideContent();
      },

      newPasswordRequired: function (userAttributes, requiredAttributes) {
        delete userAttributes.email_verified;

        $("#change-password").click((event) => {
          event.preventDefault();

          var usernameChangePassword = $("#change-password-box-email").val();
          var newPassword = $("#change-password-box-new-password").val();
          var newPasswordAgain = $("#change-password-box-new-password-again").val();

          if (username !== usernameChangePassword) {
            $("#change-password-box-email").parent(".form-group").addClass("has-error");
            return;
          } else {
            $("#change-password-box-email").parent(".form-group").removeClass("has-error");
          }

          if (newPassword !== newPasswordAgain || !newPassword) {
            $("#change-password-box-new-password").parent(".form-group").addClass("has-error");
            $("#change-password-box-new-password-again").parent(".form-group").addClass("has-error");

            return;
          } else {
            $("#change-password-box-new-password").parent(".form-group").removeClass("has-error");
            $("#change-password-box-new-password-again").parent(".form-group").removeClass("has-error");
          }

          $("#change-password-modal").modal("hide");
        });

        $("#change-password-modal")
          .on("hide.bs.modal", (event) => {
            var newPassword = $("#change-password-box-new-password").val();
            cognitoUser.completeNewPasswordChallenge(newPassword, userAttributes, this);
          })
          .modal(PERSISTENT_MODAL);
      }
    });
  });

  if (!!User) {
    User.getSession(function (error, freshSession) {
      Session = freshSession;

      if (error) {
        showError(error.toString());
        return;
      }

      // Decide if we have session or not.
      if (!Session.isValid()) {
        hideContent();
      } else {
        showContent(User);
      }
    });
  }

} (window));
