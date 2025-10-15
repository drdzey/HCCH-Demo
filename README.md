# HCCH-Demo
HCCH (HotCallChaining) is a distributed function routing and orchestration engine for modular AI systems, multi-agent architectures, and real-time microservices in .NET.

<p align="center">
  <b>🏷️ Annotation-based registration • 🔄 Dynamic routing • 🤖 LLM/AI support • 🧪 Unit-tested</b>
</p>

---

## ✨ Key Features

- 🏷️ **Annotation-based handler registration:**  
  Register delegates/handlers in code with simple attributes – no manual boilerplate dispatchers.

- 🔎 **Dynamic harvesting & discovery:**  
  System automatically scans and registers handlers on every node; all capabilities are discoverable at runtime.

- 🚦 **Intelligent HotCall routing:**  
  Functions (or “prompts”) are always executed on the correct node—locally, upstream, or even remote via network.

- 🤖 **OpenAI/LLM integration:**  
  Out-of-the-box support for OpenAI “functionCall” pattern – enables agent orchestration and AI skill-chaining.

- 🔌 **Plug & play modularity:**  
  HCCH can power any .NET agent, orchestrator, or modular AI application – just connect and go.

- 🧪 **Unit-tested & robust:**  
  Includes extensive unit tests for all core functionality.

---

## 🚀 Why it matters

- **Productivity:**  
  Register new AI actions in seconds—no hand-written routers or service locators.

- **Scalability:**  
  Move business logic and skills freely between nodes (Core, Agents, Peers, etc.) without breaking the system.

- **Composability:**  
  Orchestrate distributed workflows, real-time data, and multi-agent “skills” with one simple, dynamic engine.

> “HCCH lets me write, register, and trigger new AI skills anywhere in my system—no matter how distributed, complex, or dynamic it gets. It just works.”

---

## 📦 What’s inside?

- **Core HCCH engine**  
- **Handler interfaces & annotations**  
- **Unit tests**  
- **Sample code**  
- *(No .sln/.csproj or working build files—see disclaimer)*

---

## Disclaimer

This repository is provided as a **code sample** and architectural reference for the HCCH (HotCallChaining) engine.  
It does **not contain a full working application or solution files (.sln, .csproj)**.  
Use the code snippets and test examples as inspiration or a reference for building distributed orchestration in .NET.

---

*If you have questions or want to see a more complete example, feel free to reach out!*
