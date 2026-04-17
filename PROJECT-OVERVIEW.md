# Tổng quan dự án UnityGameStudioAI

> Tài liệu tiếng Việt giới thiệu toàn bộ dự án, dành cho cả thành viên kỹ thuật và non-tech.
> Phiên bản tiếng Anh chi tiết: xem [README.md](README.md).

---

## Mục lục

1. [Giới thiệu dự án](#1-giới-thiệu-dự-án)
2. [Khái niệm cốt lõi: AI Agent và Skill](#2-khái-niệm-cốt-lõi-ai-agent-và-skill)
3. [Kiến trúc Agent — 3 tầng](#3-kiến-trúc-agent--3-tầng)
4. [Tổng quan 40 Skill](#4-tổng-quan-40-skill)
5. [Cài đặt — Claude Code (cho dev)](#5-cài-đặt--claude-code-cho-dev)
6. [Cài đặt — Claude Desktop (cho non-tech)](#6-cài-đặt--claude-desktop-cho-non-tech)
7. [Cài đặt — GitHub Copilot (manual)](#7-cài-đặt--github-copilot-manual)
8. [Cách sử dụng Agent và Skill](#8-cách-sử-dụng-agent-và-skill)
9. [Đẩy nhanh sản xuất game mới](#9-đẩy-nhanh-sản-xuất-game-mới)
10. [Tham chiếu & Bước tiếp theo](#10-tham-chiếu--bước-tiếp-theo)

---

## 1. Giới thiệu dự án

**UnityGameStudioAI** là một **Unity Game Studio framework** dành cho Claude Code — biến một lập trình viên/nhóm indie thành **một studio game ảo** với gần 40 chuyên gia AI, mỗi người phụ trách một mảng: thiết kế, lập trình, art, audio, QA, release…

### Con số nhanh

| Thành phần | Số lượng | Mô tả |
|---|---|---|
| **Agent** | **39** | "Nhân viên ảo" chuyên môn hoá theo vai trò |
| **Skill** | **40** | Slash command `/<tên>` chạy workflow định sẵn |
| **Hook** | 9 | Tự động chạy lúc start/stop, commit, asset change |
| **Coding rules** | 9 | Chuẩn code Unity C# theo từng path |
| **Templates** | 5+ | GDD, Pillars, ADR, Sprint plan, Level design |

### Công nghệ cốt lõi

- **Engine**: Unity LTS
- **Language**: C# 10+ (nullable enabled)
- **Render Pipeline**: URP (mặc định) / HDRP (high-end)
- **Asset Pipeline**: Addressables + Unity Asset Database
- **UI**: UI Toolkit (runtime) + UGUI (world-space)
- **Framework nội bộ**: **GameFoundation** — cung cấp `DataManager` (save/load), `GameStateManager` (state machine), `UIManager` (UI layer)

### Dành cho ai?

- Indie developer muốn có "full team" trên 1 máy.
- Team 2-5 người muốn đẩy nhanh từ ý tưởng → build.
- Người làm game mới bắt đầu, cần quy trình chuẩn và agent hướng dẫn.
- Non-tech (game designer, writer, producer) muốn phối hợp với AI qua Claude Desktop.

---

## 2. Khái niệm cốt lõi: AI Agent và Skill

### AI Agent — "Ai làm"

Mỗi **agent** là một chuyên gia ảo có:
- **Vai trò cố định** (ví dụ: `gameplay-programmer` chỉ viết C# gameplay, không động đến shader).
- **System prompt** định nghĩa chuyên môn, giới hạn, cách làm việc.
- **Tool được phép dùng** (đọc/ghi file, chạy Bash, gọi MCP…).
- **Domain ownership**: không sửa file ngoài lĩnh vực của mình.

Cơ chế: file markdown + YAML frontmatter trong [.claude/agents/](.claude/agents/).

**Ví dụ**:
- [gameplay-programmer.md](.claude/agents/gameplay-programmer.md) — implement mechanic, player system, physics.
- [unity-shader-specialist.md](.claude/agents/unity-shader-specialist.md) — chỉ làm Shader Graph, VFX Graph, URP/HDRP customization.
- [producer.md](.claude/agents/producer.md) — quản lý schedule, sprint, risk.

### Skill — "Làm gì, theo quy trình nào"

Mỗi **skill** là một **slash command** (`/<tên>`) gắn với một workflow tái sử dụng:
- Khi user gõ `/code-review`, Claude đọc file [.claude/skills/code-review/SKILL.md](.claude/skills/code-review/SKILL.md) và thực thi theo kịch bản.
- Skill có thể gọi 1 hoặc nhiều agent; có skill `team-*` điều phối cả đội.

**Ví dụ**:
- `/start` — hướng dẫn onboard dự án mới, tự route workflow.
- `/brainstorm` — lên 3 concept game kèm pillar analysis.
- `/team-combat` — điều phối game-designer + gameplay-programmer + ai-programmer + qa-tester + sound-designer cùng làm hệ combat.

### Phân biệt nhanh

| | Agent | Skill |
|---|---|---|
| Trả lời câu hỏi | **Ai làm?** | **Làm gì, theo bước nào?** |
| Gọi thế nào | `@agent-name` hoặc qua Task tool | `/skill-name` |
| Vị trí file | [.claude/agents/](.claude/agents/) | [.claude/skills/](.claude/skills/) |

---

## 3. Kiến trúc Agent — 3 tầng

Dự án phân chia agent theo **hệ thống cấp bậc studio truyền thống** (xem chi tiết ở [.claude/docs/coordination-rules.md](.claude/docs/coordination-rules.md)).

### Tier 1 — Directors (3 agent, Opus model)

Ra quyết định cao nhất, giải quyết xung đột giữa các lead.

| Agent | Trách nhiệm |
|---|---|
| `creative-director` | Tầm nhìn sáng tạo, tone, thẩm mỹ |
| `technical-director` | Kiến trúc, tech stack, performance budget |
| `producer` | Schedule, scope, phối hợp cross-department |

### Tier 2 — Department Leads (8 agent, Sonnet model)

Quản lý một mảng chuyên môn, điều phối các specialist con.

| Agent | Mảng |
|---|---|
| `game-designer` | Game design, mechanic, balance |
| `lead-programmer` | Code architecture, code review |
| `art-director` | Visual identity, art pipeline |
| `audio-director` | Audio identity, music/SFX/voice |
| `narrative-director` | Story, character, world-building |
| `qa-lead` | Test strategy, bug triage |
| `release-manager` | Release pipeline, platform submission |
| `localization-lead` | Đa ngôn ngữ, string pipeline |

### Tier 3 — Specialists (28 agent, Sonnet/Haiku model)

Thực thi cụ thể. Chia thành 5 nhóm:

**Gameplay & Design** (7):
`gameplay-programmer`, `systems-designer`, `level-designer`, `economy-designer`, `ux-designer`, `prototyper`, `live-ops-designer`

**Engine & Architecture** (9):
`engine-programmer`, `ai-programmer`, `network-programmer`, `tools-programmer`, `ui-programmer`, `performance-analyst`, `devops-engineer`, `analytics-engineer`, `security-engineer`

**Art & Audio** (3):
`technical-artist`, `sound-designer`, `world-builder`

**QA & Community** (4):
`qa-tester`, `community-manager`, `accessibility-specialist`, `writer`

**Unity Specialists** (5 — cấu trúc cha-con):
```
unity-specialist (cha)
├── unity-dots-specialist        (ECS, Jobs, Burst)
├── unity-shader-specialist      (Shader Graph, VFX Graph, URP/HDRP)
├── unity-addressables-specialist (asset management, CDN)
└── unity-ui-specialist          (UI Toolkit, UGUI, data binding)
```

### Quy tắc phối hợp

- **Giao việc dọc**: Director → Lead → Specialist.
- **Tham vấn ngang**: cùng cấp có thể hỏi nhau, nhưng không quyết định chéo domain.
- **Escalate**: xung đột chuyển lên director cha chung.
- **Không xâm lấn**: agent không sửa file ngoài domain của mình trừ khi được uỷ quyền rõ ràng.

---

## 4. Tổng quan 40 Skill

Tất cả skill gọi bằng slash command — gõ `/` trong Claude Code để xem danh sách.

### Onboarding & Setup (3)
| Skill | Mô tả |
|---|---|
| `/start` | Hướng dẫn onboard; tự phát hiện giai đoạn dự án và route workflow |
| `/new-project` | Scaffold Unity project mới với GameFoundation pre-integrated |
| `/setup-engine` | Cấu hình Unity version, tạo `VERSION.md`, validate project settings |

### Analysis & Reviews (8)
| Skill | Mô tả |
|---|---|
| `/design-review` | Review GDD/feature spec theo pillar + scope + constraint |
| `/code-review` | Review C# theo SOLID, performance, Unity best practice |
| `/balance-check` | Review balance (combat/economy/progression/difficulty) |
| `/asset-audit` | Audit asset: naming, size budget, import settings |
| `/scope-check` | Review scope vs schedule + team capacity |
| `/perf-profile` | Profile performance, đề xuất optimization |
| `/tech-debt` | Audit code smell, architectural violation, missing test |
| `/project-stage-detect` | Phát hiện giai đoạn dự án hiện tại |

### Documentation (4)
| Skill | Mô tả |
|---|---|
| `/reverse-document` | Sinh doc từ code/asset có sẵn |
| `/architecture-decision` | Tạo ADR (Architecture Decision Record) |
| `/design-system` | Tạo/review design document cho 1 system hoàn chỉnh |
| `/map-systems` | Sinh systems map: dependency, data flow, agent ownership |

### Creative (3)
| Skill | Mô tả |
|---|---|
| `/brainstorm` | Lên 3 ý tưởng game kèm pillar analysis |
| `/playtest-report` | Viết playtest report từ quan sát session |
| `/prototype` | Setup prototype Unity để test mechanic |

### Sprint & Estimation (4)
| Skill | Mô tả |
|---|---|
| `/sprint-plan` | Lên sprint plan từ backlog + velocity |
| `/estimate` | Estimate thời gian cho task list |
| `/retrospective` | Chạy retrospective sau sprint |
| `/milestone-review` | Review milestone, đưa go/no-go |

### Release & Launch (5)
| Skill | Mô tả |
|---|---|
| `/release-checklist` | Checklist release cụ thể theo platform |
| `/launch-checklist` | Checklist launch-day toàn studio |
| `/hotfix` | Quy trình hotfix khẩn cấp |
| `/patch-notes` | Viết patch note cho player từ technical fix |
| `/changelog` | Sinh changelog từ git commit |

### Production Management (2)
| Skill | Mô tả |
|---|---|
| `/bug-report` | Tạo bug report chuẩn |
| `/onboard` | Onboard thành viên mới (người hoặc AI agent) |

### Localization (1)
| Skill | Mô tả |
|---|---|
| `/localize` | Chuẩn bị feature/màn hình sẵn sàng dịch đa ngôn ngữ |

### Team Orchestration (7)
Khởi chạy cả đội làm song song 1 mảng lớn.

| Skill | Đội được điều phối |
|---|---|
| `/team-combat` | game-designer, gameplay-programmer, ai-programmer, unity-specialist, qa-tester, sound-designer |
| `/team-ui` | ux-designer, unity-ui-specialist, ui-programmer, localization-lead, accessibility-specialist |
| `/team-narrative` | narrative-director, writer, sound-designer, localization-lead |
| `/team-level` | level-designer, world-builder, ai-programmer, sound-designer, performance-analyst, qa-tester |
| `/team-audio` | audio-director, sound-designer |
| `/team-polish` | Game feel, visual, audio, UX, performance polish |
| `/team-release` | qa-lead, release-manager, devops-engineer, community-manager |

### Unity-specific & Gate (3)
| Skill | Mô tả |
|---|---|
| `/unity-local-controller` | Phát hiện/validate Unity Editor cài trên máy |
| `/unity-mcp` | Setup Unity MCP (com.unity.ai.assistant) |
| `/gate-check` | Gate check milestone (Pre-Alpha → Alpha → Beta → Gold) |

---

## 5. Cài đặt — Claude Code (cho dev)

Đây là cách **chính thức và đầy đủ tính năng nhất**. Claude Code tự động load toàn bộ agent/skill/hook.

### Bước 1 — Cài Claude Code
```bash
npm install -g @anthropic-ai/claude-code
```

### Bước 2 — Clone repo
```bash
git clone https://github.com/duclong2301/UnityGameStudio.git
cd UnityGameStudio
```
*(hoặc dùng repo này làm **Template** trên GitHub rồi clone bản của bạn)*

### Bước 3 — Chạy Claude Code
```bash
claude
```

### Bước 4 — Tự động load
Claude Code tự đọc:
- [CLAUDE.md](CLAUDE.md) → master config
- [.claude/agents/](.claude/agents/) → 39 agent
- [.claude/skills/](.claude/skills/) → 40 skill
- [.claude/hooks/](.claude/hooks/) → automation hook
- [.claude/settings.json](.claude/settings.json) → permission + hook config
- [.claude/rules/](.claude/rules/) → coding rules path-scoped

### Bước 5 — Kiểm tra
- Gõ `/` → hiện danh sách skill (40 cái).
- Gõ `@` → hiện danh sách agent (39 cái).
- Chạy thử `/start` → Claude phát hiện giai đoạn dự án và hướng dẫn tiếp.

---

## 6. Cài đặt — Claude Desktop (cho non-tech)

**Dành cho**: game designer, writer, producer, artist — không cần biết terminal/CLI.

**Lưu ý giới hạn**: Claude Desktop **không chạy được hook**, **không ghi file filesystem**, **không dùng được tool Bash**. Chỉ dùng để **brainstorm, viết tài liệu, review text** — phù hợp cho công việc sáng tạo và lên ý tưởng. Muốn tự động hoá thực sự (scaffold code, commit git…) phải dùng Claude Code.

### Bước 1 — Tải Claude Desktop
1. Vào [claude.ai/download](https://claude.ai/download).
2. Chọn bản Windows hoặc Mac, tải về và cài như app thường.
3. Mở app → đăng nhập bằng tài khoản Claude.

### Bước 2 — Tạo "Project" cho 1 skill
Trong Claude Desktop, mỗi skill cài đặt như một **Project** riêng.

1. Trong repo trên máy (nếu không có, nhờ đồng nghiệp zip gửi), mở thư mục `.claude/skills/<tên-skill>/`.
   - Ví dụ: muốn cài skill brainstorm → vào `.claude/skills/brainstorm/`.
2. Mở file `SKILL.md` bằng Notepad / TextEdit / VS Code.
3. **Copy toàn bộ nội dung** (Ctrl+A, Ctrl+C).
4. Trong Claude Desktop, click **"New Project"** (hoặc "+ Projects" ở sidebar trái).
5. Đặt tên project theo skill: ví dụ **"Brainstorm Game Ideas"**.
6. Trong phần **"Custom instructions"** (hướng dẫn tuỳ chỉnh) của project → **dán nội dung SKILL.md vào** (Ctrl+V).
7. Nhấn **Save**.

### Bước 3 — Upload file tham chiếu
Để Claude hiểu ngữ cảnh dự án:

1. Trong project vừa tạo, kéo-thả (drag & drop) các file sau vào phần **"Project knowledge"**:
   - `CLAUDE.md` (master config)
   - `design/pillars.md` (nếu đã có — core design pillars)
   - Các file trong `.claude/docs/` (coordination, coding standards, …)
   - File GDD hiện tại nếu có

2. Claude Desktop cho phép lên tới ~200MB knowledge per project — thoải mái add file thiết kế, concept art, dialogue draft…

### Bước 4 — Chat như bình thường

Không cần gõ lệnh `/`. Chỉ cần viết bằng tiếng Việt tự nhiên:

> "Giúp tôi brainstorm 3 ý tưởng game puzzle 2D cho mobile, focus vào cơ chế physics-based."

Claude sẽ tự theo skill đã cài → hỏi ngược lại pillar, constraint, target audience → đưa ra 3 concept kèm pillar analysis.

### Bước 5 — Gọi "Agent" đơn lẻ

Nếu muốn triệu tập 1 chuyên gia cụ thể (ví dụ `game-designer`):

1. Mở file `.claude/agents/game-designer.md`.
2. Copy toàn bộ nội dung.
3. Trong Claude Desktop, bắt đầu chat mới → **dán nội dung vào tin nhắn đầu tiên** kèm câu hỏi:
   > *[dán nội dung agent vào đây]*
   >
   > Tôi cần bạn review tài liệu combat system dưới đây:
   > [đính kèm file GDD]

Claude sẽ đóng vai `game-designer` và review theo chuyên môn của role đó.

### Mẹo nhỏ cho non-tech

- **Pin project**: pin những project hay dùng (`brainstorm`, `design-review`, `playtest-report`) lên đầu sidebar.
- **Tên project rõ ràng**: đặt tên tiếng Việt cho dễ tìm — "Lên Ý Tưởng", "Review GDD", "Viết Patch Notes".
- **Tách project theo game**: mỗi project game nên có knowledge riêng, không trộn với game khác để tránh Claude nhầm ngữ cảnh.
- **Cập nhật knowledge**: khi GDD/pillar thay đổi → xoá file cũ trong Project knowledge, upload file mới.

---

## 7. Cài đặt — GitHub Copilot (manual)

GitHub Copilot **không có khái niệm agent/skill** và không hiểu workflow đa bước. Nhưng có thể tận dụng một phần.

### Cách 1 — Custom Instructions (VS Code)
1. Mở VS Code → Settings (Ctrl+,).
2. Tìm **"GitHub Copilot: Instructions"**.
3. Copy nội dung [.claude/docs/coding-standards.md](.claude/docs/coding-standards.md) + [CLAUDE.md](CLAUDE.md) → dán vào.
4. Copilot sẽ dùng làm context cho mọi gợi ý code.

### Cách 2 — File `.github/copilot-instructions.md`
1. Tạo file `.github/copilot-instructions.md` ở gốc repo.
2. Viết tóm tắt:
   - Naming convention, SOLID/KISS/DRY rules.
   - Mobile performance budget nếu là mobile game.
   - Pillar thiết kế (copy từ `design/pillars.md`).
3. Copilot auto detect file này trên workspace hiện tại.

### Cách 3 — Dùng skill thủ công trong Copilot Chat
Khi cần chạy workflow lớn (ví dụ sprint plan):
1. Mở `.claude/skills/sprint-plan/SKILL.md`.
2. Copy nội dung → dán vào Copilot Chat kèm context dự án.
3. Copilot sẽ thử chạy theo quy trình, nhưng kém ổn định hơn Claude do không có agent hierarchy.

### Hạn chế
- Không có multi-step team orchestration (`/team-*` không chạy được).
- Không có hook tự động.
- Không hiểu concept director/lead/specialist.

**Kết luận**: Copilot tốt cho **code completion hằng ngày trong IDE**. Workflow lớn (brainstorm, sprint-plan, team-*) nên dùng Claude Code / Claude Desktop.

---

## 8. Cách sử dụng Agent và Skill

Ví dụ workflow điển hình trên **Claude Code**:

### Tạo dự án Unity mới
```
> /new-project
```
Claude hỏi: tên project, platform (PC/mobile/console), genre → tự scaffold Unity project với GameFoundation tích hợp sẵn, tạo 3 scene cơ bản (LoadingScene, MainScene, GameplayScene).

### Lên ý tưởng ban đầu
```
> /brainstorm
```
Claude hỏi: genre preference, target audience, platform constraint → đưa ra 3 concept game, mỗi concept có pillar + unique mechanic + rủi ro sản xuất.

### Review code
```
> /code-review src/Gameplay/PlayerController.cs
```
`lead-programmer` + `unity-specialist` cùng review: SOLID, performance, Unity API usage, rồi đưa ra patch đề xuất.

### Điều phối cả đội làm combat
```
> /team-combat
```
1. `game-designer` viết combat spec dựa vào pillar.
2. `gameplay-programmer` implement core combat loop.
3. `ai-programmer` làm enemy AI behavior tree.
4. `sound-designer` design SFX hit/attack.
5. `qa-tester` viết test plan.
Tất cả chạy **song song**, kết quả tổng hợp lại.

### Gọi agent đơn lẻ
```
> @gameplay-programmer viết script dash cho player với cooldown 1.5s
```
Chỉ `gameplay-programmer` nhận việc, không gọi cả đội.

### Quy trình chuẩn (Collaboration Protocol)

Mọi agent tuân thủ: **Question → Options → Decision → Draft → Approval**

1. **Question**: agent hỏi rõ yêu cầu, ràng buộc.
2. **Options**: đưa ra 2-3 phương án với trade-off.
3. **Decision**: user chọn phương án.
4. **Draft**: agent viết draft/diff.
5. **Approval**: agent hỏi *"May I write this to [filepath]?"* → chỉ ghi file khi user duyệt.

Không tự ghi file, không auto commit, không quyết định thay user. Chi tiết: [.claude/docs/collaboration-protocol.md](.claude/docs/collaboration-protocol.md).

---

## 9. Đẩy nhanh sản xuất game mới

So sánh workflow truyền thống vs với UnityGameStudioAI:

| Giai đoạn | Truyền thống | Với UnityGameStudioAI |
|---|---|---|
| Ý tưởng → GDD | 1-2 tuần họp + viết tay | 1 buổi (`/brainstorm` → `/design-system`) |
| Scaffold Unity project | 1-2 ngày setup package, folder, build profile | 15 phút (`/new-project` + GameFoundation có sẵn) |
| Sprint plan | 0.5-1 ngày họp planning | 1 giờ (`/sprint-plan` sinh từ backlog + velocity) |
| Combat system | 2-3 tuần cho 1 lập trình viên | 3-5 ngày (`/team-combat` điều phối 5 agent song song) |
| Code review | 2-4 giờ/PR, phụ thuộc senior | 15-30 phút (`/code-review` chạy `lead-programmer` + `unity-specialist`) |
| Level đầu tiên | 1-2 tuần | 2-3 ngày (`/team-level` điều phối designer + builder + AI + audio) |
| Release checklist | 1 ngày kiểm thủ công, dễ sót | 1 giờ (`/release-checklist` + `/launch-checklist` platform-specific) |

### Đòn bẩy chính

1. **Parallel orchestration** — skill `/team-*` khởi chạy 5-6 agent cùng lúc, mỗi agent tập trung 1 mảng, kết quả tổng hợp cuối cùng.
2. **Framework GameFoundation có sẵn** — `DataManager`, `GameStateManager`, `UIManager` cover 80% boilerplate của mọi game mới.
3. **Path-scoped coding rules** — agent tự tuân thủ chuẩn theo từng folder (gameplay/, engine/, UI/, shaders/…) → giảm review manual.
4. **Hooks tự động** — gap detection, commit validation, asset audit, session state recovery chạy ngầm.
5. **Templates sẵn có** — GDD, Pillars, ADR, Sprint plan, Level design — không phải viết từ đầu.
6. **Session recovery** — bỏ dở giữa chừng, chạy lại `claude` → đọc `production/session-state/active.md` tiếp tục đúng chỗ.
7. **Collaboration Protocol** — agent luôn hỏi trước khi ghi → không "rogue commit", không phá code.

### Ước lượng thực tế

Một indie team 1-2 người dùng UnityGameStudioAI có thể rút ngắn:
- **Prototype đầu tiên**: từ 4-6 tuần xuống **1-2 tuần**.
- **Vertical slice**: từ 3-6 tháng xuống **6-10 tuần**.
- **Time-to-market** tổng: giảm **30-50%** tuỳ scope.

*Con số dựa trên giả định team đã quen Unity + Claude Code. Learning curve ban đầu ~1 tuần.*

---

## 10. Tham chiếu & Bước tiếp theo

### Checklist bắt đầu

- [ ] Đọc [CLAUDE.md](CLAUDE.md) — master config.
- [ ] Đọc [.claude/docs/collaboration-protocol.md](.claude/docs/collaboration-protocol.md) — cách agent tương tác.
- [ ] Đọc [.claude/docs/frameworks/gamefoundation/README.md](.claude/docs/frameworks/gamefoundation/README.md) — framework cốt lõi.
- [ ] Cài Claude Code (dev) hoặc Claude Desktop (non-tech) theo mục 5-6.
- [ ] Chạy `/start` lần đầu — Claude tự phát hiện giai đoạn và route workflow.

### Tài liệu nội bộ

| File | Nội dung |
|---|---|
| [README.md](README.md) | Bản tiếng Anh chi tiết kỹ thuật |
| [CLAUDE.md](CLAUDE.md) | Master config cho Claude Code |
| [.claude/docs/coordination-rules.md](.claude/docs/coordination-rules.md) | Quy tắc phối hợp 3 tầng agent |
| [.claude/docs/coding-standards.md](.claude/docs/coding-standards.md) | Chuẩn code Unity C# |
| [.claude/docs/technical-preferences.md](.claude/docs/technical-preferences.md) | Stack khuyến nghị |
| [.claude/docs/directory-structure.md](.claude/docs/directory-structure.md) | Cây thư mục dự án |
| [.claude/docs/context-management.md](.claude/docs/context-management.md) | Quản lý context window |
| [.claude/docs/collaboration-protocol.md](.claude/docs/collaboration-protocol.md) | Quy trình User ↔ Agent |

### Liên kết ngoài

- [Claude Code Docs](https://docs.anthropic.com/en/docs/claude-code)
- [Claude Desktop Download](https://claude.ai/download)
- [Unity LTS Releases](https://unity.com/releases/lts)
- [GameFoundation Repo](https://github.com/duclong2301/UnityGameStudio) (repo gốc)

### Hỏi gì tiếp theo?

Nếu còn thắc mắc:
- **Bắt đầu từ đâu?** → chạy `/start` trong Claude Code.
- **Dự án đang dở?** → chạy `/project-stage-detect`.
- **Muốn prototype nhanh?** → chạy `/prototype`.
- **Review 1 mảng cụ thể?** → chạy `/design-review`, `/code-review`, hoặc `/asset-audit`.

---

*Cập nhật lần cuối: 2026-04-16 · Duy trì bởi Unity Game Studio Team*
