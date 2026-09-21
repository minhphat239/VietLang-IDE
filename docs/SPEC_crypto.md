# Crypto Library — SPEC v1.0

## Hash

### 1. md5(text) → string
- Tinh MD5 hash
- Vi du: md5("Hello") → "8b1a9953c4611296a827abf8c47804d7"

### 2. sha1(text) → string
- Tinh SHA1 hash
- Vi du: sha1("Hello") → "f7ff9e8b7bb2e09b70935a5d7bb5e37f"

### 3. sha256(text) → string
- Tinh SHA256 hash
- Vi du: sha256("Hello") → "185f8db32271fe25f561a6fc938b2e264306ec304eda518007d1764826381969"

### 4. sha512(text) → string
- Tinh SHA512 hash
- Vi du: sha512("Hello") → "long hash..."

## HMAC

### 5. hmac_md5(key, text) → string
- HMAC-MD5
- Vi du: hmac_md5("secret", "Hello") → "hash..."

### 6. hmac_sha256(key, text) → string
- HMAC-SHA256
- Vi du: hmac_sha256("secret", "Hello") → "hash..."

## Symmetric

### 7. aes_ma(data, key) → string
- AES encrypt (CBC)
- Vi du: aes_ma("Hello", "secretkey1234567") → "encrypted..."

### 8. aes_giai(data, key) → string
- AES decrypt
- Vi du: aes_giai("encrypted...", "secretkey1234567") → "Hello"

## Random

### 9. ngau_nhien_bytes(count) → string
- Tao ngau nhien bytes
- Vi du: ngau_nhien_bytes(16) → "random bytes..."

### 10. ngau_nhien_hex(count) → string
- Tao ngau nhien hex
- Vi du: ngau_nhien_hex(16) → "a1b2c3d4..."

### 11. ngau_nhien_base64(count) → string
- Tao ngau nhien base64
- Vi du: ngau_nhien_base64(16) → "random base64..."

## Utility

### 12. so_sanh_an_toan(s1, s2) → bool
- So sanh an toan (timing-safe)
- Vi du: so_sanh_an_toan("abc", "abc") → đúng

## Luu y
- viet bang tieng Viet
- dung markdown format