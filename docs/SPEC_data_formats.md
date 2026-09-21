# Data Formats Library — SPEC v1.0

## JSON Plus

### 1. json_dinh_dang(json) → string
- Dinh dang JSON dep
- Vi du: json_dinh_dang({"a": 1}) → "{\n  \"a\": 1\n}"

### 2. json_hop_le(text) → bool
- Kiem tra JSON co hop le khong
- Vi du: json_hop_le("{\"a\": 1}") → đúng
- Vi du: json_hop_le("{a: 1}") → sai

### 3. json_tim(json, path) → any
- Tim gia tri theo path
- Vi du: json_tim({"a": {"b": 1}}, "a.b") → 1

### 4. json_gan(json, path, value) → string
- Gan gia tri theo path
- Vi du: json_gan({"a": 1}, "a", 2) → {"a": 2}

## CSV

### 5. doc_csv(path) → array
- Doc CSV file thanh mang
- Vi du: doc_csv("data.csv") → [["name", "age"], ["Nam", "25"]]

### 6. ghi_csv(path, data) → string
- Ghi mang thanh CSV
- Vi du: ghi_csv("out.csv", [["a", "b"], [1, 2]])

### 7. csv_thanh_dict(data) → array
- Chuyen CSV thanh mang dict
- Vi du: csv_thanh_dict([["name", "age"], ["Nam", "25"]]) → [{"name": "Nam", "age": "25"}]

### 8. dict_thanh_csv(data) → array
- Chuyen mang dict thanh CSV
- Vi du: dict_thanh_csv([{"name": "Nam"}]) → [["name"], ["Nam"]]

## Luu y
- viet bang tieng Viet
- dung markdown format
