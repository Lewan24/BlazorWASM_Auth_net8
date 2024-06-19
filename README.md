[![.NET Version](https://img.shields.io/badge/.NET_8-8A2BE2?style=for-the-badge)](https://img.shields.io/badge/.NET_8-8A2BE2?style=for-the-badge)
[![.NET Project](https://img.shields.io/badge/Blazor_WASM-purple?style=for-the-badge)](https://img.shields.io/badge/Blazor_WASM-purple?style=for-the-badge)
[![.NET Architecture](https://img.shields.io/badge/Monolith-Modular-blue?style=for-the-badge)](https://img.shields.io/badge/Monolith-Modular-blue?style=for-the-badge)
[![.NET Custom Auth System](https://img.shields.io/badge/Custom_Authorization-darkgreen?style=for-the-badge)](https://img.shields.io/badge/Custom_Authorization-darkgreen?style=for-the-badge)
[![Project Template](https://img.shields.io/badge/Template-orange?style=for-the-badge)](https://img.shields.io/badge/Template-orange?style=for-the-badge)
<br><br>
[![.NET Workflow](https://github.com/Lewan24/WASMWithAuth/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Lewan24/WASMWithAuth/actions/workflows/dotnet.yml)

# Idea & Problem
Currently in blazor apps, if you want client to connect to the server, authorize to api requests, etc
you need to have saved auth data like username, tokens, passwords etc in session storage, cookies
or anything else in browser.
Currently if anyone just get/steal these cookies, data storage, like by copying them by script and pasting them in browser
on other machine, he can just log in to the stolen account via cookies, without any logging process and authorizing.

# Solution
I created project to prevent these actions, and make a better security options
to authorize users, get their information, work on api etc

Now application is based on microsoft identity to store and manage users data,
and short-liveable tokens to authorize users actions on api.

Generally for now user can see his options, available by granted role etc. but cant
authorize via token if cookies are stolen.

# Important note
Authorization, tokens and most valuable data are stored in singleton service created and initialized only after
successfull connection with server. If user logs in, the service generates token and authorizes user,
data is stored in service only, so after refresh page, token is gone.
Also if cookies are stolen and pasted in other machine, service is the new instance after
connecting to app, so token is also gone and user cant authorize.

If user is successfully logged in, service stores token that its live is for 20 minutes as like
the cookies or the selected time set in appsettings.

If token is expired, or its empty by any reason, the site automatically redirects user to
confirm page where need to type his password to account. After successfuly confirmed identity, service
connects to api and generates new token. Only after these actions user can still use api and app.

# Additional problems for future solving
The problem is that the tokens need to be generated and saved after login proccess, so the token is set in local storage, then application redirects user
to authorization page, saves the token to service and removes the token from storage. It's fast, but there is a small chance that in this small period of time, the object could be stolen from storage.

# Project features & architecture
- .NET 8
- Modular Monolith
- Custom extended authorization system
- MudBlazor
- Prepared pages: login, register, authorize, confirmAuthority
- OneOf
- Nice and minimalist design just for template purposes
- Higly easy project to extend and modify with new modules and functionallities

# Last words
This project is created for template purposes and for my future projects. (Mainly prepared for quick start with new application)
It's not perfect, but greatly created and prepared for any kind of application.

> [!NOTE]  
> If you want to create small application or something that doesn't need these featires like extended authorization, then don't use this template. It would be overkill.
