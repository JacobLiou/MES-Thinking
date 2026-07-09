## Plan: 第二阶段执行计划（流程引擎+存储+SPC）

在不破坏现有 M1（工单-过站-测试上传-追溯）闭环的前提下，按“先持久化、再SPC、后实时看板”推进第二阶段。数据库采用 PostgreSQL，SPC范围采用“汇总 + 规则 + 告警 + 实时推送（SignalR）”。优先复用现有分层与接口约定，避免在 API 层写业务规则。

**当前实现差异（As-Is vs 第二阶段目标）**
1. 测试流程引擎：已具备基础流程定义/激活与顺序校验（Create/Get/Activate + TestFlowValidator），但缺少流程版本治理（状态迁移、发布审计）、批量导入与流程变更影响分析。
2. 数据存储：当前全部 Repository 为 InMemory，实现不具备重启持久化、事务一致性、查询索引、迁移管理。
3. SPC：后端/前端无 SPC 业务接口与页面；仅测试结果中存在 Metrics 字段，但未做聚合、规则管理、越界告警。
4. 实时能力：当前无 SignalR Hub、无后台聚合任务、无 Dashboard 实时订阅。
5. 测试覆盖：现有测试集中在流程校验与执行状态机；缺少持久化集成测试、SPC统计正确性测试、实时推送验证。

**Steps**
1. Phase 1 - 基线与边界固化
1.1 锁定第二阶段范围：测试流程引擎增强（中等）、PostgreSQL 持久化、SPC基础版（汇总+规则+告警+实时看板）。
1.2 明确不纳入：APS/WMS/ERP 深度集成、复杂统计建模（如 Cpk/Ppk 全量体系）、多租户隔离。
1.3 输出数据库与API增量清单，作为后续开发验收基线。

2. Phase 2 - PostgreSQL 持久化改造（阻塞后续步骤）
2.1 在 Infrastructure 新增 EF Core 持久化层：DbContext、EntityTypeConfiguration、迁移脚本、Repository EF 实现。*阻塞 3/4/5*
2.2 建立关键表与索引：work_orders/stations/test_flows/route_steps/serial_units/test_records/trace_events/spc_rules/spc_snapshots/alarm_events；确保 test_records(sn, station_code, test_batch_id) 唯一约束。
2.3 Program 注册切换为 PostgreSQL（Npgsql），保留 InMemory 仅用于测试或开发开关。
2.4 增加事务边界策略：执行服务中涉及“写记录+改SN状态+写追溯”的操作要求同事务提交。
2.5 引入数据库迁移与启动策略：开发环境自动迁移可选，生产环境使用显式迁移脚本。

3. Phase 3 - 测试流程引擎增强（可与 Phase 2 部分并行，依赖实体落库）
3.1 流程版本与激活治理：同产品单活跃约束落到数据库唯一条件/业务约束，补充激活历史审计事件。
3.2 流程可追溯能力：记录工单绑定流程版本，保证流程变更不影响已下发工单执行。
3.3 扩展流程查询：增加流程版本列表、启停时间、最近激活人（先预留字段，可先SYSTEM）。

4. Phase 4 - SPC 核心能力（依赖 Phase 2）
4.1 Application 增加 SPC 服务接口（ISpcService）：summary、rules CRUD、告警查询。
4.2 实现 summary 聚合：按时间窗/产品/工站统计良率、一次通过率（FPY）、关键指标均值/极差。
4.3 实现规则管理：spc_rules 的增删改查；规则字段包含 metric、LSL/USL、scope（product/station）和启用状态。
4.4 测试结果上传时接入规则判定：越界写入 alarm_events，并沉淀到 trace_events。
4.5 增加 spc_snapshots（小时或班次）以支持看板快读与历史趋势。

