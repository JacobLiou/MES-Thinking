# AGENTS.md

面向 AI 编码代理的项目说明。详细架构与里程碑见 [design.md](design.md)，业务背景见 [MES调研.md](MES调研.md)。

## Project Overview

制造业执行系统（MES）第一阶段 MVP：打通 **工单 → SN 过站 → 测试结果上传 → 追溯查询** 的执行闭环。

- **用户**：产线操作员、测试工程师、质量/生产管理人员
- **设备接入**：首期通过 HTTP/REST 接收上位机/测试仪数据
- **当前阶段**：M1 执行闭环（内存仓储原型，尚未接入数据库与 Blazor 前端）
- **明确不做（第一阶段）**：APS 排产、WMS、ERP 深度集成

## Tech Stack

| 层级 | 技术 |
| --- | --- |
| 运行时 | .NET 8、C# 12 |
| API | ASP.NET Core Minimal API、Swagger |
| 前端（计划） | Blazor |
| 持久化（计划） | EF Core + SQL Server 或 PostgreSQL（二选一，不用 SQLite/MySQL） |
| 消息/缓存（计划） | RabbitMQ、Redis |
| 实时推送（计划） | SignalR |
| 可观测性（计划） | Serilog、OpenTelemetry |
| 部署目标 | Windows Server + IIS / Windows Service |

## Setup Commands

```bash
# 构建整个解决方案
dotnet build src/MES.sln

# 运行 API（开发环境，默认 https://localhost:7xxx）
dotnet run --project src/MES.Api

# 发布
dotnet publish src/MES.Api -c Release -o ./publish
```

开发时 Swagger UI 在 `Development` 环境自动启用。尚无测试项目，添加后在此补充 `dotnet test` 命令。

## Project Structure

```
src/
├── MES.sln
├── MES.Domain/          # 实体、枚举、领域规则（无外部依赖）
├── MES.Application/     # 用例编排、Contracts（Request/Response/Repository 接口）
├── MES.Infrastructure/  # EF Core、Redis、MQ 等实现（当前为 InMemory 仓储）
└── MES.Api/             # HTTP 端点、DI 注册、鉴权（待实现）
```

### 分层依赖规则

```
MES.Api → MES.Application + MES.Infrastructure
MES.Infrastructure → MES.Application + MES.Domain
MES.Application → MES.Domain
MES.Domain → （无项目引用）
```

- **Domain**：核心业务规则与状态机，不引用 Application/Infrastructure
- **Application**：`IMesExecutionService` 等用例服务；Repository 接口定义在 `Contracts/Repositories.cs`
- **Infrastructure**：实现仓储与外部集成；新增持久化实现放此层
- **Api**：仅做 HTTP 映射、鉴权、参数校验；业务逻辑不放 Api 层

### 核心领域概念

| 实体 | 说明 |
| --- | --- |
| `WorkOrder` | 工单：编号、产品、计划数量、状态 |
| `SerialUnit` | SN 实例：当前工站、过站/测试状态 |
| `TestRecord` | 测试记录：SN + 工站 + 批次，支持幂等 |
| `TraceEvent` | 追溯事件：过站、测试上传等时间线 |

