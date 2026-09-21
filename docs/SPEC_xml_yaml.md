# XML + YAML Library — SPEC v1.0

## XML

### 1. xml_phan_tach(text) → dict
- Phân tách XML thành dict
- Ví dụ: `xml_phan_tach("<root><name>Nam</name></root>")` → `{"root": {"name": "Nam"}}`

### 2. xml_tao(data) → string
- Tạo XML từ dict
- Ví dụ: `xml_tao({"root": {"name": "Nam"}})` → `<root><name>Nam</name></root>`

### 3. xml_tim(text, xpath) → string
- Tìm theo XPath
- Ví dụ: `xml_tim(xml, "//name")` → `"Nam"`

### 4. xml_hop_le(text) → bool
- Kiểm tra XML hợp lệ
- Ví dụ: `xml_hop_le("<root></root>")` → `đúng`

### 5. xml_thu_gon(text) → string
- Thu gọn XML (loại bỏ khoảng trắng)
- Ví dụ: `xml_thu_gon("<root>  <a>  </a>  </root>")` → `<root><a></a></root>`

## YAML

### 6. yaml_phan_tach(text) → dict
- Phân tách YAML thành dict
- Ví dụ: `yaml_phan_tach("name: Nam\nage: 25")` → `{"name": "Nam", "age": 25}`

### 7. yaml_tao(data) → string
- Tạo YAML từ dict
- Ví dụ: `yaml_tao({"name": "Nam"})` → `name: Nam`

### 8. yaml_hop_le(text) → bool
- Kiểm tra YAML hợp lệ
- Ví dụ: `yaml_hop_le("name: Nam")` → `đúng`

## Lưu ý
- Viết bằng tiếng Việt
- Dùng markdown format
