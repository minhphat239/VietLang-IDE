# Array Plus Library — SPEC v1.0

## Tổng quan
Thư viện nâng cao xử lý mảng cho VietLang.

## Danh sách functions

### 1. `sap_xep(array)` → `array`
Sắp xếp mảng theo thứ tự tăng dần.

**Ví dụ:**
```
sap_xep([3, 1, 2]) → [1, 2, 3]
```

---

### 2. `dao_nguoc(array)` → `array`
Đảo ngược thứ tự các phần tử trong mảng.

**Ví dụ:**
```
dao_nguoc([1, 2, 3]) → [3, 2, 1]
```

---

### 3. `phang(array)` → `array`
Phẳng hóa mảng đa cấp thành mảng một chiều.

**Ví dụ:**
```
phang([[1, 2], [3, 4]]) → [1, 2, 3, 4]
```

---

### 4. `doc_lap(array)` → `array`
Loại bỏ các phần tử trùng lặp, giữ lại phần tử xuất hiện đầu tiên.

**Ví dụ:**
```
doc_lap([1, 2, 2, 3, 3]) → [1, 2, 3]
```

---

### 5. `chia_nho(array, size)` → `array`
Chia mảng thành các mảng con có độ dài tối đa bằng `size`.

**Ví dụ:**
```
chia_nho([1, 2, 3, 4, 5], 2) → [[1, 2], [3, 4], [5]]
```

---

### 6. `gop_mang(arrays)` → `array`
Gộp nhiều mảng thành một mảng duy nhất.

**Ví dụ:**
```
gop_mang([[1, 2], [3, 4]]) → [1, 2, 3, 4]
```

---

### 7. `zip(arrays)` → `array`
Gộp các mảng theo chỉ mục tương ứng.

**Ví dụ:**
```
zip([[1, 3], [2, 4]]) → [[1, 2], [3, 4]]
```

---

### 8. `lay(array, n)` → `array`
Lấy `n` phần tử đầu tiên của mảng.

**Ví dụ:**
```
lay([1, 2, 3, 4], 2) → [1, 2]
```

---

### 9. `bo(array, n)` → `array`
Bỏ `n` phần tử đầu tiên, trả về phần còn lại.

**Ví dụ:**
```
bo([1, 2, 3, 4], 2) → [3, 4]
```

---

### 10. `tim_kiem(array, value)` → `number | null`
Tìm chỉ mục của `value` trong mảng. Trả về `null` nếu không tìm thấy.

**Ví dụ:**
```
tim_kiem([1, 2, 3], 2) → 1
```

---

### 11. `dem(array, value)` → `number`
Đếm số lần xuất hiện của `value` trong mảng.

**Ví dụ:**
```
dem([1, 2, 2, 3], 2) → 2
```

---

### 12. `hop_khong(array1, array2)` → `boolean`
Kiểm tra hai mảng có giống hệt nhau về nội dung và thứ tự không.

**Ví dụ:**
```
hop_khong([1, 2], [1, 2]) → true
hop_khong([1, 2], [2, 1]) → false
```

---

### 13. `chua(array, value)` → `boolean`
Kiểm tra mảng có chứa `value` hay không.

**Ví dụ:**
```
chua([1, 2, 3], 2) → true
```

---

### 14. `gan(array, index, value)` → `array`
Gán `value` tại vị trí `index`, trả về mảng mới.

**Ví dụ:**
```
gan([1, 2, 3], 1, 10) → [1, 10, 3]
```