状态机详见 [design.md §4.2](design.md#42-关键状态机)。

## Code Style

- 启用 `Nullable` 与 `ImplicitUsings`；公共 API 使用 `required` 属性
- 类型使用 `sealed class`；服务接口以 `I` 前缀命名（如 `IMesExecutionService`）
- 命名空间与文件夹一致：`MES.{Layer}.{Area}`
- 时间一律使用 `DateTimeOffset`，存储/比较用 UTC
- 错误码格式：`MES-{4位数字}`，与 [design.md §8.4](design.md#84-幂等与错误码) 保持一致
- 新增端点优先使用 Minimal API（`Program.cs` 或按功能拆分的 `Endpoints/` 扩展方法）
- 保持变更范围最小：不重构无关代码，不引入未请求的新依赖

### 已实现的错误码

| 代码 | 含义 |
| --- | --- |
| `MES-0000` | 成功 |
| `MES-4001` | 参数非法 |
| `MES-4002` | 业务不匹配（工单不存在、SN 未进站等） |
| `MES-4091` | 重复提交（工单重复、测试结果重复上传） |

## API Conventions

当前已实现端点：

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| POST | `/api/work-orders` | 创建工单 |
| POST | `/api/station/pass` | SN 过站 |
| POST | `/api/test-results` | 上传测试结果 |
| GET | `/api/traceability/{sn}` | SN 追溯（时间线 + 测试记录） |

扩展 API 时遵循：

- 写入接口必须支持幂等：测试结果以 `(sn, station_code, test_batch_id)` 唯一；后续支持 `Idempotency-Key` Header
- 命令响应使用 `CommandResult`（`Success`、`Code`、`Message`）
- 查询响应使用专用 DTO（如 `TraceabilityResponse`），不直接暴露 Domain 实体
- 完整契约清单见 [design.md §8](design.md#8-api-契约首期)

## Testing Instructions

尚无自动化测试。新增测试时：

- 测试项目命名：`MES.{Layer}.Tests` 或 `tests/MES.Application.Tests`
- 优先覆盖：幂等去重、SN 状态流转、工序顺序校验（M2 实现后）
- 仓储层用内存实现或 Testcontainers（接入数据库后）
- 运行：`dotnet test src/MES.sln`

## Architecture Notes

### 当前实现 vs 计划

| 能力 | 状态 |
| --- | --- |
| 工单创建、过站、测试上传、追溯查询 | ✅ 内存实现 |
| 工序顺序 / 工站权限校验 | ⏳ M2 |
| EF Core 持久化 | ⏳ 待实现 |
| JWT + RBAC | ⏳ 待实现 |
| RabbitMQ 事件发布 | ⏳ 待实现 |
| Blazor 前端 | ⏳ 待实现 |
| SPC / 看板 | ⏳ M2–M3 |

### 业务原则（来自设计）

1. **先记录、再约束、后分析** — 先采集事实，再做流程控制与质量优化
2. **先小后大** — 单线体闭环验证后再扩展
3. **设备不稳定** — 上位机本地缓存 + 服务端幂等 + 批量补传

## Security & Boundaries

- **禁止**提交凭据、连接字符串、API Key 到仓库；使用 `appsettings.Development.json`（已在 `.gitignore`）或环境变量
- **禁止**在第一阶段引入 SQLite 或 MySQL
- **禁止**在 Domain 层引用 EF Core、ASP.NET、MQ 等基础设施
- **禁止**跳过幂等校验直接写入测试记录
- 手动放行、工单状态变更、规则修改需审计日志（实现鉴权后）
- 修改数据库 Schema 必须提供可回滚的迁移脚本

## Things to Avoid

- 不要一次性实现「大而全 MES」；严格按 M1 → M2 → M3 里程碑交付
- 不要在 Api 层写业务规则；复杂校验放 Domain 或 Application
- 不要为演示方便绕过 SN 必须先过站才能上传测试结果的约束
- 不要在没有设计文档支撑时自行扩展 APS/WMS/ERP 集成
- 不要删除或弱化 `MES-4091` 重复检测逻辑

## PR & Commit Guidelines

- 提交信息：简洁中文或英文，说明「为什么」而非仅罗列文件
- PR 前：`dotnet build src/MES.sln` 必须通过
- 涉及 API 变更：同步更新 [design.md](design.md) 契约章节或在本文件 API 表中注明
- 大范围重构前先与 [design.md](design.md) 对齐，避免偏离分层架构

## Nested AGENTS.md

若在子目录新增独立服务（如 Blazor 前端、Device Gateway），在该目录放置专属 `AGENTS.md`；代理优先读取最近一层的文件。当前单体后端以本文件为准。
