# Deadline Drift: A Unity-based CPU Scheduling Visualizer

A Unity-based CPU scheduling simulator & visualizer that represents processes as racing cars on a track, enhanced with an integrated LLM-powered chat interface for automatic scenario parsing and simulation.

---

## 🎯 Key Features

* *Preemptive Priority Scheduling*
  Simulates a priority‑based CPU scheduler: the highest‑priority “car” (process) always runs next, with context switching.

* *Car‑Race Animation*
  Each process is represented by a car GameObject that moves forward proportionally to its remaining burst time.

* *Textual Gantt Chart*
  When the simulation finishes, a text‑based Gantt chart is generated in a TextMeshProUGUI box, showing exactly when each process ran.

* *Easy Manual Input*
  A simple UI panel lets you enter arrival time, burst time and priority for each process before you hit *Start Race*.

* *LLM-Powered Automation*
  Type natural‑language scheduling commands into a chat window; the LLM parses algorithm name, arrival times, burst times, and priorities, then automatically spawns cars and starts the race.

* *Configurable Resolution*
  Scale “time per character” in the Gantt chart to get coarse or fine‑grained timelines.

---

## 🚀 Getting Started

### Prerequisites

* *Unity*: Tested on Unity 2021.3 LTS (and newer).
* *TextMeshPro*: Included via Package Manager (required for the Gantt chart textbox).
* *Gemini/LLM API Key*: Needed for chat-based scenario parsing. Place your key in Assets/Resources/unity_and_gemini_key.json.

### Installation

1. *Clone the Repo*

   ```bash
   git clone https://github.com/your-username/UnityCPUSchedulingVisualizer.git
   cd UnityCPUSchedulingVisualizer
   ```
   
2. *Open in Unity*

   * Launch Unity Hub.
   * Click *Add* and select the cloned folder.
   * Open the project with a supported Unity version.
3. *Configure API Key*

   * In Assets/Resources/unity_and_gemini_key.json insert your LLM API key:

     ```json
     { "key": "YOUR_API_KEY_HERE" }
     ```
     

---

## ▶ Usage

### Manual Mode

1. *Enter Process Data*

   * In the *Process Input* panel, specify each process’s:

     * *Arrival Time* (in seconds)
     * *Burst Time* (in seconds)
     * *Priority* (lower number = higher priority)
2. *Start the Simulation*

   * Click *Start Race!*
   * Cars will spawn and move toward the finish line based on scheduling.
3. *View Gantt Chart*

   * When all cars finish, the textual Gantt chart will appear in the designated TextMeshPro box.

### LLM Chat Mode

1. *Open the Chat Panel*

   * Click the *Chatbot* button in the top toolbar to open the chat window.
2. *Type a Natural-Language Command*
   e.g.:

   > Simulate 5 processes using preemptive priority scheduling. Arrival times: 0,1,2,3,4. Burst times: 5,3,4,2,6. Priorities: 1,3,2,1,2.
3. *Automatic Parsing & Simulation*

   * The integrated LLM will parse your command, spawn the cars under the hood, and immediately start the race—no manual data entry required.
4. *Review Results*

   * Watch the race unfold on the track and inspect the Gantt chart when it completes.

---

Happy Scheduling & Chatting! 🚗🤖💨
