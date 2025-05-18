# Unity CPU Scheduling Visualizer

A Unity-based CPU scheduling simulator & visualizer that represents processes as racing cars on a track, and outputs a textual Gantt chart of the scheduling timeline.

## Team members:
- Mula Sohan
- Vaibhav Soin
- Srivanth Srinivasan
- Manya Chadaga
- Anumaneni Venkat Balachandra
---

## 🎯 Features

- **Preemptive Priority Scheduling**  
  Simulates a priority‑based scheduler: the highest‑priority “car” (process) always runs next, with context switching.

- **Car‑Race Animation**  
  Each process is represented by a car GameObject that moves forward proportionally to its remaining burst time.

- **Textual Gantt Chart**  
  When the simulation finishes, a text‑based Gantt chart is generated in a TextMeshProUGUI box, showing exactly when each process ran.

- **Easy Process Input**  
  A simple UI panel lets you enter arrival time, burst time and priority for each process before you hit **Start Race**.

- **Configurable Resolution**  
  Scale “time per character” in the Gantt chart to get coarse or fine‑grained timelines.

---

## 🚀 Getting Started

### Prerequisites

- **Unity**: Tested on Unity 2021.3 LTS (and newer).  
- **TextMeshPro**: Included via Package Manager (required for the Gantt chart textbox).  

### Installation

1. **Clone the Repo**  
   ```bash
   git clone https://github.com/venkat1924/TeachReach_EduAIThon
   cd TeachReach_EduAIThon

2. **Open in Unity**

   * Launch Unity Hub.
   * Click **Add** and select the cloned folder.
   * Open the project with a supported Unity version.

---

## ▶️ Usage

1. **Enter Process Data**

   * In the **Process Input** panel, specify each process’s:

     * **Arrival Time** (in seconds)
     * **Burst Time** (in seconds)
     * **Priority** (lower number = higher priority)
2. **Start the Simulation**

   * Click **Start Race!**
   * Cars will spawn and move toward the finish line based on scheduling.
3. **View Gantt Chart**

   * When all cars finish, the textual Gantt chart will appear in the bottom‑right TextMeshPro box.
   * Each `#` represents a time slice of activity; `.` is idle.

---

## 🗂 Project Structure

```
Assets/
├─ Scenes/
│  └─ MainScene.unity
├─ Scripts/
│  ├─ ProcessManager.cs    # Core simulation & Gantt chart logic
│  ├─ ProcessCar.cs        # Defines CarData & spawning behavior
│  └─ …  
├─ Prefabs/
│  ├─ GanttBar.prefab      # (unused in text mode; for graphical bars)
│  └─ Car.prefab           # The “car” Process representation
├─ UI/
│  ├─ ProcessInputPanel    # Input panel for arrival/burst/priority
│  └─ Canvas.prefab        # Contains Start button & Gantt text box
└─ TextMesh Pro/           # Font assets & materials
```

---

*Happy Scheduling!* 🚗💨

```
```
