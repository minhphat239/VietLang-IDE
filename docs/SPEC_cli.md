# CLI Library — SPEC v1.0

## ArgParse

### 1. tao_phan_tich(ten, mo_ta) → object
- Tao parser moi
- Vi du: tao_phan_tich("myapp", "My application")

### 2. them_arg(parser, name, mo_ta, mac_dinh) → object
- Them argument
- Vi du: them_arg(p, "--input", "Input file", "")

### 3. them_co(parser, name, mo_ta) → object
- Them flag (khong co gia tri)
- Vi du: them_co(p, "--verbose", "Verbose output")

### 4. phan_tich(parser, args) → dict
- Phan tich arguments
- Vi du: phan_tich(p, ["--input", "file.txt"]) → {"input": "file.txt"}

### 5. hien_thi_help(parser) → string
- Hien thi huong dan su dung
- Vi du: hien_thi_help(p) → "Usage: myapp [options]"

## Prompt

### 6. xac_nhan(cau_hoi) → bool
- Hoi yes/no
- Vi du: xac_nhan("Ban co muon tiep tuc?") → đúng

### 7. chon_muc(cau_hoi, danh_sach) → string
- Chon tu danh sach
- Vi du: chon_muc("Chon mau:", ["do", "xanh", "vang"]) → "do"

### 8. nhap_so(cau_hoi) → number
- Nhap so
- Vi du: nhap_so("Nhap tuoi:") → 25

### 9. nhap_mat_khau(cau_hoi) → string
- Nhap mat khau (an *)
- Vi du: nhap_mat_khau("Nhap mat khau:") → "abc123"

## Table

### 10. in_bang(data, headers) → string
- In bang dep
- Vi du: in_bang([["Nam", 25], ["Lan", 22]], ["Ten", "Tuoi"])

### 11. in_danh_sach(items) → string
- In danh sach co dau den
- Vi du: in_danh_sach(["Item 1", "Item 2"])

### 12. in_tien_trinh(percent, message) → string
- In thanh tien trinh
- Vi du: in_tien_trinh(50, "Dang tai...")

## Luu y
- viet bang tieng Viet
- dung markdown format