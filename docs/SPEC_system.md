# System Library — SPEC v1.0

## Process

### 1. tao_process(command) → string
- Tao process moi
- Vi du: tao_process("notepad.exe") → "pid: 1234"

### 2. ket_thuc_process(pid) → string
- Ket thuc process
- Vi du: ket_thuc_process(1234) → "da ket thuc"

### 3. lay_process_dang_chay() → array
- Lay danh sach process dang chay
- Vi du: lay_process_dang_chay() → ["chrome", "code", "..."]

### 4. kiem_tra_process(name) → bool
- Kiem tra process co dang chay khong
- Vi du: kiem_tra_process("chrome") → đúng

## Env

### 5. lay_env(name) → string
- Lay environment variable
- Vi du: lay_env("PATH") → "C:\\Windows;..."

### 6. gan_env(name, value) → string
- Gan environment variable
- Vi du: gan_env("MY_VAR", "hello") → "da gan"

### 7. xoa_env(name) → string
- Xoa environment variable
- Vi du: xoa_env("MY_VAR") → "da xoa"

### 8. lay_tat_ca_env() → dict
- Lay tat ca environment variables
- Vi du: lay_tat_ca_env() → {"PATH": "...", "HOME": "..."}

## OS

### 9. he_dieu_hanh() → string
- Lay ten he dieu hanh
- Vi du: he_dieu_hanh() → "Windows"

### 10. kich_thuoc_man_hinh() → dict
- Lay kich thuoc man hinh
- Vi du: kich_thuoc_man_hinh() → {"width": 1920, "height": 1080}

### 11. lay_ten_may() → string
- Lay ten may tinh
- Vi du: lay_ten_may() → "DESKTOP-ABC123"

### 12. lay_nguon() → number
- Lay bo nho dang su dung (MB)
- Vi du: lay_nguon() → 1024

## Luu y
- viet bang tieng Viet
- dung markdown format
