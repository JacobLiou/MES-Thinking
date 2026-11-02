# MES 业务 UML 建模

以 UML（类图 / 状态机 / 时序图 / 活动图）为工具，对 MES（制造执行系统）核心业务进行系统性建模的知识库。

本目录回答「业务是什么」，与仓库中其他文档互补：

| 文档 | 视角 |
| --- | --- |
| 本目录 | 业务建模视角：领域对象、状态机、事件、追溯 |
| [`design.md`](../../design.md) | 实现视角：技术方案、API 契约、里程碑 |
| [`MES调研.md`](../../MES调研.md) | 调研视角：MES 5W2H、能力优先级 |
| [`ISA-95学习指南.md`](../ISA-95学习指南.md) | 标准视角：L0–L4 层级模型 |
| [`从MIMS到MES-光通信制造信息化战略思考.md`](../从MIMS到MES-光通信制造信息化战略思考.md) | 战略视角 |

## 建模框架：MES 六问模型

所有内容按「六问」组织（详见 [00.2_MES六问模型](00_MES业务建模总纲/00.2_MES六问模型.md)）：

| # | 问题 | 追问 | 目录 |
| --- | --- | --- | --- |
| 00 | 总纲 | 建模方法与全景地图 | [00_MES业务建模总纲](00_MES业务建模总纲/) |
| 01 | What | 应该生产什么？（产品 / BOM / 规格） | [01_What_应该生产什么](01_What_应该生产什么/) |
| 02 | WhatToDo | 应该执行什么？（订单 / 工艺路线 / 工序） | [02_WhatToDo_应该执行什么](02_WhatToDo_应该执行什么/) |
| 03 | WhoWhereWithWhat | 谁在哪里用什么生产？（组织 / 资源 / 5M1E） | [03_WhoWhereWithWhat_谁在哪里用什么生产](03_WhoWhereWithWhat_谁在哪里用什么生产/) |
| 04 | WhatIsHappening | 现在正在发生什么？（WIP / SN / 工序执行） | [04_WhatIsHappening_现在正在发生什么](04_WhatIsHappening_现在正在发生什么/) |
| 05 | WhatActuallyHappened | 实际发生了什么？（事件 / 实测 / 质量异常） | [05_WhatActuallyHappened_实际发生了什么](05_WhatActuallyHappened_实际发生了什么/) |
| 06 | CanWeProveIt | 能否证明？（追溯 / 谱系 / 证据链） | [06_CanWeProveIt_能否证明](06_CanWeProveIt_能否证明/) |
| 07 | 综合案例 | 用一个产品的一生串联六问 | [07_综合案例](07_综合案例/) |
| 08 | 业务全景 | 全局领域模型 / 端到端流程 / 方法论 | [08_MES业务全景](08_MES业务全景/) |

## 目录导航

```text
MES业务UML建模/
├── README.md                          # 本文件
├── 00_MES业务建模总纲/
│   ├── 00.1_为什么要对MES业务建模.md
│   ├── 00.2_MES六问模型.md
│   ├── 00.3_产品生命周期模型.md
│   └── 00.4_MES核心业务对象地图.md
├── 01_What_应该生产什么/
│   ├── 01.1_产品Product模型.md
│   ├── 01.2_BOM与物料模型.md
│   └── 01.3_产品定义UML类图.md
├── 02_WhatToDo_应该执行什么/
│   ├── 02.1_生产订单模型.md
│   ├── 02.2_Routing与Operation模型.md
│   └── 02.3_产品制造流程UML活动图.md
├── 03_WhoWhereWithWhat_谁在哪里用什么生产/
│   ├── 03.1_制造组织与物理工厂模型.md
│   ├── 03.2_人员与技能模型.md
│   ├── 03.3_设备工具物料资源模型.md
│   └── 03.4_5M1E制造上下文模型.md
├── 04_WhatIsHappening_现在正在发生什么/
│   ├── 04.1_WIP业务模型.md
│   ├── 04.2_SN生命周期状态机.md
│   ├── 04.3_Operation执行状态机.md
│   └── 04.4_MoveIn到MoveOut时序图.md
├── 05_WhatActuallyHappened_实际发生了什么/
│   ├── 05.1_ManufacturingEvent制造事件.md
│   ├── 05.2_Expected与Actual模型.md
│   ├── 05.3_质量与异常业务模型.md
│   └── 05.4_Hold_Rework_Retest_Scrap状态模型.md
├── 06_CanWeProveIt_能否证明/
│   ├── 06.1_Traceability业务模型.md
│   ├── 06.2_Genealogy谱系模型.md
│   ├── 06.3_正向与反向追溯.md
│   └── 06.4_制造证据链模型.md
├── 07_综合案例/
│   ├── 07.1_一个产品的一生.md
│   ├── 07.2_VOA产品生命周期业务建模.md
│   └── 07.3_异常与质量案例.md
└── 08_MES业务全景/
    ├── 08.1_MES核心Domain_Model.md
    ├── 08.2_MES端到端业务活动图.md
    ├── 08.3_MES全局状态模型.md
    └── 08.4_MES业务建模方法论.md
```

## 建议阅读路径

- **自顶向下（建模者）**：00 总纲 → 08.4 方法论 → 按六问顺序 01→06 → 07 案例串联 → 08 全景收束
- **自底向上（实现者）**：先读 [`design.md`](../../design.md) 核心实体，再从 00.4 对象地图找到对应建模篇目深入
- **快速理解业务**：直接读 07.1 一个产品的一生，再按需跳转对应章节

## 文档约定

- **图表**：统一使用 Mermaid（`classDiagram` / `stateDiagram-v2` / `sequenceDiagram` / `flowchart`）
- **每篇结构**：本篇要回答的业务问题 → 章节骨架 → UML 图表 → 与系统实现的映射 → 关联文档
- **状态标记**：`⏳ 骨架（待填充）` → `✍️ 撰写中` → `✅ 已完成`
- **术语对齐**：与 `AGENTS.md` / `design.md` 保持一致（WorkOrder、Station、TestFlow、RouteStep、SerialUnit、TestRecord、TraceEvent）
- **业务原则**：遵循 design.md 的三条原则 —— 先记录、再约束、后分析；先小后大；设备不稳定假设

## 与代码实现的对照入口

| 建模篇目 | 实现参照 |
| --- | --- |
| 02.1 工单模型 | `WorkOrder` + `POST /api/work-orders` |
| 02.2 Routing/Operation | `TestFlow` / `RouteStep` + `/api/test-flows` |
| 03.1 物理工厂 | `Station` + `/api/stations` |
| 04.1–04.4 WIP/状态机/过站 | `SerialUnit` + `/api/station/pass` |
| 05.1–05.2 事件/实测 | `TestRecord` + `/api/test-results`（幂等键 `sn+station_code+test_batch_id`） |
| 06.x 追溯 | `TraceEvent` + `GET /api/traceability/{sn}` |