5. Phase 5 - API 与实时看板（依赖 Phase 4）
5.1 新增 API：GET /api/spc/summary、GET/POST/PUT/DELETE /api/spc/rules、GET /api/dashboard/realtime（初版可返回快照+告警）。
5.2 增加 SignalR Hub：在测试结果入库与告警触发后推送 summary/alarm 变更事件。
5.3 前端新增页面：SPC Summary、SPC Rules、Dashboard；在主菜单增加入口，保持现有工站工作台不回归。
5.4 MesApiClient 与 Models 增加 SPC DTO；前端支持按站点/产品/时间窗口筛选与自动刷新。

6. Phase 6 - 测试与验收（可与 Phase 5 后段并行）
6.1 单元测试：SPC 汇总计算、规则越界判定、流程激活互斥逻辑。
6.2 集成测试：基于 PostgreSQL（建议 Testcontainers）验证唯一索引、事务一致性、幂等冲突返回 MES-4091。
6.3 API 验证：补充 .http 样例与 Swagger 手工用例，覆盖全链路。
6.4 性能基线：验证 summary 查询与看板刷新延迟，记录可观测指标（请求耗时、规则命中率）。

**并行与依赖关系**
1. 必须串行：Phase 2 -> Phase 4 -> Phase 5。
2. 可并行：Phase 3 与 Phase 2后半段可并行；Phase 6 的单元测试可随 Phase 4 同步推进。
3. 建议节奏：先完成最小可用持久化和 summary，再补规则/告警，再接实时看板。

**Relevant files**
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/backend/MES.Api/Program.cs - 当前 DI、端点注册入口；将增加 EF/Npgsql、SPC端点与 SignalR 映射。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/backend/MES.Application/Contracts/Repositories.cs - 现有仓储抽象；需补充 SPC/告警相关仓储接口。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/backend/MES.Application/Services/MesExecutionService.cs - 上传测试结果主链路；将接入规则判定与告警落库。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/backend/MES.Application/Services/TestFlowService.cs - 流程创建/激活逻辑；将补版本治理与审计。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/backend/MES.Domain/Services/TestFlowValidator.cs - 核心状态与流程校验；保持规则在 Domain/Application，不下沉到 API。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/backend/MES.Infrastructure/Repositories/ - 当前 InMemory 实现目录；新增 EF 实现与保留测试替身。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/frontend/MES.Web/Services/MesApiClient.cs - 新增 SPC API 调用与实时订阅入口。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/frontend/MES.Web/Models/MesModels.cs - 新增 SPC DTO、告警 DTO、看板 DTO。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/src/frontend/MES.Web/Components/Layout/MainLayout.razor - 增加 SPC/看板菜单入口。
- c:/Users/menghl2/WorkSpace/Projects/Platform/MES-Thinking/tests/MES.Domain.Tests/ - 补充服务层与规则层测试；新增集成测试项目（建议 tests/MES.Integration.Tests）。

**Verification**
1. 构建与回归：dotnet build MES.sln + dotnet test MES.sln。
2. 迁移验证：执行 EF migration apply，重启后验证工单、流程、测试记录仍可查询（确认非内存态）。
3. 幂等验证：同一(sn, station_code, test_batch_id)重复上传返回 MES-4091。
4. SPC 准确性：构造已知样本集，校验 summary 的良率/FPY/均值与期望一致。
5. 告警与实时：触发超限指标，验证 alarm_events 入库、trace_events记录、前端 dashboard 实时收到推送。
6. 性能烟测：批量写入测试结果后，summary 与 dashboard 首屏在目标时间窗内可返回。

**Decisions**
- 数据库：PostgreSQL。
- SPC范围：选择 C（含图表看板实时推送），即在 B（汇总+规则+告警）基础上增加 dashboard/realtime + SignalR。
- 实施策略：不重构现有 M1 业务接口语义，采用增量扩展，优先保障兼容性。

**Further Considerations**
1. 统计窗口建议先固定“小时 + 班次”双模式，避免一次支持过多维度导致查询复杂度失控。
2. 实时推送建议先推“聚合结果变化事件”而非原始明细，减少网络与前端渲染压力。
3. 若后续接设备高吞吐，建议在第三阶段再引入消息队列消费聚合，第二阶段先保证数据库一致性与查询正确性。