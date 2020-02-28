import socket

HOST = '127.0.0.1'  # The server's hostname or IP address
PORT = 10000        # The port used by the server
USER_ID = "u12345"

info = {
    "req_type": "get_messages",
    "user_id": USER_ID,
}

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))
    s.sendall(str(info).encode("utf-8"))