# flowcube

A cross-disciplinary project bridging **hardware (M5Stack) + game engine (Unity) + AI large language models**.

---

## Features

### Feature 1 – Flip to Start (Focus → Light the Planet)
Flipping the M5Stack cube so the *learning face* is up automatically starts a forward timer.  
The virtual planet in the app transitions from dull grey to vibrant colours and glowing grass sprouts as you focus.

**Key scripts:**
| Script | Responsibility |
|--------|---------------|
| `Assets/Scripts/Hardware/M5StackConnector.cs` | BLE communication with M5Stack; keyboard fallback for Editor testing |
| `Assets/Scripts/Core/TimingManager.cs` | Manages the session timer; fires `OnSessionStarted` / `OnSessionEnded` / `OnTick` events |
| `Assets/Scripts/Planet/PlanetController.cs` | Drives planet visual state (light colour, surface colour, grass particles) |

---

### Feature 2 – Dynamic Ecosystem Generation
Based on cumulative focus time, Low-Poly plants appear on the planet surface:

| Milestone | Unlocked Item |
|-----------|--------------|
| 15 min total focus | 荧光草 (Glowing Grass Patch) |
| 45 min total focus | 发光树 (Glowing Tree) |
| 3 consecutive focus days | 遗迹石碑 (Ancient Relic) |

Progress is persisted between sessions via a JSON file (`flowcube_data.json` in `Application.persistentDataPath`).

**Key scripts:**
| Script | Responsibility |
|--------|---------------|
| `Assets/Scripts/Core/DataManager.cs` | JSON-based session persistence; exposes stats helpers |
| `Assets/Scripts/Ecosystem/PlantData.cs` | ScriptableObject data descriptor for each plant milestone |
| `Assets/Scripts/Ecosystem/EcosystemManager.cs` | Evaluates unlock conditions and instantiates plant prefabs |

---

### Feature 3 – Short-Memory AI Therapist
After each session the app packages behavioural context and calls an AI API
(DeepSeek / Kimi / ZhipuAI GLM). The AI responds with ≤ 30 Chinese characters
in a warm, restrained therapeutic style.

Context packaged per request:
- Today's focus session count & total duration
- Today's exercise duration
- Current hour (time-of-day label)
- Consecutive focus-day streak
- Last session type & duration

**Key scripts:**
| Script | Responsibility |
|--------|---------------|
| `Assets/Scripts/AI/AIRequestContext.cs` | Context data model + helper labels |
| `Assets/Scripts/AI/AITherapistManager.cs` | Builds prompt, sends HTTP request (UnityWebRequest), parses response |
| `Assets/Scripts/UI/UIManager.cs` | Loading animation, AI message fade-in/out, unlock toasts |

---

## Project Structure

```
Assets/
├── Scripts/
│   ├── Flowcube.Runtime.asmdef
│   ├── Core/
│   │   ├── DataManager.cs
│   │   └── TimingManager.cs
│   ├── Hardware/
│   │   └── M5StackConnector.cs
│   ├── Planet/
│   │   └── PlanetController.cs
│   ├── Ecosystem/
│   │   ├── EcosystemManager.cs
│   │   └── PlantData.cs
│   ├── AI/
│   │   ├── AITherapistManager.cs
│   │   └── AIRequestContext.cs
│   └── UI/
│       └── UIManager.cs
├── Tests/
│   └── EditMode/
│       ├── Flowcube.Tests.EditMode.asmdef
│       ├── DataManagerTests.cs
│       ├── EcosystemManagerTests.cs
│       └── AIRequestContextTests.cs
├── Prefabs/          (see Prefabs/README.md)
├── Materials/        (see Materials/README.md)
└── Resources/
    └── PlantConfigs/ (PlantData ScriptableObject assets go here)
```

---

## Getting Started

1. Open the project in **Unity 2022 LTS** or later (URP recommended).
2. Set your AI API key in the Inspector for `AITherapistManager` (or use environment-variable injection at build time).
3. Wire up the `M5StackConnector` service/characteristic UUIDs to match your M5Stack firmware.
4. Run in the **Editor**: press **F1** to simulate "focus face up", **F2** for exercise, **F3** for rest.
5. Open the Unity **Test Runner** window (`Window → General → Test Runner`) and run the *EditMode* tests.

---

## AI API Configuration

The `AITherapistManager` uses the **OpenAI-compatible chat completion format** supported by:
- [DeepSeek](https://platform.deepseek.com/) — `https://api.deepseek.com/v1/chat/completions`
- [Moonshot (Kimi)](https://platform.moonshot.cn/) — `https://api.moonshot.cn/v1/chat/completions`
- [ZhipuAI GLM](https://open.bigmodel.cn/) — `https://open.bigmodel.cn/api/paas/v4/chat/completions`

Set the `apiEndpoint`, `apiKey`, and `modelName` fields in the Inspector.  
**Do NOT commit real API keys to version control.**
