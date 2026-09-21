# File Plus + Path Library — SPEC v1.0

## Tong quan
Thu vien nang cao xu ly file va duong dan cho VietLang.

## DANH SACH FUNCTIONS

### File Plus

#### 1. doc_dong(path) → array
- Doc file theo dong
- Vi du: doc_dong("test.txt") → ["line1", "line2"]

#### 2. ghi_dong(path, lines) → string
- Ghi mang thanh file
- Vi du: ghi_dong("out.txt", ["a", "b"]) → "da ghi"

#### 3. them(path, text) → string
- Them text vao cuoi file
- Vi du: them("log.txt", "new line") → "da them"

#### 4. sao_thu_muc(src, dst) → string
- Sao chep thu muc
- Vi du: sao_thu_muc("src/", "dst/") → "da sao chep"

#### 5. di_dung(path) → array
- Duyet thu muc de
- Vi du: di_dung(".") → ["file1.txt", "dir/file2.txt"]

#### 6. kich_thuoc_thu_muc(path) → number
- Kich thuoc thu muc (bytes)
- Vi du: kich_thuoc_thu_muc(".") → 12345

#### 7. trong(path) → bool
- Kiem tra path co rong khong
- Vi du: trong("empty_dir") → đúng

### Path

#### 8. ket_noi(...parts) → string
- Ket noi cac phan thanh duong dan
- Vi du: ket_noi("home", "user", "file.txt") → "home/user/file.txt"

#### 9. phan_tach(path) → dict
- Phan tach duong dan
- Vi du: phan_tach("/home/user/file.txt") → {"dir": "/home/user", "file": "file.txt"}

#### 10. mo_rong(path) → string
- Lay mo rong file
- Vi du: mo_rong("file.txt") → ".txt"

#### 11. ten_file(path) → string
- Lay ten file
- Vi du: ten_file("/home/user/file.txt") → "file.txt"

#### 12. thu_muc(path) → string
- Lay thu muc cha
- Vi du: thu_muc("/home/user/file.txt") → "/home/user"

#### 13. tuy_duong(path) → string
- Chuyen thanh tuyet doi
- Vi du: tuy_duong("./file.txt") → "/current/dir/file.txt"

#### 14. tuong_duong(path) → string
- Chuyen thanh tuong doi
- Vi du: tuong_duong("/home/user/file.txt") → "file.txt"

## Luu y
- viet bang tieng Viet
- dung markdown format
