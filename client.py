import socket
import sys
import json

r = {'type': 'REGISTER', 'parameters': {'username':'33', 'password':'34', 'nationalid': '100'}}
r = json.dumps(r)
print(r)
loaded_r = json.loads(r)


# Create a TCP/IP socket
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Connect the socket to the port where the server is listening
server_address = ('192.168.1.5', 4466)
print("Connecting..");
sock.connect(server_address)

# Send data
print("sending " + r)
sock.send(r.encode())
message = 'Waiting to receive data from server..'
# Look for the response
amount_received = 0
amount_expected = len(message)

while amount_received < amount_expected:
    data = sock.recv(1024)
    amount_received += len(data)
    print("received " + str(data.decode()))

print("Closing socket")
sock.close()