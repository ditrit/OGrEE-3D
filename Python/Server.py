#!/usr/bin/env python

import socket, select


# Function to broadcast chat messages to all connected clients
def ParseMessage(sock, msg):
    # Edit for your purpose

    # We broadcast the msg to all other clients for this tutorial
    for socket in CONNECTION_LIST:

        if socket != server:
            try:
                socket.send(msg.encode())
            except:
                # Assume client disconnected if it refused the message
                socket.close()
                CONNECTION_LIST.remove(socket)


if __name__ == "__main__":

    # List to keep track of socket descriptors
    CONNECTION_LIST = []
    RECV_BUFFER = 4096

    # Enter the server's IP and Port
    host = '172.30.146.51'
    port = 6000

    # Boolean to keep the server running
    runServer = True

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((host, port))
    server.listen(10)

    print("Chat server started on port " + str(port))

    # Add server socket to the list of readable connections
    CONNECTION_LIST.append(server)

    while runServer:

        # Obtain list of readable sockets
        read_sockets, write_sockets, error_sockets = select.select(CONNECTION_LIST, [], [])

        for sock in read_sockets:

            # New client request
            if sock == server:

                # Add new client
                client, addr = server.accept()
                CONNECTION_LIST.append(client)

                print("Client (%s, %s) connected" % addr)

            # broadcast_data(client, "[%s:%s] entered room\n" % addr)

            # New message from a client
            else:

                try:
                    # Recieve message
                    data = sock.recv(RECV_BUFFER).decode()

                    if data:
                        # Parse valid message
                        ParseMessage(sock, "\r" + '<' + str(sock.getpeername()) + '> | ' + data)
                        print(data)

                except:

                    # Assume client disconnected if they failed to send the meesage

                    ParseMessage(sock, "Client (%s, %s) is offline" % addr)
                    print("Client (%s, %s) is offline" % addr)

                    sock.close()
                    CONNECTION_LIST.remove(sock)

                    continue

    server.close()