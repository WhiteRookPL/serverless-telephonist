- API Gateway for 3 endpoints.
- Lambdas:
  - get-on-call - GET, returns JSON.
  - test-on-call - POST, no body mapping, returns status code only, empty response.
  - twilio-call-handler - POST, form-like body, returns status code + XML (TwiML).
    - Calling get-on-call lambda internally.
    - Easiest from those 3 lambdas will be in F#.

- Cognito used for user management.
  - Just login page, no way to sign-up.

- S3 bucket for website (+ CORS)
  - Elm complied to JS, HTML + CSS for a simple webapp
    - UI contains test button and information who is on-call.
      - 2 Lambdas used there (AJAX):
        - get-on-call
        - test-on-call-number
      - Cognito Browser JS SDK used externally in JS, flags passed afterwards to the Elm.
        - If possible do it in Elm completely.
