import socket
import sys
import json


def make_resp(data):
    return str({"resp": data}).encode('utf-8')

# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to the port
server_address = ('0.0.0.0', 10000)
#print(sys.stderr, 'starting up on',server_address'port',)
sock.bind(server_address)

pending_messages = {
    "u12345": [(1582895000, "Hello, Bob McMullin!")],
    "u99999": None,
}

# Listen for incoming connections
sock.listen(1)

while True:
    # Wait for a connection
    print('waiting for a connection')
    conn, client_addr = sock.accept()
    with conn:
        print("Connection accepted from", client_addr)
        while True:
            data = conn.recv(1024)
            if data:
                try:
                    new_data = json.loads(str(data))
                    print(new_data)
                    if new_data["req_type"] == "get_messages":
                        print("Attempting to get messages for", new_data["user_id"])
                        messages = pending_messages[new_data["user_id"]]
                        conn.sendall(make_resp(messages))
                except ValueError:
                    print("Malformed data error!")

