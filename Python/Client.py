#!/usr/bin/env python

import socket, select, string, sys


def ParseMessage(msg):
    # Edit for your purpose

    # We print the msg for this tutorial
    sys.stdout.write(msg)


def RequestMessage():
    sys.stdout.write('<You> ')
    sys.stdout.flush()


if __name__ == "__main__":

    # Enter the server's IP and Port
    host = '192.168.1.38'
    port = 5000

    # Boolean to keep the client running
    runClient = True

    # Create socket instance
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(2)

    # Attempt to connect to server
    try:
        s.connect((host, port))
    except:
        print('Unable to connect')
        sys.exit()

    print('Successfully connected to remote host.')
    RequestMessage()

    while runClient:

        # Monitor the user's input and the server
        socket_list = [socket.socket(), s]

        # Obtain list of readable sockets
        read_sockets, write_sockets, error_sockets = select.select(socket_list, [], [])

        for sock in read_sockets:

            # New messages from server
            if sock == s:

                # Recieve message
                data = sock.recv(4096).decode()

                if not data:
                    print('\nDisconnected from chat server')
                    sys.exit()

                else:
                    # Parse valid message
                    sys.stdout.write(data)
                    # ParseMessage(data.decode())
                    RequestMessage()

            # Recieve message from user
            else:
                msg = sys.stdin.readline()
                s.send(msg.encode())
                RequestMessage()

    s.close()