# echo-server.py

import socket

HOST = "192.168.1.38"  # Standard loopback interface address (localhost)
PORT = 6000  # Port to listen on (non-privileged ports are > 1023)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s: #create a socket object that works with Ipv4, TCP protocols
    s.bind((HOST, PORT)) #assocaiate the socket with the host and port
    s.listen() #enables connection to the server
    conn, addr = s.accept() #when a connection is made, retrieve the Ip adress + port of the client and create a new socket conn
    with conn: #while the socket is open
        print(f"\nConnected by {addr}") #print the ip adress
        while True:
            data = conn.recv(1024) #read the data that was sent by the client
            if not data:
                break
            print(data)
            conn.send(b"hello world") #send back the data to the client