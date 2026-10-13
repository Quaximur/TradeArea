# TradeArea 🛒

A Unity client for my real-time game auction backend.

Log in. Browse the market. Place an order. Watch the backend push the result back.

Built as the client-side part of the [Aucy](https://github.com/Quaximur/Aucy) microservice ecosystem.

## 🛠️ Tech Stack

[![Unity](https://img.shields.io/badge/Unity-6.3-000000?style=for-the-badge&logo=unity&logoColor=white)](https://github.com/Unity-Technologies/UnityCsReference)
[![URP](https://img.shields.io/badge/URP-17-000000?style=for-the-badge&logo=unity&logoColor=white)](https://github.com/Unity-Technologies/Graphics)
[![Input System](https://img.shields.io/badge/Input_System-1.19-000000?style=for-the-badge&logo=unity&logoColor=white)](https://github.com/Unity-Technologies/InputSystem)

[![MagicOnion](https://img.shields.io/badge/MagicOnion-7.10-512BD4?style=for-the-badge)](https://github.com/Cysharp/MagicOnion)
[![R3](https://img.shields.io/badge/R3-Reactive-512BD4?style=for-the-badge)](https://github.com/Cysharp/R3)
[![UniTask](https://img.shields.io/badge/UniTask-Async-512BD4?style=for-the-badge)](https://github.com/Cysharp/UniTask)
[![VContainer](https://img.shields.io/badge/VContainer-DI-4B4B4B?style=for-the-badge)](https://github.com/hadashiA/VContainer)
[![YetAnotherHttpHandler](https://img.shields.io/badge/YAHH-HTTP%2F2-244C5A?style=for-the-badge)](https://github.com/Cysharp/YetAnotherHttpHandler)

[![DevKit](https://img.shields.io/badge/DevKit-Quaximur-222222?style=for-the-badge)](https://github.com/Quaximur/DevKit)
[![DevKit.UI](https://img.shields.io/badge/DevKit.UI-MVVM-222222?style=for-the-badge)](https://github.com/Quaximur/DevKit.UI)

## ⚡ What's Inside

- **Authentication** — login and registration through the Authy microservice.
- **Auction Market** — browse item listings and current buy/sell price levels.
- **Lots & Bids** — create, remove and collect auction orders.
- **Real-Time Updates** — MagicOnion StreamingHub delivers auction and inventory changes without polling.
- **Server Streaming** — market listings are received as streaming data from Aucy.
- **Reactive UI** — R3 keeps UI state synchronized with network and application state.
- **Dependency Injection** — VContainer composes networking, data and UI services.
- **Async without Coroutines** — UniTask is used for asynchronous client workflows.
- **Shared Contracts** — the client consumes the same MagicOnion contracts as Aucy and Authy.
- **HTTP/2 Transport** — YetAnotherHttpHandler provides the transport used by MagicOnion inside Unity.

## 🧩 Backend

TradeArea talks to the backend through a single gateway:

```text
                         ┌───────────────┐
                         │   TradeArea   │
                         │     Unity     │
                         └───────┬───────┘
                                 │
                         MagicOnion / gRPC
                                 │
                                 ▼
                         ┌───────────────┐
                         │     Gaty      │
                         │    Gateway    │
                         └───┬───────┬───┘
                             │       │
                    /authy/* │       │ /aucy/*
                             ▼       ▼
                      ┌──────────┐ ┌──────────┐
                      │  Authy   │ │   Aucy   │
                      │          │ │          │
                      │ Login    │ │ Market   │
                      │ Register │ │ Orders   │
                      │ JWT      │ │ Inventory│
                      └──────────┘ └─────┬────┘
                                         │
                                  StreamingHub
                                  Server Stream
                                         │
                                         └──────► TradeArea
```

The backend consists of three separate services:

- [**Gaty**](https://github.com/Quaximur/Gaty) — gateway, routing and JWT validation.
- [**Authy**](https://github.com/Quaximur/Authy) — lightweight authentication service.
- [**Aucy**](https://github.com/Quaximur/Aucy) — auction, inventory, order book and real-time trading.

TradeArea references the shared `Aucy.OnionContracts` and `Authy.OnionContracts` projects, keeping the MagicOnion API contract strongly typed on both sides.

## 📡 Real-Time Client

The auction isn't treated like a regular request/refresh UI.

```text
TradeArea                          Aucy
    │                               │
    │──── Create Bid ──────────────►│
    │                               │
    │                          Order Book
    │                           Matching
    │                               │
    │◄──── Order Updated ───────────│
    │◄──── Inventory Changed ───────│
    │◄──── Item Bought/Sold ────────│
    │                               │
```

Unary calls are used for actions while long-lived MagicOnion connections handle server-driven updates.

This lets the UI react to trades as they happen instead of continuously polling the backend.

## 🧱 Project Structure

The Unity solution is split into several assemblies:

```text
TradeArea/
├── TradeArea.UI              # UI and presentation
├── TradeArea.Gameplay        # Client gameplay/application logic
├── TradeArea.Data            # Client-side data layer
├── TradeArea.Infrastructure  # Networking and external services
├── TradeArea.Shared          # Shared client code
├── TradeArea.Editor          # Editor tooling
└── TradeArea.Tests           # Tests

Shared contracts:
├── Aucy.OnionContracts
└── Authy.OnionContracts
```

## 🔗 The Whole Project

```text
                         ┌─────────────┐
                         │  TradeArea  │
                         │ Unity Client│
                         └──────┬──────┘
                                │
                                ▼
                         ┌─────────────┐
                         │    Gaty     │
                         │   Gateway   │
                         └───┬─────┬───┘
                             │     │
                             ▼     ▼
                        ┌────────┐ ┌────────┐
                        │ Authy  │ │  Aucy  │
                        └────────┘ └────────┘
```

| Project | Role |
| --- | --- |
| **TradeArea** | Unity client |
| [**Gaty**](https://github.com/Quaximur/Gaty) | API gateway |
| [**Authy**](https://github.com/Quaximur/Authy) | Authentication |
| [**Aucy**](https://github.com/Quaximur/Aucy) | Auction & inventory |

## 🚀 Running

TradeArea currently uses **Unity 6000.3.15f1**.

Clone the project and open it through Unity Hub.

The
