var authenticationData = {
  Username: 'afronski',
  Password: 'Bliblibli89',
};

var authenticationDetails = new AWSCognito.CognitoIdentityServiceProvider.AuthenticationDetails(authenticationData);
var poolData = {
  UserPoolId : 'eu-central-1_U355geDs6',
  ClientId : '21t9dkrgjbukb0q40cq6ln68sr'
};

var userPool = new AWSCognito.CognitoIdentityServiceProvider.CognitoUserPool(poolData);
var userData = {
    Username: 'afronski',
    Pool: userPool
};

var cognitoUser = new AWSCognito.CognitoIdentityServiceProvider.CognitoUser(userData);

cognitoUser.authenticateUser(authenticationDetails, {
    onSuccess: function (result) {
        console.log('access token + ' + result.getAccessToken().getJwtToken());

        AWS.config.region = 'eu-central-1';

        AWS.config.credentials = new AWS.CognitoIdentityCredentials({
            IdentityPoolId : 'eu-central-1:d3fc0162-987d-47b5-95ed-796e77bce2d8',
            Logins : {
                'cognito-idp.eu-central-1.amazonaws.com/eu-central-1_U355geDs6' : result.getIdToken().getJwtToken()
            }
        });

        AWS.config.credentials.refresh((error) => {
            if (error) {
                 console.error(error);
            } else {
                 console.log('Successfully logged!');
            }
        });
    },

    onFailure: function(err) {
        alert(err);
    },

    newPasswordRequired: function(userAttributes, requiredAttributes) {
      // User was signed up by an admin and must provide new
      // password and required attributes, if any, to complete
      // authentication.

      // the api doesn't accept this field back
      delete userAttributes.email_verified;

      // Get these details and call
      cognitoUser.completeNewPasswordChallenge('Bliblibli89', userAttributes, this);
    }
});
