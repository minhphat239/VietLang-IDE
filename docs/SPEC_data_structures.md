# Data Structures Library — SPEC v1.0

## Set

### 1. tao_set(array) → set
- Tao set tu mang
- Vi du: tao_set([1, 2, 2, 3]) → {1, 2, 3}

### 2. hop(set1, set2) → set
- Hop (union)
- Vi du: hop({1, 2}, {2, 3}) → {1, 2, 3}

### 3. giao(set1, set2) → set
- Giao (intersection)
- Vi du: giao({1, 2}, {2, 3}) → {2}

### 4. hieu(set1, set2) → set
- Hieu (difference)
- Vi du: hieu({1, 2, 3}, {2}) → {1, 3}

### 5. la_set(value) → bool
- Kiem tra co phai set khong
- Vi du: la_set({1, 2}) → đúng

## Queue (FIFO)

### 6. tao_hang_doi() → queue
- Tao hang doi moi
- Vi du: tao_hang_doi() → empty queue

### 7. them_hang_doi(queue, value) → queue
- Them vao cuoi
- Vi du: them_hang_doi(q, 1)

### 8. lay_hang_doi(queue) → value
- Lay tu dau (xoa)
- Vi du: lay_hang_doi(q) → 1

### 9. xem_hang_doi(queue) → value
- Xem dau khong xoa
- Vi du: xem_hang_doi(q) → 1

### 10. kich_thuoc_hang_doi(queue) → number
- Kich thuoc
- Vi du: kich_thuoc_hang_doi(q) → 3

## Stack (LIFO)

### 11. tao_ngan_xep() → stack
- Tao ngan xep moi
- Vi du: tao_ngan_xep() → empty stack

### 12. day_ngan_xep(stack, value) → stack
- Day vao dinh
- Vi du: day_ngan_xep(s, 1)

### 13. lay_ngan_xep(stack) → value
- Lay tu dinh (xoa)
- Vi du: lay_ngan_xep(s) → 1

### 14. xem_ngan_xep(stack) → value
- Xem dinh khong xoa
- Vi du: xem_ngan_xep(s) → 1

### 15. kich_thuoc_ngan_xep(stack) → number
- Kich thuoc
- Vi du: kich_thuoc_ngan_xep(s) → 3

## Luu y
- viet bang tieng Viet
- dung markdown format
