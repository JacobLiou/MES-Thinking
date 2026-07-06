# backend — MES 后端

.NET 8 分层架构：Domain → Application → Infrastructure → Api。

## 项目

| 项目 | 职责 |
| --- | --- |
| `MES.Domain` | 实体、枚举、领域规则（`TestFlowValidator` 等） |
| `MES.Application` | 用例服务、Contracts（Request/Response/Repository 接口） |
| `MES.Infrastructure` | InMemory 仓储、种子数据 |
| `MES.Api` | Minimal API 端点、DI、Swagger |

## 依赖方向

```
MES.Api → MES.Application + MES.Infrastructure
MES.Infrastructure → MES.Application + MES.Domain
MES.Application → MES.Domain
MES.Domain → （无引用）
```

## 命令

```bash
# 从仓库根目录执行
dotnet build MES.sln
dotnet run --project src/backend/MES.Api
dotnet test MES.sln
```

API 默认：`https://localhost:7255`（Swagger 在 Development 环境启用）。

## 约定

- 业务逻辑不放 Api 层；Repository 接口定义在 `MES.Application/Contracts/Repositories.cs`
- 错误码格式 `MES-{4位数字}`，见根目录 [AGENTS.md](../../AGENTS.md)
