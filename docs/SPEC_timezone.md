# Timezone Library — SPEC v1.0

### 1. lay_mui_gio() → string
- Lay mui gio hien tai
- Vi du: lay_mui_gio() → "UTC+07:00"

### 2. chuyen_gio(date, from_tz, to_tz) → string
- Chuyen gio giua 2 mui gio
- Vi du: chuyen_gio("2026-09-21T10:00:00", "UTC+07:00", "UTC+00:00") → "2026-09-21T03:00:00"

### 3. lay_danh_sach_mui_gio() → array
- Lay danh sach mui gio
- Vi du: lay_danh_sach_mui_gio() → ["UTC+07:00", "UTC+08:00", ...]

### 4. hien_thi(date, timezone) → string
- Hien thi date voi mui gio
- Vi du: hien_thi(bay_gio(), "UTC+07:00") → "2026-09-21T10:00:00+07:00"

### 5. lay_thoi_gian_theo_mui_gio(timezone) → string
- Lay thoi gian theo mui gio
- Vi du: lay_thoi_gian_theo_mui_gio("UTC+00:00") → "2026-09-21T03:00:00Z"

## Luu y
- viet bang tieng Viet
- dung markdown format