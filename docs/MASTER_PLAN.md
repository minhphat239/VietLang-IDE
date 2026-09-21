# VietLang Standard Libraries — Master Plan
# Phiên bản: v1.0 | Ngày: 21/09/2026

## TỔNG QUAN

Mục tiêu: Xây dựng hệ thống thư viện chuẩn cho VietLang, giúp ngôn ngữ có thể sử dụng trong thực tế.

## DANH MỤC THƯ VIỆN

### NHÓM 1: CORE (Đã có)
| # | Thư viện | Trạng thái | Mô tả |
|---|----------|------------|-------|
| 1 | Builtins Core | ✅ DONE | in_ra, nhap, do_dai, chuyen_chuoi, chuyen_so, them |
| 2 | Dict Methods | ✅ DONE | CRUD + iterator |
| 3 | Array Methods | ✅ DONE | loc, map, gop |
| 4 | String Methods | ✅ DONE | tim, thay, cat, chua, phan_tach |
| 5 | Exception | ✅ DONE | try/catch/finally/nem |

### NHÓM 2: TEXT PROCESSING (Cần xây)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 6 | String Plus | CAO | Upper, lower, trim, replace, split, join, contains, startswith, endswith |
| 7 | Regex | CAO | Pattern matching, search, replace, extract |
| 8 | NLP Plus | TB | Sentiment analysis, tokenization, stemming |
| 9 | Encoding | TB | Base64, URL encode, HTML encode |

### NHÓM 3: DATA STRUCTURES (Cần xây)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 10 | Array Plus | CAO | Sort, reverse, flatten, unique, chunk, zip |
| 11 | Set | TB | Union, intersection, difference |
| 12 | Queue/Stack | TB | FIFO, LIFO operations |
| 13 | Tree | THẤP | Binary tree, traverse |

### NHÓM 4: MATH (Đã có cơ bản, cần nâng cao)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 14 | Math Plus | CAO | Random, clamp, lerp, map_range |
| 15 | Statistics | TB | Mean, median, mode, std_dev |
| 16 | Linear Algebra | THẤP | Vector, matrix operations |

### NHÓM 5: FILE SYSTEM (Đã có cơ bản, cần nâng cao)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 17 | File Plus | CAO | Read/write lines, append, copy dir, walk |
| 18 | Path | CAO | Join, split, ext, basename, dirname |
| 19 | Watch | TB | File system watcher |

### NHÓM 6: DATE/TIME (Đã có cơ bản, cần nâng cao)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 20 | DateTime Plus | CAO | Parse, format, add/subtract, diff |
| 21 | Timezone | TB | Convert timezone |
| 22 | Cron | THẤP | Cron expression parser |

### NHÓM 7: NETWORK (Đã có HTTP cơ bản)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 23 | HTTP Plus | CAO | Headers, cookies, auth, upload |
| 24 | TCP/UDP | TB | Socket programming |
| 25 | WebSocket | THẤP | Real-time communication |

### NHÓM 8: DATA FORMATS (Cần xây)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 26 | JSON Plus | CAO | Pretty print, validate, schema |
| 27 | XML | TB | Parse, build, xpath |
| 28 | CSV | CAO | Read/write CSV files |
| 29 | YAML | THẤP | Parse, build |

### NHÓM 9: SYSTEM (Cần xây)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 30 | Process | CAO | Run, kill, spawn processes |
| 31 | Env | CAO | Get/set environment variables |
| 32 | OS | TB | Platform info, paths |
| 33 | Crypto | TB | Hash, encrypt, decrypt |

### NHÓM 10: DEBUGGING (Cần xây)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 34 | Logging | CAO | Log levels, file logging |
| 35 | Assert | CAO | Unit test assertions |
| 36 | Debug | TB | Breakpoints, inspect |

### NHÓM 11: CLI (Cần xây)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 37 | ArgParse | CAO | Command line argument parsing |
| 38 | Prompt | TB | Interactive prompts |
| 39 | Table | TB | Pretty print tables |

### NHÓM 12: GUI (Tương lai)
| # | Thư viện | Ưu tiên | Mô tả |
|---|----------|---------|-------|
| 40 | Window | THẤP | Create windows |
| 41 | Widget | THẤP | Buttons, labels, inputs |
| 42 | Canvas | THẤP | Drawing |

## PHÂN BỔ WORKER (20 workers)

| Worker | Thư viện | Priority | Deliverable |
|--------|----------|----------|-------------|
| W1 | String Plus | CAO | SPEC_string_plus.md |
| W2 | Regex | CAO | SPEC_regex.md |
| W3 | Array Plus | CAO | SPEC_array_plus.md |
| W4 | Math Plus | CAO | SPEC_math_plus.md |
| W5 | File Plus + Path | CAO | SPEC_file_plus.md |
| W6 | DateTime Plus | CAO | SPEC_datetime_plus.md |
| W7 | HTTP Plus | CAO | SPEC_http_plus.md |
| W8 | JSON Plus + CSV | CAO | SPEC_data_formats.md |
| W9 | Process + Env | CAO | SPEC_system.md |
| W10 | Logging + Assert | CAO | SPEC_debugging.md |
| W11 | ArgParse + Prompt | CAO | SPEC_cli.md |
| W12 | Encoding | TB | SPEC_encoding.md |
| W13 | Set + Queue/Stack | TB | SPEC_data_structures.md |
| W14 | Statistics | TB | SPEC_statistics.md |
| W15 | TCP/UDP | TB | SPEC_network.md |
| W16 | XML + YAML | TB | SPEC_xml_yaml.md |
| W17 | Timezone | TB | SPEC_timezone.md |
| W18 | Watch + OS | TB | SPEC_watch_os.md |
| W19 | Crypto | TB | SPEC_crypto.md |
| W20 | Tree + Linear Algebra | THẤP | SPEC_advanced.md |

## TIẾN ĐỘ

| Phase | Thời gian | Nội dung |
|-------|-----------|----------|
| Phase 1 | Tuần 1 | W1-W11 (11 libraries CAO) |
| Phase 2 | Tuần 2 | W12-W18 (7 libraries TB) |
| Phase 3 | Tuần 3 | W19-W20 (2 libraries THẤP) |
| Phase 4 | Tuần 4 | Test, docs, release |

## TIÊU CHUẨN ĐÁNH GIÁ

Mỗi thư viện phải có:
1. **SPEC** — Mô tả chi tiết (input/output, examples)
2. **Tests** — Ít nhất 5 test cases bằng .vl
3. **Documentation** — Cập nhật SPEC_v0_1.md
4. **Examples** — Ít nhất 2 ví dụ thực tế
