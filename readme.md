# MES-Thinking

制造业执行系统（MES）第一阶段 MVP：工单 → SN 过站 → 测试上传 → 追溯查询。

## 仓库结构

```
MES-Thinking/
├── MES.sln                 # 统一解决方案
├── AGENTS.md               # AI/开发者全局约定
├── design.md               # 技术方案
├── src/
│   ├── backend/            # ASP.NET Core 分层后端
│   │   ├── MES.Domain/
│   │   ├── MES.Application/
│   │   ├── MES.Infrastructure/
│   │   └── MES.Api/
│   └── frontend/           # Blazor 工站模拟前端
│       └── MES.Web/
└── tests/
    └── MES.Domain.Tests/
```

## 快速开始

```bash
dotnet build MES.sln
dotnet run --project src/backend/MES.Api      # https://localhost:7255
dotnet run --project src/frontend/MES.Web   # https://localhost:7023
dotnet test MES.sln
```

更多说明见 [AGENTS.md](AGENTS.md)。

---

# 制造业信息化 数字化 全景图探讨
