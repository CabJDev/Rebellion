"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/ClientHub").build();

document.getElementById("submitButton").disabled = true;

connection.start().then(function () {
    document.getElementById("submitButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
})

document.getElementById("submitButton").addEventListener("click", function (event) {
    var name = document.getElementById("name").value;
    var lobby = document.getElementById("lobbyCode").value;
    connection.invoke("SendMessage", name, lobby).catch(function (err) {
        return console.error(err.toString());
    })

    event.preventDefault();
})