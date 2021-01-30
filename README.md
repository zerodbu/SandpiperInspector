 This project serves as an API testing client (like Insomnia or Postman) for new Sandpiper implementations. It is being developed in parallel with the Sandpiper open API specification. My hope is that this tool will make it a little easier for those developing the server side of the Sandpiper API. Over the last few years, I've been asked to write API services to conform to existing specs. Every time was a miserable experience because I didn’t have a good method to see how a spec-compliant client would interact with my server. I always had to work with test systems that were opaque and required other people when I was “ready” to test/certify. Most of these would have taken half the time if someone had given me a purpose-build test client. WHI’s NexPart system and Epicor’s Aconnex protocol are prime examples.


Learn about Sandpiper at sandpiperframework.org



 Sandpiper servers use an authentication mechanism around JWTs (JSON Web Tokens). The /login route accepts a POST containing username, password and plandocument and (on successful auth) returns a JWT. The JWT's "payload" section enumerates the specific of the client/server relationship - including the endpoints the client may access on the server.


This client does Level-1 (full-file) grain interaction with the server. The local pool is a collection of files in the local cache folder. A timed housekeeping routine watches for changes in the local cache and indexes the files accordingly. Grainlist.txt and slicelist.txt serve as the indexes for the directory. The role (primary/secondary) of the client determines the direction (push/pull) that content will flow between the local and remote pools.









