# Encoding Library — SPEC v1.0

## Base64

### 1. base64_ma(text) → string
- Ma hoa Base64
- Vi du: base64_ma("Hello") → "SGVsbG8="

### 2. base64_giai(text) → string
- Giai ma Base64
- Vi du: base64_giai("SGVsbG8=") → "Hello"

## URL

### 3. url_ma(text) → string
- URL encode
- Vi du: url_ma("hello world") → "hello%20world"

### 4. url_giai(text) → string
- URL decode
- Vi du: url_giai("hello%20world") → "hello world"

## HTML

### 5. html_ma(text) → string
- HTML encode
- Vi du: html_ma("<b>Hello</b>") → "&lt;b&gt;Hello&lt;/b&gt;"

### 6. html_giai(text) → string
- HTML decode
- Vi du: html_giai("&lt;b&gt;Hello&lt;/b&gt;") → "<b>Hello</b>"

## Hash

### 7. md5(text) → string
- Tinh MD5 hash
- Vi du: md5("Hello") → "8b1a9953c4611296a827abf8c47804d7"

### 8. sha1(text) → string
- Tinh SHA1 hash
- Vi du: sha1("Hello") → "f7ff9e8b7bb2e09b70935a5d7bb5e37f"

### 9. sha256(text) → string
- Tinh SHA256 hash
- Vi du: sha256("Hello") → "185f8db32271fe25f561a6fc938b2e26"

## Luu y
- viet bang tieng Viet
- dung markdown format
