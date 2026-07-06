# frontend — MES Blazor 工站模拟前端

.NET 8 Blazor Web App（Interactive Server）+ BootstrapBlazor，通过 HTTP 调用 `MES.Api`。

## 启动

```bash
# 终端 1：后端 API（必须先启动）
dotnet run --project src/MES.Api

# 终端 2：前端
dotnet run --project frontend/MES.Web
```

默认地址：

| 服务 | HTTPS | HTTP |
| --- | --- | --- |
| MES.Api | https://localhost:7255 | http://localhost:5269 |
| MES.Web | https://localhost:7023 | http://localhost:5153 |

## 配置

[`MES.Web/appsettings.Development.json`](MES.Web/appsettings.Development.json)：

```json
{
  "MesApi": {
    "BaseUrl": "https://localhost:7255"
  }
}
```

API 开发环境 CORS 允许来源见 [`src/MES.Api/appsettings.Development.json`](../src/MES.Api/appsettings.Development.json)。

## 页面

| 路由 | 说明 |
| --- | --- |
| `/` | 首页：API 健康检查、演示说明 |
| `/station` | **工站工作台**：选工站、扫码过站、模拟 PASS/FAIL |
| `/work-orders` | 创建工单 |
| `/traceability` | SN 追溯查询 |

## 演示流程

1. 创建工单 `WO-DEMO-001` / `PCB-A`
2. 工站工作台选 `ST-ASSY-01`，SN 过站
3. 选 `ST-ICT-01`：过站 → 模拟 PASS
4. 选 `ST-FCT-01`：过站 → 模拟 PASS → SN 状态 `Done`

## 技术栈

- Blazor Web App (.NET 8, Interactive Server)
- BootstrapBlazor + FontAwesome
- `MesApiClient`（Typed HttpClient）

## 明确不做

- 登录 / RBAC
- 测试流程 CRUD 管理界面
- 设备硬件联机
