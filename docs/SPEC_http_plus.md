# HTTP Plus Library — SPEC v1.0

## Danh sach functions

### 1. lay_tieu_de(url) → dict
- Lay HTTP headers
- Vi du: lay_tieu_de("https://example.com") → {"Content-Type": "text/html"}

### 2. gui_tieu_de(url, headers, data) → string
- GUI request voi custom headers
- Vi du: gui_tieu_de(url, {"Authorization": "Bearer token"}, data)

### 3. tai_ve(url, path) → string
- Tai file ve local
- Vi du: tai_ve("https://example.com/file.txt", "local.txt") → "da tai"

### 4. tai_len(url, file_path) → string
- Tai file len server
- Vi du: tai_len("https://example.com/upload", "file.txt") → "da tai"

### 5. lay_cookie(url) → dict
- Lay cookies tu URL
- Vi du: lay_cookie("https://example.com") → {"session": "abc123"}

### 6. gui_cookie(url, cookies, data) → string
- GUI request voi cookies
- Vi du: gui_cookie(url, {"session": "abc"}, data)

### 7. lay_json(url) → dict
- GET va parse JSON
- Vi du: lay_json("https://api.example.com/data") → {"key": "value"}

### 8. gui_json(url, data) → string
- POST JSON data
- Vi du: gui_json("https://api.example.com/data", {"key": "value"})

### 9. tra_loi(status_code) → string
- Chuyen HTTP status code thanh message
- Vi du: tra_loi(200) → "OK"
- Vi du: tra_loi(404) → "Not Found"

### 10. la_thanh_cong(status_code) → bool
- Kiem tra status code co thanh cong khong (2xx)
- Vi du: la_thanh_cong(200) → đúng
- Vi du: la_thanh_cong(404) → sai

## Luu y
- viet bang tieng Viet
- dung markdown format
