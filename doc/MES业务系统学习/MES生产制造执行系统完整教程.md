# MES 生产制造执行系统完整教程

> **文档版本**：v1.0  
> **更新日期**：2026-11-02  
> **适用读者**：制造业 IT 工程师、MES 实施顾问、生产管理人员、系统架构师、工业软件开发者  
> **文档目标**：从概念、架构、功能、设计、实现、实施到趋势，系统化讲透 MES

---

## 目录

- [第一章 MES 概述](#第一章-mes-概述)
- [第二章 MES 在企业信息化中的定位](#第二章-mes-在企业信息化中的定位)
- [第三章 MES 核心功能模块详解](#第三章-mes-核心功能模块详解)
- [第四章 MES 系统架构设计](#第四章-mes-系统架构设计)
- [第五章 数据库与数据模型设计](#第五章-数据库与数据模型设计)
- [第六章 关键技术实现](#第六章-关键技术实现)
- [第七章 MES 与外部系统集成](#第七章-mes-与外部系统集成)
- [第八章 MES 实施方法论](#第八章-mes-实施方法论)
- [第九章 典型行业案例](#第九章-典型行业案例)
- [第十章 MES 未来发展趋势](#第十章-mes-未来发展趋势)
- [附录 A 术语表](#附录-a-术语表)
- [附录 B 常见问题](#附录-b-常见问题)
- [附录 C 推荐参考资料](#附录-c-推荐参考资料)

---

## 第一章 MES 概述

### 1.1 什么是 MES

**MES（Manufacturing Execution System，制造执行系统）** 是面向车间层的生产信息化管理系统。它位于上层的企业资源计划系统（ERP）与底层的工业控制系统（PCS/PLC）之间，是连接计划与执行、管理与控制的核心枢纽。

MES 国际权威组织 **MESA International** 给出的定义是：

> "MES 是一种战略性信息系统，它在整个工厂车间使用双向通信来提供从订单下达至成品产出的全部生产活动的信息。MES 在适当的时间将正确的信息传递给正确的人，使他们能够做出明智的决策并采取行动。"

从工程实践角度看，MES 的本质可以归纳为三点：

1. **数据采集层**：实时采集人、机、料、法、环（4M1E）数据；
2. **过程控制层**：根据计划与规则指导车间作业，约束违规操作；
3. **决策支持层**：通过统计分析与可视化，支撑车间调度、质量改进、成本核算。

### 1.2 MES 的发展历程

MES 的发展大致经历了四个阶段：

| 阶段 | 时间 | 特征 | 代表技术 |
| --- | --- | --- | --- |
| 起步期 | 1970s–1980s | 单点功能软件，如物料追踪、设备状态监控 | MRP、独立模块 |
| 形成期 | 1990s | 出现"MES"概念，开始集成化 | 关系型数据库、C/S 架构 |
| 标准化期 | 2000s | ISA-95 标准发布，功能模块明确 | J2EE、Web 化、SOA |
| 智能化期 | 2010s 至今 | 工业 4.0、工业互联网、AI 融合 | 微服务、云原生、大数据、AI |

### 1.3 为什么需要 MES

在引入 MES 之前，制造业普遍面临以下痛点：

- **计划与执行脱节**：ERP 下达的计划难以精确指导车间，车间实际进度也难以及时回传 ERP；
- **信息孤岛严重**：设备、质量、物料、人员系统各自独立，数据无法贯通；
- **在制品（WIP）管理混乱**：车间在制数量、位置、状态不清，账物不符；
- **质量追溯困难**：出现质量问题时无法快速定位批次、责任人、设备、参数；
- **设备利用率低**：停机原因不透明，OEE 难以量化；
- **交付周期长**：紧急插单响应慢，瓶颈工序不清晰。

MES 通过实时数据采集与过程管控，能够显著缓解上述问题。根据 MESA 统计，典型 MES 上线后可带来：

- 制造周期缩短 **35%–50%**
- 在制品库存降低 **20%–30%**
- 数据录入工作量减少 **60%–80%**
- 产品不良率下降 **15%–25%**
- 设备 OEE 提升 **10%–20%**

### 1.4 MES 与 ERP、SCADA、PLM 的关系

| 系统 | 关注层级 | 核心职责 | 数据特征 |
| --- | --- | --- | --- |
| ERP | 企业层 | 财务、销售、采购、计划 | 周期长、聚合度高 |
| MES | 车间层 | 生产执行、调度、追溯 | 实时、事务性、细粒度 |
| SCADA / PLC | 设备层 | 设备控制与状态采集 | 毫秒级、海量时序 |
| PLM | 全生命周期 | 产品设计与工艺管理 | 结构化、变更频繁 |
| WMS | 仓储层 | 仓库出入库、库存 | 事务性 |
| QMS | 全过程 | 质量标准与质量事件 | 流程性 |

MES 是这些系统的"信息枢纽"：从 ERP 接收生产订单，从 PLM 获取工艺路线，从 WMS 拉取物料信息，向 SCADA 下发工艺参数，回传执行结果给 ERP，向 QMS 提交质量数据。

---

## 第二章 MES 在企业信息化中的定位

### 2.1 ISA-95 企业集成标准

**ISA-95**（ANSI/ISA-95）是国际自动化学会发布的"企业与控制系统集成"国际标准，也是 MES 行业事实上的功能参考模型。它将企业信息化分为 5 层：

```
Level 4 ─ Enterprise (ERP)           企业层  ── 销售计划、生产计划、采购计划
Level 3 ─ Manufacturing Operations   制造运营层 ── MES  ← 本教程的主角
Level 2 ─ Supervisory (SCADA/HMI)    监控层  ── 设备监控、报警
Level 1 ─ Control (PLC/DCS)          控制层  ── 实时控制
Level 0 ─ Field (Sensors/Actuators)  现场层  ── 传感器、执行器
```

**MES = ISA-95 Level 3**。它的边界就是"承接 Level 4 的计划、调度 Level 2 的设备、并向上反馈执行结果"。

### 2.2 MESA 11 大功能模型

MESA 在 ISA-95 基础上提出了 MES 的 11 大功能模块（也常被合并表述为 9 大模块或 13 大模块，本教程采用经典 11 模块版本）：

1. 生产调度（Production Scheduling）
2. 生产排产（Production Dispatching）
3. 生产执行管理（Execution Management）
4. 资源分配与状态（Resource Allocation & Status）
5. 质量管理（Quality Management）
6. 维护管理（Maintenance Management）
7. 数据采集与获取（Data Collection & Acquisition）
8. 性能分析（Performance Analysis）
9. 物料管理（Material Management）
10. 人员管理（Personnel Management）
11. 文档控制（Document Control）

> 在 ISA-95 中，这些功能被进一步归纳为 **生产、质量、维护、库存** 四大运营活动。

### 2.3 信息流总览

下图（文字描述）展示了 MES 在典型企业中的信息流：

```
        ┌──────────┐  生产订单   ┌──────────┐
        │   ERP    │ ──────────▶ │          │
        └──────────┘             │          │
              ▲                  │   MES    │
   执行结果回传 │                  │          │
              │     工艺路线     │          │
        ┌──────────┐ ──────────▶ │          │
        │   PLM    │             │          │
        └──────────┘             └──────────┘
                                    │   │
              工艺参数下发/数据采集    │   │  质量事件
                                    ▼   ▼
                            ┌───────────────┐
                            │ SCADA / PLC   │
                            └───────────────┘
```

---

## 第三章 MES 核心功能模块详解

### 3.1 生产订单与排产管理

#### 3.1.1 模块职责

接收 ERP 的销售订单或生产计划，拆解为车间可执行的生产工单（Work Order），并按工序、设备、人员、时间进行排产。

#### 3.1.2 核心数据对象

- **生产订单（Production Order）**：ERP 下发的最上层生产任务，含产品、数量、计划开始/完工时间；
- **工单（Work Order）**：MES 内部按批次拆分的执行单元；
- **工序（Operation）**：工单的工艺步骤，含标准工时、设备需求、物料需求；
- **派工单（Dispatch）**：工序到具体设备/人员的实际执行单据。

#### 3.1.3 排产算法概览

| 算法 | 适用场景 | 复杂度 |
| --- | --- | --- |
| FIFO 先到先做 | 单品种大批量 | 低 |
| SPT 最短加工时间优先 | 多品种小批量 | 低 |
| EDD 最早交期优先 | 交期严格 | 低 |
| 关键比率法（CR） | 多订单插单 | 中 |
| 有限产能排产（FCS） | 设备瓶颈明显 | 高 |
| 启发式 / 遗传算法 / 强化学习 | 复杂多约束 | 高 |

工业实践中，常见做法是 **"规则 + 人工调整"**：先用规则得到初始排程，再由调度员在甘特图上拖拽微调。

### 3.2 工艺路线与 BOM 管理

#### 3.2.1 工艺路线（Routing）

工艺路线定义产品在车间内的加工路径：

```
工序10：下料   →  工序20：粗车  →  工序30：精车
                                     │
                                     ▼
                          工序50：检验  ←  工序40：热处理
```

每个工序节点记录：标准工时、设备类型、模具/工装、所需人员资质、所需物料、检验要求。

#### 3.2.2 制造 BOM（MBOM）

ERP 中的 EBOM（设计 BOM）只描述产品结构，而 MBOM 还需补充：

- 物料替代关系（主料/替代料）
- 损耗系数
- 工序级物料挂接（哪道工序用什么料、用多少）
- 虚拟件与副料处理

> **注意**：MBOM 是 MES 的数据基础。MBOM 维护混乱的工厂，MES 上线成功率极低。

### 3.3 生产执行与作业指导

#### 3.3.1 电子作业指导书（e-SOP）

MES 将工艺工程师维护的电子作业指导书推送到工位终端，工人按图作业。优点：

- 版本受控，防止旧工艺在用；
- 多媒体支持（视频、图片、动画）；
- 与当前工单/工序自动匹配，无需人工查找。

#### 3.3.2 工单过账与状态流转

工单典型状态机：

```
已下达 ─▶ 已派工 ─▶ 正在执行 ─▶ 暂停 ─▶ 已完工
                                  ▲       │
                                  └───────┘
                                  异常恢复
```

每一次状态变更都需要记录：操作人、时间、设备、班次、原因码，作为后续追溯依据。

### 3.4 物料与在制品（WIP）管理

#### 3.4.1 物料全程追踪

MES 通过条码、二维码、RFID 实现物料"一物一码"：

```
来料入库 → IQC 检验 → 上线投料 → 工序间流转 → 成品入库 → 出库
   ▲                                                    │
   └──────────── 全程批次号 / 序列号绑定 ─────────────────┘
```

#### 3.4.2 WIP 账实相符

通过车间工序报工（每完成一道工序即扣减在制、增加下道工序在制），实现车间在制库存的动态平衡。关键 KPI：

- WIP 周转率
- 工序间滞留时间
- 物料齐套率

### 3.5 质量管理

#### 3.5.1 全过程质量数据

MES 质量模块覆盖：来料检验（IQC）、首件检验（FAI）、过程检验（IPQC）、成品检验（FQC）、出货检验（OQC）。

#### 3.5.2 SPC 统计过程控制

对关键参数实时计算：

- 均值-极差控制图（Xbar-R）
- 均值-标准差控制图（Xbar-S）
- 不合格品率图（P 图）
- 缺陷数图（C 图）

当出现 **8 大判异规则** 之一时自动报警并锁定工序。

#### 3.5.3 质量追溯

按下述维度可实现"4M1E"双向追溯：

- **正向追溯**：从物料批次 → 用于哪些产品批次 → 发往哪些客户；
- **逆向追溯**：从客户投诉 → 产品序列号 → 哪条产线、哪些设备、哪些参数、谁操作、用了哪批料。

### 3.6 设备管理

#### 3.6.1 设备状态监控

实时采集设备状态：运行 / 待机 / 故障 / 调试 / 维护。

#### 3.6.2 OEE 计算

OEE（Overall Equipment Effectiveness，设备综合效率）：

```
OEE = 时间开动率 × 性能开动率 × 合格品率
    = (实际运行时间 / 计划工作时间)
    × (实际产量 / 理论产量)
    × (合格品数 / 实际产量)
```

世界级制造（WCM）标准：OEE ≥ 85%。

#### 3.6.3 TPM 集成

MES 支持设备点检、保养计划下发、故障报修、备件领用全流程闭环。

### 3.7 数据采集与设备集成

#### 3.7.1 采集方式

| 方式 | 说明 | 典型协议 |
| --- | --- | --- |
| 人工录入 | 工位终端扫码、点检表 | Web Form |
| 半自动 | PLC 信号 + 人工触发 | OPC DA/UA |
| 全自动 | 设备直连采集 | OPC UA、Modbus、MQTT |

#### 3.7.2 设备通信协议

- **OPC UA**：现代工业设备首选，含信息模型与安全机制；
- **Modbus TCP/RTU**：低成本、广泛使用；
- **Profinet / EtherCAT**：实时控制网；
- **MQTT**：物联网场景，轻量、易穿越防火墙；
- **SECS/GEM**：半导体行业专用。

### 3.8 人员管理

- 人员资质与技能矩阵（谁能在哪台设备做哪道工序）；
- 班次排班与考勤；
- 工时统计（直接工时 / 间接工时 / 加班工时）；
- 绩效计算（计件 / 计时 / KPI）。

### 3.9 文档与变更管理

- 工艺文件、作业指导书版本控制；
- 工程变更（ECN）的接收、审批、生效、废止；
- 文档下发记录留痕。

### 3.10 绩效分析

- 实时仪表盘（车间大屏）：当前工单进度、设备 OEE、合格率、报警；
- 报表：日 / 周 / 月 / 班次生产报表、质量报表、设备报表；
- 多维分析：按产品、按设备、按班组、按客户。

---

## 第四章 MES 系统架构设计

### 4.1 总体架构原则

现代 MES 架构设计遵循以下原则：

1. **分层解耦**：表现层 / 服务层 / 数据层分离；
2. **微服务化**：核心功能按业务边界拆分为独立服务；
3. **事件驱动**：跨模块通信走消息总线，降低同步耦合；
4. **平台化**：基础能力（用户、权限、流程、规则引擎）下沉为平台；
5. **云边协同**：边缘节点负责实时采集与本地控制，云端负责分析与调度；
6. **开放可扩展**：通过 API 网关对外提供能力，支持低代码扩展。

### 4.2 经典分层架构

```
┌─────────────────────────────────────────────────┐
│              表现层 (Presentation)              │
│   Web 端 / 移动端 / 车间大屏 / 工位一体机        │
└─────────────────────────────────────────────────┘
                      ▲
                      │ HTTPS / WebSocket
                      ▼
┌─────────────────────────────────────────────────┐
│            API 网关 (Gateway)                   │
│       认证 / 限流 / 路由 / 协议转换              │
└─────────────────────────────────────────────────┘
                      ▲
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
┌──────────────┐ ┌──────────┐ ┌──────────────┐
│  生产服务    │ │ 质量服务 │ │  设备服务    │ ...
│ Order/Route  │ │ IQC/SPC  │ │ OEE/TPM      │
└──────────────┘ └──────────┘ └──────────────┘
        │             │             │
        └─────────────┼─────────────┘
                      ▼
┌─────────────────────────────────────────────────┐
│           事件总线 / 消息队列 (Kafka)           │
└─────────────────────────────────────────────────┘
                      ▲
                      │
        ┌─────────────┼─────────────┐
        ▼             ▼             ▼
┌──────────────┐ ┌──────────┐ ┌──────────────┐
│ 业务数据库   │ │ 时序库   │ │  对象存储    │
│ PostgreSQL   │ │ InfluxDB │ │  MinIO/S3    │
└──────────────┘ └──────────┘ └──────────────┘
                      ▲
                      │ OPC UA / MQTT
                      ▼
┌─────────────────────────────────────────────────┐
│        边缘采集层 (Edge / IoT Gateway)          │
│        设备驱动 / 协议适配 / 数据清洗           │
└─────────────────────────────────────────────────┘
                      ▲
                      │
                      ▼
              PLC / SCADA / 设备
```

### 4.3 微服务划分建议

| 服务 | 职责 | 主要实体 |
| --- | --- | --- |
| order-service | 工单生命周期管理 | ProductionOrder, WorkOrder |
| routing-service | 工艺路线与 MBOM | Routing, Operation, MBOM |
| dispatch-service | 派工与排产 | Dispatch, Schedule |
| wip-service | 在制品与物料 | MaterialLot, WIPAccount |
| quality-service | 质量检验与 SPC | Inspection, SPCChart |
| equipment-service | 设备状态与 OEE | Equipment, StatusLog |
| maintenance-service | TPM 与故障 | WorkOrder_Maint, Spare |
| datacollect-service | 设备数据采集 | TagValue, EventLog |
| personnel-service | 人员资质与班次 | Operator, Skill, Shift |
| trace-service | 正逆向追溯 | Genealogy |
| report-service | 报表与仪表盘 | Dashboard, Report |
| integration-service | 外部系统接口 | ERP/PLM/WMS Adapter |

### 4.4 关键架构决策

#### 4.4.1 同步 vs 异步

- **同步**（REST/gRPC）：用于查询类、强一致性要求场景，如工单详情查询；
- **异步**（消息队列）：用于事件通知、跨模块解耦，如"工单完工"事件触发"质量抽样"。

#### 4.4.2 数据一致性

MES 中的数据一致性需求差异巨大：

| 场景 | 一致性要求 | 推荐方案 |
| --- | --- | --- |
| 工单状态流转 | 强一致 | 单库事务 |
| 物料扣减 | 强一致 | 分布式事务 / Saga |
| 设备状态推送 | 最终一致 | 消息队列 |
| 报表统计 | 弱一致 | 异步 ETL |

#### 4.4.3 实时性分级

- **毫秒级**：设备控制参数下发 → 边缘节点处理；
- **秒级**：工位报工、状态查询 → 业务数据库 + 缓存；
- **分钟级**：车间大屏聚合 → Redis 实时计算；
- **小时/天级**：报表分析 → 离线数据仓库。

### 4.5 部署架构

#### 4.5.1 单工厂部署

适用于单一工厂场景，所有服务部署在一套集群中。

```
工厂数据中心
   ├── 应用集群 (K8s)
   ├── 数据库集群 (PG 主从 + Redis)
   └── 边缘节点 (各车间)
```

#### 4.5.2 多工厂集团部署

采用 **"中心 + 边缘"** 模式：

```
                集团中心云
          ┌──────────────────────┐
          │  统一主数据 / 报表   │
          │  跨工厂对比分析      │
          └──────────────────────┘
                    ▲
        数据汇聚 / 主数据下发
                    │
   ┌────────────────┼────────────────┐
   ▼                ▼                ▼
工厂A MES       工厂B MES        工厂C MES
(本地自治)      (本地自治)       (本地自治)
```

---

## 第五章 数据库与数据模型设计

### 5.1 数据库选型

| 数据类型 | 推荐数据库 | 说明 |
| --- | --- | --- |
| 业务事务数据 | PostgreSQL / MySQL | 工单、报工、检验记录 |
| 时序数据 | InfluxDB / TDengine / IoTDB | 设备采集、SPC 参数 |
| 文档数据 | MongoDB | 工艺文件、作业指导 |
| 缓存 | Redis | 在线状态、限流 |
| 搜索 | Elasticsearch | 工单/物料全文搜索 |
| 对象存储 | MinIO / S3 | 图纸、视频、附件 |

### 5.2 核心实体 ER 模型

下面给出 MES 最核心的实体关系（简化版）：

```
ProductionOrder (生产订单)
    │ 1:N
    ▼
WorkOrder (工单)
    │ 1:N
    ▼
WorkOrderOperation (工单工序)
    │ N:1
    ▼
RoutingOperation (工艺工序)
    │ N:1                │ N:1
    ▼                    ▼
Routing (工艺路线)   Equipment (设备)
                        │ 1:N
                        ▼
                  EquipmentStatusLog (设备状态)
```

```
MaterialLot (物料批次)
    │ 1:N
    ▼
MaterialTransaction (物料事务)
    │
    │ N:N  (装配关系)
    ▼
WorkOrderOperation (工单工序)
```

```
WorkOrder ──1:N── InspectionRecord (检验记录)
                          │
                          │ 1:N
                          ▼
                  DefectRecord (缺陷记录)
```

### 5.3 关键表设计示例

#### 5.3.1 工单表（work_order）

```sql
CREATE TABLE work_order (
    id              BIGSERIAL PRIMARY KEY,
    order_no        VARCHAR(32) UNIQUE NOT NULL,        -- 工单号
    erp_order_no    VARCHAR(32),                        -- ERP 来源单号
    product_code    VARCHAR(64) NOT NULL,               -- 产品编码
    product_name    VARCHAR(255),
    routing_id      BIGINT NOT NULL,                    -- 工艺路线 ID
    qty_plan        NUMERIC(18,4) NOT NULL,             -- 计划数量
    qty_complete    NUMERIC(18,4) DEFAULT 0,            -- 完工数量
    qty_scrapped    NUMERIC(18,4) DEFAULT 0,            -- 报废数量
    plan_start      TIMESTAMP NOT NULL,
    plan_finish     TIMESTAMP NOT NULL,
    actual_start    TIMESTAMP,
    actual_finish   TIMESTAMP,
    status          VARCHAR(16) NOT NULL,               -- CREATED/DISPATCHED/RUNNING/PAUSED/DONE
    priority        INT DEFAULT 50,
    customer_code   VARCHAR(64),
    workshop_code   VARCHAR(32) NOT NULL,
    line_code       VARCHAR(32),
    created_at      TIMESTAMP DEFAULT NOW(),
    updated_at      TIMESTAMP DEFAULT NOW()
);
CREATE INDEX idx_wo_status ON work_order(status);
CREATE INDEX idx_wo_plan_start ON work_order(plan_start);
```

#### 5.3.2 工单工序表（work_order_operation）

```sql
CREATE TABLE work_order_operation (
    id              BIGSERIAL PRIMARY KEY,
    work_order_id   BIGINT NOT NULL REFERENCES work_order(id),
    seq             INT NOT NULL,                       -- 工序序号 10/20/30
    operation_code  VARCHAR(32) NOT NULL,
    operation_name  VARCHAR(128),
    equipment_id    BIGINT,
    standard_time   NUMERIC(10,2),                      -- 标准工时(分钟)
    qty_complete    NUMERIC(18,4) DEFAULT 0,
    qty_scrapped    NUMERIC(18,4) DEFAULT 0,
    status          VARCHAR(16) NOT NULL,
    UNIQUE(work_order_id, seq)
);
```

#### 5.3.3 报工记录表（work_transaction）

```sql
CREATE TABLE work_transaction (
    id              BIGSERIAL PRIMARY KEY,
    work_order_id   BIGINT NOT NULL,
    operation_seq   INT NOT NULL,
    operator_id     BIGINT NOT NULL,
    equipment_id    BIGINT,
    shift_code      VARCHAR(8),
    txn_type        VARCHAR(16) NOT NULL,               -- START/COMPLETE/SCRAP/REWORK
    qty             NUMERIC(18,4) NOT NULL,
    labor_time_min  NUMERIC(10,2),
    defect_code     VARCHAR(32),
    txn_time        TIMESTAMP DEFAULT NOW(),
    remark          TEXT
);
CREATE INDEX idx_wt_wo ON work_transaction(work_order_id, operation_seq);
CREATE INDEX idx_wt_time ON work_transaction(txn_time);
```

#### 5.3.4 物料批次表（material_lot）

```sql
CREATE TABLE material_lot (
    id              BIGSERIAL PRIMARY KEY,
    lot_no          VARCHAR(64) UNIQUE NOT NULL,
    material_code   VARCHAR(64) NOT NULL,
    material_name   VARCHAR(255),
    supplier_code   VARCHAR(32),
    qty             NUMERIC(18,4) NOT NULL,
    uom             VARCHAR(16),
    status          VARCHAR(16),                       -- RECEIVED/IQC/AVAILABLE/CONSUMED/SCRAPPED
    receive_time    TIMESTAMP,
    expiry_date     DATE,
    parent_lot_id   BIGINT,                            -- 父批次(分批/合批)
    location_code   VARCHAR(32)
);
```

#### 5.3.5 设备状态时序数据（TimescaleDB / InfluxDB）

```sql
-- TimescaleDB hypertable
CREATE TABLE equipment_status (
    equipment_id    BIGINT,
    status          VARCHAR(16),
    timestamp       TIMESTAMPTZ NOT NULL,
    duration_sec    INT
);
SELECT create_hypertable('equipment_status', 'timestamp');
```

### 5.4 数据治理要点

- **主数据统一**：物料、设备、工艺路线必须由主数据系统统一管理；
- **编码规范**：物料编码、设备编码、班次编码全公司唯一；
- **审计日志**：所有变更操作必须留痕，便于追溯；
- **数据归档**：历史报工、设备数据按月分区，超 1 年归档至冷存。

---

## 第六章 关键技术实现

### 6.1 设备数据采集与协议适配

#### 6.1.1 OPC UA 客户端示例（Python）

```python
from asyncua import Client

async def collect_temperature(node_id="ns=2;s=Temperature"):
    url = "opc.tcp://192.168.1.10:4840"
    async with Client(url) as client:
        node = client.get_node(node_id)
        value = await node.read_value()
        print(f"Temperature = {value}")
        return value
```

#### 6.1.2 MQTT 设备数据上报

设备端通过 MQTT 上报 JSON 报文：

```json
{
  "deviceId": "CNC-001",
  "ts": 1730419200000,
  "status": "RUNNING",
  "metrics": {
    "spindle_temp": 45.2,
    "feed_rate": 1200,
    "program_no": "O1234"
  },
  "alarms": []
}
```

边缘节点订阅主题 `mes/factory1/+/telemetry`，进行清洗后写入时序库并触发业务事件。

### 6.2 工单状态机实现

推荐使用**状态机引擎**（如 Spring StateMachine、Python-transitions），避免散落的 if-else：

```python
from transitions import Machine

class WorkOrder:
    states = ['CREATED', 'DISPATCHED', 'RUNNING', 'PAUSED', 'DONE', 'CLOSED']
    transitions = [
        {'trigger': 'dispatch', 'source': 'CREATED',   'dest': 'DISPATCHED'},
        {'trigger': 'start',    'source': 'DISPATCHED','dest': 'RUNNING'},
        {'trigger': 'pause',    'source': 'RUNNING',   'dest': 'PAUSED'},
        {'trigger': 'resume',   'source': 'PAUSED',    'dest': 'RUNNING'},
        {'trigger': 'complete', 'source': 'RUNNING',   'dest': 'DONE'},
        {'trigger': 'close',    'source': 'DONE',      'dest': 'CLOSED'},
    ]

    def __init__(self):
        self.machine = Machine(model=self, states=self.states,
                               transitions=self.transitions, initial='CREATED')
```

### 6.3 排产算法伪代码

```python
def finite_capacity_schedule(orders, resources, horizon_days=7):
    """
    简化版有限产能排产
    orders: 待排产工单列表
    resources: 设备 / 工人资源日历
    """
    schedule = []
    for order in sorted(orders, key=lambda o: (o.priority, o.plan_finish)):
        current_time = horizon_start
        for op in order.operations:
            # 找到满足资质与可用时间的资源
            candidate_res = find_qualified_resource(op.required_skill,
                                                     op.required_equipment,
                                                     from_time=current_time)
            if not candidate_res:
                order.status = 'UNSCHEDULED'
                break
            # 占用资源
            start = candidate_res.next_available_slot(current_time, op.standard_time)
            end = start + op.standard_time
            candidate_res.book(start, end)
            schedule.append((order, op, candidate_res, start, end))
            current_time = end
    return schedule
```

### 6.4 SPC 实时计算

#### 6.4.1 Xbar-R 控制图

```python
import numpy as np

def xbar_r_chart(samples, subgroup_size=5):
    """
    samples: 一维数组样本值
    返回: CL, UCL, LCL (Xbar 图与 R 图)
    """
    groups = np.array(samples).reshape(-1, subgroup_size)
    group_means = groups.mean(axis=1)
    group_ranges = groups.max(axis=1) - groups.min(axis=1)

    xbar_bar = group_means.mean()
    r_bar = group_ranges.mean()

    # n=5 时 A2=0.577, D3=0, D4=2.114
    A2, D3, D4 = 0.577, 0, 2.114

    xbar_ucl = xbar_bar + A2 * r_bar
    xbar_lcl = xbar_bar - A2 * r_bar
    r_ucl = D4 * r_bar
    r_lcl = D3 * r_bar

    return {
        'Xbar': {'CL': xbar_bar, 'UCL': xbar_ucl, 'LCL': xbar_lcl},
        'R':    {'CL': r_bar,    'UCL': r_ucl,    'LCL': r_lcl},
    }
```

#### 6.4.2 8 大判异规则

| 规则 | 描述 |
| --- | --- |
| 规则 1 | 1 点落在 3σ 控制限外 |
| 规则 2 | 连续 9 点落在中心线同一侧 |
| 规则 3 | 连续 6 点递增或递减 |
| 规则 4 | 连续 14 点交替上下 |
| 规则 5 | 连续 3 点中有 2 点落在 2σ 外（同侧） |
| 规则 6 | 连续 5 点中有 4 点落在 1σ 外（同侧） |
| 规则 7 | 连续 15 点全部落在 1σ 内 |
| 规则 8 | 连续 8 点全部落在 1σ 外 |

### 6.5 物料全程追溯

核心数据结构：**谱系表（Genealogy）**

```sql
CREATE TABLE material_genealogy (
    id              BIGSERIAL PRIMARY KEY,
    parent_lot_id   BIGINT NOT NULL,    -- 父批次
    child_lot_id    BIGINT NOT NULL,    -- 子批次
    work_order_id   BIGINT NOT NULL,    -- 发生装配的工单
    operation_seq   INT NOT NULL,
    qty_used        NUMERIC(18,4),
    txn_time        TIMESTAMP DEFAULT NOW()
);
CREATE INDEX idx_genealogy_parent ON material_genealogy(parent_lot_id);
CREATE INDEX idx_genealogy_child  ON material_genealogy(child_lot_id);
```

正逆向追溯通过 **递归 CTE** 实现：

```sql
-- 逆向追溯：从成品序列号找到所有上游批次
WITH RECURSIVE upstream AS (
    SELECT child_lot_id AS lot_id, 1 AS depth
    FROM material_genealogy
    WHERE child_lot_id = :target_lot_id
    UNION ALL
    SELECT g.parent_lot_id, u.depth + 1
    FROM material_genealogy g
    JOIN upstream u ON g.child_lot_id = u.lot_id
)
SELECT DISTINCT lot_id FROM upstream;
```

### 6.6 实时车间大屏

技术栈推荐：

- **数据层**：Redis（实时聚合）+ ClickHouse（明细查询）；
- **计算层**：Flink 流处理（实时 OEE、SPC 告警）；
- **展示层**：ECharts / Grafana / 自研 Vue 大屏。

典型指标卡：

```
┌─────────────┬─────────────┬─────────────┬─────────────┐
│  今日产量   │   合格率    │   设备 OEE   │  在制工单   │
│   1,234     │   98.7%     │    82.4%    │     56      │
└─────────────┴─────────────┴─────────────┴─────────────┘
```

### 6.7 权限与组织模型

采用 **RBAC + 数据权限**：

- **功能权限**：菜单 / 按钮 / API 接口控制；
- **数据权限**：按工厂、车间、产线、班组过滤数据；
- **字段权限**：敏感字段（成本、客户）按角色屏蔽。

```sql
-- 角色-权限
role (id, name)
permission (id, code, name)
role_permission (role_id, permission_id)

-- 用户-角色-数据范围
user_role (user_id, role_id, factory_code, workshop_code)
```

### 6.8 性能优化要点

1. **热点表分区**：报工表、设备状态表按月分区；
2. **读写分离**：报表查询走只读副本；
3. **缓存策略**：工艺路线、设备清单等静态数据进 Redis；
4. **批量写入**：设备时序数据按秒聚合后批量入库；
5. **CDN 静态资源**：作业指导书图片、视频走 CDN；
6. **数据库连接池**：HikariCP / PgBouncer。

---

## 第七章 MES 与外部系统集成

### 7.1 集成总览

```
       ┌──────┐    ┌──────┐    ┌──────┐    ┌──────┐
       │ ERP  │    │ PLM  │    │ WMS  │    │ QMS  │
       └──┬───┘    └──┬───┘    └──┬───┘    └──┬───┘
          │           │           │           │
          ▼           ▼           ▼           ▼
   ┌──────────────────────────────────────────────┐
   │           企业服务总线 ESB / API 网关          │
   └──────────────────────────────────────────────┘
                       │
                       ▼
                  ┌─────────┐
                  │   MES   │
                  └─────────┘
                       │
                       ▼
              ┌──────────────────┐
              │  OPC UA / MQTT   │
              └──────────────────┘
                       │
                       ▼
                 PLC / SCADA
```

### 7.2 ERP 集成

| 方向 | 接口 | 触发频率 |
| --- | --- | --- |
| ERP → MES | 销售订单、生产订单、采购到货通知 | 实时 / 批量 |
| MES → ERP | 工单完工回报、物料消耗、废品回报 | 实时 |
| 双向 | 物料主数据、客户主数据 | 定时同步 |

常用协议：**REST API、Web Service、IDoc（SAP）、中间表**。

#### 7.2.1 与 SAP 集成常用 BAPI

- `BAPI_PRODORD_CREATE`：创建生产订单
- `BAPI_PRODORD_RELEASE`：下达生产订单
- `BAPI_PRODORDCONF_CREATE_TT`：报工确认
- `BAPI_GOODSMVT_CREATE`：货物移动（领料、入库）

### 7.3 PLM 集成

PLM → MES 主要同步：物料 EBOM、工艺路线、工艺文件、工程变更（ECN）。

推荐方式：PLM 发布到文件服务器 + MES 通过 API 拉取元数据 + 文件 CDN 分发。

### 7.4 WMS 集成

- WMS → MES：车间叫料时返回库位与批次数；
- MES → WMS：成品完工入库、废品报废入库；
- 关键点：**物料批次号必须全链路唯一**，避免双方系统重复编号。

### 7.5 QMS 集成

- QMS 维护质量标准、检验规范、不合格品处理流程；
- MES 执行检验并回传结果；
- 不合格品走 QMS 流程闭环处理（返工/让步/报废）。

### 7.6 集成最佳实践

1. **统一接口规范**：定义企业内部 OpenAPI 规范；
2. **幂等设计**：所有接口必须支持重复调用幂等；
3. **断点续传**：批量同步支持从某个时间点重传；
4. **异常监控**：接口失败自动告警 + 人工补偿界面；
5. **数据对账**：每日定时对账，发现差异自动通知。

---

## 第八章 MES 实施方法论

### 8.1 实施阶段总览

```
准备 → 蓝图 → 开发 → 测试 → 试点 → 推广 → 运维
 4周   8周   12周   4周   8周   16周   持续
```

### 8.2 各阶段关键活动

#### 8.2.1 准备阶段

- 项目立项与组织架构建立；
- 供应商选型（自研 / 商用 / 二次开发）；
- 项目章程与里程碑确认；
- 基础网络与硬件准备。

#### 8.2.2 蓝图设计阶段

- 现状调研：访谈、流程梳理、痛点识别；
- TO-BE 流程设计；
- 差异分析（GAP Analysis）；
- 蓝图文档评审与签批。

#### 8.2.3 开发阶段

- 主数据准备（物料、BOM、工艺路线）；
- 接口开发；
- 客户化功能开发；
- 单元测试。

#### 8.2.4 测试阶段

- SIT 集成测试；
- UAT 用户验收测试；
- 性能测试与安全测试；
- 试生产演练。

#### 8.2.5 试点阶段

- 选择 1–2 条典型产线试点；
- 双轨运行（纸质 + 系统）1 个月；
- 问题修复与流程优化；
- 试点验收。

#### 8.2.6 推广阶段

- 分批推广至其他产线/车间/工厂；
- 用户培训；
- 系统切换。

### 8.3 关键成功因素

1. **一把手工程**：高层强力支持；
2. **主数据先行**：物料、BOM、工艺必须先规范；
3. **流程优化优先**：不要把混乱流程直接信息化；
4. **培训到位**：从车间工人到管理层的全员培训；
5. **数据质量 KPI**：把数据准确率纳入车间考核。

### 8.4 常见失败原因

- 主数据混乱，强行上线导致账实不符；
- 流程未优化，"垃圾进垃圾出"；
- 范围失控，需求蔓延；
- 设备集成难度被低估；
- 用户抵触，回归纸质。

### 8.5 选型建议

| 选项 | 优势 | 劣势 | 适用场景 |
| --- | --- | --- | --- |
| 国际商用（Siemens Opcenter、Rockwell PharmaSuite、GE Proficy） | 功能完整、行业最佳实践 | 价格高、本地化弱 | 大型跨国、强合规行业 |
| 国内商用（用友 U9 MES、宝信、华天、鼎捷） | 本地化好、性价比高 | 行业深度不一 | 国内中大型企业 |
| 自研 | 完全贴合业务 | 周期长、人力投入大 | 集团级、有 IT 团队 |
| 开源二次开发（如基于 Camunda、OpenMES） | 灵活、成本低 | 生态弱、需技术能力 | 创新型中小企业 |

---

## 第九章 典型行业案例

### 9.1 离散制造：汽车零部件

**场景特点**：多品种小批量、强追溯、设备多（CNC、加工中心）、节拍紧。

**MES 重点功能**：

- 工艺路线复杂，支持并行工序与外协；
- 物料按序列号追踪，支持整车厂追溯召回；
- 设备 OEE 实时监控，刀具寿命预警；
- 与 ERP 紧集成，支持看板拉料。

**关键收益**（典型客户）：

- 工单按时交付率从 78% → 94%；
- OEE 从 65% → 82%；
- 客户质量追溯响应时间从 2 天 → 10 分钟。

### 9.2 流程制造：食品饮料

**场景特点**：配方管理为主、批次追踪、保质期管理、HACCP 合规。

**MES 重点功能**：

- 配方 BOM 与版本管理；
- 批次全程追踪（原料 → 半成品 → 成品 → 出库）；
- 过程参数实时记录（温度、压力、pH）；
- 电子批记录（EBR）；
- 召回模拟。

### 9.3 半导体 / 电子

**场景特点**：超净间、设备数据海量、SECS/GEM 协议、晶圆全程跟踪。

**MES 重点功能**：

- 晶圆 Wafer ID 全程跟踪；
- 设备 SECS/GEM 集成；
- 工艺配方（Recipe）下发与校验；
- 在制品动态调度（Fab 调度）；
- 良率分析。

### 9.4 制药

**场景特点**：FDA 21 CFR Part 11 合规、电子签名、批记录、GMP。

**MES 重点功能**：

- 电子批记录（EBR）；
- 三级电子签名；
- 偏差管理；
- 称量与配料管控；
- 与 LIMS、ERP、SCADA 全集成。

---

## 第十章 MES 未来发展趋势

### 10.1 工业 4.0 与 MES

工业 4.0 的核心是 **CPS（信息物理系统）**，MES 是 CPS 在车间层的落地载体。未来 MES 将进一步：

- **数据驱动**：从"按规则执行"转向"按模型优化"；
- **自适应**：根据实时数据自动调整排产与参数；
- **自主决策**：在边界范围内由 AI 代理自动决策。

### 10.2 云原生 MES

- 多租户 SaaS 化，降低中小企业使用门槛；
- 容器化部署，弹性扩缩容；
- 微服务化，按需订阅功能模块。

### 10.3 AI + MES

| 应用场景 | AI 技术 | 价值 |
| --- | --- | --- |
| 智能排产 | 强化学习 / 遗传算法 | OEE 提升 5%–10% |
| 良率预测 | 时序预测 / LSTM | 提前预警 |
| 视觉质检 | CNN / Transformer | 替代人工目检 |
| 设备预测性维护 | 异常检测 / 生存分析 | 非计划停机下降 30%+ |
| 工艺参数优化 | 贝叶斯优化 / RL | 良率与节拍双提升 |

### 10.4 低代码 / 无代码

业务人员通过拖拽配置完成：

- 表单设计；
- 流程编排；
- 报表配置；
- 仪表盘搭建。

### 10.5 数字孪生

将物理车间在虚拟世界 1:1 映射，MES 提供实时数据驱动数字孪生体：

- 工艺仿真验证；
- 假设分析（What-if）；
- 远程诊断与培训。

### 10.6 5G + 边缘计算

- 5G 大带宽低时延支持 AGV、AR 远程协助；
- 边缘节点本地处理实时控制，云端聚合分析；
- 解决传统有线部署灵活性差的问题。

### 10.7 可持续制造

MES 将承担碳足迹核算职责：

- 能耗实时采集；
- 单位产品碳排放计算；
- 碳数据可追溯，支持 ESG 报告。

---

## 附录 A 术语表

| 缩写 | 全称 | 含义 |
| --- | --- | --- |
| MES | Manufacturing Execution System | 制造执行系统 |
| ERP | Enterprise Resource Planning | 企业资源计划 |
| PLM | Product Lifecycle Management | 产品生命周期管理 |
| WMS | Warehouse Management System | 仓库管理系统 |
| QMS | Quality Management System | 质量管理系统 |
| SCADA | Supervisory Control and Data Acquisition | 监控与数据采集 |
| PLC | Programmable Logic Controller | 可编程逻辑控制器 |
| OEE | Overall Equipment Effectiveness | 设备综合效率 |
| WIP | Work in Process | 在制品 |
| BOM | Bill of Materials | 物料清单 |
| EBOM | Engineering BOM | 设计 BOM |
| MBOM | Manufacturing BOM | 制造 BOM |
| SPC | Statistical Process Control | 统计过程控制 |
| SOP | Standard Operating Procedure | 标准作业程序 |
| ISA-95 | — | 企业与控制系统集成国际标准 |
| MESA | Manufacturing Enterprise Solutions Association | 制造执行系统解决方案协会 |
| CPS | Cyber-Physical System | 信息物理系统 |
| TPM | Total Productive Maintenance | 全员生产维护 |
| RFID | Radio Frequency Identification | 射频识别 |
| OPC UA | OPC Unified Architecture | 统一架构 OPC |
| AGV | Automated Guided Vehicle | 自动导引车 |
| EBR | Electronic Batch Record | 电子批记录 |

---

## 附录 B 常见问题

### Q1：MES 与 ERP 的边界在哪里？

**核心原则**：ERP 管"计划与账务"，MES 管"执行与现场"。  
具体边界：

- ERP 下达工单到 MES 后，工单进度、报工、报废由 MES 管理；
- 物料账务（库存增减）最终由 ERP 主导，MES 提供原始数据；
- 财务成本核算在 ERP，MES 提供工时与物料消耗明细。

### Q2：MES 一定要做设备直连吗？

不一定。可以从"人工报工 + 关键设备直连"起步，逐步扩展。但若不做设备直连，OEE 与 SPC 数据将依赖人工录入，准确性受限。

### Q3：MES 上线周期一般多长？

中型企业单工厂典型周期 6–12 个月；大型集团多工厂通常 1.5–3 年。周期取决于：

- 主数据成熟度；
- 设备集成难度；
- 流程标准化程度；
- 客户化开发量。

### Q4：自研还是采购？

| 维度 | 自研 | 采购 |
| --- | --- | --- |
| 初期成本 | 高 | 中 |
| 长期 TCO | 看维护能力 | 看订阅费与升级 |
| 贴合度 | 极高 | 中 |
| 上线速度 | 慢 | 快 |
| 灵活性 | 极高 | 受限 |

**建议**：核心差异化能力自研，通用能力采购，避免重复造轮子。

### Q5：MES 数据要保存多久？

- 业务数据（工单、报工、检验）：≥ 5 年；
- 设备时序数据：原始 3 个月，聚合数据 3 年；
- 质量追溯数据：按行业法规，制药 ≥ 10 年，汽车 ≥ 15 年；
- 审计日志：永久或 ≥ 7 年。

### Q6：MES 上线后如何衡量 ROI？

建议建立上线前基线，对比以下指标：

- 制造周期；
- WIP 周转率；
- OEE；
- 一次合格率（FTY）；
- 工时录入工作量；
- 质量追溯响应时间；
- 计划达成率。

---

## 附录 C 推荐参考资料

### 标准与白皮书

1. **ISA-95** (ANSI/ISA-95.00.01-2010)：Enterprise-Control System Integration；
2. **ISA-88**：Batch Process Control 批次控制标准；
3. **MESA White Papers**：MESA International 系列白皮书；
4. **FDA 21 CFR Part 11**：电子记录与电子签名（制药行业）；
5. **GAMP 5**：制药行业自动化系统验证。

### 经典书籍

1. 《Manufacturing Execution Systems – Optimal Design, Planning, and Deployment》— 范乃托等；
2. 《MES 制造执行系统》— 黄培 等；
3. 《ISA-95 Implementation Experiences》— ISA；
4. 《工业 4.0 实战》— 乌尔里希·森德勒；
5. 《智能制造之路》— 周济 等。

### 开源 / 工具参考

1. **Eclipse BaSyx**：工业 4.0 开源平台；
2. **Camunda**：开源工作流引擎，可用于工单流程；
3. **Apache PLC4X**：多协议 PLC 通信库；
4. **Eclipse Milo**：OPC UA Java 实现；
5. **Node-RED**：低代码边缘数据流编排。

### 行业组织

1. **MESA International**：https://www.mesa.org
2. **ISA（International Society of Automation）**：https://www.isa.org
3. **工业互联网产业联盟**：https://www.aii-alliance.org
4. **国家智能制造标准化协调组**

---

> **结语**：MES 不是一套单纯的软件，而是一种"用数据驱动车间运营"的管理理念与工程实践。它的价值不在于功能多寡，而在于能否真正打通"计划—执行—反馈"闭环。希望本教程能为您构建或选型 MES 提供完整的知识地图。
>
> 如需深入某一模块（如 SPC 算法实现、排产引擎、设备集成协议栈），可在本教程基础上进一步扩展。

---

*— 文档结束 —*
