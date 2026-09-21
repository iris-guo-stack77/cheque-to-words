# BookingTimes 二面 Take-Home：支票金额转文字（Cheque Amount to Words）

## Context（背景）

Adam Clinckett（BookingTimes）在二面前发来一个小型 coding task：用 .NET 写一个程序，把支票金额（如 1234.56）转成英文大写文字（"One thousand, two hundred and thirty-four dollars and fifty-six cents."），要求有良好的 UI，并附上"测试过什么、考虑了哪些 edge case"的说明。这是 [BookingTimes 面试流程](project_bookingtimes_interview.md) 的第二轮技术关卡，一面已于 2026-09-18 完成（评分 3.1/5）。

我看了职位描述和一面录音记录，有两条关键证据决定了技术选型：

1. **JD**（`/Users/irisguo/Document/CV/tailored/2026-09-05/BookingTimes/job_ad_original.txt`）：他们的技术栈是 **ASP.NET、VB.NET、C#、T-SQL**，工作内容包含 "Develop customer-facing interfaces"——是网页方向，不是桌面方向。
2. **一面录音**（`booktimings.transcript.txt` 第 837-846 行）：面试官原话——现有系统是 VB.NET 写的，正在往 C# 迁移，"I'm hoping in the next 12 months, all of it will be in C sharp and **we'll be running on razor**"。这基本点名了他们的目标架构：**ASP.NET Core + Razor + C#**。

另外一个硬性约束：你现在用的是 **macOS**（Darwin），而 **WPF（配 MVVM）是 Windows-only**，在 Mac 上完全无法编译运行——如果选 WPF，你连本地测试都做不到，这和 JD 里强调的 "Test your work and take responsibility for it through to production" 直接冲突。所以 MVVM 桌面方案在这里不是"风格选择"，而是技术上走不通。

**结论／推荐架构：ASP.NET Core MVC（C#，Razor 视图）+ 独立的 Class Library 放核心算法 + xUnit 单元测试。**
- 完全匹配面试官亲口说的技术方向（razor + C#）
- MVC 模式清晰可讲（Controller/View/Model 分离），回应你问的"MVC 还是 MVVM"——这里选 MVC，MVVM 不适用（没有桌面 UI）
- 跨平台，`dotnet run` 在 Mac 上就能跑、能测，不依赖 Windows/Visual Studio
- 核心转换逻辑放进独立 Class Library，不依赖 Web 框架，方便写 xUnit 单元测试，也体现"good software design / readable code"（JD 原文要求）

.NET SDK 目前**未安装**（`dotnet` 命令不存在），第一步需要先装。

---

## 项目结构

**统一放在 `/Users/irisguo/Document/interview prep/booktimings/technical-test/ChequeToWords/`** ——不只是代码，这个 plan 本身（存一份副本进去）、后面做的 PPT、README、所有跟这次 take-home 有关的东西都放在这一个文件夹里，跟这轮面试的其他资料（一面录音/报告）放在同一个 `booktimings` 目录下，方便你以后找。

```
ChequeToWords/
├── ChequeToWords.sln
├── README.md                          ← 提交给 Adam 的说明文档（测试范围 + edge cases + 运行方式）
├── PLAN.md                             ← 这份 plan 的副本，存档用
├── CLAUDE.md                           ← 项目规范/coding standard（见下方 Harness 部分）
├── scripts/
│   └── verify.sh                      ← 一键跑 build + test + format check 的校验脚本
├── slides/                             ← 第 10 步做的面试展示 PPT 放这里
├── .gitignore
├── src/
│   ├── ChequeToWords.Core/            ← 纯 C# class library，无框架依赖
│   │   ├── AmountToWordsConverter.cs  ← 核心转换算法
│   │   ├── ChequeAmountValidator.cs   ← 输入校验（负数/非数字/超范围/四舍五入到分）
│   │   └── ConversionResult.cs        ← 成功/失败结果模型
│   └── ChequeToWords.Web/             ← ASP.NET Core MVC (.NET 最新 LTS)
│       ├── Controllers/HomeController.cs
│       ├── Controllers/Api/ConvertController.cs   ← 供前端即时预览调用的 JSON API
│       ├── Models/ChequeAmountViewModel.cs
│       ├── Views/Home/Index.cshtml
│       ├── Views/Shared/_Layout.cshtml
│       └── wwwroot/css/site.css, wwwroot/js/site.js
└── tests/
    └── ChequeToWords.Core.Tests/      ← xUnit
        └── AmountToWordsConverterTests.cs
```

---

## 项目治理 / Harness（Coding Standard + 自动化测试）

