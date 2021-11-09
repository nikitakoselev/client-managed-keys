# Client Managed Keys Reference Server
For connecting to APIs on behalf of its clients, Aiia Enterprise needs to
perform cryptographic operations on behalf of the client. 

For added security, Aiia Enterprise can proxy signing and decryption to
a client managed service, removing the need for Aiia Enterprise to be in possession
of the private keys and enabling the client to audit and control access to the
keys. 

This project is a reference implementation of the proxy protocol supported by 
Aiia Enterprise.
 
## Response status codes

Implementations MUST use HTTP status codes as the primary means of communicating success / failure.

*Successful codes*
- 200 OK
 - Returns payload

*Failure codes*
- 400 Bad Request
 - Missing required properties from request
 - Malformed payload
 - Invalid payload for operation
- 401 Unauthorized
 - Invalid or missing API-key provided
- 403 Forbidden 
 - Operation rejected, eg. due to audit checks of correlation IDs
- 404 Not Found
 - Invalid keyId
 - KeyId not known to server
- 429 Too Many Requests 
 - Rate limit reached, clients should retry

Implementations SHOULD populate the `message` property of the error message when possible.
 
## Operational requirements
 
All servers implementing this specification MUST NOT reject signing requests for the specific payload:
 
`TkFHQ2xpZW50TWFuYWdlZEtleXNWZXJpZmljYXRpb24=` (in ASCII `NAGClientManagedKeysVerification`)

This test vector is used for regularly verifying and testing system health.

## Demo Key Material

The server is preloaded with a demo key in `DemoKey.pfx` (password `test`). 
The key has the following certificate:

````
-----BEGIN CERTIFICATE-----
MIICsjCCAZoCCQCTFTQ8LTq9JjANBgkqhkiG9w0BAQsFADAbMRkwFwYDVQQDDBBU
RVNUIENFUlRJRklDQVRFMB4XDTIwMDkwOTE5NDEwN1oXDTMwMDkwNzE5NDEwN1ow
GzEZMBcGA1UEAwwQVEVTVCBDRVJUSUZJQ0FURTCCASIwDQYJKoZIhvcNAQEBBQAD
ggEPADCCAQoCggEBAJ2U9t8Gm+2MS0kQiZllBDUEFT7Ozqzb6U+kX4+MCxZEqLib
sgAwKrU8eHmUbe39HZcJ+fmGUDDDZ6namH7wqicdgjk6PFTeCTG+JjpPJWanRQbe
FBfuyg2wYZ41C6xAQ1lv9PG1YrEq8P26HYNhj8+wggsxV3iSl8+WiyxaDwv59qGf
0H430HMLpF04D21PiN2rh+5DaW/xHZdRRw0MRrHpfVFum8WLJswYulO92K0rlfkp
DP66PMEJld9n/1wDBXmeUDkdj/75fI0S1IIS6cwRMBHkT0JPNNA6dQKjA6F7Lqjy
tneJYp2ZfYN/ZSqwjiOvdD6+PkI+9FBRiTODzJUCAwEAATANBgkqhkiG9w0BAQsF
AAOCAQEARmYTFpQEn2B7U7kLB2a/i2hNtQtPMXR8FoBWnLnUZF63Z3C454j7wLaB
6CjTonCST2LDcO4GSryzZdiEdbxmGU0mtu/wH2KB1tFyjKLFBwbMdYqpq0fiktIa
W8hqbP1m1/qLCqOHVawnmfp6An1oJWsUA4da+TdeIoLB3LCs8odw+Ghz37cDcbS/
BQqmjxtmNRayNS5lvxr/+vIbOLeHY1DUUQyfBFL53VfAYoUJqZlnm49/qM3PVhuj
fmxaoo7/O1AonI3qr84KUJ/M3n++ODebvn+nsMRzMGLoU/UO0A6pMycB2L8gPWjA
uNbwKvpYPJy9mJEQs9ON7+UjQynP7w==
-----END CERTIFICATE-----
````

Thumbprint / keyId is: `0e4085c0ae94078ad8992e3fdf6ebffb707ffa8a`

The corresponding private can be extracted from the PFX file.

## Examples / test-vectors

### Signing

````shell
curl --location --request POST 'https://localhost:5001/v1/0e4085c0ae94078ad8992e3fdf6ebffb707ffa8a/sign' \
--header 'Content-Type: application/json' \
--header 'Authorization: Signature keyId=...' \
--data-raw '{
    "algorithm": "RS256",
    "clientId": "client-9778a952-4f22-4a57-abaa-61f99e14869a",
    "providerId": "XX_DemoBank",
    "correlationId": "|9e74f0e5-efc4-41b5-86d1-3524a43bd891.bcec871c_1.",
    "operation": "SupervisedLogin",
    "payload": "TkFHQ2xpZW50TWFuYWdlZEtleXNWZXJpZmljYXRpb24="
}'
````

Results in the response 
````
{
     "payload": "fFTZOo2+Sa9xquCm7Kcy74T7OhS+DaSVh7cQVmXXVkHUXcwqiLldjKwqEOk9ux/DliZ3mMJT6xo7cc28rfWa+54hzhjEmIdkBos0ZUs+6YFezYcabzGlTmGPm6K5zLZqMGSwx2bvKThrCI0q7mv+Nc7jNctUZ2S5zei6HrCELXy2UR5zLcaZUBUAyECKl19hYzx2eilSCMy4dt2lp3QnR1b/KM/7HIgJLFdlDHDpbZNA0qwqvq3j8bzOjpXAK5W0SN/rkNRiKPT/1hXMpPEh75iR0rFyyM/oope7ccCJ3iCghVLZ8s7S5ulntjA2lPYTnfKMIJuoeKAqEuVJQxqUJA=="
}
````

### Decryption

````shell
curl --location --request POST 'https://localhost:5001/v1/0e4085c0ae94078ad8992e3fdf6ebffb707ffa8a/decrypt' \
--header 'Content-Type: application/json' \
--header 'Authorization: Signature keyId=...' \
--data-raw '{
    "algorithm": "RSA-OAEP-256",
    "clientId": "client-9778a952-4f22-4a57-abaa-61f99e14869a",
    "providerId": "XX_DemoBank",
    "correlationId": "|9e74f0e5-efc4-41b5-86d1-3524a43bd891.bcec871c_1.",
    "operation": "SupervisedLogin",
    "payload": "mpBi7+q9XvpDWm3VOnyU1si1lpIqEPw0lGfZrNiONpg5rhZADmoIYwf96wXyZKXggBsfxQNd54KBtZ2ZgfFvnR6ONmMDtQiohGJUA7lKShnFIgllC7sC+PgN2i+BEXTZoWEfXJz4NtYf+PlPKdfit63WGP5rvpJaRbo3/cn0JaPUOqIRwbkx6dd46dt1d+zFiu993SiTXm1LxvEw1ZFGf0fd110THskkXOcFWxJO1Yg9wtUMihrB0hOJ97Kfdt8CjOiMiIAVAIyqXCrQvLiNivVEAmorsRTu2OC832/EmfnTu+fdK5zgquFM9ujymyS3ZbcpMe96IJmZmxsu+DEm5w=="
}'
````

Results in the response: 

```
{"payload":"SGVsbG8gd29ybGQK"}
``` 

which decodes to `Hello world`.