# Dotnet IO Logger extenstion

## Intro

Software development has a lot of challenges. 

One of them: tracing all IO bound operations. 

Our extenstions should help you and handle all boring steps during debugging.

## Architecture

We provide Core lib where you can find how we log all events. 

If you use Visual Studio you can find IoLogger.VsExtenstion project. Just run it.

If you use VS code you can use IoLogger.Server app. This is aspnet core app with SignalR.

Provide process id and receive all events in your client application.

## How to use

*coming soon...*

## Features

We provide two extenstion:

* Visual Studio
* Visual Studio Code

### Visual Studio

1. run your app in debug mode
2. Go to View -> Other Windows -> Io logger Extension
3. Provide your Process id
4. see HTTP client and Aspnet reuqest information
5. Have fun :-)

### Vusial Studio Code

*comming soon...*

## What's next?

What we can improme:

### For VS

1. Get automaticly process id 
2. Add additional panel for request/response detail infromation (like HEADERS, Body and etc.)

### VS code

*comming soon...*

### Additional

We also need to prepare additional changes for dotnet source code

1. Provide correlation id between requests and responses
2. Add additional information about requests and responses

## Bugs

1. Response time calculation is not correct
2. Aspnet request provides events double 

## References
