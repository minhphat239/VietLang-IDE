# Network Library — SPEC v1.0

## TCP

### 1. tao_server(port) → object
- Tao TCP server
- Vi du: tao_server(8080) → "server created"

### 2. ket_noi(host, port) → object
- Ket noi TCP
- Vi du: ket_noi("localhost", 8080) → "connected"

### 3. gui_ket_noi(connection, data) → string
- Gui du lieu
- Vi du: gui_ket_noi(conn, "Hello") → "da gui"

### 4. nhan_ket_noi(connection) → string
- Nhan du lieu
- Vi du: nhan_ket_noi(conn) → "Hello"

### 5. dong_ket_noi(connection) → string
- Dong ket noi
- Vi du: dong_ket_noi(conn) → "da dong"

## UDP

### 6. tao_udp(port) → object
- Tao UDP socket
- Vi du: tao_udp(9090) → "udp created"

### 7. gui_udp(socket, host, port, data) → string
- Gui UDP packet
- Vi du: gui_udp(sock, "localhost", 9090, "Hello")

### 8. nhan_udp(socket) → string
- Nhan UDP packet
- Vi du: nhan_udp(sock) → "Hello"

## DNS

### 9. phan_giai(hostname) → string
- DNS resolve
- Vi du: phan_giai("example.com") → "93.184.216.34"

### 10. nguoc(hostname) → array
- Reverse DNS
- Vi du: nguoc("93.184.216.34") → ["example.com"]

## Utility

### 11. kiem_tra_mang() → bool
- Kiem tra ket noi mang
- Vi du: kiem_tra_mang() → đúng

### 12. lay_ip() → string
- Lay IP address
- Vi du: lay_ip() → "192.168.1.1"

### 13. ping(host) → bool
- Ping host
- Vi du: ping("google.com") → đúng

## Luu y
- viet bang tieng Viet
- dung markdown format
