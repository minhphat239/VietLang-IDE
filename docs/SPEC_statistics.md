# Statistics Library — SPEC v1.0

## Basic Stats

### 1. trung_binh(mang) → number
- Trung binh cong
- Vi du: trung_binh([1, 2, 3, 4]) → 2.5

### 2. trung_vi(mang) → number
- Trung vi (median)
- Vi du: trung_vi([1, 2, 3, 4, 5]) → 3

### 3. tan_suat(mang) → dict
- Tan xuat xuat hien
- Vi du: tan_suat([1, 2, 2, 3]) → {1: 1, 2: 2, 3: 1}

### 4. phan_vi(mang, percent) → number
- Phan vi (percentile)
- Vi du: phan_vi([1, 2, 3, 4, 5], 50) → 3

## Spread

### 5. do_lech_chuan(mang) → number
- Do lech chuan
- Vi du: do_lech_chuan([1, 2, 3, 4, 5]) → 1.414...

### 6. phuong_sai(mang) → number
- Phuong sai
- Vi du: phuong_sai([1, 2, 3, 4, 5]) → 2

### 7. khoang_cach(mang) → number
- Khoang cach (max - min)
- Vi du: khoang_cach([1, 5, 3]) → 4

### 8. tong_dai(mang) → number
- Tong gia tri
- Vi du: tong_dai([1, 2, 3]) → 6

### 9. tich_lon_nhat(mang) → number
- Tich lon nhat
- Vi du: tich_lon_nhat([1, 2, 3, 4]) → 24

### 10. tich_nho_nhat(mang) → number
- Tich nho nhat
- Vi du: tich_nho_nhat([1, 2, 3, 4]) → 1

## Correlation

### 11. he_so_tuong_quan(x, y) → number
- He so tuong quan Pearson
- Vi du: he_so_tuong_quan([1, 2, 3], [2, 4, 6]) → 1

### 12. duong_hoi_quy(x, y) → dict
- Duong hoi quy tuyen tinh
- Vi du: duong_hoi_quy([1, 2, 3], [2, 4, 6]) → {"slope": 2, "intercept": 0}

## Luu y
- viet bang tieng Viet
- dung markdown format
