# Advanced Libraries — SPEC v1.0

## Tree

### 1. tao_cay(value) → object
- Tao nut goc
- Vi du: tao_cay(1) → {value: 1, children: []}

### 2. them_con(parent, child) → object
- Them nut con
- Vi du: them_con(parent, child) → "da them"

### 3. dfs(cay, callback) → string
- Duyet sau (depth-first)
- Vi du: dfs(root, ham_callback)

### 4. bfs(cay, callback) → string
- Duyet rong (breadth-first)
- Vi du: bfs(root, ham_callback)

### 5. chieu_cao(cay) → number
- Chieu cao cay
- Vi du: chieu_cao(root) → 3

### 6. dem_nut(cay) → number
- Dem so nut
- Vi du: dem_nut(root) → 10

## Linear Algebra

### 7. tao_vector(...values) → array
- Tao vector
- Vi du: tao_vector(1, 2, 3) → [1, 2, 3]

### 8. cong_vector(v1, v2) → array
- Cong 2 vector
- Vi du: cong_vector([1, 2], [3, 4]) → [4, 6]

### 9. nhan_vector(v, scalar) → array
- Nhan voi scalar
- Vi du: nhan_vector([1, 2], 3) → [3, 6]

### 10. tich_vo_huong(v1, v2) → number
- Tich vo huong
- Vi du: tich_vo_huong([1, 2], [3, 4]) → 11

### 11. do_dai_vector(v) → number
- Do dai vector
- Vi du: do_dai_vector([3, 4]) → 5

### 12. chuan_hoa_vector(v) → array
- Chuan hoa vector
- Vi du: chuan_hoa_vector([3, 4]) → [0.6, 0.8]

## Complex Numbers

### 13. tao_phuc(real, imag) → object
- Tao so phuc
- Vi du: tao_phuc(3, 4) → {real: 3, imag: 4}

### 14. cong_phuc(z1, z2) → object
- Cong so phuc
- Vi du: cong_phuc({real: 1, imag: 2}, {real: 3, imag: 4}) → {real: 4, imag: 6}

### 15. nhan_phuc(z1, z2) → object
- Nhan so phuc
- Vi du: nhan_phuc({real: 1, imag: 2}, {real: 3, imag: 4}) → {real: -5, imag: 10}

### 16. lien_hop(z) → object
- Lien hop so phuc
- Vi du: lien_hop({real: 3, imag: 4}) → {real: 3, imag: -4}

### 17. modul(z) → number
- Modul so phuc
- Vi du: modul({real: 3, imag: 4}) → 5

## Luu y
- viet bang tieng Viet
- dung markdown format