你提到既然是让我帮你写代码，希望有一个"简单的 harness"——coding standard + project rules + automated test，来约束/校验 AI 写出来的代码。这两个文件就是这个 harness：

**`CLAUDE.md`**（项目规范，放在项目根目录）——内容包括：
- 架构规则：`ChequeToWords.Core` 不能引用任何 ASP.NET/Web 相关包，保持框架无关，方便单元测试；业务逻辑只能放 Core，Controller 只做编排不写业务逻辑
- 金额一律用 `decimal`，禁止 `double`/`float`（避免金额浮点误差）
- 数字转文字用英式/新西兰英文的 "and" 连接规则，不是美式
- 每新增一个 edge case，必须在测试项目里补一条对应的 `[InlineData]` 测试
- AI 辅助开发的验收标准：任何代码改动，提交前必须跑一遍下面的 `verify.sh` 全绿

**`scripts/verify.sh`**（一键自动化校验脚本）：
```bash
#!/usr/bin/env bash
set -euo pipefail
echo "==> dotnet build"
dotnet build --nologo
echo "==> dotnet test"
dotnet test --nologo
echo "==> dotnet format (style check)"
dotnet format --verify-no-changes --no-restore
echo "All checks passed."
```
一条命令跑完编译、全部单元测试、代码风格检查三件事，任何一步失败就直接退出——这就是"project rules automated test"要的东西。以后不管是我改代码还是你自己改，跑一下这个脚本就知道有没有破坏已有规则/测试。

顺带会给项目 `git init` 一个本地仓库（只在这个项目文件夹内，不影响其他地方），加 `.gitignore` 排除 `bin/`、`obj/` 这些编译产物——这样你手上就有完整的改动历史，也方便万一需要展示"AI 辅助开发怎么一步步验证"的过程（对应 JD 里 "how you checked its work" 那道申请问题）。**只到本地 commit 为止，不会做任何 push 或联网操作。**

---

## 核心算法设计（精确匹配 Adam 给的格式）

拆解范例："1234.56" → "One thousand, two hundred and thirty-four dollars and fifty-six cents."
- 整数部分按 3 位一组（千/百万/十亿…），每组内部用英式 "and" 连接百位和十/个位：`234` → "two hundred **and** thirty-four"
- 组与组之间：如果低位组 ≥ 100（含百位），用**逗号**连接（"one thousand**,** two hundred and thirty-four"）；如果低位组 < 100（只有十/个位，没有百位），用 **"and"** 连接（例："1005" → "one thousand **and** five"）——这是标准英式/新西兰英文数字读法规则，我会按这个规则实现一个通用算法（支持到 thousand/million/billion 多级分组），不是只硬编码这一个例子。
- 用 `decimal` 类型而非 `double/float`，避免金额出现浮点误差。

会直接手写这个算法（不引入 Humanizer 之类的第三方 NuGet 包做 ToWords）——因为这道题本身就是在考察问题拆解能力，用现成库直接返回结果会显得没有真正解决问题；但会在 README 里提一句"生产环境可考虑 Humanizer 等成熟库"，说明我知道这个选项、只是这次题目故意自己实现。

## Edge Cases 清单（这部分直接回答你问的"客人输入负数或 0 怎么办"）

| 输入 | 处理方式 |
|---|---|
| 负数（如 -12.50）| 校验拒绝，提示"支票金额不能为负数"，不崩溃 |
| 0 / 0.00 | "Zero dollars only."（或按你确认的措辞） |
| 整数金额，无分（如 100.00）| 不读"and zero cents"，改成更符合支票书写习惯的 "One hundred dollars only."——这是我的设计决定，会在 README 里说明理由 |
| 只有分，无元（如 0.05）| "Zero dollars and five cents."，注意 05 不能读成"oh five"，要读"five" |
| 单数 1 元 / 1 分（如 1.01）| "One dollar and one cent."——单复数（dollar/dollars, cent/cents）正确处理 |
| 超过 2 位小数（如 12.345）| 四舍五入到分（MidpointRounding.AwayFromZero），并在 UI 提示"已四舍五入" |
| 十一到十九（11-19）| 用专门的 teens 数组，不能套用"ten + 位数"规则 |
| 复合数连字符（21 → twenty-one）| 正确加连字符 |
| 非数字输入 / 空输入 | 校验拒绝，不抛异常，前端给出清晰提示 |
| 带千分位逗号或 $ 符号输入（如 "$1,234.56"）| 解析前先清理符号，尽量宽容用户输入 |
| 超大数值（超过支持的最大量级，如万亿以上）| 给出"超出支持范围"的友好错误，而不是抛异常或输出乱码 |
| 前导/尾随空格 | Trim 后再解析 |

