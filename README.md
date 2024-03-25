[![.NET](https://github.com/Lewan24/WASMWithAuth/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Lewan24/WASMWithAuth/actions/workflows/dotnet.yml)

Project is created for one specific reason.

Currently in blazor apps, if you want client to connect to the server, authorize to api requests, etc
you need to have saved auth data like username, tokens, passwords etc in session storage, cookies
or anything else in browser.
Currently if anyone just get/steal these cookies, data storage, like by copying them by script and pasting them in browser
on other machine, he can just log in to the stolen account via cookies, without any logging process and authorizing.

I created project to prevent these actions, and make a better security options
to authorize users, get their information, work on api etc

Now application is based on microsoft identity to store and manage users data,
and short-liveable tokens to authorize users actions on api.

Generally for now user can see his options, available by granted role etc. but cant
authorize via token if cookies are stolen.

Authorization, tokens and most valuable data are stored in singleton service created and initialized only after
successfull connection with server. If user logs in, the service generates token and authorizes user,
data is stored in service only, so after refresh page, token is gone.
Also if cookies are stolen and pasted in other machine, service is the new instance after
connecting to app, so token is also gone and user cant authorize.

If user is successfully logged in, service stores token that its live is for 20 minutes as like
the cookies.

If token is expired, or its empty by any reason, the site automatically redirects user to
confirm page where need to type his password to account. After successfuly confirmed identity, service
connects to api and generates new token. Only after these actions user can still use api and app.

There are 2 problems for this solution.
The first one is that the generating proccess of token and sending from server to client cant be stolen by virus
or spy program. For this i used security like encrypting token, send, decrypt and save.
Encryption and decryption needs special 16 char length key used for specific text. Every encryption is different.

The second problem is that the tokens need to be generated and saved after login proccess, so the token is send via get method
to other pager, of course it uses the encrypted version of token so cant be just read and used.
