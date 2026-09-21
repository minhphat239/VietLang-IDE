# Watch + OS Library — SPEC v1.0

## Watch

### 1. theo_doi_file(path, callback) → string
- Theo doi file thay doi
- Vi du: theo_doi_file("data.txt", ham_callback)

### 2. theo_doi_thu_muc(path, callback) → string
- Theo doi thu muc
- Vi du: theo_doi_thu_muc("./src", ham_callback)

### 3. dung_theo_doi(watcher) → string
- Dung theo doi
- Vi du: dung_theo_doi(watcher) → "da dung"

## OS

### 4. lay_duong_dan_hien_tai() → string
- Lay duong dan hien tai
- Vi du: lay_duong_dan_hien_tai() → "D:\\Documents\\VietLang"

### 5. doi_thu_muc(path) → string
- Doi thu muc lam viec
- Vi du: doi_thu_muc("D:\\Projects") → "da doi"

### 6. lay_danh_sach_thu_muc(path) → array
- Lay danh sach thu muc con
- Vi du: lay_danh_sach_thu_muc(".") → ["src", "tests", "docs"]

### 7. lay_danh_sach_file(path) → array
- Lay danh sach file
- Vi du: lay_danh_sach_file(".") → ["README.md", "test.vl"]

### 8. kich_thuoc(path) → number
- Lay kich thuoc file/thu muc
- Vi du: kich_thuoc("file.txt") → 1234

### 9. ngay_sua(path) → string
- Lay ngay sua doi gan nhat
- Vi du: ngay_sua("file.txt") → "2026-09-21T10:00:00"

### 10. la_file(path) → bool
- Kiem tra co phai file
- Vi du: la_file("file.txt") → đúng

### 11. la_thu_muc(path) → bool
- Kiem tra co phai thu muc
- Vi du: la_thu_muc("src") → đúng

## Luu y
- viet bang tieng Viet
- dung markdown format