会用 xUnit 的 `[Theory]/[InlineData]` 把这些场景全部写成自动化测试用例（预计 25-30 条），跑 `dotnet test` 保证全绿。

## UI 设计（回应 "It should also have a good UI"）

- 单页卡片布局，居中，输入框带 "$" 前缀，标注 "Cheque Amount (NZD)"
- **输入即预览**：用一小段原生 JS（无需前端框架）debounce 后调用 `/api/convert`，实时把文字结果显示在下方,做成类似支票"Pay the sum of ___"那一行的视觉样式，专业但不花哨
- 服务端也做校验兜底（MVC POST action），JS 被禁用时依然可用
- 错误信息清晰展示（如"金额不能为负数"）
- "复制到剪贴板"按钮，方便直接贴到支票上
- Bootstrap 5（CDN）+ 少量自定义 CSS，响应式（手机上也能用），并加 `aria-live` 让结果对屏幕阅读器友好

---

## 实施步骤（Step by step）

1. **安装 .NET SDK**（当前机器未装）——通过 Homebrew 安装最新 LTS 版本的 .NET SDK，装完用 `dotnet --version` 确认
2. **搭建 solution 骨架**：`dotnet new sln` + 3 个项目（`classlib` 核心库、`mvc` Web 项目、`xunit` 测试项目），建立项目引用关系（Web → Core，Tests → Core）
3. **建立项目 Harness**：写 `CLAUDE.md`（coding standard/项目规则）+ `scripts/verify.sh`（build+test+format 一键校验）+ `git init` 本地仓库 + `.gitignore`，后续所有代码改动都按这个规范来、用这个脚本校验
4. **实现核心转换算法**：`AmountToWordsConverter`（数字转文字）+ `ChequeAmountValidator`（负数/非数字/超范围/四舍五入校验），使用 `decimal`
5. **写 xUnit 单元测试**，覆盖上面 edge cases 表格中的全部场景，跑 `scripts/verify.sh` 确认全绿
6. **搭建 MVC Web 项目**：`HomeController` + `Views/Home/Index.cshtml` + 一个轻量 `Controllers/Api/ConvertController`（JSON API 给前端即时预览用）
7. **实现 UI**（Bootstrap + 自定义 CSS + 一小段 JS 做实时预览、复制按钮、无障碍支持）
8. **本地跑起来验证**：`dotnet run` 启动，浏览器里手动过一遍所有 edge cases；我也会用 `curl` 直接打 API endpoint 验证行为，再跑一遍 `scripts/verify.sh` 确认没有破坏任何规则
9. **撰写 `README.md`**：项目如何运行 + 测试范围说明 + edge cases 清单（对应邮件里 Adam 要求的"a note on what you tested and what edge cases you considered"）
10. **整理成可提交的形式**（文件夹 / zip），供你确认内容无误后，再由你决定何时、以什么方式回复 Adam 的邮件——**这一步我不会自动发送邮件**，会先给你看草稿
11. **制作面试展示用 PPT**（参考你 Ara BCDE211 Assignment #3 那份 30 页 PPT 的风格：需求总览 → 每个功能点一页配真实代码截图 → 结尾测试结果+覆盖率截图），存进项目里的 `slides/` 文件夹。这一步必须在第 8 步（本地跑起来验证）完成之后做，因为要用真实的代码截图、真实跑出来的测试结果和真实 UI 截图，不能编造。用 Artifact 的 "Slides" 类型（16:9，可翻页、可下载）来做，结构大致是：
    - 封面：项目名 + 你的名字
    - 需求总览（对应 Adam 邮件里的要求）
    - 架构说明：为什么选 ASP.NET Core MVC（对应面试官原话 "we'll be running on razor"），为什么不用 WPF/MVVM（Mac 上跑不了）
    - 核心算法讲解（配代码截图）
    - 输入校验 / edge cases 处理（配代码截图 + edge cases 表格）
    - UI 实机截图（几种典型输入的效果）
    - 单元测试结果截图（`dotnet test` 全绿）
    - 结尾总结

---

## 验证方式

- `dotnet test`：xUnit 测试全绿，覆盖 edge cases 表格中的场景
- `dotnet run` 启动 Web 项目后，用浏览器手动测试范例（1234.56）能精确输出 "One thousand, two hundred and thirty-four dollars and fifty-six cents."，以及负数、0、超大数值、非数字输入等场景在 UI 上都有清晰提示，不报错、不崩溃
- 我会用 `curl` 对 `/api/convert` 做几次快速回归检查，确认 API 行为符合预期
