"use strict";

const http = require("http");

const server = http.createServer(function (rq, rs) {
  let a = [];

  rq.on("data", function (ch) { a.push(ch); });
  rq.on("end", function () {
    console.log("%s %s", rq.method, rq.url);
    console.log("%j", rq.headers);
    console.log("%s", a.join(""));

    rs.setHeader("Content-Type", "application/xml");
    rs.writeHead(200);

    rs.end('<?xml version="1.0" encoding="UTF-8"?><Response><Say voice="woman">Please leave a message after the tone.</Say><Hangup /></Response>');
  });
});

server.listen(8080, function () {
  console.log("Server is listening on 8080...");
});
