# Regex Library — SPEC v1.0

## Tong quan
Thu vien regular expression cho VietLang.

## Danh sach functions

### 1. khop(text, pattern) → bool
- Kiem tra text co khop pattern khong
- Vi du: khop("123", "\\d+") → true

### 2. tim(text, pattern) → string/null
- Tim first match
- Vi du: tim("abc123def", "\\d+") → "123"

### 3. tim_tat_ca(text, pattern) → array
- Tim tat ca matches
- Vi du: tim_tat_ca("a1b2c3", "\\d+") → ["1", "2", "3"]

### 4. thay_the(text, pattern, replacement) → string
- Thay the pattern bang replacement
- Vi du: thay_the("a1b2", "\\d+", "x") → "axbx"

### 5. tach(text, pattern) → array
- Tach text theo pattern
- Vi du: tach("a,b,,c", ",+") → ["a", "b", "c"]

### 6. trich_xuat(text, pattern) → array
- Trich xuat groups
- Vi du: trich_xuat("2026-09-21", "(\\d+)-(\\d+)-(\\d+)") → ["2026", "09", "21"]

### 7. hop_le(text, pattern) → bool
- Kiem tra toan bo text khop pattern
- Vi du: hop_le("abc", "[a-z]+") → true

## Pattern Syntax

| Ky hieu | Y nghia | Vi du |
|---------|---------|-------|
| `\\d` | Ky tu so [0-9] | `\\d+` : 1 hoac nhieu so |
| `\\w` | Ky tu chu, so, gach duoi [a-zA-Z0-9_] | `\\w+` : tu |
| `\\s` | Ky tu khoang trang | `\\s+` : khoang trang |
| `[...]` | Tap ky tu | `[aeiou]` : nguyen am |
| `(...)` | Nhom (capture group) | `(\\d+)` : nhom so |
| `+` | 1 hoac nhieu lan | `a+` : a, aa, aaa, ... |
| `*` | 0 hoac nhieu lan | `a*` : "", a, aa, ... |
| `?` | 0 hoac 1 lan | `a?` : "", a |
| `{n}` | Exactly n lan | `a{3}` : aaa |
| `{n,m}` | Tu n den m lan | `a{2,4}` : aa, aaa, aaaa |
| `^` | Bat dau chuoi | `^Hello` : bat dau bang "Hello" |
| `$` | Ket thuc chuoi | `world$` : ket thuc bang "world" |
| `.` | Bat ky ky tu nao (trong ngoac) | `a.b` : a + bat ky + b |
| `\|` | Hoac | `cat\|dog` : "cat" hoac "dog" |
| `[^...]` | Negated character class | `[^0-9]` : khong phai so |

## Luu y
- viet bang tieng Viet
- dung markdown format
- pattern dung backslash tron trong ngoac ké: `\\d`, `\\w`, `\\s` (khong phai `\d`, `\w`, `\s`)
