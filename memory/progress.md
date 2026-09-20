# PROGRESS — VietLang

## M1 — MVP interpreter (DONE 2026-09-20)
- [x] T1: Skeleton + Token + AST + Lexer + tests + SPEC
- [x] T1.1: Fix lexer dấu trừ (số âm vỡ phép trừ)
- [x] T2: Parser
- [x] T2.1: BoolLit/NullLit
- [x] T2.2: Stmt.Dong (số dòng cho lỗi runtime)
- [x] T2.3: if-chain nhiều dòng (còn_nếu/không_thì sau xuống dòng)
- [x] T3: Interpreter + Builtins + Program.cs
- [x] T4: Demo + README
- [x] T4.1: Tài liệu khớp thực tế
- [x] T5: Verify tổng — 45/45 test, demo exit 0, 0 sentinel

## Chặng tiếp theo
- M2 (dict, exception, module) → M3 (thư viện chuẩn tiếng Việt — ưu tiên NLP) → M4 (REPL/CLI) → M5+ (100+ thư viện)